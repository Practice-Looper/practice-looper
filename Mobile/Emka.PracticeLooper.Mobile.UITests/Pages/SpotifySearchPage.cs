// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.UITests.Common;
using Emka.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using Xamarin.UITest.Queries;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace Emka.PracticeLooper.Mobile.UITests.Pages
{
    public class SpotifySearchPage : BasePage
    {
        readonly Query ActivityIndicator;
        readonly Query ListView;
        private readonly Mock<ILogger> loggerMock;
        private readonly IStringLocalizer stringLocalizer;

        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Text(stringLocalizer.GetLocalizedString("SpotifySearchView_Title")),
            iOS = x => x.Text(stringLocalizer.GetLocalizedString("SpotifySearchView_Title"))
        };

        public SpotifySearchPage()
        {
            loggerMock = new Mock<ILogger>();
            stringLocalizer = new StringLocalizer(loggerMock.Object);

            ActivityIndicator = x => x.Marked("ActivityIndicator");
            ListView = x => x.Class("LabelRenderer");
        }

        public SpotifySearchPage SearchSong(string name) {
            app.Query(x => x.Id("search_src_text").Invoke("setText", name));
            AwaitSearchResults();
            return this;
        }

        public string SelectSong(int id)
        {
            var songs = app.Query(ListView);

            try
            {
               app.Tap(songs[id].Id);
               AwaitSongHasLoaded(songs[id].Label);
               return songs[id].Text;
            }
            catch (IndexOutOfRangeException e)
            {
                throw e;
            }
        }

        protected void AwaitSearchResults() {
            app.WaitForElement(ActivityIndicator);
            app.WaitForNoElement(ActivityIndicator);
        }

        protected void AwaitSongHasLoaded(string label)
        {
            app.WaitForNoElement(x => x.Marked(label));
            app.WaitForElement(x => x.Marked(label));
        }

    }
}
