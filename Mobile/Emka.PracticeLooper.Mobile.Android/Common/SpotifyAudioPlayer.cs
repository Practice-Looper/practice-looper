// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading;
using System.Threading.Tasks;
using Com.Spotify.Android.Appremote.Api;
using Com.Spotify.Protocol.Types;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Java.Interop;
using static Com.Spotify.Protocol.Client.Subscription;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class SpotifyAudioPlayer : IAudioPlayer
    {
        class EventCallback : Java.Lang.Object, IEventCallback
        {
            public void OnEvent(Java.Lang.Object p0)
            {
                var state = p0 as PlayerState;
                Console.WriteLine(state.PlaybackPosition);
            }
        }

        #region Fields
        private SpotifyAppRemote api;
        private readonly IPlayerTimer timer;
        private readonly ISpotifyLoader spotifyLoader;
        private Session session;
        private double internalSongDuration;
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        private CancellationTokenSource currentTimeCancelTokenSource;
        #endregion

        #region Ctor

        public SpotifyAudioPlayer(IPlayerTimer timer, ISpotifyLoader spotifyLoader)
        {
            this.timer = timer;
            this.spotifyLoader = spotifyLoader;
            IsPlaying = false;
        }
        #endregion

        #region Properties
        public bool IsPlaying { get; private set; }
        public double SongDuration => internalSongDuration * 1000;
        public Loop CurrentLoop { get; set; }
        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }
        #endregion

        #region Events

        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler CurrentTimePositionChanged;
        public event EventHandler TimerElapsed;
        #endregion

        #region Methods

        public void GetCurrentPosition(Action<double> callback)
        {
            try
            {
                var subscription = api.PlayerApi.SubscribeToPlayerState().SetEventCallback(new EventCallback());
                //GetPlayerState((o, e) =>
                //{
                //    if (callback != null)
                //    {
                //        callback.Invoke(o.PlaybackPosition);
                //    }
                //});
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Init(Loop loop)
        {
            api = spotifyLoader.RemoteApi as SpotifyAppRemote;

            session = loop.Session;
            CurrentLoop = session.Loops[0];
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
            CurrentEndPosition = ConvertToInt(CurrentLoop.EndPosition);
            internalSongDuration = TimeSpan.FromMilliseconds(session.AudioSource.Duration).TotalSeconds;
            currentTimeCancelTokenSource = new CancellationTokenSource();

            timer.LoopTimerExpired += LoopTimerExpired;
            timer.CurrentPositionTimerExpired += CurrentPositionTimerExpired;

        }

        public void Pause()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                timer.StopTimers();
                api.PlayerApi.Pause();
                RaisePlayingStatusChanged();
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                IsPlaying = true;
                CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
                api.PlayerApi.Play(session.AudioSource.Source);
                timer.SetLoopTimer(CurrentEndPosition - CurrentStartPosition);
                timer.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
                RaisePlayingStatusChanged();
                CurrentPositionTimerExpired(this, new EventArgs());
            }
        }

        public void Seek(double time)
        {
            if (api != null)
            {
                try
                {
                    var seekTo = (time * session.AudioSource.Duration);
                    api.PlayerApi.SeekTo(0);
                }
                catch (System.Exception)
                {
                    // todo: log
                    throw;
                }
            }
        }

        public Task InitAsync(Loop loop)
        {
            throw new NotImplementedException();
        }

        public Task PauseAsync()
        {
            throw new NotImplementedException();
        }

        public Task PlayAsync()
        {
            throw new NotImplementedException();
        }

        public Task SeekAsync(double time)
        {
            throw new NotImplementedException();
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
                result = Convert.ToInt32(inValue * session.AudioSource.Duration);
            }
            catch (System.Exception)
            {
                throw;
            }

            return result;
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            CurrentStartPosition = ConvertToInt(e);
            Console.WriteLine("CurrentStartPosition " + CurrentStartPosition);

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
            Console.WriteLine("CurrentEndPosition " + e * internalSongDuration);

            if (IsPlaying)
            {
                ResetAllTimers();
            }
        }

        private void ResetAllTimers()
        {
            timer.StopTimers();
            var delta = CurrentEndPosition - CurrentStartPosition;
            timer.SetLoopTimer(delta);
            timer.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }
        #endregion
    }
}
