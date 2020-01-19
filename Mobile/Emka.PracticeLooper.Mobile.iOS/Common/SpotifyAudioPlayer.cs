// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.iOS.Delegates;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using SpotifyBindings.iOS;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class SpotifyAudioPlayer : SPTAppRemoteDelegate, IAudioPlayer
    {
        #region Fields
        private SPTAppRemote api;
        private Session session;
        private readonly IPlayerTimer timer;
        private readonly ISpotifyLoader spotifyLoader;
        private CancellationTokenSource currentTimeCancelTokenSource;
        private bool isPlaying;
        private double internalSongDuration;
        const int CURRENT_TIME_UPDATE_INTERVAL = 1000;
        private bool hasSpotifyPlayerSubscription;
        #endregion

        #region Ctor
        public SpotifyAudioPlayer(IPlayerTimer timer, ISpotifyLoader spotifyLoader)
        {
            this.spotifyLoader = spotifyLoader;
            isPlaying = false;
            this.timer = timer;
        }
        #endregion

        #region Properties
        public bool IsPlaying => isPlaying;

        public double SongDuration { get { return internalSongDuration * 1000; } }

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
        public void Init(Loop loop)
        {
            var loader = MappingsFactory.Factory.GetResolver().Resolve<ISpotifyLoader>();
            api = spotifyLoader.RemoteApi as SPTAppRemote;
            api.ConnectionParameters.AccessToken = loader.Token;
            api.Delegate = new SpotifyAppRemoteDelegate();
            api.Connect();

            this.session = loop.Session;
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
                isPlaying = false;
                timer.StopTimers();
                api.PlayerAPI.Pause((o, e) => { /* log error*/ });
                RaisePlayingStatusChanged();
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                isPlaying = true;
                CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
                api.PlayerAPI.Play(session.AudioSource.Source, (o, e) => { });
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
                    var seekTo = Convert.ToInt32(time * session.AudioSource.Duration);
                    api.PlayerAPI.SeekToPosition(seekTo, (o, e) =>
                    {
                        if (e != null)
                        {
                            Debug.WriteLine(e.DebugDescription);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void GetCurrentPosition(Action<double> callback)
        {
            try
            {
                api.PlayerAPI.GetPlayerState((o, e) =>
                {
                    if (callback != null)
                    {
                        callback.Invoke(o.PlaybackPosition);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Task InitAsync(Loop session)
        {
            throw new NotImplementedException();
        }

        public Task PlayAsync()
        {
            throw new NotImplementedException();
        }

        public Task PauseAsync()
        {
            throw new NotImplementedException();
        }

        public Task SeekAsync(double time)
        {
            throw new NotImplementedException();
        }

        public override void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {

        }

        public override void DidEstablishConnection(SPTAppRemote appRemote)
        {

        }

        public override void DidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {

        }

        private void CurrentPositionTimerExpired(object sender, EventArgs e)
        {
            CurrentTimePositionChanged.Invoke(this, e);
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
                result = Convert.ToInt32(inValue * session.AudioSource.Duration);
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

        #endregion
    }
}
