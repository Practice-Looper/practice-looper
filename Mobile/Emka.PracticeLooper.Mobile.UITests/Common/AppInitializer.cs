// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using Xamarin.UITest;

namespace Emka.PracticeLooper.Mobile.UITests
{
    public class AppInitializer
    {
        public static bool IsLite { get; set; }

        static IApp app;
        public static IApp App
        {
            get
            {
                if (app == null)
                    throw new NullReferenceException("'AppManager.App' not set. Call 'AppManager.StartApp()' before trying to access it.");
                return app;
            }
        }

        static Platform? platform;
        public static Platform Platform
        {
            get
            {
                if (platform == null)
                    throw new NullReferenceException("'AppManager.Platform' not set.");
                return platform.Value;
            }

            set
            {
                platform = value;
            }
        }

        public static void StartApp()
        {
             
            if (platform == Platform.Android)
            {
#if PREMIUM 
                app = ConfigureApp
                        .Android
                        .KeyStore("/Users/developer/PlayConsoleAlpha.keystore", "8ExDbiTtXNN3ECugopuB", "8ExDbiTtXNN3ECugopuB", "PlayConsoleAlpha")
                        .ApkFile("../../../../Mobile/Emka.PracticeLooper.Mobile.Android/bin/DebugPremium/de.emka3.practice_looper-Signed.apk")
                        .StartApp();
                IsLite = false;
#else
                app = ConfigureApp
                        .Android
                        .KeyStore("/Users/developer/PlayConsoleAlpha.keystore", "8ExDbiTtXNN3ECugopuB", "8ExDbiTtXNN3ECugopuB", "PlayConsoleAlpha")
                        .ApkFile("../../../../Mobile/Emka.PracticeLooper.Mobile.Android/bin/DebugLite/de.emka3.practice_looper-Signed.apk")
                        .StartApp();
                IsLite = true;
#endif
            }

            if (platform == Platform.iOS)
            {
#if PREMIUM
                app = ConfigureApp
                    .iOS
                    .Debug()
                    .InstalledApp("de.emka3.practice-looper.loopr")
                    .StartApp();
                IsLite = false;
#else
                app = ConfigureApp
                    .iOS
                    .Debug()
                    .InstalledApp("de.emka3.practice-looper.loopr")
                    .StartApp();
                IsLite = true;
#endif
                
            }
        }
    }
}
