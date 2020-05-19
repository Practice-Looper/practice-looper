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
        private Session session;
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
            this.spotifyLoader = spotifyLoader;
            this.logger = logger;
            IsPlaying = false;
            this.timer = timer;
        }
        #endregion

        #region Properties
        public bool Initialized { get; private set; }
        public bool IsPlaying { get; private set; }
        public double SongDuration => internalSongDuration;
        public Loop CurrentLoop { get; set; }
        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }
        public SPTAppRemote Api { get => spotifyLoader.RemoteApi as SPTAppRemote; }
        public AudioSourceType Type => AudioSourceType.Spotify;

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
            session = loop.Session;

            internalSongDuration = TimeSpan.FromSeconds(session.AudioSource.Duration).TotalMilliseconds;
            CurrentLoop = loop;
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
            CurrentEndPosition = ConvertToInt(CurrentLoop.EndPosition);
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
                Api?.PlayerAPI?.Pause((o, e) =>
                {
                    if (e != null)
                    {
                        logger.LogError(new Exception(e.DebugDescription));
                    }
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
            if (!IsPlaying)
            {
                CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Api?.PlayerAPI?.Play(session.AudioSource.Source, (o, e) =>
                    {
                        if (e != null)
                        {
                            logger?.LogError(new Exception(e.DebugDescription));
                        }

                        Seek(CurrentLoop.StartPosition);
                    });
                });

                timer.SetLoopTimer(CurrentEndPosition - CurrentStartPosition);
                timer.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);

                CurrentPositionTimerExpired(this, new EventArgs());
                IsPlaying = true;
                RaisePlayingStatusChanged();
            }
        }

        public void Seek(double time)
        {
            if (Api != null)
            {
                var seekTo = Convert.ToInt32(time * internalSongDuration);
                Api?.PlayerAPI.SeekToPosition(seekTo, (o, e) =>
                {
                    if (e != null)
                    {
                        logger.LogError(new Exception(e.DebugDescription));
                    }
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

        private void LoopTimerExpired(object sender, EventArgs e)
        {
            TimerElapsed?.Invoke(this, e);
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        private int ConvertToInt(double inValue)
        {
            int result;
            try
            {
                result = Convert.ToInt32(inValue * internalSongDuration);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }

            return result;
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            try
            {
                CurrentStartPosition = ConvertToInt(e);

                Seek(e);
                if (IsPlaying)
                {
                    ResetAllTimers();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            try
            {
                CurrentEndPosition = ConvertToInt(e);

                if (IsPlaying)
                {
                    ResetAllTimers();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
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
                CurrentLoop.StartPositionChanged -= OnStartPositionChanged;
                CurrentLoop.EndPositionChanged -= OnEndPositionChanged;
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
