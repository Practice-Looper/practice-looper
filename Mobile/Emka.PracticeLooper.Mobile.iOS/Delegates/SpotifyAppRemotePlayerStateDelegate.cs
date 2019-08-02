// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using SpotifyBindings.iOS;

namespace Emka.PracticeLooper.Mobile.iOS.Delegates
{
    public class SpotifyAppRemotePlayerStateDelegate : SPTAppRemotePlayerStateDelegate
    {

        public override void PlayerStateDidChange(ISPTAppRemotePlayerState playerState)
        {
            Console.WriteLine(playerState);
        }
    }
}
