// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using System.Linq;

namespace Emka3.PracticeLooper.Config.Feature
{
    public static class FeatureRegistry
    {
        #region Ctor

        static FeatureRegistry()
        {
            Features = new List<object>();
        }
        #endregion

        #region Properties

        public static List<object> Features { get; private set; }

        #endregion

        #region Methods

        public static void Add<T>(bool enabled) where T : IFeature
        {
            Features.Add(new FeatureToggle<T>(enabled));
        }

        public static bool IsEnabled<T>() where T : IFeature
        {
            var feature = Features
                .FirstOrDefault(f => f.GetType() == typeof(FeatureToggle<T>))
                as FeatureToggle<T>;

            return feature != null && feature.Enabled;
        }
        #endregion
    }
}
