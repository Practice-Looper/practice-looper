// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka3.PracticeLooper.Services.Common
{
    public class HttpApiClient : IHttpApiClient
    {
        public HttpApiClient()
        {
        }

        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string baseAddress, string path, string token, CancellationToken cancelToken, HttpContent content = null, AuthenticationHeaderValue authenticationHeader = null, HttpClientHandler handler = null)
        {
            try
            {
                var httpClient = handler == null ? new HttpClient() : new HttpClient(handler);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.BaseAddress = new Uri(baseAddress);
                httpClient.DefaultRequestHeaders.Authorization = authenticationHeader ?? new AuthenticationHeaderValue("Bearer", token);
                return await ExecuteRequestAsync(httpClient, method, path, content, cancelToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
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
    }
}
