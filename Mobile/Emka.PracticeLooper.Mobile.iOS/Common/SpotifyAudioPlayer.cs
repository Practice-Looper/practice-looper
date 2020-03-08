// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using SpotifyBindings.iOS;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class SpotifyAudioPlayer : IPremiumAudioPlayer
    {
        #region Fields
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
        public bool Initialized { get; private set; }
        public bool IsPlaying => isPlaying;
        public double SongDuration { get { return internalSongDuration * 1000; } }
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
            Initialized = true;
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                isPlaying = false;
                timer.StopTimers();
                Api.PlayerAPI.Pause((o, e) => { /* log error*/ });
                RaisePlayingStatusChanged();
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                isPlaying = true;
                CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Api.PlayerAPI.Play(session.AudioSource.Source, (o, e) => {   });
                });

                timer.SetLoopTimer(CurrentEndPosition - CurrentStartPosition);
                timer.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);

                RaisePlayingStatusChanged();
                CurrentPositionTimerExpired(this, new EventArgs());
            }
        }

        public void Seek(double time)
        {
            if (Api != null)
            {
                try
                {
                    var seekTo = Convert.ToInt32(time * session.AudioSource.Duration);
                    Api.PlayerAPI.SeekToPosition(seekTo, (o, e) =>
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
                Api.PlayerAPI.GetPlayerState((o, e) =>
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

        public async Task PlayAsync()
        {
            await Task.Run(Play);
        }

        public Task PauseAsync()
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
