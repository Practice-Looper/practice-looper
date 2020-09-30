// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using Emka.PracticeLooper.Mobile.UITests.Pages;
using NUnit.Framework;
using Xamarin.UITest;
using static Emka.PracticeLooper.Mobile.UITests.Common.TestFixtures;

namespace Emka.PracticeLooper.Mobile.UITests.Tests
{
    public class SpoitfySearchPageTest: BaseTestFixture
    {
        public SpoitfySearchPageTest(Platform platform)
            : base (platform)
        {

        }

        [Test]
        [TestCase("Coumarin Mirage")]
        public void When_SearchForSong_Then_SongIsFound(string name)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            new MainPage()
                .AddNewSpotifiySession();

            var song = new SpotifySearchPage()
                .SearchSong(name)
                .SelectSong(0);

            Assert.IsNotEmpty(song);
        }

        [Test]
        [TestCase("Blah Blah Blauwal!")]
        public void When_SearchForNonsene_Then_NoSongIsFound(string name)
        {
            if (AppInitializer.IsLite)
            {
                Assert.Ignore();
            }

            new MainPage()
                .AddNewSpotifiySession();

            Assert.Throws<IndexOutOfRangeException>(() => new SpotifySearchPage()
                .SearchSong(name)
                .SelectSong(0)
            );
        }
    }
}
