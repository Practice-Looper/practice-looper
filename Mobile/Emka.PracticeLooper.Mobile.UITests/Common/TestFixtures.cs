// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System.Globalization;
using System.Threading;
using Emka.PracticeLooper.Mobile.UITests.Pages;
using NUnit.Framework;
using Xamarin.UITest;

namespace Emka.PracticeLooper.Mobile.UITests.Common
{
    public class TestFixtures
    {
        // [TestFixture(Platform.iOS)]
        [TestFixture(Platform.Android)]
        public abstract class BaseTestFixture
        {
            protected IApp app => AppInitializer.App;
            protected bool OnAndroid => AppInitializer.Platform == Platform.Android;
            protected bool OniOS => AppInitializer.Platform == Platform.iOS;

            protected BaseTestFixture(Platform platform)
            {
                AppInitializer.Platform = platform;
            }

            [SetUp]
            public virtual void BeforeEachTest()
            {
                AppInitializer.StartApp();
                var culture = app.Invoke("GetLanguage");
                var cultureID = "en-US";
                switch (culture.ToString())
                {
                    case "de":
                        cultureID = "de-DE";
                        break;
                    default:
                        break;
                }

                Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureID);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureID);
                OnboardingPage onboardingPage = new OnboardingPage();
                onboardingPage.SkipTutorial();
            }

        }
    }

}
