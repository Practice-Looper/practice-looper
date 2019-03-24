// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Ctor
        public FileAudioPlayer()
        {
        }
        #endregion

        #region Properties
        public bool IsPlaying => throw new NotImplementedException();

        public double SongDuration => throw new NotImplementedException();

        public Loop CurrentLoop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<bool> PlayStatusChanged;
        #endregion

        #region Methods
        public void Init(Session session)
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Seek(double time)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
