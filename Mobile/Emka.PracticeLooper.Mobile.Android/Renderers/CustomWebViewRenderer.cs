// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using System;
using Android.Content;
using Emka.PracticeLooper.Mobile.Droid.Common;
using Emka.PracticeLooper.Mobile.Droid.Renderers;
using Emka.PracticeLooper.Mobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Emka.PracticeLooper.Mobile.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class CustomWebViewRenderer : WebViewRenderer
    {
        public CustomWebViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Settings.JavaScriptEnabled = true;
                Control.Settings.AllowContentAccess = true;
                Control.Settings.SaveFormData = true;
                Control.SetWebChromeClient(new SpotifyWebViewClient());
            }
        }
    }
}
