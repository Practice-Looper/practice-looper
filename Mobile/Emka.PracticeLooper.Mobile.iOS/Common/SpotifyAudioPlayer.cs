// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
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
        private readonly IConfigurationService configurationService;
        private bool isPlaying;
        Session session;
        #endregion

        #region Ctor
        public SpotifyAudioPlayer()
        {
            configurationService = Factory.GetConfigService();
            api = GlobalApp.SPTRemoteApi;
            isPlaying = false;
        }
        #endregion

        #region Properties
        public bool IsPlaying => isPlaying;

        public double SongDuration => 0;

        public Loop CurrentLoop { get; set; }
        #endregion

        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler<int> CurrentTimePositionChanged;
        #endregion

        #region MyRegion
        public void Init(Session session)
        {
            this.session = session;
        }

        public void Pause()
        {
            isPlaying = false;
            api.PlayerAPI.Pause((o, e) => { });
            RaisePlayingStatusChanged();
        }

        public void Play()
        {
            api.PlayerAPI.Play(session.AudioSource.Source, (o, e) => { });
            isPlaying = true;
            RaisePlayingStatusChanged();
        }

        public void Seek(double time)
        {

        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }
        #endregion
    }
}
