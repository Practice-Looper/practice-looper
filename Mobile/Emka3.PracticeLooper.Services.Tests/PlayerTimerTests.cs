// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Player;
using Moq;
using Xunit;

namespace Emka3.PracticeLooper.Services.Tests
{
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

        [Theory]
        [InlineData(500)]
        [InlineData(1000)]
        public async Task When_SetLooperTimer_Expect_FiresLoopTimerExpiredEvent(int ms)
        {
            await Task.Run(() => timerEvent = new AutoResetEvent(false));
            var watch = new Stopwatch();
            playerTimer.LoopTimerExpired += (s, e) =>
            {
                watch.Stop();
                timerEvent.Set();
            };

            playerTimer.SetLoopTimer(ms);
            watch.Start();
            await Task.Run(timerEvent.WaitOne);
            playerTimer.StopTimers();
            Assert.InRange(watch.ElapsedMilliseconds, ms - Deviation, ms + Deviation);
        }

        [Fact]
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

            Assert.Equal(3, counter);
        }
    }
}
