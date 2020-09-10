// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class LooprTimer
    {
        private readonly TimeSpan timespan;
        private readonly Action callback;
        private object activeLock = new object();
        private CancellationTokenSource cancellation;
        private bool isActive;
        private System.Timers.Timer timer;
        public LooprTimer(TimeSpan timespan, Action callback)
        {
            this.timespan = timespan;
            this.callback = callback;
            cancellation = new CancellationTokenSource();
            IsActive = false;
        }

        public bool IsActive
        {
            get
            {
                lock (activeLock)
                {
                    return isActive;
                }
            }

            private set
            {
                lock (activeLock)
                {
                    isActive = value;
                }
            }
        }

        public void Start()
        {
            timer = new System.Timers.Timer(timespan.TotalMilliseconds);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = false;
            timer.Enabled = true;
            IsActive = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                CancellationTokenSource cts = cancellation;
                if (!cts.IsCancellationRequested)
                {
                    callback.Invoke();
                }

                if (sender is System.Timers.Timer timer && timer == this.timer)
                {
                    timer.Elapsed -= OnTimedEvent;
                    timer.Enabled = false;
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        public void Stop()
        {
            Interlocked.Exchange(ref cancellation, new CancellationTokenSource()).Cancel();
            IsActive = false;
        }
    }
}
