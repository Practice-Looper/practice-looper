// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Rest;
using Moq;
using NUnit.Framework;

namespace Emka3.PracticeLooper.Service.Tests
{
    [TestFixture()]
    public class SpotifyApiTests
    {
        private readonly Mock<IHttpApiClient> HttpApiClientMock;
        private readonly Mock<IConfigurationService> ConfigurationServiceMock;
        private readonly Mock<ILogger> LoggerMock;
        private readonly Mock<ITokenStorage> TokenStorageMock;
        public SpotifyApiTests()
        {
            HttpApiClientMock = new Mock<IHttpApiClient>();
            ConfigurationServiceMock = new Mock<IConfigurationService>();
            LoggerMock = new Mock<ILogger>();
            TokenStorageMock = new Mock<ITokenStorage>();
        }

        [Test()]
        public async Task When_SearchMeshuggah_Expect_30Results()
        {
            var jsonString = LoadJson("JsonTestData.txt");
            HttpApiClientMock
                .Setup(c => c.SendRequestAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<HttpContent>(), null, null))
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(jsonString) })
                .Verifiable();

            var spotifyService = new SpotifyApiService(HttpApiClientMock.Object, ConfigurationServiceMock.Object, LoggerMock.Object, TokenStorageMock.Object);
            var result = await spotifyService.SearchTrackByName(string.Empty, CancellationToken.None);

            Assert.NotNull(result);
            Assert.That(result, Has.Count.EqualTo(30));
            Assert.True(result.TrueForAll(r => r.ArtistNames.ToLower().Contains("meshuggah") || r.Name.ToLower().Contains("meshuggah")));
            HttpApiClientMock.Verify();
        }

        [Test()]
        public async Task When_CheckPremiumUser_Await_True()
        {
            var jsonString = LoadJson("JsonUserData.txt");
            HttpApiClientMock
                .Setup(c => c.SendRequestAsync(HttpMethod.Get, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<HttpContent>(), null, null))
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(jsonString) })
                .Verifiable();

            var spotifyService = new SpotifyApiService(HttpApiClientMock.Object, ConfigurationServiceMock.Object, LoggerMock.Object, TokenStorageMock.Object);
            var result = await spotifyService.IsPremiumUser();

            Assert.NotNull(result);
            Assert.True(result.Item1 == System.Net.HttpStatusCode.OK && result.Item2);
            HttpApiClientMock.Verify();
        }

        private string LoadJson(string fileName)
        {
            string json;
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Emka3.PracticeLooper.Service.Tests.TestData.{fileName}");
            using StreamReader r = new StreamReader(stream);
            json = r.ReadToEnd();
            return json;
        }
    }
}
