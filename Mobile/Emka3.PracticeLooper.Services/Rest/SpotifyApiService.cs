﻿// Copyright (C)  - All Rights Reserved
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
using Emka3.PracticeLooper.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Emka3.PracticeLooper.Services.Rest
{
    [Preserve(AllMembers = true)]
    public class SpotifyApiService : ISpotifyApiService
    {
        HttpApiClient apiClient;
        int limit;
        public SpotifyApiService(IAccountManager accountManager)
        {
            apiClient = new HttpApiClient(Factory.GetConfigService().GetValue("SpotifyClientApiUri"), accountManager);
            limit = Factory.GetConfigService().GetValue<int>("SpotifyApiLimit");
        }

        public async Task<SpotifyAlbum> SearchAlbumById(string id, CancellationToken cancellationToken)
        {
            List<SpotifyAlbum> results = new List<SpotifyAlbum>();
            var uri = $"albums/{id}";

            try
            {
                string result;
                using (var response = await apiClient.SendRequestAsync(HttpMethod.Get, uri, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }

                var album = JsonConvert.DeserializeObject<SpotifyAlbum>(result);
                return album;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<SpotifyTrack>> SearchTrackByName(string name, CancellationToken cancellationToken)
        {
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
    }
}
