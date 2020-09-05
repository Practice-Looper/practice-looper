// Copyright (C)  - All Rights Reserved
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
        private readonly IPlayerTimer timer;
        private readonly ISpotifyLoader spotifyLoader;
        private readonly ILogger logger;
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        private CancellationTokenSource currentTimeCancelTokenSource;
        private SpotifyDelegate spotifyDelegate;
        bool pausedByUser;
        private double loopStart;
        private double loopEnd;
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
        public double SongDuration => TimeSpan.FromSeconds(CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;
        public Loop CurrentLoop { get; set; }
        public SpotifyAppRemote Api { get => spotifyLoader.RemoteApi as SpotifyAppRemote; }
        public AudioSourceType Types => AudioSourceType.Spotify;
        public string DisplayName => "Spotify";
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

        public void GetCurrentPosition(Action<double> callback)
        {
            Task.Run(() =>
            {
                CallResult playerStateCall = Api.PlayerApi.PlayerState;
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
            });
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
            CurrentLoop.StartPositionChanged += OnLoopPositionChanged;
            CurrentLoop.EndPositionChanged += OnLoopPositionChanged;
            loopStart = TimeSpan.FromSeconds(CurrentLoop.StartPosition * loop.Session.AudioSource.Duration).TotalMilliseconds;
            loopEnd = TimeSpan.FromSeconds(CurrentLoop.EndPosition * loop.Session.AudioSource.Duration).TotalMilliseconds;
            currentTimeCancelTokenSource = new CancellationTokenSource();
            timer.LoopTimerExpired += LoopTimerExpired;
            timer.CurrentPositionTimerExpired += CurrentPositionTimerExpired;
            spotifyDelegate = new SpotifyDelegate();
            Initialized = true;
        }

        public void Pause(bool triggeredByUser = true)
        {
            if (IsPlaying)
            {
                Api?
                .PlayerApi?
                .Pause()?
                .SetErrorCallback(spotifyDelegate);
            }

            IsPlaying = false;
            timer?.StopTimers();

            if (spotifyDelegate != null)
            {
                spotifyDelegate.SpotifyErrorHandler -= OnError;
                spotifyDelegate.SpotifyEventHandler -= OnEvent;
                spotifyDelegate.SpotifyResultHandler -= OnResult;
            }

            if (spotifyLoader != null && spotifyLoader.Authorized && triggeredByUser)
            {
                spotifyLoader.Disconnect();
            }

            Initialized = false;
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                spotifyDelegate.SpotifyErrorHandler += OnError;
                spotifyDelegate.SpotifyEventHandler += OnEvent;
                spotifyDelegate.SpotifyResultHandler += OnResult;
                Api?.PlayerApi
                    ?.SubscribeToPlayerState()
                    ?.SetEventCallback(spotifyDelegate)
                    ?.SetErrorCallback(spotifyDelegate);

                IsPlaying = true;
            }

            Api?.PlayerApi
                ?.Play(CurrentLoop.Session.AudioSource.Source)
                ?.SetResultCallback(spotifyDelegate)
                ?.SetErrorCallback(spotifyDelegate);

            ResetAllTimers();
            RaisePlayingStatusChanged();
            CurrentPositionTimerExpired(this, new EventArgs());
        }

        public void Seek(double time)
        {
            Api?.PlayerApi?.SeekTo((long)loopStart);
        }

        public async Task InitAsync(Loop loop)
        {
            if (!spotifyLoader.Authorized)
            {
                await spotifyLoader?.InitializeAsync();
            }

            await Task.Run(() => Init(loop));
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
                loopEnd = TimeSpan.FromSeconds(CurrentLoop.EndPosition * CurrentLoop.Session.AudioSource.Duration).TotalMilliseconds;

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
                timer?.StopTimers();
                timer?.SetLoopTimer(loopEnd - loopStart);
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
                Api?.PlayerApi?.SeekTo((long)loopStart);
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public async void OnError(object sender, Throwable error)
        {
            // todo: show dialog
            await logger?.LogErrorAsync(new System.Exception(error.Message));
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        ~SpotifyAudioPlayer()
        {
            CurrentLoop.StartPositionChanged -= OnLoopPositionChanged;
            CurrentLoop.EndPositionChanged -= OnLoopPositionChanged;
            timer.LoopTimerExpired -= LoopTimerExpired;
            timer.CurrentPositionTimerExpired -= CurrentPositionTimerExpired;
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
}
