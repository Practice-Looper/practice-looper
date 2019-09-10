// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using System.Threading.Tasks;
using AVFoundation;
using CoreMedia;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Fields
        AVPlayer audioPlayer;
        Session session;
        CancellationTokenSource loopTimerCancelTokenSource;
        CancellationTokenSource currentTimeCancelTokenSource;
        const int CURRENT_TIME_UPDATE_INTERVAL = 500;
        double internalSongDuration;
        #endregion

        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler<int> CurrentTimePositionChanged;
        public event EventHandler TimerElapsed;
        #endregion

        #region Properties
        public bool IsPlaying => audioPlayer != null && audioPlayer.Rate != 0;
        public double SongDuration { get { return internalSongDuration * 1000; } }
        public Loop CurrentLoop { get; set; }
        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }
        #endregion Properties

        #region Methods
        // https://stackoverflow.com/questions/12902410/trying-to-understand-cmtime
        public void Init(Session session)
        {
            this.session = session;

            var asset = AVAsset.FromUrl(NSUrl.FromFilename(session.AudioSource.Source));

            var playerItem = new AVPlayerItem(asset);
            var status = playerItem.Status;
            audioPlayer = new AVPlayer(playerItem);
            var playerLayer = AVPlayerLayer.FromPlayer(audioPlayer);

            internalSongDuration = audioPlayer.CurrentItem.Asset.Duration.Seconds;
            CurrentLoop = session.Loops[0];
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            TimerElapsed += FileAudioPlayer_TimerElapsed;
            CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
            CurrentEndPosition = ConvertToInt(CurrentLoop.EndPosition);

            loopTimerCancelTokenSource = new CancellationTokenSource();
            currentTimeCancelTokenSource = new CancellationTokenSource();
            audioPlayer.Seek(CMTime.FromSeconds(CurrentStartPosition * internalSongDuration, 1));
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                audioPlayer.Play();
                SetCurrentTimeTimer();
                CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
                RaisePlayingStatusChanged();
            }
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                StopTimers();
                audioPlayer.Pause();
                CurrentStartPosition = (int)audioPlayer.CurrentTime.Seconds;
                RaisePlayingStatusChanged();
            }
        }

        public void Seek(double time)
        {
            if (audioPlayer != null)
            {
                Console.WriteLine("seek to " + CMTime.FromSeconds(time * internalSongDuration, 1));
                audioPlayer.Seek(CMTime.FromSeconds(time * internalSongDuration, 1));
            }
        }

        private int ConvertToInt(double inValue)
        {
            int result;
            try
            {
                result = Convert.ToInt32(inValue * internalSongDuration);
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
            Console.WriteLine("CurrentStartPosition " + CurrentEndPosition);

            Seek(e);
            if (IsPlaying)
            {
                ResetAllTimers();
            }
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            CurrentEndPosition = ConvertToInt(e);
            Console.WriteLine("CurrentEndPosition " + CurrentEndPosition);
            if (IsPlaying)
            {
                ResetAllTimers();
            }
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        private void RaiseTimePositionChanged()
        {
            Console.WriteLine("CurrentTimePosition " + Convert.ToInt32(audioPlayer.CurrentTime.Seconds * 1000));
            CurrentTimePositionChanged?.Invoke(this, Convert.ToInt32(audioPlayer.CurrentTime.Seconds * 1000));
        }

        private void ResetAllTimers()
        {
            StopTimers();
            SetLoopTimer();
            SetCurrentTimeTimer();
        }

        private void SetLoopTimer()
        {
            if (loopTimerCancelTokenSource != null && !loopTimerCancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    // looping task
                    Task.Run(async () =>
                    {
                        {
                            var delta = CurrentEndPosition - CurrentStartPosition;
                            Console.WriteLine("wait for " + delta);
                            await Task.Delay(delta * 1000, loopTimerCancelTokenSource.Token);
                            Seek(CurrentLoop.StartPosition);
                            Play();
                            SetLoopTimer();
                        }
                    }, loopTimerCancelTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
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
                    if (!currentTimeCancelTokenSource.IsCancellationRequested)
                    {
                        RaiseTimePositionChanged();
                        await Task.Delay(CURRENT_TIME_UPDATE_INTERVAL, currentTimeCancelTokenSource.Token);
                        SetCurrentTimeTimer();
                    }

                }, currentTimeCancelTokenSource.Token);
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void FileAudioPlayer_TimerElapsed(object sender, EventArgs e)
        {
            TimerElapsed?.Invoke(this, e);
        }
        #endregion
    }
}
