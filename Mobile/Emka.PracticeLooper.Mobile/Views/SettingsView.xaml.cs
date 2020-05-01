﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class SettingsView : ContentPage
    {
        public SettingsView()
        {
            InitializeComponent();
            var resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();
            BindingContext = new SettingsViewModel(Factory.GetConfigService(), resolver.Resolve<ILogger>(), resolver.Resolve<IDialogService>(), resolver.Resolve<IAppTracker>());
        }
        
        void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (BindingContext is SettingsViewModel vm)
            {
                vm.PurchaseItemCommand.Execute(e.Item);
            }
        }
    }
}
