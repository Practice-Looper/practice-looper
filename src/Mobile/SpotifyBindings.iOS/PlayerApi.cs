// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Foundation;

namespace SpotifyBindings.iOS
{
    public class PlayerApi : SPTAppRemotePlayerAPI
    {

        public SPTAppRemotePlayerStateDelegate Delegate { get; set; }
        public override NSObject WeakDelegate { get; set; }

        public override void EnqueueTrackUri(string trackUri, SPTAppRemoteCallback callback)
        {

        }

        public override void GetAvailablePodcastPlaybackSpeeds(SPTAppRemoteCallback callback)
        {

        }

        public override void GetCrossfadeState(SPTAppRemoteCallback callback)
        {

        }

        public override void GetCurrentPodcastPlaybackSpeed(SPTAppRemoteCallback callback)
        {

        }

        public override void GetPlayerState(SPTAppRemoteCallback callback)
        {

        }

        public override void Pause(SPTAppRemoteCallback callback)
        {

        }

        public override void Play(string entityIdentifier, SPTAppRemoteCallback callback)
        {

        }

        public override void PlayItem(SPTAppRemoteContentItem contentItem, SPTAppRemoteCallback callback)
        {

        }

        public override void PlayItem(SPTAppRemoteContentItem contentItem, nint index, SPTAppRemoteCallback callback)
        {

        }

        public override void Resume(SPTAppRemoteCallback callback)
        {

        }

        public override void SeekBackward15Seconds(SPTAppRemoteCallback callback)
        {

        }

        public override void SeekForward15Seconds(SPTAppRemoteCallback callback)
        {

        }

        public override void SeekToPosition(nint position, SPTAppRemoteCallback callback)
        {

        }

        public override void SetPodcastPlaybackSpeed(SPTAppRemotePodcastPlaybackSpeed speed, SPTAppRemoteCallback callback)
        {

        }

        public override void SetRepeatMode(SPTAppRemotePlaybackOptionsRepeatMode repeatMode, SPTAppRemoteCallback callback)
        {

        }

        public override void SetShuffle(bool shuffle, SPTAppRemoteCallback callback)
        {

        }

        public override void SkipToNext(SPTAppRemoteCallback callback)
        {

        }

        public override void SkipToPrevious(SPTAppRemoteCallback callback)
        {

        }

        public override void SubscribeToPlayerState(SPTAppRemoteCallback callback)
        {

        }

        public override void UnsubscribeToPlayerState(SPTAppRemoteCallback callback)
        {
            
        }
    }
}
