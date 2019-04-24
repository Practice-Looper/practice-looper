// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Foundation;
using SpotifyBindings.iOS;

namespace Emka.PracticeLooper.Mobile.iOS.Delegates
{
    public class SpotifyAppRemoteDelegate : SPTAppRemoteDelegate
    {
        public override void AppRemoteDidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            Console.WriteLine("AppRemoteDidDisconnectWithError");
        }

        public override void AppRemoteDidEstablishConnection(SPTAppRemote appRemote)
        {
            Console.WriteLine("AppRemoteDidEstablishConnection");
            appRemote.PlayerAPI.Delegate = new SpotifyAppRemotePlayerStateDelegate();
            appRemote.PlayerAPI.SubscribeToPlayerState((NSObject arg0, NSError arg1) =>
            {
                Console.WriteLine(arg0);
            });
        }

        public override void AppRemoteDidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {
            Console.WriteLine("AppRemoteDidFailConnectionAttemptWithError");
        }
    }
}
