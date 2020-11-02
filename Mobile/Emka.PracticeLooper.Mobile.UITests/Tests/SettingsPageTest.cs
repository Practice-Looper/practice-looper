// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using System.Text;
using Emka.PracticeLooper.Mobile.UITests.Pages;
using Emka3.PracticeLooper.Model.Common;
using NUnit.Framework;
using Xamarin.UITest;
using static Emka.PracticeLooper.Mobile.UITests.Common.TestFixtures;

namespace Emka.PracticeLooper.Mobile.UITests.Tests
{
    public class SettingsPageTest : BaseTestFixture
    {
        public SettingsPageTest(Platform platform)
            : base(platform)
        {

        }

        [Test]
        public void When_FetchingProducts_Then_InAppItemIsAvailable()
        {
            new MainPage()
                .GoToSettings();

            List<InAppPurchaseProduct> items = new SettingsPage()
                .GetInAppPurchaseProducts();

            var product = new InAppPurchaseProduct();
            if (OnAndroid)
            {
                product.Name = "Practice Looper App Premium (Practice Looper App)";
                product.LocalizedPrice = "€ 5,99";
            }
            else
            {
                product.Name = "Practice Looper App Premium";
                product.LocalizedPrice = "€ 6,99";
            }

            Assert.AreEqual(items.Count, 1);

            Assert.AreEqual(items[0].Name, product.Name);
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
                // Assert.AreEqual(items[0].LocalizedPrice, product.LocalizedPrice);
            }
            else
            {
                byte[] checkmark_bytes = Encoding.Unicode.GetBytes(items[0].LocalizedPrice);
                var checkmark_hex = BitConverter.ToString(checkmark_bytes);
                Assert.AreEqual(checkmark_hex, "2C-F1");
            }
            
           
        }

        [Test]
        public void When_SwitchingTheme_ThemeIsSaved()
        {
            MainPage mainPage = new MainPage();
            mainPage.GoToSettings();

            SettingsPage settingsPage = new SettingsPage();
            settingsPage.EnableDarkMode();

            app.Back();

            mainPage.GoToSettings();
            
            Assert.IsTrue(settingsPage.IsDarkModeEnabled());
        }
    }
}
