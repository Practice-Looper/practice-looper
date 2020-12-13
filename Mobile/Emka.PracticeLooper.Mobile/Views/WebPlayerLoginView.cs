// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.ViewModels;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public class WebPlayerLoginView : ContentPage
    {
        public WebPlayerLoginView()
        {
            BindingContext = new WebPlayerLoginViewModel();
        }
    }
}

