// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using Emka.PracticeLooper.Mobile.UITests.Common;

using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace Emka.PracticeLooper.Mobile.UITests.Pages
{
    public class OnboardingPage: BasePage
    {
        readonly Query SkipTutorialButton;

        protected override PlatformQuery Trait => new PlatformQuery()
        {
            Android = x => x.Marked("SkipTutorialButton"),
            iOS = x => x.Marked("SkipTutorialButton")
        };

        public OnboardingPage()
        {
            SkipTutorialButton = x => x.Marked("SkipTutorialButton");
        }

        public void SkipTutorial()
        {
            app.Tap(SkipTutorialButton);
            app.WaitForNoElement(SkipTutorialButton);
        }
        
    }
}
