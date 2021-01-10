// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Collections.Generic;
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
        private readonly IAudioFileLoader audioFileLoader;
        private readonly ISimpleAudioPlayer audioPlayer;
        private double loopStart;
        private double loopEnd;
        #endregion

        #region Ctor
        public FileAudioPlayer(IPlayerTimer timer, ILogger logger, IConfigurationService configService, IAudioFileLoader audioFileLoader)
        {
            this.timer = timer ?? throw new ArgumentNullException(nameof(timer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configService = configService ?? throw new ArgumentNullException(nameof(configService));
            this.audioFileLoader = audioFileLoader ?? throw new ArgumentNullException(nameof(audioFileLoader));
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
        public AudioSourceType Types => AudioSourceType.LocalInternal | AudioSourceType.LocalExternal;
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

        public void Init(Loop loop, bool useWebPlayer = false)
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
            loopStart = TimeSpan.FromSeconds(CurrentLoop.StartPosition * loop.Session.AudioSource.Duration).TotalMilliseconds;
            loopEnd = TimeSpan.FromSeconds(CurrentLoop.EndPosition * loop.Session.AudioSource.Duration).TotalMilliseconds;
            RemoveEventHandlers();
            var file = audioFileLoader.GetFileStream(loop.Session.AudioSource);
            audioPlayer.Load(file);
            timer.LoopTimerExpired += LoopTimerExpired;
            timer.CurrentPositionTimerExpired += CurrentPositionTimerExpired;
            CurrentLoop.StartPositionChanged += OnLoopPositionChanged;
            CurrentLoop.EndPositionChanged += OnLoopPositionChanged;
            Initialized = true;
            PausedByUser = false;
        }

        public async Task InitAsync(Loop loop, bool useWebPlayer = false)
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
            audioPlayer.Seek(TimeSpan.FromMilliseconds(loopStart).TotalSeconds);
        }

        public async Task SeekAsync(double time)
        {
            await Task.Run(() => Seek(time));
        }

        private async void OnLoopPositionChanged(object sender, double e)
        {
            loopStart = TimeSpan.FromSeconds(CurrentLoop.StartPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;
            loopEnd = TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;

            if (IsPlaying)
            {
                await SeekAsync(e);
                ResetAllTimers();
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

        private async void LoopTimerExpired(object sender, EventArgs e)
        {
            TimerElapsed?.Invoke(this, e);

            if (IsPlaying)
            {
                await SeekAsync(CurrentLoop.StartPosition);
                ResetAllTimers();
            }
            else
            {
                await PlayAsync();
            }
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
                timer?.SetLoopTimer((int)(loopEnd - loopStart));
                timer?.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
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
