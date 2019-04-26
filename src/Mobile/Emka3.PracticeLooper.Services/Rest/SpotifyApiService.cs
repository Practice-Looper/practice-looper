// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Rest;
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

        public async Task SearchForTerm(string term)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["q"] = term;
            query["type"] = "artist,album,track";
            var response = await apiClient.SendRequestAsync(HttpMethod.Get, "search?" + query, CancellationToken.None);
            var result = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(result);
            var albums = ((JArray)jObject["albums"]["items"]).Select(a => (string)a).ToList();
            var artists = ((JArray)jObject["artists"]["items"]).Select(a => (string)a).ToList();
            var tracks = ((JArray)jObject["tracks"]["items"]).Select(a => (string)a).ToList();
        }
    }
}
