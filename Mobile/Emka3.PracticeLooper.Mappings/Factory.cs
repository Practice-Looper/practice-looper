// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Emka3.PracticeLooper.Mappings.Contracts;

namespace Emka3.PracticeLooper.Mappings
{
    /// <summary>
    /// Provides factory methods for baza resolvers.
    /// </summary>
    public static class Factory
    {
        public static IResolver GetResolver()
        {
            return PracticeLooperResolver.Instance;
        }
    }
}
