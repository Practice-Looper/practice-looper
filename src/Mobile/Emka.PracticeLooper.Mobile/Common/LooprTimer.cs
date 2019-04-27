// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class LooprTimer
    {
        private readonly TimeSpan timespan;
        private readonly Action callback;
        private object activeLock = new object();
        private CancellationTokenSource cancellation;
        private bool isActive;

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
            CancellationTokenSource cts = cancellation; // safe copy
            Device.StartTimer(timespan,
                () =>
                {
                    if (cts.IsCancellationRequested) return false;
                    callback.Invoke();
                    return false; // or true for periodic behavior
                });

            IsActive = true;
        }

        public void Stop()
        {
            Interlocked.Exchange(ref cancellation, new CancellationTokenSource()).Cancel();
            IsActive = false;
        }
    }
}
