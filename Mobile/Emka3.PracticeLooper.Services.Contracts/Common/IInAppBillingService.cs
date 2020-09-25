// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Common;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IInAppBillingService
    {
        IDictionary<string, InAppPurchaseProduct> Products { get; }
        void Init();
        Task FetchOfferingsAsync();
        void StartFetchingOfferings();
        Task<(bool Success, string Error, bool UserCancelled)> PurchaseProductAsync(object package);
        Task<(bool Success, string Error)> RestorePurchasesAsync(); 
    }
}
