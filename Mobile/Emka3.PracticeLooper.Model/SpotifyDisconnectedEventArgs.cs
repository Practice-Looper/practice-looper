// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
namespace Emka3.PracticeLooper.Model
{
    public class SpotifyDisconnectedEventArgs : EventArgs
    {
        public SpotifyDisconnectedEventArgs(bool terminatedByUser)
        {
            TerminatedByUser = terminatedByUser;
        }

        public bool TerminatedByUser { get; }
    }
}
