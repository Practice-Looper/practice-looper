// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        int limit;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
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
                var result = await client.Player.PausePlayback();
                return result;
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }

            return false;
        }

        public async Task<bool> PlayTrack(string trackId, int positionMs)
        {
            try
            {
                var client = await GetSpotifyClient();
                var playbackRequest = new PlayerResumePlaybackRequest { Uris = new List<string> { trackId }, PositionMs = positionMs };
                var result = await client.Player.ResumePlayback(playbackRequest);
                return result;
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }

            return false;
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
            }

            return accessToken;
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        #endregion
    }
}
