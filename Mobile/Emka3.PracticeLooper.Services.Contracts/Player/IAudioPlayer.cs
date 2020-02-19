﻿// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;

namespace Emka3.PracticeLooper.Services.Contracts.Player
{
    public interface IAudioPlayer
    {
        #region Events
        event EventHandler<bool> PlayStatusChanged;
        event EventHandler CurrentTimePositionChanged;
        event EventHandler TimerElapsed;
        #endregion

        #region Properties
        bool IsPlaying { get; }
        double SongDuration { get; }
        AudioSourceType Type { get; }
        string DisplayName { get; }
        #endregion

        #region Methods
        void Init(Loop loop);
        void Play();
        void Pause();
        void Seek(double time);
        Task InitAsync(Loop loop);
        Task PlayAsync();
        Task PauseAsync();
        Task SeekAsync(double time);
        void GetCurrentPosition(Action<double> callback);
        #endregion
    }
}
