// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class AudioSourceVieModel
    {
        public AudioSourceVieModel(string displayName, AudioSourceType type)
        {
            DisplayName = displayName;
            AudioType = type;
        }
        
        public string DisplayName { get; set; }
        public AudioSourceType AudioType { get; set; }
    }
}
