// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class InAppPurchaseProductViewModel : ViewModelBase
    {
        private bool purchased;

        public InAppPurchaseProductViewModel(InAppPurchaseProduct product)
        {
            Name = product.Name;
            Description = product.Description;
            ProductId = product.ProductId;
            LocalizedPrice = product.LocalizedPrice;
            CurrencyCode = product.CurrencyCode;
            MicrosPrice = product.MicrosPrice;
            LocalizedIntroductoryPrice = product.LocalizedIntroductoryPrice;
            MicrosIntroductoryPrice = product.MicrosIntroductoryPrice;
            Package = product.Package;
            Purchased = product.Purchased;
            Image = product.Image;
        }

        /// <summary>
        /// Name of the product
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description of the product
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Product ID or sku
        /// </summary>
        public string ProductId { get; }

        /// <summary>
        /// Localized Price (not including tax)
        /// </summary>
        public string LocalizedPrice { get; }

        /// <summary>
        /// ISO 4217 currency code for price. For example, if price is specified in British pounds sterling is "GBP".
        /// </summary>
        public string CurrencyCode { get; }

        /// <summary>
        /// Price in micro-units, where 1,000,000 micro-units equal one unit of the 
        /// currency. For example, if price is "€7.99", price_amount_micros is "7990000". 
        /// This value represents the localized, rounded price for a particular currency.
        /// </summary>
        public long MicrosPrice { get; }

        /// <summary>
        /// Gets or sets the localized introductory price.
        /// </summary>
        /// <value>The localized introductory price.</value>
        public string LocalizedIntroductoryPrice { get; }

        /// <summary>
        /// Introductory price of the product in micor-units
        /// </summary>
        /// <value>The introductory price.</value>
        public long MicrosIntroductoryPrice { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Plugin.InAppBillingProduct" />
        /// has introductory price. This is an optional value in the answer from the server, requires a boolean to check if this exists
        /// </summary>
        /// <value><c>true</c> if has introductory price; otherwise, <c>false</c>.</value>
        public bool HasIntroductoryPrice => !string.IsNullOrEmpty(LocalizedIntroductoryPrice);

        /// <summary>
        /// RevenueCat Package.
        /// Argument for purchase package call.
        /// </summary>
        public object Package { get; }

        /// <summary>
        /// Indicates whether the item has been purchased or not.
        /// </summary>
        public bool Purchased
        {
            get => purchased;
            set
            {
                purchased = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Image for paywall presentation.
        /// </summary>
        public string Image { get; set; }

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }
    }
}
