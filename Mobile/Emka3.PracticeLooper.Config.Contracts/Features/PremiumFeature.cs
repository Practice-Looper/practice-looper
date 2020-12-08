// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Contracts.Features
{
    [Preserve(AllMembers = true)]
    public class PremiumFeature : IFeature
    {
        public PremiumFeature(string storeId)
        {
            StoreId = storeId;
        }

        public string StoreId { get; }
    }
}
