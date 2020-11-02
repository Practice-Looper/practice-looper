// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using System.Linq;
using Emka.PracticeLooper.Mobile.UITests.Common;
using Emka3.PracticeLooper.Model.Player;
using Xamarin.UITest;
using Emka.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;
using Emka.PracticeLooper.Mobile.Common;

namespace Emka.PracticeLooper.Mobile.UITests.Pages
{
    public class SessionDetailPage : BasePage
    {
        readonly Query DeleteLoopButton;
        readonly Query Ok;
        private static readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static readonly IStringLocalizer stringLocalizer = new StringLocalizer(loggerMock.Object);

        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Text(stringLocalizer.GetLocalizedString("LoopsDetailsView_Title")),
            iOS = x => x.Text(stringLocalizer.GetLocalizedString("LoopsDetailsView_Title"))
        };

        public SessionDetailPage()
        {

            DeleteLoopButton = x => x.Text(stringLocalizer.GetLocalizedString("Delete"));
            Ok = x => x.Text(stringLocalizer.GetLocalizedString("Ok"));
        }

        public List<Loop> GetLoops(List<string> labels)
        {
            var loops = new List<Loop>();

            foreach (string label in labels) {
                var items = app.Query(x => x.Text(label).Sibling());

                if (items.Length != 0) {
                    var loop = new Loop()
                    {
                        Name = label,
                        StartPosition = TimeSpan.Parse("00:" + items[0].Text).TotalSeconds,
                        EndPosition = TimeSpan.Parse("00:" + items[2].Text).TotalSeconds
                    };

                    loops.Add(loop);
                }

            }
           
            return loops;
        }

        public void SelectLoop(string label)
        {
            var bookmark = app.Query(x => x.Text(label)).First();
            app.Tap(bookmark.Id);

            var popup = app.Query(Ok).FirstOrDefault();
            if (popup != null)
            {
                app.Tap(popup.Id);
            }
           
            app.WaitForNoElement(bookmark.Id);
        }

        public void DeleteLoop(string label)
        {
            if (AppInitializer.Platform == Platform.Android)
            {
                app.TouchAndHold(x => x.Text(label));
            }
            else
            {
                app.SwipeRightToLeft(x => x.Text(label));
            }

            app.WaitForElement(DeleteLoopButton, "Button not found!", TimeSpan.FromSeconds(2));
            app.Tap(DeleteLoopButton);
        }

    }
}
