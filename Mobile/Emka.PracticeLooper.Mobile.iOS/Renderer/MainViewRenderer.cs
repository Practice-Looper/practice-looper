// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MainView), typeof(MainViewViewRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    [Preserve(AllMembers = true)]
    public class MainViewViewRenderer : PageRenderer
    {
        private readonly ISpotifyLoader spotifyLoader;

        public MainViewViewRenderer()
        {
            spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>() ?? throw new ArgumentNullException(nameof(spotifyLoader));
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            spotifyLoader.WebAuthorizationRequested += OnWebAuthorizationRequested;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            spotifyLoader.WebAuthorizationRequested -= OnWebAuthorizationRequested;
        }

        private void OnWebAuthorizationRequested(object sender, AudioSourceType sourceType)
        {
            switch (sourceType)
            {
                case AudioSourceType.Spotify:
                    if (!spotifyLoader.Authorized)
                    {
                        var authenticator = spotifyLoader.GetAuthenticator();
                        authenticator.Completed += OnAuthenticatorCompleted;
                        PresentViewController(authenticator.GetUI(), true, null);
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            DismissViewController(true, null);
            var authenticator = sender as OAuthAuthenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthenticatorCompleted;
            }
        }
    }
}
