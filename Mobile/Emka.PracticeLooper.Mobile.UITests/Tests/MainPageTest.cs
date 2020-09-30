// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using Emka.PracticeLooper.Mobile.UITests.Pages;
using Emka3.PracticeLooper.Model.Player;
using NUnit.Framework;
using Xamarin.UITest;
using static Emka.PracticeLooper.Mobile.UITests.Common.TestFixtures;

namespace Emka.PracticeLooper.Mobile.UITests.Tests
{
    public class MainPageTest : BaseTestFixture
    {
        public MainPageTest(Platform platform)
            : base(platform)
        {

        }

        [Test]
        [TestCase("Nothing Else Matters")]
        public void When_AddNewSong_Then_LoopMarkersInitializedCorrectly(string name) {

            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            new SpotifySearchPage()
               .SearchSong(name)
               .SelectSong(0);

            var loopStartPosition = mainPage.GetLoopStartPosition();
            var loopEndPosition = mainPage.GetLoopEndPosition();

            var currentSongTime = mainPage.GetCurrentSongTime();
            var songDuration = mainPage.GetSongDuration();

            Assert.AreEqual(loopStartPosition, currentSongTime);
            Assert.AreEqual(loopEndPosition, songDuration);
        }

        [Test]
        [TestCase("Nothing Else Matters", "My Loop", 30, 80)]
        public void When_AddNewBookmark_Then_LoopsContainsBookmarkAndDefaultBookmark(string name, string bookmark, int lower, int upper)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            var song = new SpotifySearchPage()
               .SearchSong(name)
               .SelectSong(0);

            var currentSongTime = mainPage.GetCurrentSongTimeAsDouble();
            var songDuration = mainPage.GetSongDurationAsDouble();

            var mainLoop = new Loop()
            {
                Name = song,
                StartPosition = currentSongTime,
                EndPosition = songDuration
            };

            mainPage.SetSlider(lower, upper);

            var loopStartPosition = mainPage.GetLoopStartPositionAsDouble();
            var loopEndPosition = mainPage.GetLoopEndPositionAsDouble();

            var bookmarkLoop = new Loop()
            {
                Name = bookmark,
                StartPosition = loopStartPosition,
                EndPosition = loopEndPosition
            };

            mainPage
                .AddBookmark(bookmark)
                .SaveBookmark();

            mainPage.OpenBookmarks(song);

            SessionDetailPage sessionDetailPage = new SessionDetailPage();

            var labels = new List<string>();
            labels.Add(song);
            labels.Add(bookmark);

            var Loops = sessionDetailPage.GetLoops(labels);
 
            Assert.AreEqual(Loops.Count, 2);
            Assert.IsTrue(Loops.Contains(mainLoop));
            Assert.IsTrue(Loops.Contains(bookmarkLoop));
        }

        [Test]
        [TestCase("Nothing Else Matters", "My Loop To Cancel")]
        public void When_CancleNewBookmark_Then_LoopsContainsOnlyDefaultBookmark(string name, string bookmark)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            var song = new SpotifySearchPage()
               .SearchSong(name)
               .SelectSong(0);

            var currentSongTime = mainPage.GetCurrentSongTimeAsDouble();
            var songDuration = mainPage.GetSongDurationAsDouble();

            var mainLoop = new Loop()
            {
                Name = song,
                StartPosition = currentSongTime,
                EndPosition = songDuration
            };

            var loopStartPosition = mainPage.GetLoopStartPositionAsDouble();
            var loopEndPosition = mainPage.GetLoopEndPositionAsDouble();

            var bookmarkLoop = new Loop()
            {
                Name = bookmark,
                StartPosition = loopStartPosition,
                EndPosition = loopEndPosition
            };

            mainPage
                .AddBookmark(bookmark)
                .CancelBookmark();

            mainPage.OpenBookmarks(song);

            SessionDetailPage sessionDetailPage = new SessionDetailPage();

            var labels = new List<string>() { song, bookmark };

            var Loops = sessionDetailPage.GetLoops(labels);

            Assert.AreEqual(Loops.Count, 1);
            Assert.IsTrue(Loops.Contains(mainLoop));
            Assert.IsFalse(Loops.Contains(bookmarkLoop));
        }

        [Test]
        [TestCase("Nothing Else Matters", "My Loop")]
        public void When_SwitchingBookmark_Then_LoopMarkersAreInitializedCorrectly(string name, string bookmark)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            var song = new SpotifySearchPage()
               .SearchSong(name)
               .SelectSong(0);

            mainPage.SetSlider(40, 90);

            mainPage
                .AddBookmark(bookmark)
                .SaveBookmark();

            mainPage.OpenBookmarks(song);

            SessionDetailPage sessionDetailPage = new SessionDetailPage();

            var labels = new List<string>() { song, bookmark };
            var Loops = sessionDetailPage.GetLoops(labels);

            sessionDetailPage.SelectLoop(labels[0]);
            
            Assert.AreEqual(mainPage.GetLoopStartPositionAsDouble(), Loops[0].StartPosition);
            Assert.AreEqual(mainPage.GetLoopEndPositionAsDouble(), Loops[0].EndPosition);

            mainPage.OpenBookmarks(song);
            sessionDetailPage.SelectLoop(labels[1]);
            Assert.AreEqual(mainPage.GetLoopStartPositionAsDouble(), Loops[1].StartPosition);
            Assert.AreEqual(mainPage.GetLoopEndPositionAsDouble(), Loops[1].EndPosition);
        }

        
        [Test]
        [TestCase("Coumarin Mirage", 88, 97, 5)]
        [TestCase("Nothing Else Matters", 146, 177, 5)]
        [TestCase("Schüsse in die Luft", 56, 126, 5)]
        [TestCase("Nothing Else Matters", 100, 105, 5)]
        public void When_LoopingSong_Then_LoopMarkersAreCorrectly(string name, double leftSlider, double rightSlider, int loops) {

            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            new SpotifySearchPage()
               .SearchSong(name)
               .SelectSong(0);

            mainPage.SetSlider(leftSlider, rightSlider);
            var loopLength = mainPage.GetLoopEndPositionAsDouble() - mainPage.GetLoopStartPositionAsDouble();

            var markers = new List<double>();
            DateTime start = DateTime.Now;
            mainPage.Play();
            while ((DateTime.Now - start).TotalSeconds < loopLength * loops)
            {
                markers.Add(mainPage.GetCurrentSongTimeAsDouble());
            }
                
            mainPage.Stop();
            var markersCorrect = markers.FindAll(m => (m <= rightSlider && m >= leftSlider));

            Assert.AreEqual(markers, markersCorrect);
        }

        [Test]
        [TestCase("Nothing Else Matters", "Coumarin Mirage")]
        public void When_SwitchingSessions_Then_LoopMarkersAreInitializedCorrectly(string name1, string name2)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage();
            mainPage.AddNewSpotifiySession();

            var firstSong = new SpotifySearchPage()
               .SearchSong(name1)
               .SelectSong(0);

           
            mainPage.AddNewSpotifiySession();
            var secondSong = new SpotifySearchPage()
                .SearchSong(name2)
                .SelectSong(0);

            var secondSongLoopStartPosition = mainPage.GetLoopStartPositionAsDouble();
            var secondSongLoopEndPosition = mainPage.GetLoopEndPositionAsDouble();
            var secondSongCurrentSongTime = mainPage.GetCurrentSongTimeAsDouble();
            var secondSongSongDuration = mainPage.GetSongDurationAsDouble();

            mainPage.SelectSession(firstSong);
            var firstSongLoopStartPosition = mainPage.GetLoopStartPositionAsDouble();
            var firstSongLoopEndPosition = mainPage.GetLoopEndPositionAsDouble();
            var firstSongCurrentSongTime = mainPage.GetCurrentSongTimeAsDouble();
            var firstSongSongDuration = mainPage.GetSongDurationAsDouble();


            Assert.AreEqual(firstSongLoopStartPosition, firstSongCurrentSongTime);
            Assert.AreEqual(firstSongLoopEndPosition, firstSongSongDuration);
            Assert.AreEqual(secondSongLoopStartPosition, secondSongCurrentSongTime);
            Assert.AreEqual(secondSongLoopEndPosition, secondSongSongDuration);
        }

        [Test]
        public void When_AppIsInitialized_Then_AdmobBannerIsVisible()
        {
            MainPage mainPage = new MainPage();

            if (AppInitializer.IsLite)
            {
                var isBannerVisible = mainPage.IsAdMobBannerVisible();
                Assert.IsTrue(isBannerVisible);
            }
            else
            {
                Assert.Throws<Exception>(() => mainPage.IsAdMobBannerVisible());
            }
        }

        [Test]
        public void When_AppIsInitialized_Then_AddBookmarkButtonIsVisible()
        {
            MainPage mainPage = new MainPage();

            if (AppInitializer.IsLite)
            {
                Assert.Throws<Exception>(() => mainPage.IsAddBookmarkButtonVisisble());
            }
            else
            {
                var isButtonVisible = mainPage.IsAddBookmarkButtonVisisble();
                Assert.IsTrue(isButtonVisible);
            }
        }

        [Test]
        [TestCase("Nothing Else Matters")]
        public void When_AppIsInitializedAndSongAdded_Then_OpenBookmarksButtonIsVisible(string name)
        {
            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            var song = new SpotifySearchPage()
              .SearchSong(name)
              .SelectSong(0);

            if (AppInitializer.IsLite)
            {
                var isButtonVisible = mainPage.IsOpenBookmarkButtonVisible(song);
                Assert.IsFalse(isButtonVisible);
            }
            else
            {
                var isButtonVisible = mainPage.IsOpenBookmarkButtonVisible(song);
                Assert.IsTrue(isButtonVisible);
            }
        }

        [Test]
        [TestCase("Amen Tom Grennan", 60, 70)]
        [TestCase("Coumarin Mirage", 47, 90)]
        [TestCase("Coumarin Mirage", 52, 200)]
        [TestCase("Coumarin Mirage", 160, 167)]
        [TestCase("Nothing Else Matters", 77, 90)]
        [TestCase("Nothing Else Matters", 330, 350)]
        [TestCase("Nothing Else Matters", 240, 260)]
        [TestCase("Nothing Else Matters", 100, 105)]
        public void When_PickerSet_LeftFirst_Then_LoopMarkersAreInitializedCorrectly(string name, int left, int right)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            new SpotifySearchPage()
              .SearchSong(name)
              .SelectSong(0);

            mainPage.SetLeftPicker(left);
            mainPage.SetRightPicker(right);

            Assert.That(mainPage.GetLoopStartPositionAsDouble(), Is.EqualTo(left).Within(1));
            Assert.That(mainPage.GetLoopEndPositionAsDouble(), Is.EqualTo(right).Within(1));
        }

        [Test]
        [TestCase("Amen Tom Grennan", 60, 70)]
        [TestCase("Coumarin Mirage", 47, 90)]
        [TestCase("Coumarin Mirage", 52, 200)]
        [TestCase("Coumarin Mirage", 160, 167)]
        [TestCase("Nothing Else Matters", 77, 90)]
        [TestCase("Nothing Else Matters", 330, 350)]
        [TestCase("Nothing Else Matters", 240, 260)]
        [TestCase("Nothing Else Matters", 100, 105)]
        public void When_PickerSet_RightFirst_Then_LoopMarkersAreInitializedCorrectly(string name, int left, int right)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            new SpotifySearchPage()
              .SearchSong(name)
              .SelectSong(0);

            mainPage.SetRightPicker(right);
            mainPage.SetLeftPicker(left);

            Assert.That(mainPage.GetLoopStartPositionAsDouble(), Is.EqualTo(left).Within(1));
            Assert.That(mainPage.GetLoopEndPositionAsDouble(), Is.EqualTo(right).Within(1));
        }

        [Test]
        [TestCase("Coumarin Mirage", 70, 74)]
        [TestCase("Coumarin Mirage", 99, 102)]
        [TestCase("Coumarin Mirage", 15, 17)]
        public void When_PickerSet_LeftFirst_WithToSmallLoop_Then_LoopMarkersAreInitializedCorrectly(string name, int left, int right)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            new SpotifySearchPage()
              .SearchSong(name)
              .SelectSong(0);

            mainPage.SetLeftPicker(left);
            mainPage.SetRightPicker(right);

            Assert.That(mainPage.GetLoopStartPositionAsDouble(), Is.EqualTo(left).Within(1));
            Assert.That(mainPage.GetLoopEndPositionAsDouble(), Is.EqualTo(left + 5));
        }

        [Test]
        [TestCase("Coumarin Mirage", 70, 74)]
        [TestCase("Coumarin Mirage", 99, 102)]
        [TestCase("Coumarin Mirage", 15, 17)]
        public void When_PickerSet_RightFirst_WithToSmallLoop_Then_LoopMarkersAreInitializedCorrectly(string name, int left, int right)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            new SpotifySearchPage()
              .SearchSong(name)
              .SelectSong(0);

            mainPage.SetRightPicker(right);
            mainPage.SetLeftPicker(left);
            
            Assert.That(mainPage.GetLoopStartPositionAsDouble(), Is.EqualTo(right - 5));
            Assert.That(mainPage.GetLoopEndPositionAsDouble(), Is.EqualTo(right).Within(1));
        }

        [Test]
        [TestCase("Coumarin Mirage", 47, 90)]
        [TestCase("Coumarin Mirage", 52, 200)]
        [TestCase("Coumarin Mirage", 160, 167)]
        [TestCase("Nothing Else Matters", 77, 90)]
        [TestCase("Nothing Else Matters", 330, 350)]
        [TestCase("Nothing Else Matters", 240, 260)]
        [TestCase("Nothing Else Matters", 100, 105)]
        public void When_SliderSet_Then_LoopMarkersAreInitializedCorrectly(string name, int left, int right)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            MainPage mainPage = new MainPage()
                .AddNewSpotifiySession();

            new SpotifySearchPage()
              .SearchSong(name)
              .SelectSong(0);

            mainPage.SetSlider(left, right);

            Assert.That(mainPage.GetLoopStartPositionAsDouble(), Is.EqualTo(left).Within(1));
            Assert.That(mainPage.GetLoopEndPositionAsDouble(), Is.EqualTo(right).Within(1));
        }

        
    }
}
