using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Rest;
using Moq;
using Xunit;

namespace Emka3.PracticeLooper.Services.Tests
{
    public class SpotifyApiTests
    {
        private readonly Mock<IHttpApiClient> HttpApiClientMock;
        private readonly Mock<IConfigurationService> ConfigurationServiceMock;
        private readonly Mock<ILogger> LoggerMock;

        public SpotifyApiTests()
        {
            HttpApiClientMock = new Mock<IHttpApiClient>();
            ConfigurationServiceMock = new Mock<IConfigurationService>();
            LoggerMock = new Mock<ILogger>();
        }

        [Fact]
        public async Task When_SearchMeshuggah_Expect_30Results()
        {
            var jsonString = LoadJson("JsonTestData.txt");
            HttpApiClientMock
                .Setup(c => c.SendRequestAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(jsonString) })
                .Verifiable();

            var spotifyService = new SpotifyApiService(HttpApiClientMock.Object, ConfigurationServiceMock.Object, LoggerMock.Object);
            var result = await spotifyService.SearchTrackByName(string.Empty, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(30, result.Count);
            Assert.True(result.TrueForAll(r => r.ArtistNames.ToLower().Contains("meshuggah") || r.Name.ToLower().Contains("meshuggah")));
            HttpApiClientMock.Verify();
        }

        [Fact]
        public async Task When_CheckPremiumUser_Await_True()
        {
            var jsonString = LoadJson("JsonUserData.txt");
            HttpApiClientMock
                .Setup(c => c.SendRequestAsync(HttpMethod.Get, It.IsAny<string>(), CancellationToken.None, It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(jsonString) })
                .Verifiable();

            var spotifyService = new SpotifyApiService(HttpApiClientMock.Object, ConfigurationServiceMock.Object, LoggerMock.Object);
            var result = await spotifyService.IsPremiumUser();

            Assert.NotNull(result);
            Assert.True(result.Item1 == System.Net.HttpStatusCode.OK && result.Item2);
            HttpApiClientMock.Verify();
        }

        private string LoadJson(string fileName)
        {
            using (StreamReader r = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName)))
            {
                return r.ReadToEnd();
            }
        }
    }
}
