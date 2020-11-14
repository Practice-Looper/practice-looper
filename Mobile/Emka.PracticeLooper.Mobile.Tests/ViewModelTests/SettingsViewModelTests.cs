// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using NUnit.Framework;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture]
    public class SettingsViewModelTests
    {
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        Mock<IDialogService> dialogServiceMock;
        Mock<IInAppBillingService> inAppBillingServiceMock;
        Mock<IConfigurationService> configurationServiceMock;
        StringLocalizer localizer;

        [SetUp]
        public void Setup()
        {
            dialogServiceMock = new Mock<IDialogService>();
            navigationServiceMock = new Mock<INavigationService>();
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();
            inAppBillingServiceMock = new Mock<IInAppBillingService>();
            configurationServiceMock = new Mock<IConfigurationService>();
            localizer = new StringLocalizer(loggerMock.Object);
        }

        [Test]
        public async Task When_InitHasNoPurchasedProduct_Expect_Has1NotPurchasedProduct()
        {
            var premiumProduct = new InAppPurchaseProduct
            {
                Name = "Premium",
                ProductId = "id",
                Description = "Description",
                Image = "image.png",
                Purchased = false,
                LocalizedPrice = "5,99€",
                CurrencyCode = "de",
                Package = "Premium"
            };

            inAppBillingServiceMock
                .Setup(i => i.Init())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Initialized).Returns(true); })
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.FetchOfferingsAsync())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Products).Returns(new Dictionary<string, InAppPurchaseProduct> { { "Premium", premiumProduct } }); })
                .Returns(Task.FromResult((true, string.Empty)))
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.RestorePurchasesAsync())
                .ReturnsAsync((true, string.Empty))
                .Verifiable();

            var vm = new SettingsViewModel(loggerMock.Object, dialogServiceMock.Object, appTrackerMock.Object, navigationServiceMock.Object, inAppBillingServiceMock.Object, configurationServiceMock.Object);
            await vm.InitializeAsync(null);

            Assert.IsTrue(vm.HasProducts);
            Assert.IsNotEmpty(vm.Products);
            Assert.AreEqual(vm.Products.First().ProductId, premiumProduct.ProductId);
            Assert.IsFalse(vm.Products.First().Purchased);

            inAppBillingServiceMock
                .Verify(i => i.Init(), Times.Once);

            inAppBillingServiceMock
                .Verify(i => i.FetchOfferingsAsync(), Times.Once);

            inAppBillingServiceMock
               .Verify(i => i.RestorePurchasesAsync(), Times.Once);
        }

        [Test]
        public async Task When_InitHasPurchasedProduct_Expect_Has1PurchasedProduct()
        {
            var premiumProduct = new InAppPurchaseProduct
            {
                Name = "Premium",
                ProductId = "id",
                Description = "Description",
                Image = "image.png",
                Purchased = false,
                LocalizedPrice = "5,99€",
                CurrencyCode = "de",
                Package = "Premium"
            };

            inAppBillingServiceMock
                .Setup(i => i.Init())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Initialized).Returns(true); })
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.FetchOfferingsAsync())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Products).Returns(new Dictionary<string, InAppPurchaseProduct> { { "Premium", premiumProduct } }); })
                .Returns(Task.FromResult((true, string.Empty)))
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.RestorePurchasesAsync())
                .Callback(() => { inAppBillingServiceMock.Object.Products.First().Value.Purchased = true; })
                .ReturnsAsync((true, string.Empty))
                .Verifiable();

            var vm = new SettingsViewModel(loggerMock.Object, dialogServiceMock.Object, appTrackerMock.Object, navigationServiceMock.Object, inAppBillingServiceMock.Object, configurationServiceMock.Object);
            await vm.InitializeAsync(null);

            Assert.IsTrue(vm.HasProducts);
            Assert.IsNotEmpty(vm.Products);
            Assert.AreEqual(vm.Products.First().ProductId, premiumProduct.ProductId);
            Assert.IsTrue(vm.Products.First().Purchased);

            inAppBillingServiceMock
                .Verify(i => i.Init(), Times.Once);

            inAppBillingServiceMock
                .Verify(i => i.FetchOfferingsAsync(), Times.Once);

            inAppBillingServiceMock
               .Verify(i => i.RestorePurchasesAsync(), Times.Once);
        }

        [Test]
        public async Task When_BillingServiceInizalized_Expect_BillingServiceInitNotInvoked()
        {
            var premiumProduct = new InAppPurchaseProduct
            {
                Name = "Premium",
                ProductId = "id",
                Description = "Description",
                Image = "image.png",
                Purchased = false,
                LocalizedPrice = "5,99€",
                CurrencyCode = "de",
                Package = "Premium"
            };

            inAppBillingServiceMock
                .SetupGet(s => s.Initialized)
                .Returns(true)
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.FetchOfferingsAsync())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Products).Returns(new Dictionary<string, InAppPurchaseProduct> { { "Premium", premiumProduct } }); })
                .Returns(Task.FromResult((true, string.Empty)))
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.RestorePurchasesAsync())
                .Callback(() => { inAppBillingServiceMock.Object.Products.First().Value.Purchased = true; })
                .ReturnsAsync((true, string.Empty))
                .Verifiable();

            var vm = new SettingsViewModel(loggerMock.Object, dialogServiceMock.Object, appTrackerMock.Object, navigationServiceMock.Object, inAppBillingServiceMock.Object, configurationServiceMock.Object);
            await vm.InitializeAsync(null);

            Assert.IsTrue(vm.HasProducts);
            Assert.IsNotEmpty(vm.Products);
            Assert.AreEqual(vm.Products.First().ProductId, premiumProduct.ProductId);
            Assert.IsTrue(vm.Products.First().Purchased);

            inAppBillingServiceMock
                .Verify(i => i.Init(), Times.Never);

            inAppBillingServiceMock
                .Verify(i => i.FetchOfferingsAsync(), Times.Once);

            inAppBillingServiceMock
               .Verify(i => i.RestorePurchasesAsync(), Times.Once);
        }

        [Test]
        public async Task When_InitFailsToFetchProducts_Expect_HasNoProducts()
        {
            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.Init())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Initialized).Returns(true); })
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.FetchOfferingsAsync())
                .Returns(Task.FromResult((false, localizer.GetLocalizedString("Error_Content_General"))))
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.RestorePurchasesAsync())
                .ReturnsAsync((true, string.Empty))
                .Verifiable();

            var vm = new SettingsViewModel(loggerMock.Object, dialogServiceMock.Object, appTrackerMock.Object, navigationServiceMock.Object, inAppBillingServiceMock.Object, configurationServiceMock.Object);
            await vm.InitializeAsync(null);

            Assert.IsFalse(vm.HasProducts);
            Assert.IsEmpty(vm.Products);

            inAppBillingServiceMock
                .Verify(i => i.FetchOfferingsAsync(), Times.Once);

            inAppBillingServiceMock
               .Verify(i => i.RestorePurchasesAsync(), Times.Never);

            dialogServiceMock
                .Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_General"))));
        }

        [Test]
        public async Task When_InitFailsToRestoreProducts_Expect_DialogShown()
        {
            var premiumProduct = new InAppPurchaseProduct
            {
                Name = "Premium",
                ProductId = "id",
                Description = "Description",
                Image = "image.png",
                Purchased = false,
                LocalizedPrice = "5,99€",
                CurrencyCode = "de",
                Package = "Premium"
            };

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            inAppBillingServiceMock
                .SetupGet(s => s.Initialized)
                .Returns(true)
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.FetchOfferingsAsync())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Products).Returns(new Dictionary<string, InAppPurchaseProduct> { { "Premium", premiumProduct } }); })
                .Returns(Task.FromResult((true, string.Empty)))
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.RestorePurchasesAsync())
                .ReturnsAsync((false, localizer.GetLocalizedString("Error_Content_General")))
                .Verifiable();

            var vm = new SettingsViewModel(loggerMock.Object, dialogServiceMock.Object, appTrackerMock.Object, navigationServiceMock.Object, inAppBillingServiceMock.Object, configurationServiceMock.Object);
            await vm.InitializeAsync(null);

            Assert.IsTrue(vm.HasProducts);
            Assert.IsNotEmpty(vm.Products);
            Assert.IsFalse(vm.Products.First().Purchased);

            inAppBillingServiceMock
                .Verify(i => i.FetchOfferingsAsync(), Times.Once);

            inAppBillingServiceMock
               .Verify(i => i.RestorePurchasesAsync(), Times.Once);

            dialogServiceMock
                .Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_General"))));
        }

        [Test]
        public async Task When_InitHasOneNotPurchasedProduct_Expect_Has1PurchasedProductAfterRestore()
        {
            var premiumProduct = new InAppPurchaseProduct
            {
                Name = "Premium",
                ProductId = "id",
                Description = "Description",
                Image = "image.png",
                Purchased = false,
                LocalizedPrice = "5,99€",
                CurrencyCode = "de",
                Package = "Premium"
            };

            inAppBillingServiceMock
                .Setup(i => i.Init())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Initialized).Returns(true); })
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.FetchOfferingsAsync())
                .Callback(() => { inAppBillingServiceMock.SetupGet(s => s.Products).Returns(new Dictionary<string, InAppPurchaseProduct> { { "Premium", premiumProduct } }); })
                .Returns(Task.FromResult((true, string.Empty)))
                .Verifiable();

            inAppBillingServiceMock
                .Setup(i => i.RestorePurchasesAsync())
                .Callback(() => { inAppBillingServiceMock.Object.Products.First().Value.Purchased = true; })
                .ReturnsAsync((true, string.Empty))
                .Verifiable();

            var vm = new SettingsViewModel(loggerMock.Object, dialogServiceMock.Object, appTrackerMock.Object, navigationServiceMock.Object, inAppBillingServiceMock.Object, configurationServiceMock.Object);
            await vm.InitializeAsync(null);

            Assert.IsTrue(vm.HasProducts);
            Assert.IsNotEmpty(vm.Products);
            Assert.AreEqual(vm.Products.First().ProductId, premiumProduct.ProductId);
            Assert.IsTrue(vm.Products.First().Purchased);

            inAppBillingServiceMock
                .Verify(i => i.Init(), Times.Once);

            inAppBillingServiceMock
                .Verify(i => i.FetchOfferingsAsync(), Times.Once);

            inAppBillingServiceMock
               .Verify(i => i.RestorePurchasesAsync(), Times.Once);
        }

        [Test]
        public async Task When_ShowProductPayWallProduct_Expect_NavigatesToProductPayWallViewModel()
        {
            var tcs = new TaskCompletionSource<bool>();
            var premiumProduct = new InAppPurchaseProduct
            {
                Name = "Premium",
                ProductId = "id",
                Description = "Description",
                Image = "image.png",
                Purchased = false,
                LocalizedPrice = "5,99€",
                CurrencyCode = "de",
                Package = "Premium"
            };

            navigationServiceMock
                .Setup(n => n.NavigateToAsync<ProductPaywallViewModel>(It.IsAny<object>()))
                .Callback((object p) => { tcs.SetResult((p as InAppPurchaseProductViewModel).ProductId == premiumProduct.ProductId); })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var vm = new SettingsViewModel(loggerMock.Object, dialogServiceMock.Object, appTrackerMock.Object, navigationServiceMock.Object, inAppBillingServiceMock.Object, configurationServiceMock.Object);
            await vm.InitializeAsync(null);
            vm.ShowProductPaywallCommand.Execute(new InAppPurchaseProductViewModel(premiumProduct));
            await tcs.Task;

            Assert.IsTrue(tcs.Task.Result);
            navigationServiceMock
                .Verify(n => n.NavigateToAsync<ProductPaywallViewModel>(It.IsAny<InAppPurchaseProductViewModel>()), Times.Once);
        }
    }
}
