// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Plugin.InAppBilling;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class InAppBillingVerifyPurchase : IInAppBillingVerifyPurchase
    {
        #region Ctor

        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public InAppBillingVerifyPurchase(IConfigurationService configurationService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }
        #endregion

        #region Methods

        public Task<bool> VerifyPurchase(string signedData, string signature, string productId = null, string transactionId = null)
        {
            var key1Transform = InAppBillingImplementation.InAppBillingSecurity.TransformString(configurationService.GetValue("PublicKey1"), 1);
            var key2Transform = InAppBillingImplementation.InAppBillingSecurity.TransformString(configurationService.GetValue("PublicKey2"), 2);
            var key3Transform = InAppBillingImplementation.InAppBillingSecurity.TransformString(configurationService.GetValue("PublicKey3"), 3);

            return Task.FromResult(InAppBillingImplementation.InAppBillingSecurity.VerifyPurchase(key1Transform + key2Transform + key3Transform, signedData, signature));
        }
        #endregion
    }
}
