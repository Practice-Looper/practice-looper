// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using System.Threading;
using Emka3.PracticeLooper.Model.Common;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

namespace Emka.PracticeLooper.Mobile.UITests.Tests.MainPage
{
    [TestFixture]
    public class AndroidPremiumTests : BasePage<AndroidDriver<AndroidElement>, AndroidElement>
    {
        private InAppPurchaseProduct product;

        public AndroidPremiumTests() : base()
        {
            product = new InAppPurchaseProduct
            {
                Name = "Practice Looper App Premium (Practice Looper ∞ Create & repeat Spotify loops ♫)",
                Purchased = true,
                LocalizedPrice = "",
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
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.App, "/Users/simonsymhoven/Projects/practice-looper/Mobile/Emka.PracticeLooper.Mobile.Android/bin/DebugPremium/de.emka3.practice_looper-Signed.apk");
            appiumOptions.AddAdditionalCapability(AndroidMobileCapabilityType.AppActivity, "crc64106a7de43e0e5d6c.MainActivity");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.FullReset, true);
            appiumOptions.AddAdditionalCapability(AndroidMobileCapabilityType.AutoGrantPermissions, true);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.NewCommandTimeout, 300);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.AutomationName, "UIAutomator2");
            appiumOptions.AddAdditionalCapability("systemPort", 8211);
        }

        [Test]
        public void When_StartApp_Expect_AddLoopButtonIsVisible()
        {
            Assert.IsTrue(IsLoopButtonVisible());
        }

        [Test]
        public void When_SwitchToDarkTheme_Expect_ThemeIsSaved()
        {
            OpenSettings();
            EnableDarkmode();
            NavigateBack();
            OpenSettings();
            Assert.IsTrue(IsDarkModeEnabled());
        }

        [Test]
        public void When_SwitchToLightTheme_Expect_ThemeIsSaved()
        {
            OpenSettings();
            EnableDarkmode();
            NavigateBack();
            OpenSettings();
            DisableDarkmode();
            NavigateBack();
            OpenSettings();
            Assert.IsFalse(IsDarkModeEnabled());
        }

        [Test]
        public void When_FetchingProduct_Expect_PremiumProductIsAvailable()
        {
            OpenSettings();
            var premiumProduct = GetProduct();
            Assert.AreEqual(product.Name, premiumProduct.Name);
            Assert.AreEqual(product.LocalizedPrice, premiumProduct.LocalizedPrice);
            Assert.AreEqual(product.Purchased, premiumProduct.Purchased);
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
        public void When_AddNewSession_Expect_SessionLoopButtonIsVisible()
        {
            OpenSpotifySearchPage();
            SearchSong("Mirage Coumarin");
            SelectSong(0);
            Assert.IsTrue(IsSessionLoopsButtonVisible("Mirage"));
        }

        [Test]
        public void When_AddNewSessionWithExistingSession_Expect_BothSessionsAreVisible()
        {
            OpenSpotifySearchPage();
            SearchSong("Nothing Else Matters");
            SelectSong(0);
            OpenSpotifySearchPage();
            SearchSong("Mirage Coumarin");
            SelectSong(0);
            Assert.IsTrue(IsSongInSessionList("Nothing Else Matters"));
            Assert.IsTrue(IsSongInSessionList("Mirage"));
        }

        [Test]
        public void When_DeletingSession_Expect_OtherIsStillVisible()
        {
            OpenSpotifySearchPage();
            SearchSong("Nothing Else Matters");
            SelectSong(0);
            OpenSpotifySearchPage();
            SearchSong("Mirage Coumarin");
            SelectSong(0);
            DeleteSong("Nothing Else Matters");
            Assert.IsFalse(IsSongInSessionList("Nothing Else Matters"));
            Assert.IsTrue(IsSongInSessionList("Mirage"));
        }

        [Test]
        public void When_SaveNewLoop_Expect_LoopIsAvailable()
        {
            OpenSpotifySearchPage();
            SearchSong("Mirage");
            SelectSong(0);
            SetSlider(20, 90);
            CreateLoop("My First Loop");
            SaveLoop();
            OpenLoops("Mirage");

            var defaultLoop = GetLoop("Mirage");
            Assert.AreEqual(defaultLoop.Name, "Mirage");

            var loop = GetLoop("My First Loop");
            Assert.AreEqual(loop.Name, "My First Loop");
        }

        [Test]
        public void When_CancelNewLoop_Expect_LoopIsNotAvailable()
        {
            OpenSpotifySearchPage();
            SearchSong("Mirage");
            SelectSong(0);
            SetSlider(20, 90);
            CreateLoop("My First Loop");
            CancelLoop();
            OpenLoops("Mirage");
            Assert.Throws<ArgumentOutOfRangeException>(() => GetLoop("My First Loop"));
        }

        [Test]
        public void When_AddTwoNewLoopsAndDeleteOne_Expect_OnlyOneIsVisible()
        {
            OpenSpotifySearchPage();
            SearchSong("Mirage");
            SelectSong(0);
            SetSlider(20, 90);
            CreateLoop("My First Loop");
            SaveLoop();
            SetSlider(70, 140);
            CreateLoop("Outro");
            SaveLoop();
            OpenLoops("Mirage");

            var loop1 = GetLoop("My First Loop");
            Assert.AreEqual(loop1.Name, "My First Loop");

            var loop2 = GetLoop("Outro");
            Assert.AreEqual(loop2.Name, "Outro");

            DeleteLoop("My First Loop");
            Assert.Throws<ArgumentOutOfRangeException>(() => GetLoop("My First Loop"));
        }

        [Test]
        public void When_SwitchingLoops_Expect_BoundsAreInitializedCorrectly()
        {
            OpenSpotifySearchPage();
            SearchSong("Mirage");
            SelectSong(0);
            SetSlider(20, 90);
            CreateLoop("My First Loop");
            SaveLoop();
            OpenLoops("Mirage");

            var defaultLoop = SelectLoop("Mirage");
            Assert.AreEqual(defaultLoop.StartPosition, GetLoopStartPosition());
            Assert.AreEqual(defaultLoop.StartPosition, GetCurrentSongTime());
            Assert.AreEqual(defaultLoop.EndPosition, GetLoopEndPosition());
            Assert.AreEqual(defaultLoop.EndPosition, GetSongDuration());

            OpenLoops("Mirage");
            var loop = SelectLoop("My First Loop");
            Assert.AreEqual(loop.StartPosition, GetLoopStartPosition());
            Assert.AreEqual(loop.EndPosition, GetLoopEndPosition());
        }

        [Test]
        public void When_SwitchingSessions_Expect_BoundsAreInitializedCorrectly()
        {
            OpenSpotifySearchPage();
            SearchSong("Nothing Else Matters");
            SelectSong(0);
            OpenSpotifySearchPage();
            SearchSong("Mirage Coumarin");
            SelectSong(0);

            OpenLoops("Nothing Else Matters");
            var loop1 = GetLoop("Nothing Else Matters");
            NavigateBack();
            SelectSong("Nothing Else Matters");
            Assert.AreEqual(loop1.StartPosition, GetLoopStartPosition());
            Assert.AreEqual(loop1.StartPosition, GetCurrentSongTime());
            Assert.AreEqual(loop1.EndPosition, GetLoopEndPosition());
            Assert.AreEqual(loop1.EndPosition, GetSongDuration());

            OpenLoops("Mirage");
            var loop2 = GetLoop("Mirage");
            NavigateBack();
            SelectSong("Mirage");
            Assert.AreEqual(loop2.StartPosition, GetLoopStartPosition());
            Assert.AreEqual(loop2.StartPosition, GetCurrentSongTime());
            Assert.AreEqual(loop2.EndPosition, GetLoopEndPosition());
            Assert.AreEqual(loop2.EndPosition, GetSongDuration());
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
        public void When_PickerSetLeftFirstWithToSmallLoop_Expect_BoundsAreInitializedCorrectly(string name, int start, int end)
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
        public void When_PickerSetRightFirstWithToSmallLoop_Expect_BoundsAreInitializedCorrectly(string name, int start, int end)
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

        [Test]
        [TestCase("Nothing Else Matters", 146, 177)]
        [TestCase("Coumarin Mirage", 88, 97)]
        public void When_TriggerPlayPlause_Expect_LoopStartPositionIsInititalizedCorrectly(string name, int start, int end)
        {
            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetLeftPicker(start);
            SetRightPicker(end);

            var markersPlay = new List<double>();

            var markersStopBefore = new List<double>();
            var markersStop = new List<double>();

            for (int i = 0; i < 20; i++)
            {
                Play();
                Thread.Sleep(2000);
                markersPlay.Add(GetCurrentSongTime());
                
                Stop();
                markersStopBefore.Add(GetCurrentSongTime());
                Thread.Sleep(2000);
                markersStop.Add(GetCurrentSongTime());

            }

            var markersPlayCorrect = markersPlay.FindAll(m => (m <= end + 1 && m >= start - 1));
            Assert.AreEqual(markersPlay, markersPlayCorrect);
            Assert.AreEqual(markersStopBefore, markersStop);
        }
    }
}
