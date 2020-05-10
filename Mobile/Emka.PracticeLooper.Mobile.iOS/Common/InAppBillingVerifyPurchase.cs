// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.IO;
using System.Threading.Tasks;
using Org.BouncyCastle.Cms;
using Plugin.InAppBilling;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class InAppBillingVerifyPurchase : IInAppBillingVerifyPurchase
    {
        public Task<bool> VerifyPurchase(string signedData, string signature, string productId = null, string transactionId = null)
        {
            bool result = false;
            var data = Convert.FromBase64String(signedData);
            var cmsParser = new CmsSignedDataParser(data);
            var cmsSignedContent = cmsParser.GetSignedContent();
            var contentStream = cmsSignedContent.ContentStream;
            var memoryStream = new MemoryStream();
            contentStream.CopyTo(memoryStream);
            byte[] contentBytes = memoryStream.ToArray();

            cmsParser.GetSignedContent().Drain();
            var certStore = cmsParser.GetCertificates("Collection");
            var signerInfos = cmsParser.GetSignerInfos();
            var signers = signerInfos.GetSigners();

            foreach (SignerInformation signer in signers)
            {
                var certCollection = certStore.GetMatches(signer.SignerID);
                foreach (Org.BouncyCastle.X509.X509Certificate cert in certCollection)
                {
                    result = signer.Verify(cert);
                    if (!result)
                    {
                        throw new Exception("Certificate verification error, the signer could not be verified.");
                    }
                }
            }

            return Task.FromResult(result);
        }
    }
}
