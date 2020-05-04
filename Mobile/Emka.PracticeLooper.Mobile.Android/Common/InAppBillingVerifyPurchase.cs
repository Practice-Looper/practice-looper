// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Plugin.InAppBilling;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class InAppBillingVerifyPurchase : IInAppBillingVerifyPurchase
    {
        public async Task<bool> VerifyPurchase(string signedData, string signature, string productId = null, string transactionId = null)
        {
            return true;
        }
    }
}
