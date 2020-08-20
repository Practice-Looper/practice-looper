// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka3.PracticeLooper.Services.Common
{
    public class HttpApiClient : IHttpApiClient
    {
        readonly HttpClient httpClient;
        private readonly IConfigurationService configurationService;
        private readonly ITokenStorage accountManager;

        public HttpApiClient(IConfigurationService configurationService, ITokenStorage accountManager)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager)); ;
            this.accountManager.TokenChanged += OnTokenChanged;
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(configurationService.GetValue("SpotifyClientApiUri"))
            };

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accountManager.GetTokenAsync().Result);
        }

        void OnTokenChanged(object sender, Contracts.EventArgs.TokenChangedEventArgs e)
        {
            SetBearerToken(e.Token);
        }

        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string path, CancellationToken cancelToken, HttpContent content = null)
        {
            try
            {
                return await ExecuteRequestAsync(httpClient, method, path, content, cancelToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        async Task<HttpResponseMessage> ExecuteRequestAsync(HttpClient client, HttpMethod method, string path, HttpContent content, CancellationToken cancelToken)
        {
            HttpResponseMessage response;
            try
            {
                switch (method.Method)
                {
                    case "GET":
                        response = await client.GetAsync(path, cancelToken).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();
                        break;
                    case "POST":
                        response = await client.PostAsync(path, content, cancelToken).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();
                        break;
                    case "DELET":
                        response = await client.DeleteAsync(path, cancelToken).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();
                        break;
                    case "PUT":
                        response = await client.PutAsync(path, content, cancelToken).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();
                        break;
                    case "PATCH":
                        response = await client.PutAsync(path, content, cancelToken).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(method), method, null);
                }
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }

        public void SetBearerToken(string token)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
