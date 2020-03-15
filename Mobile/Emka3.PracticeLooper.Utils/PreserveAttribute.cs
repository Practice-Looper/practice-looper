using System;

namespace Emka3.PracticeLooper.Utils
{
    public sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers;
        public bool Conditional;
    }
}
