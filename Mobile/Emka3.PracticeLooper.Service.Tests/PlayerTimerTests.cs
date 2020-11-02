﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Player;
using Moq;
using NUnit.Framework;

namespace Emka3.PracticeLooper.Service.Tests
{
    [TestFixture()]
    public class PlayerTimerTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly PlayerTimer playerTimer;
        private static AutoResetEvent timerEvent;
        private const int Deviation = 50;

        public PlayerTimerTests()
        {
            loggerMock = new Mock<ILogger>();
            playerTimer = new PlayerTimer(loggerMock.Object);

        }

        [Test()]
        public async Task When_SetCurrentPositionTimer_Expect_FiresCurrentPositionTimerExpired()
        {
            await Task.Run(() => timerEvent = new AutoResetEvent(false));
            int counter = 0;
            playerTimer.CurrentPositionTimerExpired += (s, e) =>
            {
                counter++;
                if (counter == 3)
                {
                    playerTimer.StopTimers();
                    timerEvent.Set();
                }
            };

            playerTimer.SetCurrentTimeTimer(1000);
            await Task.Run(timerEvent.WaitOne);

            Assert.AreEqual(3, counter);
        }

        [TestCase(500)]
        [TestCase(1000)]
        [TestCase(2000)]
        [TestCase(10000)]
        public async Task When_SetLooperTimer_Expect_FiresLooperTimerExpired(double time)
        {
            await Task.Run(() => timerEvent = new AutoResetEvent(false));
            int counter = 0;
            playerTimer.LoopTimerExpired += (s, e) =>
            {
                counter++;
                if (counter == 3)
                {
                    playerTimer.StopTimers();
                    timerEvent.Set();
                }
            };

            playerTimer.SetLoopTimer(time);
            await Task.Run(timerEvent.WaitOne);
            Assert.AreEqual(3, counter);
        }
    }
}