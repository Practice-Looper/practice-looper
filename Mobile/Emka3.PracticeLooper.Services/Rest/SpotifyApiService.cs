// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Emka3.PracticeLooper.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;

namespace Emka3.PracticeLooper.Services.Rest
{
    [Preserve(AllMembers = true)]
    public class SpotifyApiService : ISpotifyApiService
    {
        #region Fields

        IHttpApiClient apiClient;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private readonly ITokenStorage tokenStorage;
        private readonly string spotifyApiBaseAddress;
        private readonly string spotifyTokenApiBaseAddress;
        private SpotifyClient spotifyClient;
        private int averageRequestTime; //ms
        private int limit;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private Stopwatch requestTimer;
        private CircularBuffer<long> requestTimeBuffer;
        #endregion

        #region Ctor

        public SpotifyApiService(IHttpApiClient apiClient, IConfigurationService configurationService, ILogger logger, ITokenStorage tokenStorage)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
            UserPremiumCheckSuccessful = false;
            spotifyApiBaseAddress = configurationService.GetValue("SpotifyClientApiUri");
            spotifyTokenApiBaseAddress = configurationService.GetValue("SpotifyClientAccountApiUri");
            averageRequestTime = 50;
            requestTimer = new Stopwatch();
            requestTimeBuffer = new CircularBuffer<long>(10);
        }
        #endregion

        #region Properties
        public bool UserPremiumCheckSuccessful { get; private set; }

        #endregion

        #region Methods

        public async Task<List<SpotifyTrack>> SearchTrackByName(string name, CancellationToken cancellationToken)
        {
            var token = await GetAccessToken();
            limit = configurationService.GetValue<int>("SpotifyApiLimit");
            List<SpotifyTrack> results = new List<SpotifyTrack>();
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["q"] = name;
            query["type"] = "track";
            query["limit"] = limit.ToString();

            try
            {
                string result;
                using (var response = await apiClient.SendRequestAsync(HttpMethod.Get, spotifyApiBaseAddress, "search?" + query, token, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }

                var jObject = JObject.Parse(result);
                var tracks = JsonConvert.DeserializeObject<List<SpotifyTrack>>(jObject["tracks"]["items"].ToString());

                if (tracks != null && tracks.Any())
                {
                    results.AddRange(tracks);
                }

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Tuple<HttpStatusCode, bool>> IsPremiumUser()
        {
            try
            {
                var token = await GetAccessToken();
                string result;
                HttpStatusCode statusCode;
                using (var response = await apiClient.SendRequestAsync(HttpMethod.Get, spotifyApiBaseAddress, "me", token, default))
                {
                    response.EnsureSuccessStatusCode();
                    statusCode = response.StatusCode;
                    result = await response.Content.ReadAsStringAsync();
                }

                var jObject = JObject.Parse(result);
                var hasPremium = jObject["product"].ToString().Equals("premium");

                UserPremiumCheckSuccessful = true;
                return Tuple.Create(statusCode, hasPremium);
            }
            catch (HttpRequestException)
            {
                UserPremiumCheckSuccessful = false;
                return Tuple.Create(HttpStatusCode.Conflict, false);
            }
            catch (Exception)
            {
                UserPremiumCheckSuccessful = false;
                return Tuple.Create(HttpStatusCode.Conflict, false);
            }
        }

        public async Task<bool> PauseCurrentPlayback()
        {
            try
            {
                var client = await GetSpotifyClient();
                StartRequestTimeMeasurement();
                var result = await client.Player.PausePlayback();
                return result;
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
            finally
            {
                StopRequestTimeMeasurement();
            }

            return false;
        }

        public async Task<bool> PlayTrack(string trackId, int positionMs)
        {
            try
            {
                var client = await GetSpotifyClient();
                var playbackRequest = new PlayerResumePlaybackRequest { Uris = new List<string> { trackId }, PositionMs = positionMs };
                StartRequestTimeMeasurement();
                var result = await client.Player.ResumePlayback(playbackRequest);
                return result;
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
            finally
            {
                StopRequestTimeMeasurement();
            }

            return false;
        }

        public async Task<bool> SeekTo(long position)
        {
            try
            {
                var client = await GetSpotifyClient();
                var seekToRequest = new PlayerSeekToRequest(position);
                StartRequestTimeMeasurement();
                var result = await client.Player.SeekTo(seekToRequest);
                return result;
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
            finally
            {
                StopRequestTimeMeasurement();
            }

            return false;
        }

        public async Task<List<SpotifyDevice>> GetActiveDevices()
        {
            try
            {
                var client = await GetSpotifyClient();
                StartRequestTimeMeasurement();
                var response = await client.Player.GetAvailableDevices();
                return response?.Devices?.Where(d => d.IsActive)?.Select(d => new SpotifyDevice(d.Id, d.Name, d.IsActive, d.Type))?.ToList();
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
            finally
            {
                StopRequestTimeMeasurement();
            }

            return new List<SpotifyDevice>();
        }

        public async Task<double> GetCurrentPlaybackPosition()
        {
            try
            {
                var client = await GetSpotifyClient();
                StartRequestTimeMeasurement();
                var response = await client.Player.GetCurrentPlayback();
                return response.ProgressMs;
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
            finally
            {
                StopRequestTimeMeasurement();
            }

            return default;
        }

        public double GetAverageRequestTime()
        {
            var averageTime = requestTimeBuffer.Buffer.Average(r => r);
            return averageTime;
        }

        private async Task<SpotifyClient> GetSpotifyClient()
        {
            var accessToken = await GetAccessToken();
            return spotifyClient ??= new SpotifyClient(accessToken);
        }

        private async Task<string> RefreshTokenAsync()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var spotifyClientId = configurationService.GetValue("SpotifyClientId");
                var spotifyClientSecret = configurationService.GetValue("SpotifyClientSecret");
                var refreshToken = await tokenStorage.GetRefreshTokenAsync(AudioSourceType.Spotify);
                var base64Secrets = Base64Encode($"{spotifyClientId}:{spotifyClientSecret}");
                var bodyParams = new Dictionary<string, string>();
                bodyParams.Add("grant_type", "refresh_token");
                bodyParams.Add("refresh_token", refreshToken);

                var content = new FormUrlEncodedContent(bodyParams);
                var basicAuthHeader = new AuthenticationHeaderValue("Basic", base64Secrets);

                string result;
                StartRequestTimeMeasurement();
                using (var response = await apiClient.SendRequestAsync(HttpMethod.Post, spotifyTokenApiBaseAddress, "token", null, default, content, basicAuthHeader))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(result);
                    var accessToken = jObject["access_token"]?.ToString();
                    var accessTokenExpiresIn = jObject["expires_in"]?.ToString();
                    var newRefreshToken = jObject["refresh_token"]?.ToString();

                    tokenStorage.UpdateAccessToken(AudioSourceType.Spotify, accessToken, int.Parse(accessTokenExpiresIn));

                    if (!string.IsNullOrWhiteSpace(newRefreshToken))
                    {
                        await tokenStorage.UpdateRefreshTokenAsync(AudioSourceType.Spotify, newRefreshToken);
                    }

                    return accessToken;
                }
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
            finally
            {
                semaphoreSlim.Release();
                StopRequestTimeMeasurement();
            }

            return string.Empty;
        }

        private async Task<string> GetAccessToken()
        {
            var accessToken = tokenStorage.GetAccessToken(AudioSourceType.Spotify);
            var tokenExpired = tokenStorage.HasTokenExpired(AudioSourceType.Spotify);

            // token not yet fetched or already expired => send refresh request and create a fresh client
            if (string.IsNullOrWhiteSpace(accessToken) || tokenExpired)
            {
                accessToken = await RefreshTokenAsync();
                spotifyClient = null;
            }

            return accessToken;
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private void StartRequestTimeMeasurement()
        {
            requestTimer.Reset();
            requestTimer.Start();
        }

        private void StopRequestTimeMeasurement()
        {
            requestTimer.Stop();
            requestTimeBuffer.Write(requestTimer.ElapsedMilliseconds);
        }
        #endregion
    }

    internal class CircularBuffer<T>
    {
        private T[] buffer;
        private int start;
        private int end;

        public CircularBuffer()
            : this(capacity: 3)
        {
        }

        public CircularBuffer(int capacity)
        {
            buffer = new T[capacity + 1];
            start = 0;
            end = 0;
        }

        public void Write(T value)
        {
            buffer[end] = value;
            end = (end + 1) % buffer.Length;
            if (end == start)
            {
                start = (start + 1) % buffer.Length;
            }
        }

        public T Read()
        {
            T result = buffer[start];
            start = (start + 1) % buffer.Length;
            return result;
        }

        public int Capacity
        {
            get { return buffer.Length; }
        }

        public bool IsEmpty
        {
            get { return end == start; }
        }

        public bool IsFull
        {
            get { return (end + 1) % buffer.Length == start; }
        }

        public T[] Buffer => buffer;
    }
}
