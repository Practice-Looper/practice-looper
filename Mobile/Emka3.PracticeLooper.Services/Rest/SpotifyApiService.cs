// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

namespace Emka3.PracticeLooper.Services.Rest
{
    [Preserve(AllMembers = true)]
    public class SpotifyApiService : ISpotifyApiService
    {
        #region Fields

        IHttpApiClient apiClient;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        int limit;
        public bool UserPremiumCheckSuccessful { get; set; }
        #endregion

        #region Ctor

        public SpotifyApiService(IHttpApiClient apiClient, IConfigurationService configurationService, ILogger logger)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            UserPremiumCheckSuccessful = false;
        }
        #endregion

        #region Methods

        public async Task<List<SpotifyTrack>> SearchTrackByName(string name, CancellationToken cancellationToken)
        {
            limit = configurationService.GetValue<int>("SpotifyApiLimit");
            List<SpotifyTrack> results = new List<SpotifyTrack>();
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["q"] = name;
            query["type"] = "track";
            query["limit"] = limit.ToString();

            try
            {
                string result;
                using (var response = await apiClient.SendRequestAsync(HttpMethod.Get, "search?" + query, cancellationToken))
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
                string result;
                HttpStatusCode statusCode;
                using (var response = await apiClient.SendRequestAsync(HttpMethod.Get, "me", default))
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

        public async Task PauseCurrentPlayback()
        {
            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["state"] = "off";
                await apiClient.SendRequestAsync(HttpMethod.Put, "me/player/pause", default);
                await apiClient.SendRequestAsync(HttpMethod.Put, "me/player/repeat?" + query, default);
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
        }
        #endregion
    }
}
