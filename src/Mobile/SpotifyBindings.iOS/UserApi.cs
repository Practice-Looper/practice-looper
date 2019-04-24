// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Foundation;

namespace SpotifyBindings.iOS
{
    public class UserApi : SPTAppRemoteUserAPI
    {
        public SPTAppRemoteUserAPIDelegate Delegate { get; set; }
        public override NSObject WeakDelegate { get; set; }

        public override void AddItemToLibraryWithURI(string URI, SPTAppRemoteCallback callback)
        {

        }

        public override void FetchCapabilitiesWithCallback(SPTAppRemoteCallback callback)
        {
           
        }

        public override void FetchLibraryStateForURI(string URI, SPTAppRemoteCallback callback)
        {

        }

        public override void RemoveItemFromLibraryWithURI(string URI, SPTAppRemoteCallback callback)
        {

        }

        public override void SubscribeToCapabilityChanges(SPTAppRemoteCallback callback)
        {
           
        }

        public override void UnsubscribeToCapabilityChanges(SPTAppRemoteCallback callback)
        {
           
        }
    }
}
