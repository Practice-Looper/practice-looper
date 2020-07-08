using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Rest;
using Moq;
using Xunit;

namespace Emka3.PracticeLooper.Services.Tests
{
    public class SpotifyApiTests
    {
        private readonly Mock<IHttpApiClient> HttpApiClientMock;

        public SpotifyApiTests()
        {
            HttpApiClientMock = new Mock<IHttpApiClient>();
        }

        [Fact]
        public async Task When_SearchMeshuggah_Expect_30Results()
        {
            var jsonString = LoadJson();
            HttpApiClientMock
                .Setup(c => c.SendRequestAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(jsonString) })
                .Verifiable();

            var spotifyService = new SpotifyApiService(HttpApiClientMock.Object);
            var result = await spotifyService.SearchTrackByName(string.Empty, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(30, result.Count);
            Assert.True(result.TrueForAll(r => r.ArtistNames.ToLower().Contains("meshuggah") || r.Name.ToLower().Contains("meshuggah")));
            HttpApiClientMock.Verify();
        }

        private string LoadJson()
        {
            using (StreamReader r = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "TestData", "JsonTestData.txt")))
            {
                return r.ReadToEnd();
            }
        }
    }
}
