// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Threading.Tasks;
using System.Timers;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Services.Player
{
    [Preserve(AllMembers = true)]
    public class PlayerTimer : IPlayerTimer
    {

        #region Fields
        private Timer looperTimer;
        private Timer currentPositionTimer;
        private readonly ILogger logger;
        #endregion Fields

        #region Ctor
        public PlayerTimer(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion

        #region Events
        public event EventHandler LoopTimerExpired;
        public event EventHandler CurrentPositionTimerExpired;
        #endregion Events

        #region Methods
        public void SetCurrentTimeTimer(int time)
        {
            try
            {
                currentPositionTimer = new Timer(time);
                currentPositionTimer.Elapsed += OnCurrentPositionTimedEvent;
                currentPositionTimer.AutoReset = true;
                currentPositionTimer.Enabled = true;
            }
            catch (TaskCanceledException ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public void SetLoopTimer(double time)
        {
            // avoid negative values and values larger than int.MaxValue and timer to 10s
            time = time < 0 || time > int.MaxValue ? 10000 : time;

            try
            {
                looperTimer = new Timer(time);
                looperTimer.Elapsed += OnLooperTimedEvent;
                looperTimer.AutoReset = true;
                looperTimer.Enabled = true;
            }
            catch (TaskCanceledException ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        private void OnLooperTimedEvent(object sender, ElapsedEventArgs e)
        {
            LoopTimerExpired?.Invoke(this, new EventArgs());
        }

        private void OnCurrentPositionTimedEvent(object sender, ElapsedEventArgs e)
        {
            CurrentPositionTimerExpired?.Invoke(this, new EventArgs());
        }

        public void StopTimers()
        {
            if (looperTimer != null)
            {
                looperTimer.Stop();
                looperTimer.Elapsed -= OnLooperTimedEvent;
            }

            if (currentPositionTimer != null)
            {
                currentPositionTimer.Stop();
                currentPositionTimer.Elapsed -= OnCurrentPositionTimedEvent;
            }
        }
        #endregion Methods
    }
}
