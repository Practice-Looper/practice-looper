// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public class CustomWebView : WebView
    {
        public static readonly BindableProperty IsNavigatingProperty =
                BindableProperty.Create("IsNavigating", typeof(bool), typeof(CustomWebView), null);

        public static readonly BindableProperty IsLoadedProperty =
                BindableProperty.Create("IsLoaded", typeof(bool), typeof(CustomWebView), null);

        public CustomWebView()
        {

        }

        public bool IsNavigating
        {
            get { return (bool)GetValue(IsNavigatingProperty); }
            set { SetValue(IsNavigatingProperty, value); }
        }

        public bool IsLoaded
        {
            get { return (bool)GetValue(IsLoadedProperty); }
            set { SetValue(IsLoadedProperty, value); }
        }
    }
}
