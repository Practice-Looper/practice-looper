// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using NUnit.Framework;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture]
    public class SessionViewModelTests
    {
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        Mock<IDialogService> dialogServiceMock;

        public SessionViewModelTests()
        {
            navigationServiceMock = new Mock<INavigationService>();
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();
            dialogServiceMock = new Mock<IDialogService>();
        }

        [Test]
        public async Task When_DeleteSession_Expect_DeleteMessageSent()
        {
            var tcs = new TaskCompletionSource<bool>();
            var session = new Session()
            {
                Id = 123456789,
                Name = "MySession",
                AudioSource = null,
                AudioSourceId = 0,
                Loops = null
            };

            MessagingCenter.Subscribe<SessionViewModel, SessionViewModel>(this, MessengerKeys.DeleteSession, (s, a) =>
            {
                tcs.SetResult(a.Session.Id == session.Id);
            });

            var sessionViewModel = new SessionViewModel(session, dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);
            sessionViewModel.DeleteSessionCommand.Execute(null);
            await tcs.Task;

            Assert.IsTrue(tcs.Task.Result);
        }

        [Test]
        public async Task When_PickLoop_Expect_NavigateToSessionDetailsView()
        {
            var tcs = new TaskCompletionSource<bool>();
            var session = new Session()
            {
                Id = 123456789,
                Name = "MySession",
                AudioSource = null,
                AudioSourceId = 0,
                Loops = null
            };

            navigationServiceMock
                .Setup(n => n.NavigateToAsync<SessionDetailsViewModel>(It.IsAny<SessionViewModel>()))
                .Callback(() => { tcs.SetResult(true); })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sessionViewModel = new SessionViewModel(session, dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);
            sessionViewModel.PickLoopCommand.Execute(null);
            await tcs.Task;

            Assert.IsTrue(tcs.Task.Result);
            navigationServiceMock.Verify(n => n.NavigateToAsync<SessionDetailsViewModel>(It.IsAny<SessionViewModel>()), Times.Once);
        }
    }
}
