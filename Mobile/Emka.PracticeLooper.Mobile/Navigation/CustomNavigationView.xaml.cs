// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Emka.PracticeLooper.Mobile.Views;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Navigation
{
    public partial class CustomNavigationView : NavigationPage
    {
        public CustomNavigationView()
        {
            InitializeComponent();
            SetColors();
        }

        public CustomNavigationView(Page root) : base(root)
        {
            InitializeComponent();
            SetColors();
        }

        private void SetColors()
        {
            BarBackgroundColor = ColorConstants.Background;
            BarTextColor = ColorConstants.Secondary;
        }
    }
}
