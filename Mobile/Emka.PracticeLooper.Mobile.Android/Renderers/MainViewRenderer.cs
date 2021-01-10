// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using System;
using Android.App;
using Android.Content;
using Emka.PracticeLooper.Mobile.Droid.Renderers;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MainView), typeof(MainViewViewRenderer))]
namespace Emka.PracticeLooper.Mobile.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class MainViewViewRenderer : PageRenderer
    {
        private readonly ISpotifyLoader spotifyLoader;

        public MainViewViewRenderer(Context context) : base(context)
        {
            spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>() ?? throw new ArgumentNullException(nameof(spotifyLoader));
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null || Element == null)
            {
                return;
            }

            spotifyLoader.WebAuthorizationRequested += OnWebAuthorizationRequested;
        }

        private void OnWebAuthorizationRequested(object sender, AudioSourceType sourceType)
        {
            var activity = Context as Activity;
            switch (sourceType)
            {
                case AudioSourceType.Spotify:
                    if (!spotifyLoader.Authorized)
                    {
                        var authenticator = spotifyLoader.GetAuthenticator();
                        authenticator.Completed += OnAuthenticatorCompleted;
                        activity.StartActivity(authenticator.GetUI(activity));
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            //var activity = Context as Activity;
            //activity.Finish();

            var authenticator = sender as OAuthAuthenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthenticatorCompleted;
            }
        }
    }
}
