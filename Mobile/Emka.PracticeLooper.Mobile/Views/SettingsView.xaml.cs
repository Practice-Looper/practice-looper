// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Plugin.InAppBilling;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class SettingsView : ContentPage
    {
        public SettingsView()
        {
            InitializeComponent();
            var resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();
            BindingContext = new SettingsViewModel(resolver.Resolve<IConfigurationService>(),
                resolver.Resolve<ILogger>(),
                resolver.Resolve<IDialogService>(),
                resolver.Resolve<IAppTracker>(),
                resolver.Resolve<IInAppBillingVerifyPurchase>(),
                resolver.Resolve<INavigationService>());
        }
        
        void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (BindingContext is SettingsViewModel vm)
            {
                vm.PurchaseItemCommand.Execute(e.Item);
            }

            if (sender is Xamarin.Forms.ListView listView)
            {
                listView.SelectedItem = null;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                var safeInsets = On<iOS>().SafeAreaInsets();
                safeInsets.Left = safeInsets.Right = 10;
                Padding = safeInsets;
            }
        }
    }
}
