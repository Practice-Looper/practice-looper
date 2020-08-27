// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Plugin.SimpleAudioPlayer;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Fields
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        double internalSongDuration;
        private object locker = new object();
        private bool pausedByUser;
        private readonly IPlayerTimer timer;
        private readonly ILogger logger;
        private readonly IConfigurationService configService;
        private readonly ISimpleAudioPlayer audioPlayer;
        #endregion

        #region Ctor
        public FileAudioPlayer(IPlayerTimer timer, ILogger logger, IConfigurationService configService)
        {
            this.timer = timer ?? throw new ArgumentNullException(nameof(timer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configService = configService ?? throw new ArgumentNullException(nameof(configService));
            audioPlayer = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        }
        #endregion


        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler CurrentTimePositionChanged;
        public event EventHandler TimerElapsed;
        #endregion

        #region Propeties
        public bool Initialized { get; private set; }
        public bool IsPlaying => audioPlayer != null && audioPlayer.IsPlaying;
        public double SongDuration { get { return internalSongDuration * 1000; } }
        private Loop CurrentLoop { get; set; }
        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }

        public AudioSourceType Type => AudioSourceType.Local;

        public string DisplayName => "File";

        public bool PausedByUser
        {
            get
            {
                lock (locker)
                {
                    return pausedByUser;
                }
            }
            private set
            {
                lock (locker)
                {
                    pausedByUser = value;
                }
            }
        }
        #endregion

        #region Methods

        public void GetCurrentPosition(Action<double> callback)
        {
            callback?.Invoke(TimeSpan.FromSeconds(Math.Round(audioPlayer.CurrentPosition, 0)).TotalMilliseconds);
        }

        public void Init(Loop loop)
        {
            if (loop == null)
            {
                throw new ArgumentNullException(nameof(loop));
            }

            if (loop.Session == null)
            {
                throw new ArgumentNullException(nameof(loop.Session));
            }

            CurrentLoop = loop;
            RemoveEventHandlers();

            using (var fsSource = new FileStream(Path.Combine(configService.LocalPath, CurrentLoop.Session.AudioSource.Source), FileMode.Open, FileAccess.Read))
            {
                audioPlayer.Load(fsSource);
            }

            timer.LoopTimerExpired += LoopTimerExpired;
            timer.CurrentPositionTimerExpired += CurrentPositionTimerExpired;
            CurrentLoop.StartPositionChanged += OnLoopPositionChanged;
            CurrentLoop.EndPositionChanged += OnLoopPositionChanged;
            Initialized = true;
            PausedByUser = false;
        }

        public async Task InitAsync(Loop loop)
        {
            await Task.Run(() => Init(loop));
        }

        public void Pause(bool triggeredByUser = true)
        {
            timer.StopTimers();
            PausedByUser = triggeredByUser;
            audioPlayer.Stop();
            RaisePlayingStatusChanged();
            Initialized = false;
        }

        public async Task PauseAsync(bool triggeredByUser = true)
        {
            await Task.Run(() => Pause(triggeredByUser));
        }

        public void Play()
        {
            if (!Initialized)
            {
                return;
            }

            CurrentStartPosition = (int)(CurrentLoop.StartPosition * CurrentLoop.Session.AudioSource.Duration);
            CurrentEndPosition = (int)(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration);
            if (!audioPlayer.IsPlaying)
            {
                audioPlayer?.Play();
            }

            Seek(CurrentLoop.StartPosition);
            PausedByUser = false;
            ResetAllTimers();
            RaisePlayingStatusChanged();
            CurrentPositionTimerExpired(this, new EventArgs());
        }

        public async Task PlayAsync()
        {
            await Task.Run(Play);
        }

        public void Seek(double time)
        {
            var position = TimeSpan.FromSeconds(time * CurrentLoop.Session.AudioSource.Duration);
            audioPlayer.Seek(position.TotalSeconds);
        }

        public async Task SeekAsync(double time)
        {
            await Task.Run(() => Seek(time));
        }

        private async void OnLoopPositionChanged(object sender, double e)
        {
            var delta = CurrentEndPosition - CurrentStartPosition;

            // avoid negative values and values larger than int.MaxValue
            if (delta < 0 || delta >= int.MaxValue)
            {
                logger.LogError(new ArgumentException($"negative time value {delta}. CurrentStartPosition: {CurrentStartPosition}. CurrentEndPosition: {CurrentEndPosition}. Song: {CurrentLoop.Session.AudioSource.FileName}. Song duration {CurrentLoop.Session.AudioSource.Duration}"));

                if (IsPlaying)
                {
                    await PauseAsync();
                    return;
                }
            }

            if (IsPlaying)
            {
                await PlayAsync();
            }
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        private void RemoveEventHandlers()
        {
            timer.LoopTimerExpired -= LoopTimerExpired;
            timer.CurrentPositionTimerExpired -= CurrentPositionTimerExpired;
            CurrentLoop.StartPositionChanged -= OnLoopPositionChanged;
            CurrentLoop.EndPositionChanged -= OnLoopPositionChanged;
        }

        private void LoopTimerExpired(object sender, EventArgs e)
        {
            TimerElapsed?.Invoke(this, e);
        }

        private void CurrentPositionTimerExpired(object sender, EventArgs e)
        {
            CurrentTimePositionChanged?.Invoke(this, e);
        }

        private void ResetAllTimers()
        {
            try
            {
                timer?.StopTimers();
                var delta = CurrentEndPosition - CurrentStartPosition;

                // avoid negative values and values larger than int.MaxValue
                if (delta < 0 || delta >= int.MaxValue)
                {
                    logger.LogError(new ArgumentException($"negative time value {delta}. CurrentStartPosition: {CurrentStartPosition}. CurrentEndPosition: {CurrentEndPosition}. Song: {CurrentLoop.Session.AudioSource.FileName}. Song duration {CurrentLoop.Session.AudioSource.Duration}"));
                    Pause();
                    return;
                }

                timer.SetLoopTimer((int)TimeSpan.FromSeconds(delta).TotalMilliseconds);
                timer.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        ~FileAudioPlayer()
        {
            if (CurrentLoop != null)
            {
                CurrentLoop.StartPositionChanged -= OnLoopPositionChanged;
                CurrentLoop.EndPositionChanged -= OnLoopPositionChanged;
            }

            if (timer != null)
            {
                timer.LoopTimerExpired -= LoopTimerExpired;
            }
        }
        #endregion
    }
}
