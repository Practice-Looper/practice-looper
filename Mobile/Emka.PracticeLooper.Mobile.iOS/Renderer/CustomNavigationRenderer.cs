// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Linq;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomNavigationRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    [Preserve(AllMembers = true)]
    public class CustomNavigationRenderer : NavigationRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            NavigationBar.CompactAppearance.ShadowColor = null;
            NavigationBar.StandardAppearance.ShadowColor = null;
            NavigationBar.ScrollEdgeAppearance.ShadowColor = null;
            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();
        }

        public override void RemoteControlReceived(UIEvent theEvent)
        {
            base.RemoteControlReceived(theEvent);
            if (theEvent.Type == UIEventType.RemoteControl)
            {
                switch (theEvent.Subtype)
                {
                    case UIEventSubtype.None:
                        break;
                    case UIEventSubtype.MotionShake:
                        break;
                    case UIEventSubtype.RemoteControlPlay:
                        break;
                    case UIEventSubtype.RemoteControlPause:
                    case UIEventSubtype.RemoteControlStop:
                        MappingsFactory.Contracts.IResolver resolver = Factory.GetResolver();
                        var audioPlayers = resolver.ResolveAll<IAudioPlayer>();
                        var spotifyLoader = resolver.Resolve<ISpotifyLoader>();

                        var currentPlayer = audioPlayers.FirstOrDefault(p => p.IsPlaying);

                        if (currentPlayer != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() => currentPlayer.Pause(true));
                        }

                        if (spotifyLoader != null)
                        {
                            spotifyLoader.Disconnect();
                        }
                        break;
                    case UIEventSubtype.RemoteControlTogglePlayPause:
                        break;
                    case UIEventSubtype.RemoteControlNextTrack:
                        break;
                    case UIEventSubtype.RemoteControlPreviousTrack:
                        break;
                    case UIEventSubtype.RemoteControlBeginSeekingBackward:
                        break;
                    case UIEventSubtype.RemoteControlEndSeekingBackward:
                        break;
                    case UIEventSubtype.RemoteControlBeginSeekingForward:
                        break;
                    case UIEventSubtype.RemoteControlEndSeekingForward:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
