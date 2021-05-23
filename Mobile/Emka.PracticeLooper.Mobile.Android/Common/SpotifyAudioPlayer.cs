﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Threading;
using System.Threading.Tasks;
using Com.Spotify.Android.Appremote.Api;
using Com.Spotify.Protocol.Client;
using Com.Spotify.Protocol.Types;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Java.Lang;
using Java.Util.Concurrent;
using Xamarin.Forms.Internals;
using static Com.Spotify.Protocol.Client.CallResult;
using static Com.Spotify.Protocol.Client.Subscription;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    [Preserve(AllMembers = true)]
    public class SpotifyAudioPlayer : Java.Lang.Object, IAudioPlayer
    {
        #region Fields
        private bool useWebPlayer;
        private string deviceId;
        private readonly IPlayerTimer timer;
        private readonly ISpotifyLoader spotifyLoader;
        private readonly ILogger logger;
        private readonly ISpotifyApiService spotifyApiService;
        private IPlayerApi playerApi;
        const int CURRENT_TIME_UPDATE_INTERVAL = 500;
        private CancellationTokenSource currentTimeCancelTokenSource;
        private SpotifyDelegate spotifyDelegate;
        private PlayerStateDelegate stateDelegate;
        bool pausedByUser;
        private double loopStart;
        private double loopEnd;
        #endregion

        #region Ctor

        public SpotifyAudioPlayer(IPlayerTimer timer, ISpotifyLoader spotifyLoader, ILogger logger, ISpotifyApiService spotifyApiService)
        {
            this.spotifyLoader = spotifyLoader ?? throw new ArgumentNullException(nameof(spotifyLoader));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.spotifyApiService = spotifyApiService ?? throw new ArgumentNullException(nameof(spotifyApiService));
            this.timer = timer ?? throw new ArgumentNullException(nameof(timer));
            IsPlaying = false;
            spotifyDelegate = new SpotifyDelegate();
            stateDelegate = new PlayerStateDelegate();
        }
        #endregion

        #region Properties
        public bool Initialized { get; private set; }
        public bool IsPlaying { get; private set; }

        private bool pausePending;

        public double SongDuration => TimeSpan.FromSeconds(CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;
        public Loop CurrentLoop { get; set; }
        public SpotifyAppRemote Api { get => spotifyLoader.RemoteApi as SpotifyAppRemote; }
        public AudioSourceType Types => AudioSourceType.Spotify;
        public string DisplayName => "Spotify";
        public bool UsesWebPlayer => useWebPlayer;
        #endregion

        #region Events

        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler CurrentTimePositionChanged;
        public event EventHandler TimerElapsed;
        #endregion

        #region Methods

        public void OnEvent(object sender, Java.Lang.Object state)
        {
            if (state is PlayerState playerState)
            {
                IsPlaying = !playerState.IsPaused;
                PlayStatusChanged?.Invoke(this, !playerState.IsPaused);
            }
        }

        public async void GetCurrentPosition(Action<double> callback)
        {
            if (useWebPlayer)
            {
                var currentPosition = await spotifyApiService.GetCurrentPlaybackPosition();
                callback?.Invoke(currentPosition);
                return;
            }

            await Task.Run(() =>
            {
                CallResult playerStateCall = Api?.PlayerApi?.PlayerState;
                if (playerStateCall != null)
                {
                    IResult result = playerStateCall.Await(10, TimeUnit.Seconds);
                    if (result.IsSuccessful)
                    {
                        var state = result.Data as PlayerState;
                        callback?.Invoke(state == null ? 0 : state.PlaybackPosition);
                    }
                    else
                    {
                        logger?.LogError(new System.Exception(result.ErrorMessage));
                    }
                }
            });
        }

        public void Init(Loop loop, bool useWebPlayer = false, string deviceId = null)
        {
            if (loop == null)
            {
                throw new ArgumentNullException(nameof(loop));
            }

            if (loop.Session == null)
            {
                throw new ArgumentNullException(nameof(loop.Session));
            }

            this.useWebPlayer = useWebPlayer;
            this.deviceId = deviceId;
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
            playerApi = Api?.PlayerApi;
            Initialized = true;
        }

        public void Pause(bool triggeredByUser = true)
        {
            if (useWebPlayer)
            {
                Task.Run(async () =>
                {
                    await PauseViaWebApi();
                    Initialized = false;
                    IsPlaying = false;
                });
                return;
            }

            timer?.StopTimers();

            playerApi?
            .Pause()
            .SetErrorCallback(spotifyDelegate);

            Initialized = false;
            IsPlaying = false;
            RaisePlayingStatusChanged();

            if (triggeredByUser)
            {
                pausePending = true;
                playerApi?.SetRepeat(0).SetErrorCallback(spotifyDelegate);
            }
        }

        public void Play()
        {
            if (useWebPlayer)
            {
                Task.Run(async () => await PlayViaWebApi());
                return;
            }

            playerApi
                ?.SubscribeToPlayerState()
                ?.SetEventCallback(stateDelegate)
                ?.SetErrorCallback(stateDelegate);

            spotifyDelegate.SpotifyErrorHandler += OnError;
            spotifyDelegate.SpotifyEventHandler += OnEvent;
            spotifyDelegate.SpotifyResultHandler += OnResult;
            stateDelegate.StateChanged += OnStateChanged;

            playerApi
                ?.Play(CurrentLoop.Session.AudioSource.Source);

            ResetAllTimers();
            RaisePlayingStatusChanged();
            CurrentPositionTimerExpired(this, new EventArgs());
        }

        public void Seek(double time)
        {
            if (useWebPlayer)
            {
                Task.Run(async () => await SeekViaWebApi());
                return;
            }

            playerApi.SeekTo((long)loopStart).SetErrorCallback(spotifyDelegate);
        }

        public async Task InitAsync(Loop loop, bool useWebPlayer = false, string deviceId = null)
        {
            if (!spotifyLoader.Authorized)
            {
                await spotifyLoader?.InitializeAsync();
            }

            if (spotifyLoader.Authorized)
            {
                await Task.Run(() => Init(loop, useWebPlayer, deviceId));
            }
        }

        public async Task PauseAsync(bool triggeredByUser = true)
        {
            await Task.Run(() => Pause(triggeredByUser));
        }

        public async Task PlayAsync()
        {
            await Task.Run(Play);
        }

        public async Task SeekAsync(double time)
        {
            await Task.Run(() => Seek(time));
        }

        private async Task PlayViaWebApi()
        {
            var success = await spotifyApiService.PlayTrack(CurrentLoop.Session.AudioSource.Source, (int)loopStart, deviceId);
            if (success)
            {
                IsPlaying = true;
                ResetAllTimers();
                RaisePlayingStatusChanged();
                CurrentPositionTimerExpired(this, new EventArgs());
            }
        }

        private async Task PauseViaWebApi()
        {
            var success = await spotifyApiService.PauseCurrentPlayback();
            if (success)
            {
                timer.StopTimers();
                Initialized = false;
                IsPlaying = false;
                RaisePlayingStatusChanged();
            }
        }

        private async Task SeekViaWebApi()
        {
            var success = await spotifyApiService.SeekTo((long)loopStart);
            if (!success)
            {
                Initialized = false;
                IsPlaying = false;
                RaisePlayingStatusChanged();
                await PauseViaWebApi();
            }
        }

        private async void OnStateChanged(object sender, PlayerState s)
        {
            var state = await Task.Run(() =>
            {
                CallResult playerStateCall = playerApi.PlayerState;
                IResult result = playerStateCall.Await(5, TimeUnit.Seconds);
                return result.Data as PlayerState;
            });

            if (!IsPlaying && state != null && !state.IsPaused && state.Track?.Uri == CurrentLoop.Session.AudioSource.Source)
            {
                playerApi.SetRepeat(1).SetErrorCallback(spotifyDelegate);
                Seek(loopStart);
                IsPlaying = true;
                pausePending = false;
                RaisePlayingStatusChanged();
            }

            if (!IsPlaying && pausePending && state is PlayerState playerState && playerState.IsPaused)
            {
                pausePending = false;
                RemoveHandlers();
                if (spotifyLoader != null && spotifyLoader.Authorized)
                {
                    spotifyLoader.Disconnect();
                }

                RaisePlayingStatusChanged();
            }
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

        private async void OnLoopPositionChanged(object sender, double e)
        {
            try
            {
                loopStart = TimeSpan.FromSeconds(CurrentLoop.StartPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;
                loopEnd = CurrentLoop.EndPosition == 1
                ? TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds - 2
                : TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;

                if (IsPlaying)
                {
                    await SeekAsync(e);
                    ResetAllTimers();
                }
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        private void ResetAllTimers()
        {
            try
            {
                var timerDuration = loopEnd - loopStart;
                timer?.StopTimers();
                timer?.SetLoopTimer(timerDuration);
                timer?.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public void OnResult(object sender, Java.Lang.Object state)
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public async void OnError(object sender, Throwable error)
        {
            await logger?.LogErrorAsync(new System.Exception(error.Message));
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        private void RemoveHandlers()
        {
            spotifyDelegate.SpotifyErrorHandler -= OnError;
            spotifyDelegate.SpotifyEventHandler -= OnEvent;
            spotifyDelegate.SpotifyResultHandler -= OnResult;
            stateDelegate.StateChanged -= OnStateChanged;
        }
        #endregion
    }

    internal class SpotifyDelegate : Java.Lang.Object, IResultCallback, IErrorCallback, IEventCallback
    {
        #region Events

        public EventHandler<Java.Lang.Object> SpotifyEventHandler;
        public EventHandler<Java.Lang.Object> SpotifyResultHandler;
        public EventHandler<Throwable> SpotifyErrorHandler;
        #endregion

        #region Methods

        public void OnError(Throwable error)
        {
            SpotifyErrorHandler?.Invoke(this, error);
        }

        public void OnEvent(Java.Lang.Object state)
        {
            SpotifyEventHandler?.Invoke(this, state);
        }

        public void OnResult(Java.Lang.Object state)
        {
            SpotifyResultHandler?.Invoke(this, state);
        }
        #endregion
    }

    internal class PlayerStateDelegate : Java.Lang.Object, IEventCallback, IErrorCallback
    {
        #region Events

        public EventHandler<PlayerState> StateChanged;
        public EventHandler<Throwable> SpotifyErrorHandler;
        #endregion

        #region Methods

        public void OnError(Throwable error)
        {
            SpotifyErrorHandler?.Invoke(this, error);
        }

        public void OnEvent(Java.Lang.Object state)
        {
            StateChanged?.Invoke(this, state as PlayerState);
        }
        #endregion
    }
}
