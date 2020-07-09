// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using Xunit;

namespace Emka3.PracticeLooper.Services.Tests
{
    public class SpotifyTokenStorageTests
    {
        private SpotifyTokenStorage tokenStorage;
        private readonly Mock<ISecureRepository> tokenRepositoryMock;
        private const string DummyToken = "thisismycoolandfunkytoken-usemefortestingpurposes!";
        private Dictionary<string, object> testStorage;

        public SpotifyTokenStorageTests()
        {
            testStorage = new Dictionary<string, object>();
            tokenRepositoryMock = new Mock<ISecureRepository>();
        }

        [Fact]
        public async Task When_AddedValidToken_Expect_TokenHasValue()
        {
            tokenRepositoryMock.Setup(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string k, object v) => testStorage.Add(k, v));
            tokenRepositoryMock.Setup(r => r.GetValueAsync(It.IsAny<string>())).Returns((string a) => Task.FromResult(testStorage[a].ToString()));

            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            await tokenStorage.UpdateTokenAsync(DummyToken);
            var currentToken = await tokenStorage.GetTokenAsync();

            Assert.NotNull(currentToken);
            Assert.Equal(DummyToken, currentToken);

            tokenRepositoryMock.Verify(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            tokenRepositoryMock.Verify(r => r.GetValueAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task When_AddedEmptyToken_Expect_ArgumentException()
        {
            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => tokenStorage.UpdateTokenAsync(""));
            Assert.Empty(testStorage);
        }

        [Fact]
        public async Task When_AddedWhitespaceToken_Expect_ArgumentException()
        {
            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => tokenStorage.UpdateTokenAsync(" "));
            Assert.Empty(testStorage);
        }

        [Fact]
        public async Task When_AddedNullToken_Expect_ArgumentException()
        {
            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => tokenStorage.UpdateTokenAsync(null));
            Assert.Empty(testStorage);
        }

        [Fact]
        public async Task When_DeleteToken_Expect_EmptyStorage()
        {
            tokenRepositoryMock.Setup(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string k, object v) => testStorage.Add(k, v));
            tokenRepositoryMock.Setup(r => r.GetValueAsync(It.IsAny<string>())).Returns((string a) => Task.FromResult(testStorage.ContainsKey(a) ? testStorage[a].ToString() : null));
            tokenRepositoryMock.Setup(r => r.DeleteValueAsync(It.IsAny<string>())).Callback((string k) => testStorage.Remove(k));
            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            await tokenStorage.UpdateTokenAsync(DummyToken);
            var currentToken = await tokenStorage.GetTokenAsync();

            Assert.NotNull(currentToken);
            Assert.Equal(DummyToken, currentToken);

            await tokenStorage.DeleteTokenAsync();

            Assert.Empty(testStorage);
            currentToken = await tokenStorage.GetTokenAsync();
            Assert.Null(currentToken);

            tokenRepositoryMock.Verify(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            tokenRepositoryMock.Verify(r => r.GetValueAsync(It.IsAny<string>()), Times.Exactly(2));
            tokenRepositoryMock.Verify(r => r.DeleteValueAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
