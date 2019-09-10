﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using SpotifyBindings.iOS;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class SpotifyAudioPlayer : IAudioPlayer
    {
        #region Fields
        private SPTAppRemote api;
        private Session session;
        private readonly IConfigurationService configurationService;
        private readonly IPlayerTimer timer;
        private bool isPlaying;
        private double internalSongDuration;
        const int CURRENT_TIME_UPDATE_INTERVAL = 500;
        private bool hasSpotifyPlayerSubscription;
        #endregion

        #region Ctor
        public SpotifyAudioPlayer(IPlayerTimer timer)
        {
            configurationService = Factory.GetConfigService();
            api = GlobalApp.SPTRemoteApi;
            isPlaying = false;
            this.timer = timer;
        }
        #endregion

        #region Properties
        public bool IsPlaying => isPlaying;

        public double SongDuration => 0;

        public Loop CurrentLoop { get; set; }

        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }
        #endregion

        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler<int> CurrentTimePositionChanged;
        public event EventHandler TimerElapsed;
        #endregion

        #region MyRegion
        public void Init(Session session)
        {
            this.session = session;
            CurrentLoop = session.Loops[0];
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
            CurrentEndPosition = ConvertToInt(CurrentLoop.EndPosition);
            //TimerElapsed += SpotifyAudioPlayer_TimerElapsed;

            internalSongDuration = TimeSpan.FromMilliseconds(session.AudioSource.Duration).TotalSeconds;
        }

        private void SpotifyAudioPlayer_TimerElapsed(object sender, EventArgs e)
        {
            TimerElapsed?.Invoke(this, e);
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                isPlaying = false;
                timer.StopTimers();
                api.PlayerAPI.Pause((o, e) => { });

                if (!hasSpotifyPlayerSubscription)
                {

                    api.PlayerAPI.SubscribeToPlayerState((o, e) =>
                    {
                        //var x = o as SPTAppRemotePlayerState;
                        //if (x.Paused)
                        //{
                        //    CurrentStartPosition = (uint)x.PlaybackPosition;
                        //}
                    });
                    hasSpotifyPlayerSubscription = true;
                }
                RaisePlayingStatusChanged();
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                isPlaying = true;
                CurrentStartPosition = ConvertToInt(CurrentLoop.StartPosition);
                timer.LoopTimerExpired += LoopTimerExpired;
                api.PlayerAPI.Play(session.AudioSource.Source, (o, e) => { });
                timer.SetLoopTimer(CurrentEndPosition - CurrentStartPosition);
                RaisePlayingStatusChanged();
            }
        }

        private void CurrentPositionTimerExpired(object sender, EventArgs e)
        {

        }

        private void LoopTimerExpired(object sender, EventArgs e)
        {
            TimerElapsed?.Invoke(this, e);
        }

        public void Seek(double time)
        {
            if (api != null)
            {
                try
                {
                    var seekTo = (time * session.AudioSource.Duration);
                    api.PlayerAPI.SeekToPosition(0, (o, e) =>
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
            var delta = (int)CurrentEndPosition - (int)CurrentStartPosition;
            timer.SetLoopTimer(delta);
            //timer.SetCurrentTimeTimer(CURRENT_TIME_UPDATE_INTERVAL);
        }
        #endregion
    }
}
