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
        #endregion

        #region Ctor
        public SpotifyAudioPlayer()
        {
            configurationService = Factory.GetConfigService();
            api = GlobalApp.SPTRemoteApi;
        }
        #endregion

        #region Properties
        public bool IsPlaying => throw new NotImplementedException();

        public double SongDuration => throw new NotImplementedException();

        public Loop CurrentLoop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler<int> CurrentTimePositionChanged;
        #endregion

        #region MyRegion
        public void Init(Session session)
        {
        }

        public void Pause()
        {

        }

        public void Play()
        {

        }

        public void Seek(double time)
        {

        }
        #endregion
    }
}
