// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Foundation;

namespace SpotifyBindings.iOS
{
    public class SessionManagerDelegate : SPTSessionManagerDelegate
    {

        public override void SessionManager(SPTSessionManager manager, SPTSession session)
        {
           
        }

        public override void SessionManager(SPTSessionManager manager, NSError error)
        {

        }

        public override void SessionManagerDidFailWithError(SPTSessionManager manager, SPTSession session)
        {

        }

        public override bool SessionManagerShouldRequestAccessTokenWithAuthorizationCode(SPTSessionManager manager, string code)
        {
            return true;
        }
    }
}
