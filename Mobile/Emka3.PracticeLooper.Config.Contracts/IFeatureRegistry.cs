// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Contracts
{
    [Preserve(AllMembers = true)]
    public interface IFeatureRegistry
    {
        void Add<T>(T feature, bool enabled) where T : IFeature;
        IFeature GetFeature<T>() where T : IFeature;
        void RegisterForUpdates<T>(Action<bool> callback) where T : IFeature;
        bool IsEnabled<T>() where T : IFeature;
        void Update<T>(bool enabled) where T : IFeature;
        void Update(string storeId, bool isEnabled);
        void Clear();
    }
}
