// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Droid.Helpers;
using Plugin.InAppBilling;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class InAppBillingVerifyPurchase : IInAppBillingVerifyPurchase
    {
        public Task<bool> VerifyPurchase(string signedData, string signature, string productId = null, string transactionId = null)
        {
            var key1Transform = InAppBillingImplementation.InAppBillingSecurity.TransformString(Secrets.PublicKey1, 1);
            var key2Transform = InAppBillingImplementation.InAppBillingSecurity.TransformString(Secrets.PublicKey2, 2);
            var key3Transform = InAppBillingImplementation.InAppBillingSecurity.TransformString(Secrets.PublicKey3, 3);

            return Task.FromResult(InAppBillingImplementation.InAppBillingSecurity.VerifyPurchase(key1Transform + key2Transform + key3Transform, signedData, signature));
        }
    }
}
