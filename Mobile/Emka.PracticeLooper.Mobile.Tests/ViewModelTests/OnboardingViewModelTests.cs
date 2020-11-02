// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using NUnit.Framework;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture]
    public class OnboardingViewModelTests
    {
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        StringLocalizer localizer;

        public OnboardingViewModelTests()
        {
            navigationServiceMock = new Mock<INavigationService>();
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();
            localizer = new StringLocalizer(loggerMock.Object);
        }

        [Test]
        public void When_Created_Expect_Has7Pages()
        {
            var onboardingViewModel = new OnboardingViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object);
            Assert.That(onboardingViewModel.Items, Has.Count.EqualTo(7));
            Assert.IsTrue(onboardingViewModel.IsSkipButtonVisible);
        }

        [Test]
        public void When_Created_Expect_PagesHaveValidContent()
        {
            var onboardingViewModel = new OnboardingViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object);

            // check welcome page properties
            var welcomePage = onboardingViewModel.Items[0];
            Assert.AreEqual(welcomePage.Title, localizer.GetLocalizedString("OnboardingView_Welcome_Title"));
            Assert.AreEqual(welcomePage.Content, localizer.GetLocalizedString("OnboardingView_Welcome_Description"));
            Assert.AreEqual(welcomePage.ImageUrl, "start.png");
            Assert.IsNull(welcomePage.Icon);
            Assert.IsFalse(welcomePage.StartButtonVisible);

            // check add song page properties
            var addSongPage = onboardingViewModel.Items[1];
            Assert.AreEqual(addSongPage.Title, localizer.GetLocalizedString("OnboardingView_AddSong_Title"));
            Assert.IsNull(addSongPage.Content);
            Assert.AreEqual(addSongPage.ImageUrl, "addSong.png");
            Assert.AreEqual(addSongPage.Icon, "\uf419");
            Assert.IsFalse(addSongPage.StartButtonVisible);

            // check slider page properties
            var sliderPage = onboardingViewModel.Items[2];
            Assert.AreEqual(sliderPage.Title, localizer.GetLocalizedString("OnboardingView_Slider_Title"));
            Assert.IsNull(sliderPage.Content);
            Assert.AreEqual(sliderPage.ImageUrl, "slider.png");
            Assert.IsNull(sliderPage.Icon);
            Assert.IsFalse(sliderPage.StartButtonVisible);

            // check picker page properties
            var pickerPage = onboardingViewModel.Items[3];
            Assert.AreEqual(pickerPage.Title, localizer.GetLocalizedString("OnboardingView_Picker_Title"));
            Assert.IsNull(pickerPage.Content);
            Assert.AreEqual(pickerPage.ImageUrl, "picker.png");
            Assert.IsNull(pickerPage.Icon);
            Assert.IsFalse(pickerPage.StartButtonVisible);

            // check add marker page properties
            var addMarkerPage = onboardingViewModel.Items[4];
            Assert.AreEqual(addMarkerPage.Title, localizer.GetLocalizedString("OnboardingView_AddMarker_Title"));
            Assert.IsNull(addMarkerPage.Content);
            Assert.AreEqual(addMarkerPage.ImageUrl, "addMarker.png");
            Assert.AreEqual(addMarkerPage.Icon, "\uf0c4");
            Assert.IsFalse(addMarkerPage.StartButtonVisible);

            // check show marker page properties
            var showMarkerPage = onboardingViewModel.Items[5];
            Assert.AreEqual(showMarkerPage.Title, localizer.GetLocalizedString("OnboardingView_ShowMarker_Title"));
            Assert.IsNull(showMarkerPage.Content);
            Assert.AreEqual(showMarkerPage.ImageUrl, "showMarker.png");
            Assert.AreEqual(showMarkerPage.Icon, "\U000f03a4");
            Assert.IsFalse(showMarkerPage.StartButtonVisible);

            // check lets start page properties
            var letsStartPage = onboardingViewModel.Items[6];
            Assert.AreEqual(letsStartPage.Title, localizer.GetLocalizedString("OnboardingView_LetsStart_Title"));
            Assert.AreEqual(letsStartPage.Content, localizer.GetLocalizedString("OnboardingView_LetsStart_Description"));
            Assert.AreEqual(letsStartPage.ImageUrl, "letsStart.png");
            Assert.IsNull(letsStartPage.Icon);
            Assert.IsTrue(letsStartPage.StartButtonVisible);
        }

        [Test]
        public void When_IsOnLastPage_Expect_SkipButtonIsInvisible()
        {
            var onboardingViewModel = new OnboardingViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object);
            onboardingViewModel.Position = onboardingViewModel.Items.Count - 1;
            Assert.IsFalse(onboardingViewModel.IsSkipButtonVisible);
        }

        [Test]
        public void When_ExecuteSkipCommand_Expect_NavigationServiceCalled()
        {
            navigationServiceMock.Setup(n => n.NavigateToAsync<MainViewModel>()).Verifiable();
            var onboardingViewModel = new OnboardingViewModel(navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object);
            onboardingViewModel.SkipCommand.Execute(null);
            navigationServiceMock.Verify(n => n.NavigateToAsync<MainViewModel>(), Times.Once);
        }
    }
}
