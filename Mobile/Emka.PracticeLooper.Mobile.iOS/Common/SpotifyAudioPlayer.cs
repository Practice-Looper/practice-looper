// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using SpotifyBindings.iOS;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class SpotifyAudioPlayer : IAudioPlayer
    {
        #region Fields
        private double loopStart;
        private double loopEnd;
        private readonly IPlayerTimer timer;
        private readonly ISpotifyLoader spotifyLoader;
        private readonly ILogger logger;
        private CancellationTokenSource currentTimeCancelTokenSource;
        private double internalSongDuration;
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        #endregion

        #region Ctor
        public SpotifyAudioPlayer(IPlayerTimer timer, ISpotifyLoader spotifyLoader, ILogger logger)
        {
            this.spotifyLoader = spotifyLoader ?? throw new ArgumentNullException(nameof(spotifyLoader));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.timer = timer ?? throw new ArgumentNullException(nameof(timer));
            IsPlaying = false;
        }
        #endregion

        #region Properties
        public bool Initialized { get; private set; }
        public bool IsPlaying { get; private set; }
        public double SongDuration => internalSongDuration;
        public Loop CurrentLoop { get; set; }
        public SPTAppRemote Api { get => spotifyLoader.RemoteApi as SPTAppRemote; }
        public AudioSourceType Types => AudioSourceType.Spotify;

        public string DisplayName => "Spotify";
        #endregion

        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler CurrentTimePositionChanged;
        public event EventHandler TimerElapsed;
        #endregion

        #region Methods
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

            internalSongDuration = TimeSpan.FromSeconds(loop.Session.AudioSource.Duration).TotalMilliseconds;
            CurrentLoop = loop;
            CurrentLoop.StartPositionChanged += OnLoopPositionChanged;
            CurrentLoop.EndPositionChanged += OnLoopPositionChanged;
            loopStart = TimeSpan.FromSeconds(CurrentLoop.StartPosition * loop.Session.AudioSource.Duration).TotalMilliseconds;
            loopEnd = CurrentLoop.EndPosition == 1
                ? TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds - 100
                : TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;

            currentTimeCancelTokenSource = new CancellationTokenSource();
            timer.LoopTimerExpired += LoopTimerExpired;
            timer.CurrentPositionTimerExpired += CurrentPositionTimerExpired;
            Initialized = true;
        }

        public void Pause(bool triggeredByUser = true)
        {
            if (IsPlaying)
            {
                timer.StopTimers();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Api?.PlayerAPI?.SetRepeatMode(SPTAppRemotePlaybackOptionsRepeatMode.Off, (repeatResult, repeatError) =>
                    {
                        if (repeatError != null)
                        {
                            logger.LogError(new Exception(repeatError.Description));
                        }
                    });

                    Api?.PlayerAPI?.Pause((o, e) =>
                    {
                        if (e != null)
                        {
                            logger.LogError(new Exception(e.DebugDescription));
                        }
                    });
                });
            }

            IsPlaying = false;
            RaisePlayingStatusChanged();
            Initialized = false;

            if (spotifyLoader.Authorized && triggeredByUser)
            {
                spotifyLoader.Disconnect();
            }
        }

        public void Play()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Api?.PlayerAPI?.Play(CurrentLoop.Session.AudioSource.Source, (result, error) =>
                {
                    if (error != null)
                    {
                        throw new Exception(error.Description);
                    }

                    Seek(loopStart);
                    IsPlaying = true;
                    CurrentPositionTimerExpired(this, new EventArgs());
                    RaisePlayingStatusChanged();

                    Api?.PlayerAPI?.SetRepeatMode(SPTAppRemotePlaybackOptionsRepeatMode.Track, (repeatResult, repeatError) =>
                    {
                        if (repeatError != null)
                        {
                            throw new Exception(repeatError.Description);
                        }
                    });
                });
            });

            ResetAllTimers();
            RaisePlayingStatusChanged();
            CurrentPositionTimerExpired(this, new EventArgs());
        }

        public void Seek(double time)
        {
            if (Api != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Api?.PlayerAPI.SeekToPosition((int)loopStart, (o, e) =>
                    {
                        if (e != null)
                        {
                            logger.LogError(new Exception(e.DebugDescription));
                        }
                    });
                });
            }
        }

        public void GetCurrentPosition(Action<double> callback)
        {
            Api?.PlayerAPI?.GetPlayerState((o, e) =>
                 {
                     if (callback != null)
                     {
                         callback.Invoke(o.PlaybackPosition);
                     }

                     if (e != null)
                     {
                         logger.LogError(new Exception(e.DebugDescription));
                     }
                 });
        }

        public async Task InitAsync(Loop loop)
        {
            if (!spotifyLoader.Authorized)
            {
                await spotifyLoader.InitializeAsync();
            }

            await Task.Run(() => Init(loop));
        }

        public async Task PlayAsync()
        {
            await Task.Run(Play);
        }

        public async Task PauseAsync(bool triggeredByUser = true)
        {
            await Task.Run(() => Pause(triggeredByUser));
        }

        public async Task SeekAsync(double time)
        {
            await Task.Run(() => Seek(time));
        }

        private void CurrentPositionTimerExpired(object sender, EventArgs e)
        {
            CurrentTimePositionChanged?.Invoke(this, e);
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

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        private async void OnLoopPositionChanged(object sender, double e)
        {
            loopStart = TimeSpan.FromSeconds(CurrentLoop.StartPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;
            loopEnd = CurrentLoop.EndPosition == 1
                ? TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds - 100
                : TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;

            if (IsPlaying)
            {
                await SeekAsync(e);
                ResetAllTimers();
            }
        }

        private void ResetAllTimers()
        {
            try
            {
                timer?.StopTimers();
                timer?.SetLoopTimer(loopEnd - loopStart);
                timer?.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        ~SpotifyAudioPlayer()
        {
            if (CurrentLoop != null)
            {
                CurrentLoop.StartPositionChanged -= OnLoopPositionChanged;
                CurrentLoop.EndPositionChanged -= OnLoopPositionChanged;
            }

            if (timer != null)
            {
                timer.LoopTimerExpired -= LoopTimerExpired;
                timer.CurrentPositionTimerExpired -= CurrentPositionTimerExpired;
            }
        }
        #endregion
    }
}
