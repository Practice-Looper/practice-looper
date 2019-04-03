// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Emka.PracticeLooper.Mobile.iOS.Effects
{
    public class SafeAreaPaddingEffect : PlatformEffect
    {
        Thickness _padding;
        protected override void OnAttached()
        {
            if (Element is Layout element)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    _padding = element.Padding;
                    var insets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets; // Can't use KeyWindow this early
                    if (insets.Top > 0) // We have a notch
                    {
                        element.Padding = new Thickness(_padding.Left + insets.Left, _padding.Top + insets.Top, _padding.Right + insets.Right, _padding.Bottom);
                        return;
                    }
                }
                // Uses a default Padding of 20. Could use an property to modify if you wanted.
                element.Padding = new Thickness(_padding.Left, _padding.Top + 20, _padding.Right, _padding.Bottom);
            }
        }

        protected override void OnDetached()
        {
            if (Element is Layout element)
            {
                element.Padding = _padding;
            }
        }
    }
}
