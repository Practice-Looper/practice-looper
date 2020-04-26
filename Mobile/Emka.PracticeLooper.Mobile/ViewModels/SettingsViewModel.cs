// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Utils;
using Plugin.InAppBilling;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields

        private readonly IConfigurationService configService;
        #endregion

        #region Ctor

        public SettingsViewModel(IConfigurationService configService)
        {
            this.configService = configService;
            Products = new ObservableCollection<InAppBillingProduct>();
        }
        #endregion

        #region Properties

        public string AppVersion => VersionTracking.CurrentVersion;
        public ObservableCollection<InAppBillingProduct> Products { get; set; }
        #endregion

        #region Methods

        public async override Task InitializeAsync(object parameter)
        {
            await GetInAppItems();
        }

        private async Task GetInAppItems()
        {
            var billing = CrossInAppBilling.Current;
            try
            {

                var productIds = new[] { "pl.premium.general" };
                //You must connect
                var connected = await billing.ConnectAsync(ItemType.InAppPurchase);

                if (!connected)
                {
                    //Couldn't connect
                    return;
                }

                //check purchases

                var items = await billing.GetProductInfoAsync(ItemType.InAppPurchase, productIds);

                foreach (var item in items)
                {
                    Products.Add(item);
                }
            }
            catch (InAppBillingPurchaseException pEx)
            {
                //Handle IAP Billing Exception
            }
            catch (Exception ex)
            {
                //Something has gone wrong
            }
            finally
            {
                await billing.DisconnectAsync();
            }
        }
        #endregion
    }
}
