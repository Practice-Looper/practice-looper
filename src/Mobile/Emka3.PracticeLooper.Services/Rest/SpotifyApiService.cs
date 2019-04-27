// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Emka3.PracticeLooper.Services.Rest
{
    public class SpotifyApiService : ISpotifyApiService
    {
        HttpApiClient apiClient;

        public SpotifyApiService(IAccountManager accountManager)
        {
            apiClient = new HttpApiClient(Factory.GetConfigService().GetValue("auth:spotify:client:uri:api"), accountManager);
        }

        public async Task<List<SpotifyResult>> SearchForTerm(string term, CancellationToken cancellationToken)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["q"] = term;
            query["type"] = "artist,album,track";
            query["limit"] = "5";
            var response = await apiClient.SendRequestAsync(HttpMethod.Get, "search?" + query, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            try
            {
                var jObject = JObject.Parse(result);
                var albumStrings = ((JArray)jObject["albums"]["items"]).Select((arg) => arg.ToString()).ToList();
                var artistStrings = ((JArray)jObject["artists"]["items"]).Select((arg) => arg.ToString()).ToList();
                var trackStrings = ((JArray)jObject["tracks"]["items"]).Select((arg) => arg.ToString()).ToList();

                return JsonConvert.DeserializeObject<List<SpotifyResult>>(jObject["artists"]["items"].ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return new List<SpotifyResult>();
        }
    }
}
