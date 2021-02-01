// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Model
{
    [Preserve(AllMembers = true)]
    public static class MessengerKeys
    {
        public const string GetAudioSource = nameof(GetAudioSource);
        public const string NewTrackAdded = nameof(NewTrackAdded);
        public const string AudioSourceSelected = nameof(AudioSourceSelected);
        public const string AudioSourcePickerClosed = nameof(AudioSourcePickerClosed);
        public const string DeleteSession = nameof(DeleteSession);
        public const string DeleteLoop = nameof(DeleteLoop);
        public const string LoopChanged = nameof(LoopChanged);
        public const string ShowDialog = nameof(ShowDialog);
        public const string SpotifyActivatePlayer = nameof(SpotifyActivatePlayer);
        public const string SpotifyPlayerActivated = nameof(SpotifyPlayerActivated);
        public const string SpotifyLoadWebPlayer = nameof(SpotifyLoadWebPlayer);
        public const string SpotifyWebPlayerLoaded = nameof(SpotifyWebPlayerLoaded);
        public const string WebViewNavigationStatus = nameof(WebViewNavigationStatus);
    }
}
