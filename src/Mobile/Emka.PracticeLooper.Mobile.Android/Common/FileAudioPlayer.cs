// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Fields
        MediaPlayer player;
        Session session;
        CancellationTokenSource loopTimerCancelTokenSource;
        CancellationTokenSource currentTimeCancelTokenSource;
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        #endregion

        #region Ctor
        public FileAudioPlayer()
        {
            player = new MediaPlayer();
        }
        #endregion

        #region Properties
        public bool IsPlaying => player.IsPlaying;

        public double SongDuration { get; set; }

        public Loop CurrentLoop { get; set; }

        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler<int> CurrentTimePositionChanged;

        private int CurrentStartPosition { get; set; }

        private int CurrentEndPosition { get; set; }
        #endregion

        #region Methods
        public void Init(Session session)
        {
            this.session = session;

            player.Reset();
            player.SetDataSource(session.AudioSource.Source);
            player.Prepare();

            SongDuration = player.Duration;
            CurrentLoop = session.Loops[0];
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
            CurrentEndPosition = ConvertToInt(CurrentLoop.EndPosition);

            player.SeekTo(CurrentStartPosition);

            loopTimerCancelTokenSource = new CancellationTokenSource();
            currentTimeCancelTokenSource = new CancellationTokenSource();
        }

        private int ConvertToInt(double inValue)
        {
            int result;
            try
            {
                result = Convert.ToInt32(inValue * SongDuration);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            CurrentStartPosition = ConvertToInt(e);

            if (IsPlaying)
            {
                Seek(e);
                SetLoopTimer();
            }
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            CurrentEndPosition = ConvertToInt(e);
            Console.WriteLine("Endposition " + CurrentEndPosition);
            if (IsPlaying)
            {
                SetLoopTimer();
            }
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                StopTimers();
                player.Pause();
                CurrentStartPosition = player.CurrentPosition;
                RaisePlayingStatusChanged();
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                player.Start();
                SetLoopTimer();
                SetCurrentTimeTimer();
                CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
                RaisePlayingStatusChanged();
            }
        }

        public void Seek(double time)
        {
            try
            {
                var offset = Convert.ToInt32(time * SongDuration);
                player.SeekTo(offset);
                Console.WriteLine("seeking to" + offset);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        private void RaisTimePositionChanged()
        {
            CurrentTimePositionChanged?.Invoke(this, player.CurrentPosition);
        }

        private void SetLoopTimer()
        {
            Console.WriteLine("Refreshing token");
            if (loopTimerCancelTokenSource != null && !loopTimerCancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    // looping task
                    Task.Run(async () =>
                    {
                        {
                            var delta = CurrentEndPosition - CurrentStartPosition;
                            Console.WriteLine("waiting for " + delta);
                            await Task.Delay(delta, loopTimerCancelTokenSource.Token);
                            Console.WriteLine("Timer elapsed ");

                            Seek(CurrentLoop.StartPosition);
                            SetLoopTimer();
                            Play();
                            Console.WriteLine("seeking to " + CurrentStartPosition);
                        }
                    }, loopTimerCancelTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task cancelled");
                }
            }
        }

        private void StopTimers()
        {
            if (loopTimerCancelTokenSource != null && !loopTimerCancelTokenSource.IsCancellationRequested)
            {
                loopTimerCancelTokenSource.Cancel();
                loopTimerCancelTokenSource = new CancellationTokenSource();
            }

            if (currentTimeCancelTokenSource != null && !currentTimeCancelTokenSource.IsCancellationRequested)
            {
                currentTimeCancelTokenSource.Cancel();
                currentTimeCancelTokenSource = new CancellationTokenSource();
            }
        }

        private void SetCurrentTimeTimer()
        {
            try
            {
                Task.Run(async () =>
                {
                    await Task.Delay(CURRENT_TIME_UPDATE_INTERVAL, currentTimeCancelTokenSource.Token);
                    if (!currentTimeCancelTokenSource.IsCancellationRequested)
                    {
                        RaisTimePositionChanged();
                        SetCurrentTimeTimer();
                    }
                }, currentTimeCancelTokenSource.Token);
            }
            catch (TaskCanceledException)
            {

            }
        }
        #endregion
    }
}
