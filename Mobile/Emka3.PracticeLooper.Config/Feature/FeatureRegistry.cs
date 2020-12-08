// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Linq;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Config.Contracts.Features;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Feature
{
    [Preserve(AllMembers = true)]
    public class FeatureRegistry : IFeatureRegistry
    {
        private readonly Dictionary<object, List<Action<bool>>> features;
        #region Ctor

        public FeatureRegistry()
        {
            features = new Dictionary<object, List<Action<bool>>>();
        }

        public void Add<T>(T feature, bool enabled) where T : IFeature
        {
            if (!features.Keys.Any(k => k.GetType() == typeof(FeatureToggle<T>)))
            {
                features.Add(new FeatureToggle<T>(feature, enabled), new List<Action<bool>>());
            }
        }
        #endregion

        #region Methods
        public void Clear()
        {
            if (features != null && features.Any())
            {
                features.Clear();
            }
        }

        public IFeature GetFeature<T>() where T : IFeature
        {
            var toggle = features.Keys.FirstOrDefault(k => k.GetType() == typeof(FeatureToggle<T>)) as FeatureToggle<T>;
            var feature = toggle?.Feature;
            return feature;
        }

        public bool IsEnabled<T>() where T : IFeature
        {
            var toggle = features.Keys.FirstOrDefault(k => k.GetType() == typeof(FeatureToggle<T>)) as FeatureToggle<T>;
            var enabled = toggle != null && toggle.IsEnabled;
            return enabled;
        }

        public void RegisterForUpdates<T>(Action<bool> callback) where T : IFeature
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var toggle = features.Keys.FirstOrDefault(k => k.GetType() == typeof(FeatureToggle<T>)) as FeatureToggle<T>;

            if (toggle == null)
            {
                throw new InvalidOperationException("feature not registered!");
            }

            features[toggle].Add(callback);
        }

        public void Update<T>(bool enabled) where T : IFeature
        {
            var toggle = features.Keys.FirstOrDefault(k => k.GetType() == typeof(FeatureToggle<T>)) as FeatureToggle<T>;
            if (toggle == null)
            {
                throw new InvalidOperationException("feature not registered!");
            }

            toggle.IsEnabled = enabled;

            // are there any listeners?
            if (features[toggle].Any())
            {
                foreach (var action in features[toggle])
                {
                    action.Invoke(toggle.IsEnabled);
                }
            }
        }

        public void Update(string storeId, bool isEnabled)
        {
            if (string.IsNullOrWhiteSpace(storeId))
            {
                throw new ArgumentNullException(nameof(storeId));
            }

            IFeature feature = null;

            var toggle = features.Keys.FirstOrDefault(k =>
            {
                var keyType = k.GetType();
                feature = keyType?.GetProperty("Feature")?.GetValue(k) as IFeature;
                var id = feature?.GetType()?.GetProperty("StoreId")?.GetValue(feature) as string;
                return id == storeId;
            });

            if (toggle == null || feature == null)
            {
                throw new InvalidOperationException("feature not registered!");
            }

            var updateMethod = GetType().GetMethod("Update", new[] { typeof(bool) }).MakeGenericMethod(new[] { feature.GetType() });
            updateMethod.Invoke(this, new object[] { isEnabled });
        }
        #endregion
    }
}
