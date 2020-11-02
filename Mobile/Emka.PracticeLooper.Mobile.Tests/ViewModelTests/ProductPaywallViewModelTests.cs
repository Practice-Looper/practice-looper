// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
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
    public class ProductPaywallViewModelTests
    {
        private Mock<IDialogService> dialogServiceMock;
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        Mock<IInAppBillingService> inAppBillingServiceMock;
        InAppPurchaseProductViewModel productViewModel;
        InAppPurchaseProduct premiumProduct;
        StringLocalizer localizer;

        public ProductPaywallViewModelTests()
        {
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();
            localizer = new StringLocalizer(loggerMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            dialogServiceMock = new Mock<IDialogService>();
            navigationServiceMock = new Mock<INavigationService>();
            inAppBillingServiceMock = new Mock<IInAppBillingService>();
            premiumProduct = new InAppPurchaseProduct
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
        }

        [Test]
        public async Task When_Initialized_PremiumProduct_Expect_HasPremiumProductProperties()
        {
            productViewModel = new InAppPurchaseProductViewModel(premiumProduct);

            var vm = new ProductPaywallViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, inAppBillingServiceMock.Object, dialogServiceMock.Object);
            await vm.InitializeAsync(productViewModel);
            Assert.IsFalse(vm.IsBusy);
            Assert.AreEqual(vm.Product, productViewModel);
        }

        [Test]
        public void When_Initialized_NullProduct_Expect_ThrowsArgumentException()
        {
            productViewModel = new InAppPurchaseProductViewModel(premiumProduct);

            var vm = new ProductPaywallViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, inAppBillingServiceMock.Object, dialogServiceMock.Object);
            var ex = Assert.ThrowsAsync<ArgumentException>(() => vm.InitializeAsync(null));
            Assert.NotNull(ex);
            Assert.AreEqual(ex.Message, "parameter");
        }

        [Test]
        public async Task When_PurchaseNotPurchasedProduct_Expect_PurchaseCommandExecuted()
        {
            var tcs = new TaskCompletionSource<bool>();
            productViewModel = new InAppPurchaseProductViewModel(premiumProduct);

            inAppBillingServiceMock.Setup(b => b.PurchaseProductAsync(It.IsAny<object>()))
                .Callback(() => { Task.Run(() => tcs.SetResult(true)); })
                .ReturnsAsync(() => (true, null, false))
                .Verifiable();

            navigationServiceMock.Setup(n => n.GoBackAsync()).Verifiable();

            var vm = new ProductPaywallViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, inAppBillingServiceMock.Object, dialogServiceMock.Object);
            await vm.InitializeAsync(productViewModel);
            vm.PurchaseItemCommand.Execute(null);
            await tcs.Task;

            Assert.IsTrue(tcs.Task.Result);
            Assert.IsTrue(vm.Product.Purchased);
            inAppBillingServiceMock.Verify(b => b.PurchaseProductAsync(It.IsAny<object>()), Times.Once);
            navigationServiceMock.Verify(n => n.GoBackAsync(), Times.Once);
        }

        [Test]
        public async Task When_PurchasePurchasedProduct_Expect_AlertDialog()
        {
            premiumProduct.Purchased = true;
            productViewModel = new InAppPurchaseProductViewModel(premiumProduct);
            var tcs = new TaskCompletionSource<bool>();
            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() =>
                    {
                        Task.Run(() => tcs.SetResult(true));
                    })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var vm = new ProductPaywallViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, inAppBillingServiceMock.Object, dialogServiceMock.Object);
            await vm.InitializeAsync(productViewModel);
            vm.PurchaseItemCommand.Execute(null);

            await tcs.Task;
            Assert.IsTrue(tcs.Task.Result);
            Assert.IsTrue(vm.Product.Purchased);
            dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task When_PurchaseProductErrorOccurs_Expect_ErrorMessage()
        {
            var tcs = new TaskCompletionSource<bool>();
            productViewModel = new InAppPurchaseProductViewModel(premiumProduct);
            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_General")), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            inAppBillingServiceMock.Setup(b => b.PurchaseProductAsync(It.IsAny<object>()))
                .Callback(() => { Task.Run(() => tcs.SetResult(true)); })
                .ReturnsAsync(() => (false, localizer.GetLocalizedString("Error_Content_General"), false))
                .Verifiable();

            var vm = new ProductPaywallViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, inAppBillingServiceMock.Object, dialogServiceMock.Object);
            await vm.InitializeAsync(productViewModel);

            vm.PurchaseItemCommand.Execute(null);
            await tcs.Task;

            Assert.IsFalse(vm.Product.Purchased);
            dialogServiceMock.Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_General")), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task When_PurchaseProductUserCancels_Expect_NoActions()
        {
            var tcs = new TaskCompletionSource<bool>();
            productViewModel = new InAppPurchaseProductViewModel(premiumProduct);

            inAppBillingServiceMock.Setup(b => b.PurchaseProductAsync(It.IsAny<object>()))
                .Callback(() => { Task.Run(() => tcs.SetResult(true)); })
                .ReturnsAsync(() => (false, string.Empty, true))
                .Verifiable();

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var vm = new ProductPaywallViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, inAppBillingServiceMock.Object, dialogServiceMock.Object);
            await vm.InitializeAsync(productViewModel);

            vm.PurchaseItemCommand.Execute(null);
            await tcs.Task;

            Assert.IsFalse(vm.Product.Purchased);
            inAppBillingServiceMock.Verify(b => b.PurchaseProductAsync(It.IsAny<object>()), Times.Once);
            dialogServiceMock.Verify(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
