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
using static Com.Spotify.Protocol.Client.CallResult;
using static Com.Spotify.Protocol.Client.Subscription;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class SpotifyAudioPlayer : Java.Lang.Object, IAudioPlayer, IPremiumAudioPlayer, IResultCallback, IEventCallback, IErrorCallback
    {
        #region Fields
        private readonly IPlayerTimer timer;
        private readonly ISpotifyLoader spotifyLoader;
        private readonly ILogger logger;
        private Session session;
        private double internalSongDuration;
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        private CancellationTokenSource currentTimeCancelTokenSource;
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

        public void OnEvent(Java.Lang.Object p0)
        {
            var playerState = p0 as PlayerState;
            IsPlaying = !playerState.IsPaused;
            PlayStatusChanged?.Invoke(this, !playerState.IsPaused);
        }

        public void GetCurrentPosition(Action<double> callback)
        {
            Task.Run(() =>
            {
                Com.Spotify.Protocol.Client.CallResult playerStateCall = Api.PlayerApi.PlayerState;
                Com.Spotify.Protocol.Client.IResult result = playerStateCall.Await(10, TimeUnit.Seconds);
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
            Initialized = true;
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                timer.StopTimers();
                Api.PlayerApi
                    .Pause()
                    .SetErrorCallback(this);
            }
        }

        public void Play()
        {
            CurrentStartPosition = (int)(CurrentLoop.StartPosition * SongDuration);
            CurrentEndPosition = (int)(CurrentLoop.EndPosition * SongDuration);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Api.PlayerApi
                .Play(session.AudioSource.Source)
                .SetResultCallback(this)
                .SetErrorCallback(this);

                Api.PlayerApi.SubscribeToPlayerState()
                .SetEventCallback(this)
                .SetErrorCallback(this);
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
                    .SetErrorCallback(this);
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

        public Task PauseAsync()
        {
            return Task.CompletedTask;
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

        public void OnResult(Java.Lang.Object p0)
        {
            try
            {
                Api.PlayerApi.SeekTo(CurrentStartPosition);
                var playerState = p0 as PlayerState;
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public async void OnError(Throwable error)
        {
            await logger?.LogErrorAsync(new System.Exception(error.Message));
        }
        #endregion
    }
}
