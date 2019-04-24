// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Foundation;

namespace SpotifyBindings.iOS
{
    public class RemoteDelegate : SPTAppRemoteDelegate
    {

        public override void AppRemoteDidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {

        }

        public override void AppRemoteDidEstablishConnection(SPTAppRemote appRemote)
        {

        }

        public override void AppRemoteDidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {

        }
    }
}
