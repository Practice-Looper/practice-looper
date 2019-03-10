// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public class RangeSlider : Slider
    {
        public static readonly BindableProperty RangeValueProperty = BindableProperty.Create<RangeSlider, double>(r => r.RangeValue, 0, BindingMode.TwoWay, null, null, null, null, null);

        public RangeSlider()
        {
        }

        public double RangeValue
        {
            get { return (double)GetValue(RangeValueProperty); }
            set { SetValue(RangeValueProperty, value); }
        }
    }
}

