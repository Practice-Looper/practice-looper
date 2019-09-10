// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Threading.Tasks;
using System.Timers;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka3.PracticeLooper.Services.Player
{
    public class PlayerTimer : IPlayerTimer
    {

        #region Fields
        private Timer looperTimer;
        private Timer currentPositionTimer;
        #endregion Fields
        
        #region Events
        public event EventHandler LoopTimerExpired;
        public event EventHandler CurrentPositionTimerExpired;
        #endregion Events

        #region Methods
        public void SetCurrentTimeTimer(int time)
        {
            try
            {
                try
                {
                    currentPositionTimer = new Timer(time);
                    currentPositionTimer.Elapsed += OnCurrentPositionTimedEvent;
                    currentPositionTimer.AutoReset = true;
                    currentPositionTimer.Enabled = true;
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
            }
            catch (TaskCanceledException)
            {
                throw;
            }
        }

        public void SetLoopTimer(int time)
        {
            try
            {
                looperTimer = new Timer(time);
                looperTimer.Elapsed += OnLooperTimedEvent;
                looperTimer.AutoReset = true;
                looperTimer.Enabled = true;
            }
            catch (TaskCanceledException)
            {
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
            }

            if (currentPositionTimer != null)
            {

                currentPositionTimer.Stop();
            }
        }
        #endregion Methods
    }
}
