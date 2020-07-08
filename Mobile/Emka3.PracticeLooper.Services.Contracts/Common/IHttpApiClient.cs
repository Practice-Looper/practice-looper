// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IHttpApiClient
    {
        Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string path, CancellationToken cancelToken, HttpContent content = null);
        void SetBearerToken(string token);
    }
}
