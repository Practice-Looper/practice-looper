using System;
using CoreGraphics;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Google.MobileAds;
using UIKit;

namespace Emka.PracticeLooper.Mobile.iOS
{
    public partial class ViewController : UIViewController
    {
        BannerView adView;
        private string bannerId = "ca-app-pub-9188939990576233/9948965805";

        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            var configService = Factory.GetResolver().Resolve<IConfigurationService>();
            bannerId = configService.GetValue("admob:ios:TopBanner");
            UIView MyAdmob;
            MyAdmob = CreateNativeAdControl();
            MyAdmob.BackgroundColor = UIColor.Cyan;
            View.AddSubview(MyAdmob);
        }

        private BannerView CreateNativeAdControl()
        {

            //  montemagno admob xamarin.forms code burrowed
            if (adView != null)
            {
                return adView;
            }
            //UIScreen.MainScreen.Bounds.Size.Height - AdSizeCons.Banner.Size.Height
            CGPoint origin = new CGPoint(0, UIScreen.MainScreen.Bounds.Size.Height - AdSizeCons.Banner.Size.Height);

            var insets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets; // Can't use KeyWindow this early
            if (insets.Top > 0) // We have a notch
            {
                origin = new CGPoint(0, (UIScreen.MainScreen.Bounds.Size.Height - AdSizeCons.Banner.Size.Height) - insets.Bottom);
            }

            // Setup your BannerView, review AdSizeCons class for more Ad sizes. 
            adView = new BannerView(AdSizeCons.SmartBannerPortrait, origin)
            {
                AdUnitID = bannerId,
                RootViewController = this
            };

            // Wire AdReceived event to know when the Ad is ready to be displayed
            adView.AdReceived += (object sender, EventArgs e) =>
            {
                //ad has come in
            };

            adView.ReceiveAdFailed += (sender, e) =>
            {
                Console.WriteLine("########## Error received" + e.Error.Description);
            };

            adView.LoadRequest(GetRequest());
            return adView;
            //  montemagno admob xamarin.forms code burrowed
        }

        Request GetRequest()
        {
            var request = Request.GetDefaultRequest();
            // Requests test ads on devices you specify. Your test device ID is printed to the console when
            // an ad request is made. GADBannerView automatically returns test ads when running on a
            // simulator. After you get your device ID, add it here
            request.TestDevices = new[] { Request.SimulatorId.ToString() };
            return request;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
