// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
namespace Emka3.PracticeLooper.Config.Feature
{
    public class FeatureToggle<T> where T : IFeature
    {
        public FeatureToggle(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; private set; }
    }
}
