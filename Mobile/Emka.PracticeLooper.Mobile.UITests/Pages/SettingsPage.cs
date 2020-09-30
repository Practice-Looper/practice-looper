// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020

using System.Collections.Generic;
using System.Linq;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.UITests.Common;
using Emka.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace Emka.PracticeLooper.Mobile.UITests.Pages
{
    public class SettingsPage : BasePage
    {
        readonly Query ActivityIndicator;
        readonly Query InAppPurchaseProducts;
        readonly Query SchemeSwitch;
        private static readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static readonly IStringLocalizer stringLocalizer = new StringLocalizer(loggerMock.Object);

        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Text(stringLocalizer.GetLocalizedString("SettingsView_Title")),
            iOS = x => x.Text(stringLocalizer.GetLocalizedString("SettingsView_Title"))
        };

        public SettingsPage()
        {
            SchemeSwitch = x => x.Marked("SchemeSwitch");
            ActivityIndicator = x => x.Marked("ActivityIndicator");
            InAppPurchaseProducts = x => x.Marked("SettingsListView").Child().Child(1).Child().Child();
        }

        public List<InAppPurchaseProduct> GetInAppPurchaseProducts()
        {
            app.WaitForNoElement(ActivityIndicator);
            var items = app.Query(InAppPurchaseProducts);

            var products = new List<InAppPurchaseProduct>();

            var product = new InAppPurchaseProduct()
            {
                Name = items[0].Text,
                LocalizedPrice = items[1].Text
            };

            products.Add(product);
            
            return products;
        }

        public void EnableDarkMode()
        {   
            if (IsDarkModeEnabled() == false)
            {
                app.Tap(SchemeSwitch);
            }
        }

        public void DisableDarkMode()
        {
            if (IsDarkModeEnabled() == true)
            {
                app.Tap(SchemeSwitch);
            }
        }

        public bool IsDarkModeEnabled()
        {
            app.WaitForNoElement(ActivityIndicator);
            app.WaitForElement(SchemeSwitch);

            if (OniOS)
            {
                return app.Query(x => x.Marked("SchemeSwitch").Invoke("IsOn").Value<int>()).First() == 1;
            }
               
           return app.Query(x => x.Marked("SchemeSwitch").Invoke("isChecked").Value<bool>()).First();
        }

    }
}
