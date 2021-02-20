// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using NUnit.Framework;

namespace Emka3.PracticeLooper.Service.Tests
{
    [TestFixture()]
    public class TokenStorageTests
    {
        private TokenStorage tokenStorage;
        private Mock<ISecureRepository> tokenRepositoryMock;
        private const string DummyToken = "thisismycoolandfunkytoken-usemefortestingpurposes!";
        private const int tokenExpiresIn = 5; // Minutes
        private Dictionary<string, object> testStorage;

        [SetUp()]
        public void TearDown()
        {
            testStorage = new Dictionary<string, object>();
            tokenRepositoryMock = new Mock<ISecureRepository>();
        }

        [Test()]
        public void When_UpdateSpotifyAccessToken_Expect_TokenHasValue()
        {
            tokenStorage = new TokenStorage(tokenRepositoryMock.Object);
            tokenStorage.UpdateAccessToken(AudioSourceType.Spotify, DummyToken, tokenExpiresIn);
            var currentToken = tokenStorage.GetAccessToken(AudioSourceType.Spotify);

            Assert.NotNull(currentToken);
            Assert.AreEqual(DummyToken, currentToken);
        }

        [Test()]
        public void When_UpdateSpotifyAccessToken_Expect_FiresEvent()
        {
            tokenStorage = new TokenStorage(tokenRepositoryMock.Object);
            var resetEvent = new AutoResetEvent(false);
            string tokenFromEvent = null;

            tokenStorage.TokenChanged += (s, e) =>
            {
                tokenFromEvent = e.Token;
                Task.Run(resetEvent.Set);
            };

            tokenStorage.UpdateAccessToken(AudioSourceType.Spotify, DummyToken, tokenExpiresIn);
            

            resetEvent.WaitOne();
            Assert.NotNull(tokenFromEvent);
            Assert.AreEqual(DummyToken, tokenFromEvent);
        }

        [Test()]
        public async Task When_UpdateSpotifyRefreshToken_Expect_TokenHasValue()
        {
            tokenRepositoryMock.Setup(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string k, object v) => testStorage.Add(k, v));
            tokenRepositoryMock.Setup(r => r.GetValueAsync(It.IsAny<string>())).Returns((string a) => Task.FromResult(testStorage.ContainsKey(a) ? testStorage[a].ToString() : null));
            tokenStorage = new TokenStorage(tokenRepositoryMock.Object);
            await tokenStorage.UpdateRefreshTokenAsync(AudioSourceType.Spotify, DummyToken);
            var currentToken = await tokenStorage.GetRefreshTokenAsync(AudioSourceType.Spotify);
            var hasToken = await tokenStorage.HasRefreshToken(AudioSourceType.Spotify);
            Assert.NotNull(currentToken);
            Assert.AreEqual(DummyToken, currentToken);
            Assert.True(hasToken);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [Test()]
        public void When_AddedEmptySpotifyToken_Expect_ArgumentException(string invalidToken)
        {
            tokenStorage = new TokenStorage(tokenRepositoryMock.Object);
            var ex = Assert.Throws<ArgumentNullException>(() => tokenStorage.UpdateAccessToken(AudioSourceType.Spotify, invalidToken, tokenExpiresIn));
            Assert.NotNull(ex);
            Assert.IsEmpty(testStorage);
        }

        [Test()]
        public async Task When_DeleteSpotifyRefreshToken_Expect_EmptyStorage()
        {
            tokenRepositoryMock.Setup(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string k, object v) => testStorage.Add(k, v));
            tokenRepositoryMock.Setup(r => r.GetValueAsync(It.IsAny<string>())).Returns((string a) => Task.FromResult(testStorage.ContainsKey(a) ? testStorage[a].ToString() : null));
            tokenRepositoryMock.Setup(r => r.DeleteValueAsync(It.IsAny<string>())).Callback((string k) => testStorage.Remove(k));
            tokenStorage = new TokenStorage(tokenRepositoryMock.Object);
            await tokenStorage.UpdateRefreshTokenAsync(AudioSourceType.Spotify, DummyToken);
            var currentToken = await tokenStorage.GetRefreshTokenAsync(AudioSourceType.Spotify);

            Assert.NotNull(currentToken);
            Assert.AreEqual(DummyToken, currentToken);

            await tokenStorage.DeleteTokenAsync();

            Assert.IsEmpty(testStorage);
            currentToken = await tokenStorage.GetRefreshTokenAsync(AudioSourceType.Spotify);
            Assert.Null(currentToken);

            tokenRepositoryMock.Verify(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            tokenRepositoryMock.Verify(r => r.GetValueAsync(It.IsAny<string>()), Times.Exactly(2));
            tokenRepositoryMock.Verify(r => r.DeleteValueAsync(It.IsAny<string>()), Times.Once);
        }

        [Test()]
        public void When_SpotifyTokenExpirationDateGreaterNow_Expect_TokenNotExpired()
        {
            tokenStorage = new TokenStorage(tokenRepositoryMock.Object);
            tokenStorage.UpdateAccessToken(AudioSourceType.Spotify, DummyToken, 5); // Minutes

            var hasExpired = tokenStorage.HasTokenExpired(AudioSourceType.Spotify);
            Assert.False(hasExpired);
        }

        [Test()]
        public async Task When_SpotifyTokenExpirationDateLessNow_Expect_TokenExpired()
        {
            tokenStorage = new TokenStorage(tokenRepositoryMock.Object);
            tokenStorage.UpdateAccessToken(AudioSourceType.Spotify, DummyToken, 1); // Minutes
            await Task.Delay(70000);
            var hasExpired = tokenStorage.HasTokenExpired(AudioSourceType.Spotify);
            Assert.True(hasExpired);
        }
    }
}
