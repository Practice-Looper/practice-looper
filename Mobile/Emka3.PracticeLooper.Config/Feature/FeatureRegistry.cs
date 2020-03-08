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
            Features = new Dictionary<object, object>();
        }
        #endregion

        #region Properties

        public static Dictionary<object, object> Features { get; private set; }

        #endregion

        #region Methods

        public static void Add<T>(bool enabled, string name = "") where T : IFeature
        {
            if (!string.IsNullOrEmpty(name) && !Features.ContainsKey(name))
            {
                Features.Add(name, new FeatureToggle<T>(enabled));
            }
            else
            {
                Features.Add(typeof(T), new FeatureToggle<T>(enabled));
            }
        }

        public static bool IsEnabled<T>(string name = "") where T : IFeature
        {
            if (string.IsNullOrEmpty(name) || !Features.ContainsKey(name))
            {
                return false;
            }

            var feature = string.IsNullOrEmpty(name) ? Features[typeof(T)] as FeatureToggle<T> : Features[name] as FeatureToggle<T>;
            return feature != null && feature.Enabled;
        }
        #endregion
    }
}
