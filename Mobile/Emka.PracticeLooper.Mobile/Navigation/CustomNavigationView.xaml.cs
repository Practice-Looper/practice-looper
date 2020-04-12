// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using NavigationPage = Xamarin.Forms.NavigationPage;
using Page = Xamarin.Forms.Page;

namespace Emka.PracticeLooper.Mobile.Navigation
{
    public partial class CustomNavigationView : NavigationPage
    {
        public CustomNavigationView()
        {
            InitializeComponent();
            On<iOS>().SetHideNavigationBarSeparator(true);
        }

        public CustomNavigationView(Page root) : base(root)
        {
            InitializeComponent();
            On<iOS>().SetHideNavigationBarSeparator(true);
        }
    }
}
