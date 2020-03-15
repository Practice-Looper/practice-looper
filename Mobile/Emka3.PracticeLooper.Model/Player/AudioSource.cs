// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Emka3.PracticeLooper.Model.Common;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Emka3.PracticeLooper.Model.Player
{
    [Table("AudioSources")]
    [Utils.Preserve(AllMembers = true)]
    public class AudioSource  : EntityBase
    {
        public string Source { get; set; }
        public string FileName { get; set; }
        public double Duration { get; set; }
        public AudioSourceType Type { get; set; }

        [ForeignKey(typeof(Session))]
        public int SessionId { get; set; }

        [OneToOne]
        public Session Session { get; set; }
    }
}
