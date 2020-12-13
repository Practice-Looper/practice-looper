// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Model.Player
{
    [Preserve(AllMembers = true)]
    public class SpotifyDevice
    {
        public SpotifyDevice(string id, string name, bool isActive, string type)
        {
            Id = id;
            Name = name;
            IsActive = isActive;
            Type = type;
        }

        public string Name { get; }
        public string Id { get; }
        public bool IsActive { get; }
        public string Type { get; }
    }
}
