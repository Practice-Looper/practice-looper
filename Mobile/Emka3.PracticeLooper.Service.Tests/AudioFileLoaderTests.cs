// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Common;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Service.Tests
{
    [TestFixture()]
    public class AudioFileLoaderTests
    {
        private readonly Mock<IConfigurationService> configServiceMock;
        private List<string> paths = new List<string> { "internal/Path", "external/Path" };

        public AudioFileLoaderTests()
        {
            configServiceMock = new Mock<IConfigurationService>();
        }

        [Test()]
        public void When_GetAbsoluteFilePath_ValidInternalAudioSource_Expect_ReturnsPath()
        {
            configServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.InternalStoragePath))).Returns(paths[0]);
            var audioFileLoader = new AudioFileLoader(configServiceMock.Object);
            var audioSource = new AudioSource
            {
                Type = AudioSourceType.LocalInternal,
                Source = "source"
            };

            var path = audioFileLoader.GetAbsoluteFilePath(audioSource);
            Assert.That(Path.Combine(paths[0], audioSource.Source), Is.EqualTo(path));
        }

        [Test()]
        public void When_GetAbsoluteFilePath_ValidExternalAudioSource_Expect_ReturnsPath()
        {
            configServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.ExternalStoragePath))).Returns(paths[1]);
            var audioFileLoader = new AudioFileLoader(configServiceMock.Object);
            var audioSource = new AudioSource
            {
                Type = AudioSourceType.LocalExternal,
                Source = "source"
            };

            var path = audioFileLoader.GetAbsoluteFilePath(audioSource);
            Assert.That(Path.Combine(paths[1], audioSource.Source), Is.EqualTo(path));
        }

        [Test()]
        public void When_GetAbsoluteFilePath_NullAudioSource_Expect_ArgumentNullException()
        {
            var audioFileLoader = new AudioFileLoader(configServiceMock.Object);
            Assert.Throws<ArgumentNullException>(() => audioFileLoader.GetAbsoluteFilePath(null));
        }

        [Test()]
        public void When_GetFileStream_NullAudioSource_Expect_ArgumentNullException()
        {
            var audioFileLoader = new AudioFileLoader(configServiceMock.Object);
            Assert.Throws<ArgumentNullException>(() => audioFileLoader.GetFileStream(null));
        }
    }
}
