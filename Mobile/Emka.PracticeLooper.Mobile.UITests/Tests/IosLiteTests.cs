// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using Emka3.PracticeLooper.Model.Common;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;

namespace Emka.PracticeLooper.Mobile.UITests.Tests.MainPage
{
    [TestFixture]
    public class IosLiteTests : BasePage<IOSDriver<IOSElement>, IOSElement>
    {
        private InAppPurchaseProduct product;

        public IosLiteTests() : base()
        {
            product = new InAppPurchaseProduct
            {
                Name = "Practice Looper App Premium",
                Purchased = false,
                LocalizedPrice = "7,99",
            };
        }

        protected override IOSDriver<IOSElement> GetDriver()
        {
            return new IOSDriver<IOSElement>(driverUri, options);
        }

        protected override void InitAppiumOptions(AppiumOptions appiumOptions)
        {
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.DeviceName, "iPhone XR");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "iOS");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformVersion, "14.2");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.Udid, "00008020-00054DEE3A88003A");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.App, "/Users/simonsymhoven/Projects/practice-looper/Mobile/Emka.PracticeLooper.Mobile.iOS/bin/iPhone/DebugLite/device-builds/iphone11.8-14.2/Emka.PracticeLooper.Mobile.iOS.ipa");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.NewCommandTimeout, 300);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.AutomationName, "XCUITest");
            appiumOptions.AddAdditionalCapability("wdaLocalPort", 8212);
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
            LaunchApp();
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
            SearchSong("Mirage");
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
            SearchSong("Mirage");
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

            Assert.That(GetLoopStartPosition(), Is.EqualTo(left).Within(25));
            Assert.That(GetLoopEndPosition(), Is.EqualTo(right).Within(25));
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
        public void When_LoopingSong_Expect_LoopIsInBounds(string name, double start, double end, int loops)
        {

            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetLeftPicker(start);
            SetRightPicker(end);

            var loopLength = GetLoopEndPosition() - GetLoopStartPosition();

            var markers = new List<double>();
            DateTime actual = DateTime.Now;

            Play();

            while ((DateTime.Now - actual).TotalSeconds < loopLength * loops)
            {
                var page = driver.PageSource;
                markers.Add(GetCurrentSongTime());
            }

            Stop();

            var markersCorrect = markers.FindAll(m => (m <= end && m >= start));
            Assert.AreEqual(markers, markersCorrect);
        }
    }
}
