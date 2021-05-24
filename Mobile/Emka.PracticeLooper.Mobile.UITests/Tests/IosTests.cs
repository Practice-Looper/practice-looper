﻿// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;

namespace Emka.PracticeLooper.Mobile.UITests.Tests.MainPage
{
    [TestFixture]
    public class IosTests : BasePage<IOSDriver<IOSElement>, IOSElement>
    {
        
        public IosTests() : base()
        {

        }

        protected override IOSDriver<IOSElement> GetDriver()
        {
            return new IOSDriver<IOSElement>(driverUri, options);
        }

        protected override void InitAppiumOptions(AppiumOptions appiumOptions)
        {
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "iOS");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformVersion, "14.2");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.DeviceName, "iPhone XR");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.Udid, "00008020-00054DEE3A88003A");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.App, "/Users/simonsymhoven/Projects/practice-looper/Mobile/Emka.PracticeLooper.Mobile.iOS/bin/iPhone/DebugPremium/device-builds/iphone11.8-14.2/Emka.PracticeLooper.Mobile.iOS.ipa"); appiumOptions.AddAdditionalCapability(MobileCapabilityType.NewCommandTimeout, 300);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.AutomationName, "XCUITest");
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
            SearchSong("Mirage Coumarin");
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
            SearchSong("Mirage Coumarin");
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
            SearchSong("Mirage Coumarin");
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
            SearchSong("Mirage Coumarin");
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

        [Test]
        [TestCase("Nothing Else Matters", 146, 177)]
        [TestCase("Coumarin Mirage", 88, 97)]
        [TestCase("Coumarin Mirage", 20, 25)]
        [TestCase("Coumarin Mirage", 10, 100)]
        [TestCase("Coumarin Mirage", 100, 140)]
        public void When_TriggerPlayPlause_Expect_LoopStartPositionIsInititalizedCorrectly(string name, int start, int end)
        {
            OpenSpotifySearchPage();
            SearchSong(name);
            SelectSong(0);

            SetLeftPicker(start);
            SetRightPicker(end);

            var markersPlayBefore = new List<double>();
            var markersPlay = new List<double>();

            var markersStopBefore = new List<double>();
            var markersStop = new List<double>();

            for (int i = 0; i < 20; i++)
            {
                Play();
                markersPlayBefore.Add(GetCurrentSongTime());
                Thread.Sleep(2000);
                markersPlay.Add(GetCurrentSongTime());

                Stop();
                markersStopBefore.Add(GetCurrentSongTime());
                Thread.Sleep(2000);
                markersStop.Add(GetCurrentSongTime());

            }

            var markersPlayCorrect = markersPlay.FindAll(m => (m <= end + 1 && m >= start - 1));
            Assert.AreEqual(markersPlay, markersPlayCorrect);

            Assert.AreNotEqual(markersPlayBefore, markersPlay);
            Assert.AreEqual(markersStopBefore, markersStop);
        }

        [Test]
        public void When_DoEverything_Expect_AppIsStable()
        {
            OpenSpotifySearchPage();
            SearchSong("Coumarin Mirage");

            SelectSong(0);

            SetLeftPicker(88);
            SetRightPicker(100);

            Assert.AreEqual(88, GetLoopStartPosition());
            Assert.AreEqual(100, GetLoopEndPosition());

            CreateLoop("First Loop");
            SaveLoop();

            Play();

            Thread.Sleep(10000);

            Stop();

            SetLeftPicker(28);
            SetRightPicker(120);

            Assert.AreEqual(28, GetLoopStartPosition());
            Assert.AreEqual(120, GetLoopEndPosition());

            CreateLoop("Second Loop");
            SaveLoop();

            OpenLoops("Mirage");

            SelectLoop("First Loop");
            NavigateBack();

            Assert.AreEqual(88, GetLoopStartPosition());
            Assert.AreEqual(100, GetLoopEndPosition());

            Play();

            Thread.Sleep(20000);

            Stop();

        }
    }
}