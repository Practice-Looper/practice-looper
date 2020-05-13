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
using Xamarin.Essentials;
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
        private Session session;
        private double internalSongDuration;
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        private CancellationTokenSource currentTimeCancelTokenSource;
        private SpotifyDelegate spotifyDelegate;
        bool pausedByUser;
        #endregion

        #region Ctor

        public SpotifyAudioPlayer(IPlayerTimer timer, ISpotifyLoader spotifyLoader, ILogger logger)
        {
            this.timer = timer;
            this.spotifyLoader = spotifyLoader;
            this.logger = logger;
            IsPlaying = false;
        }
        #endregion

        #region Properties
        public bool Initialized { get; private set; }
        public bool IsPlaying { get; private set; }
        public double SongDuration => internalSongDuration;
        public Loop CurrentLoop { get; set; }
        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }
        public SpotifyAppRemote Api { get => spotifyLoader.RemoteApi as SpotifyAppRemote; }
        public AudioSourceType Type => AudioSourceType.Spotify;
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
            session = loop.Session;
            internalSongDuration = TimeSpan.FromSeconds(session.AudioSource.Duration).TotalMilliseconds;
            CurrentLoop = session.Loops[0];
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
            CurrentEndPosition = ConvertToInt(CurrentLoop.EndPosition);
            currentTimeCancelTokenSource = new CancellationTokenSource();

            timer.LoopTimerExpired += LoopTimerExpired;
            timer.CurrentPositionTimerExpired += CurrentPositionTimerExpired;
            spotifyDelegate = new SpotifyDelegate();
            Initialized = true;
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Api.PlayerApi
                    .Pause()
                    .SetErrorCallback(spotifyDelegate);
                });
            }

            IsPlaying = false;
            timer.StopTimers();
            spotifyDelegate.SpotifyErrorHandler -= OnError;
            spotifyDelegate.SpotifyEventHandler -= OnEvent;
            spotifyDelegate.SpotifyResultHandler -= OnResult;
            spotifyLoader.Disconnect();
            Initialized = false;
        }

        public void Play()
        {
            CurrentStartPosition = (int)(CurrentLoop.StartPosition * SongDuration);
            CurrentEndPosition = (int)(CurrentLoop.EndPosition * SongDuration);

            spotifyDelegate.SpotifyErrorHandler += OnError;
            spotifyDelegate.SpotifyEventHandler += OnEvent;
            spotifyDelegate.SpotifyResultHandler += OnResult;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Api.PlayerApi
            .Play(session.AudioSource.Source)
            .SetResultCallback(spotifyDelegate)
            .SetErrorCallback(spotifyDelegate);

                Api.PlayerApi.SubscribeToPlayerState()
                .SetEventCallback(spotifyDelegate)
                .SetErrorCallback(spotifyDelegate);
            });

            ResetAllTimers();

            IsPlaying = true;
        }

        public void Seek(double time)
        {
            if (Api != null)
            {
                var seekTo = Convert.ToInt64(time * internalSongDuration);
                Api.PlayerApi
                    .SeekTo(seekTo)
                    .SetErrorCallback(spotifyDelegate);
            }
        }

        public async Task InitAsync(Loop loop)
        {
            if (!spotifyLoader.Authorized)
            {
                await spotifyLoader.InitializeAsync();
            }

            await Task.Run(() => Init(loop));
        }

        public async Task PauseAsync()
        {
            await Task.Run(Pause);
        }

        public async Task PlayAsync()
        {
            await Task.Run(Play);
        }

        public Task SeekAsync(double time)
        {
            return Task.CompletedTask;
        }

        private void CurrentPositionTimerExpired(object sender, EventArgs e)
        {
            CurrentTimePositionChanged.Invoke(this, e);
        }

        private void LoopTimerExpired(object sender, EventArgs e)
        {
            TimerElapsed?.Invoke(this, e);
        }

        private int ConvertToInt(double inValue)
        {
            int result;
            try
            {
                result = Convert.ToInt32(inValue * internalSongDuration);
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }

            return result;
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            try
            {
                if (IsPlaying)
                {
                    Play();
                }
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            try
            {
                if (IsPlaying)
                {
                    Play();
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
                timer.StopTimers();
                var delta = CurrentEndPosition - CurrentStartPosition;
                timer.SetLoopTimer(delta);
                timer.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
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
                Api.PlayerApi.SeekTo(CurrentStartPosition);
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

        ~SpotifyAudioPlayer()
        {
            CurrentLoop.StartPositionChanged -= OnStartPositionChanged;
            CurrentLoop.EndPositionChanged -= OnEndPositionChanged;
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
