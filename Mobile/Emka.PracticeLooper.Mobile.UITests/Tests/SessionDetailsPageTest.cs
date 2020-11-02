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
    public class SessionDetailsPageTest : BaseTestFixture
    {
        public SessionDetailsPageTest(Platform platform)
            : base(platform)
        {

        }

        [Test]
        [TestCase("Coumarin Mirage")]
        public void When_DeleteFirstLoop_Then_ThrowsException(string name)
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

            mainPage.OpenBookmarks(song);

            SessionDetailPage sessionDetailPage = new SessionDetailPage();

            var labels = new List<string>() { song };
            var Loops = sessionDetailPage.GetLoops(labels);

            Assert.Throws<Exception>(() => sessionDetailPage.DeleteLoop(song));
            Assert.IsTrue(Loops.Contains(mainLoop));
        }

        [Test]
        [TestCase("Coumarin Mirage", "Bookmark To Delete")]
        public void When_DeleteLoop_Then_DefaultLoopIsStillThere(string name, string bookmark)
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

            mainPage.SetSlider(80, 60);

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

            var labels = new List<string>() { song, bookmark };

            sessionDetailPage.DeleteLoop(bookmark);

            var Loops = sessionDetailPage.GetLoops(labels);

            Assert.AreEqual(Loops.Count, 1);
            Assert.IsTrue(Loops.Contains(mainLoop));
            Assert.IsFalse(Loops.Contains(bookmarkLoop));
        }

    }
}
