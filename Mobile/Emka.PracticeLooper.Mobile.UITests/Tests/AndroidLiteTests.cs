// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using Emka3.PracticeLooper.Model.Common;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

namespace Emka.PracticeLooper.Mobile.UITests.Tests.MainPage
{
    [TestFixture]
    public class AndroidLiteTests : BasePage<AndroidDriver<AndroidElement>, AndroidElement>
    {
        private InAppPurchaseProduct product;

        public AndroidLiteTests() : base() {
            product = new InAppPurchaseProduct
            {
                Name = "Practice Looper App Premium (Practice Looper App)",
                Purchased = false,
                LocalizedPrice = "5,99",
            };
        }


        protected override AndroidDriver<AndroidElement> GetDriver()
        {
            return new AndroidDriver<AndroidElement>(driverUri, options);
        }

        protected override void InitAppiumOptions(AppiumOptions appiumOptions)
        {
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.DeviceName, "Samsung S20");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.Udid, "RF8N20AKN5A");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "Android");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.App, "/Users/simonsymhoven/Projects/practice-looper/Mobile/Emka.PracticeLooper.Mobile.Android/bin/DebugLite/de.emka3.practice_looper-Signed.apk");
            appiumOptions.AddAdditionalCapability(AndroidMobileCapabilityType.AppActivity, "crc64106a7de43e0e5d6c.MainActivity");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.FullReset, true);
            appiumOptions.AddAdditionalCapability(AndroidMobileCapabilityType.AutoGrantPermissions, true);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.NewCommandTimeout, 300);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.AutomationName, "UIAutomator2");
            appiumOptions.AddAdditionalCapability("systemPort", 8210);
        }

        [Test]
        public void When_StartApp_Expect_AddLoopButtonIsNotVisible()
        {
            Assert.Throws<OpenQA.Selenium.NoSuchElementException>(() => IsLoopButtonVisible());
        }

        [Test]
        public void When_FetchingProduct_Expect_LiteProductIsAvailable()
        {
            OpenSettings();
            var liteProduct = GetProduct();
            Assert.AreEqual(product.Name, liteProduct.Name);
            Assert.IsTrue(liteProduct.LocalizedPrice.Contains(product.LocalizedPrice));
            Assert.AreEqual(product.Purchased, liteProduct.Purchased);
        }

        [Test]
        public void When_SpotifyIsNotInstalled_Expect_InstallationIsRequired()
        {
            DeinstallSpotify();
            OpenSpotifySearchPage();
            AcceptSpotifyInstallation();
            LoginToSpotify();
            Assert.IsTrue(IsSpotifySearchPageVisible());
        }

        [Test]
        public void When_SearchForNonsense_Expect_NoSongIsFound()
        {
            OpenSpotifySearchPage();
            SearchSong("Zackele Dudeli");
            Assert.Throws<ArgumentOutOfRangeException>(() => SelectSong(0));
        }

        [Test]
        public void When_AddNewSession_Expect_SessionLoopButtonIsNotVisible()
        {
            OpenSpotifySearchPage();
            SearchSong("Mirage Coumarin");
            SelectSong(0);
            Assert.IsFalse(IsSessionLoopsButtonVisible("Mirage"));
        }

        [Test]
        public void When_AddNewSessionWithExistingSession_Expect_OldSessionOverwritten()
        {
            OpenSpotifySearchPage();
            SearchSong("Nothing Else Matters");
            SelectSong(0);
            OpenSpotifySearchPage();
            SearchSong("Mirage Coumarin");
            SelectSong(0);
            Assert.IsFalse(IsSongInSessionList("Nothing Else Matters"));
            Assert.IsTrue(IsSongInSessionList("Mirage"));
        }

        [Test]
        public void When_DeletingSession_Expect_NoSessionIsVisible()
        {
            OpenSpotifySearchPage();
            SearchSong("Nothing Else Matters");
            SelectSong(0);
            DeleteSong("Nothing Else Matters");
            Assert.IsFalse(IsSongInSessionList("Nothing Else Matters"));
        }

        [Test]
        [TestCase("Coumarin Mirage", 47, 90)]
        [TestCase("Coumarin Mirage", 52, 200)]
        [TestCase("Coumarin Mirage", 160, 167)]
        [TestCase("Nothing Else Matters", 77, 90)]
        [TestCase("Nothing Else Matters", 330, 350)]
        [TestCase("Nothing Else Matters", 240, 260)]
        [TestCase("Nothing Else Matters", 100, 105)]
        public void When_SliderSet_Expect_BoundsAreInitializedCorrectly(string name, int left, int right)
        {
            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetSlider(left, right);

            Assert.That(GetLoopStartPosition(), Is.EqualTo(left).Within(1));
            Assert.That(GetLoopEndPosition(), Is.EqualTo(right).Within(1));
        }

        [Test]
        [TestCase("Coumarin Mirage", 61, 90)]
        [TestCase("Coumarin Mirage", 52, 200)]
        [TestCase("Nothing Else Matters", 240, 260)]
        [TestCase("Nothing Else Matters", 100, 105)]
        public void When_PickerSetLeftFirst_Expect_BoundsAreInitializedCorrectly(string name, int start, int end)
        {
            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetLeftPicker(start);
            SetRightPicker(end);

            Assert.That(GetLoopStartPosition(), Is.EqualTo(start));
            Assert.That(GetLoopEndPosition(), Is.EqualTo(end));
        }

        [Test]
        [TestCase("Coumarin Mirage", 61, 90)]
        [TestCase("Coumarin Mirage", 52, 200)]
        [TestCase("Nothing Else Matters", 240, 260)]
        [TestCase("Nothing Else Matters", 100, 105)]
        public void When_PickerSetRightFirst_Expect_BoundsAreInitializedCorrectly(string name, int start, int end)
        {
            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetRightPicker(end);
            SetLeftPicker(start);

            Assert.That(GetLoopStartPosition(), Is.EqualTo(start));
            Assert.That(GetLoopEndPosition(), Is.EqualTo(end));
        }

        [Test]
        [TestCase("Coumarin Mirage", 70, 74)]
        [TestCase("Coumarin Mirage", 99, 102)]
        [TestCase("Coumarin Mirage", 15, 17)]
        public void When_PickerSetLeftFirst_WithToSmallLoop_Expect_BoundsAreInitializedCorrectly(string name, int start, int end)
        {
            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetLeftPicker(start);
            SetRightPicker(end);

            Assert.That(GetLoopStartPosition(), Is.EqualTo(start));
            Assert.That(GetLoopEndPosition(), Is.EqualTo(start + 5));
        }

        [Test]
        [TestCase("Coumarin Mirage", 70, 74)]
        [TestCase("Coumarin Mirage", 99, 102)]
        [TestCase("Coumarin Mirage", 15, 17)]
        public void When_PickerSetRightFirst_WithToSmallLoop_Expect_BoundsAreInitializedCorrectly(string name, int start, int end)
        {
            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetRightPicker(end);
            SetLeftPicker(start);


            Assert.That(GetLoopStartPosition(), Is.EqualTo(end - 5));
            Assert.That(GetLoopEndPosition(), Is.EqualTo(end));
        }

        [Test]
        [TestCase("Coumarin Mirage", 88, 97, 5)]
        [TestCase("Nothing Else Matters", 146, 177, 5)]
        [TestCase("Schüsse in die Luft", 56, 126, 5)]
        [TestCase("Nothing Else Matters", 100, 105, 5)]
        public void When_LoopingSong_Expect_LoopIsInBounds(string name, double leftSlider, double rightSlider, int loops)
        {

            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetSlider(leftSlider, rightSlider);

            var loopLength = GetLoopEndPosition() - GetLoopStartPosition();

            var markers = new List<double>();
            DateTime start = DateTime.Now;

            Play();

            while ((DateTime.Now - start).TotalSeconds < loopLength * loops)
            {
                markers.Add(GetCurrentSongTime());
            }

            Stop();

            var markersCorrect = markers.FindAll(m => (m <= rightSlider + 1 && m >= leftSlider - 1));
            Assert.AreEqual(markers, markersCorrect);
        }

    }
}
