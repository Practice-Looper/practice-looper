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
        public override void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            Console.WriteLine("AppRemoteDidDisconnectWithError");
        }

        public override void DidEstablishConnection(SPTAppRemote appRemote)
        {
            Console.WriteLine("AppRemoteDidEstablishConnection");
            appRemote.PlayerAPI.SetWeakDelegate(new SpotifyAppRemotePlayerStateDelegate());
            appRemote.PlayerAPI.SubscribeToPlayerState((obj, error) =>
            {
                Console.WriteLine(obj);

                appRemote.PlayerAPI.Play("spotify:track:4E6wpXABj8XosZEPXZz2OK", (e, a) =>
                {
                    
                });
            });
        }

        public override void DidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {
            Console.WriteLine("AppRemoteDidFailConnectionAttemptWithError");
        }
    }
}
