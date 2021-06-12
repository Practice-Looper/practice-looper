// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Emka.PracticeLooper.Mobile.Converters;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using NUnit.Framework;

namespace Emka.PracticeLooper.Mobile.Tests
{
    [TestFixture()]
    public class ConverterTests
    {
        public ConverterTests()
        {
        }

        [TestCase(0, 1, "Start")]
        [TestCase(0.1, 1, "Start")]
        [TestCase(0.2, 1, "Start")]
        [TestCase(0.3, 1, "Start")]
        [TestCase(0.4, 1, "Start")]
        [TestCase(0.5, 1, "Start")]
        [TestCase(0.6, 1, "Start")]
        [TestCase(0.7, 1, "Start")]
        [TestCase(0.8, 1, "Start")]
        [TestCase(0.9, 1, "Start")]
        [TestCase(0.98, 1, "Start")]
        [TestCase(0, 0.1, "End")]
        [TestCase(0, 0.2, "End")]
        [TestCase(0, 0.3, "End")]
        [TestCase(0, 0.4, "End")]
        [TestCase(0, 0.5, "End")]
        [TestCase(0, 0.6, "End")]
        [TestCase(0, 0.7, "End")]
        [TestCase(0, 0.8, "End")]
        [TestCase(0, 0.9, "End")]
        [TestCase(0, 1, "End")]
        public void When_ConvertTime_ValidValues_Expect_ValidOutput(double start, double end, string position)
        {
            var regex = new Regex("^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$");
            var loop = new Loop
            {
                Id = 0,
                Name = "Loop",
                Session = default,
                SessionId = default,
                StartPosition = start,
                EndPosition = end,
                IsDefault = false
            };

            var audioSource = new AudioSource
            {
                FileName = "file1",
                Type = AudioSourceType.Spotify,
                Source = "1234567890:666",
                Duration = 60000 / 1000
            };

            var session = new Session
            {
                Id = 0,
                Name = audioSource.FileName,
                AudioSource = audioSource,
                Loops = new List<Loop>
                        {
                            loop
                        }
            };

            loop.Session = session;

            var loopVm = new LoopViewModel(loop);
            var loggerMock = new Mock<ILogger>();
            var converter = new DoubleToTimeConverter(loggerMock.Object);

            var result = (string)converter.Convert(loopVm, null, position, Thread.CurrentThread.CurrentCulture);
            Assert.NotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsTrue(regex.IsMatch(result));
        }


        [Test]
        public void When_ConvertTime_LoopIsNull_Expect_ZeroTime()
        {
            var loopVm = new LoopViewModel(null);
            var loggerMock = new Mock<ILogger>();
            var converter = new DoubleToTimeConverter(loggerMock.Object);

            loggerMock.Setup(l => l.LogError(It.Is<Exception>(e => e.Message == "failed to convert time"), It.IsAny<Dictionary<string, string>>()))
                .Verifiable();

            var result = (string)converter.Convert(loopVm, null, "Start", Thread.CurrentThread.CurrentCulture);
            Assert.NotNull(result);
            Assert.AreEqual("00:00", result);
            loggerMock.Verify(l => l.LogError(It.Is<Exception>(e => e.Message == "failed to convert time"), It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Test]
        public void When_ConvertTime_SessionIsNull_Expect_ZeroTime()
        {
            var loop = new Loop
            {
                Id = 0,
                Name = "Loop",
                Session = default,
                SessionId = default,
                StartPosition = 0,
                EndPosition = 1,
                IsDefault = false
            };

            var loopVm = new LoopViewModel(loop);
            var loggerMock = new Mock<ILogger>();
            var converter = new DoubleToTimeConverter(loggerMock.Object);

            loggerMock.Setup(l => l.LogError(It.Is<Exception>(e => e.Message == "failed to convert time"), It.IsAny<Dictionary<string, string>>()))
                .Verifiable();

            var result = (string)converter.Convert(loopVm, null, "Start", Thread.CurrentThread.CurrentCulture);
            Assert.NotNull(result);
            Assert.AreEqual("00:00", result);
            loggerMock.Verify(l => l.LogError(It.Is<Exception>(e => e.Message == "failed to convert time"), It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Test]
        public void When_ConvertTime_AudioSourceIsNull_Expect_ZeroTime()
        {
            var loop = new Loop
            {
                Id = 0,
                Name = "Loop",
                Session = default,
                SessionId = default,
                StartPosition = 0,
                EndPosition = 1,
                IsDefault = false
            };

            var session = new Session
            {
                Id = 0,
                Name = "some Name",
                AudioSource = null,
                Loops = new List<Loop>
                        {
                            loop
                        }
            };

            loop.Session = session;

            var loopVm = new LoopViewModel(loop);
            var loggerMock = new Mock<ILogger>();
            var converter = new DoubleToTimeConverter(loggerMock.Object);

            loggerMock.Setup(l => l.LogError(It.Is<Exception>(e => e.Message == "failed to convert time"), It.IsAny<Dictionary<string, string>>()))
                .Verifiable();

            var result = (string)converter.Convert(loopVm, null, "Start", Thread.CurrentThread.CurrentCulture);
            Assert.NotNull(result);
            Assert.AreEqual("00:00", result);
            loggerMock.Verify(l => l.LogError(It.Is<Exception>(e => e.Message == "failed to convert time"), It.IsAny<Dictionary<string, string>>()), Times.Once);
        }
    }
}
