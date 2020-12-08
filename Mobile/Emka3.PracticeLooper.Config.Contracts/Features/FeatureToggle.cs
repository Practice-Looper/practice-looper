// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Contracts.Features
{
    [Preserve(AllMembers = true)]
    public class FeatureToggle<T> where T : IFeature
    {
        public FeatureToggle(IFeature feature, bool isEnabled = false)
        {
            Feature = feature;
            IsEnabled = isEnabled;
        }

        public bool IsEnabled { get; set; }
        public IFeature Feature { get; }
    }
}
