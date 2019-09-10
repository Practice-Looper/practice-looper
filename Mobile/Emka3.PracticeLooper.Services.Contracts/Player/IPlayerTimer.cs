// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
namespace Emka3.PracticeLooper.Services.Contracts.Player
{
    public interface IPlayerTimer
    {
        event EventHandler LoopTimerExpired;
        event EventHandler CurrentPositionTimerExpired;

        void SetLoopTimer(int time);
        void SetCurrentTimeTimer(int time);
        void StopTimers();
    }
}
