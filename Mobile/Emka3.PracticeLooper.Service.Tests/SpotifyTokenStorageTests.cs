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
using NUnit.Framework;

namespace Emka3.PracticeLooper.Service.Tests
{
    [TestFixture()]
    public class SpotifyTokenStorageTests
    {
        private SpotifyTokenStorage tokenStorage;
        private Mock<ISecureRepository> tokenRepositoryMock;
        private const string DummyToken = "thisismycoolandfunkytoken-usemefortestingpurposes!";
        private Dictionary<string, object> testStorage;

        [SetUp()]
        public void TearDown()
        {
            testStorage = new Dictionary<string, object>();
            tokenRepositoryMock = new Mock<ISecureRepository>();
        }

        [Test()]
        public async Task When_AddedValidToken_Expect_TokenHasValue()
        {
            tokenRepositoryMock.Setup(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string k, object v) => testStorage.Add(k, v));
            tokenRepositoryMock.Setup(r => r.GetValueAsync(It.IsAny<string>())).Returns((string a) => Task.FromResult(testStorage[a].ToString()));

            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            await tokenStorage.UpdateTokenAsync(DummyToken);
            var currentToken = await tokenStorage.GetTokenAsync();

            Assert.NotNull(currentToken);
            Assert.AreEqual(DummyToken, currentToken);

            tokenRepositoryMock.Verify(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            tokenRepositoryMock.Verify(r => r.GetValueAsync(It.IsAny<string>()), Times.Once);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [Test()]
        public void When_AddedEmptyToken_Expect_ArgumentException(string invalidToken)
        {
            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            var ex = Assert.ThrowsAsync<ArgumentNullException>(() => tokenStorage.UpdateTokenAsync(invalidToken));
            Assert.NotNull(ex);
            Assert.IsEmpty(testStorage);
        }

        [Test()]
        public async Task When_DeleteToken_Expect_EmptyStorage()
        {
            tokenRepositoryMock.Setup(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string k, object v) => testStorage.Add(k, v));
            tokenRepositoryMock.Setup(r => r.GetValueAsync(It.IsAny<string>())).Returns((string a) => Task.FromResult(testStorage.ContainsKey(a) ? testStorage[a].ToString() : null));
            tokenRepositoryMock.Setup(r => r.DeleteValueAsync(It.IsAny<string>())).Callback((string k) => testStorage.Remove(k));
            tokenStorage = new SpotifyTokenStorage(tokenRepositoryMock.Object);
            await tokenStorage.UpdateTokenAsync(DummyToken);
            var currentToken = await tokenStorage.GetTokenAsync();

            Assert.NotNull(currentToken);
            Assert.AreEqual(DummyToken, currentToken);

            await tokenStorage.DeleteTokenAsync();

            Assert.IsEmpty(testStorage);
            currentToken = await tokenStorage.GetTokenAsync();
            Assert.Null(currentToken);

            tokenRepositoryMock.Verify(r => r.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            tokenRepositoryMock.Verify(r => r.GetValueAsync(It.IsAny<string>()), Times.Exactly(2));
            tokenRepositoryMock.Verify(r => r.DeleteValueAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
