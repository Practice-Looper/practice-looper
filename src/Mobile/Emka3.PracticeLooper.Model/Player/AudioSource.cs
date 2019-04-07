// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Emka3.PracticeLooper.Model.Common;
namespace Emka3.PracticeLooper.Model.Player
{
    public class AudioSource : EntityBase
    {
        public string Source { get; set; }
        public string FileName { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }
    }
}
