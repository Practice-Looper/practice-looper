// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021

using System;
using Android.App;
using Android.Content;
using Android.OS;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Activities
{
    [Activity(Label = "Authenticator", NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataSchemes = new[] { "loopr" },
        DataPath = "/callback")]
    public class ActivityCustomUrlSchemeInterceptor : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Android.Net.Uri uri_android = Intent.Data;
            // Convert Android.Net.Url to Uri
            var uri = new Uri(uri_android.ToString());

            // Close browser 
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);

            var spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
            // Load redirectUrl page
            spotifyLoader.Authenticator.OnPageLoading(uri);

            Finish();
        }
    }
}
