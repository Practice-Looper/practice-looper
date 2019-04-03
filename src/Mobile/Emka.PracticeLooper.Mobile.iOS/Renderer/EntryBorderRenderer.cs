// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.ComponentModel;
using CoreGraphics;
using Emka.PracticeLooper.Mobile.iOS.Extensions;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka.PracticeLooper.Mobile.Views;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(EntryBorderRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    public class EntryBorderRenderer : EntryRenderer
    {
        public EntryBorderRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            Control.Layer.BorderColor = UIColor.Clear.FromHex(0x369598).CGColor;
            Control.Layer.BorderWidth = 1;
        }
    }
}
