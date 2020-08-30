using System;
using System.Linq;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using Xamarin.Forms;
using Xunit;

namespace Emka.PracticeLooper.Mobile.Tests
{
    public class SessionDetailsViewModelTests
    {
        private SessionDetailsViewModel sessionDetailsViewModel;
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IDialogService> dialogServiceMock;
        private readonly Mock<INavigationService> navigationServiceMock;
        private readonly Mock<IConfigurationService> configurationServiceMock;
        private readonly Mock<IAppTracker> appTrackerMock;

        public SessionDetailsViewModelTests()
        {
            loggerMock = new Mock<ILogger>();
            dialogServiceMock = new Mock<IDialogService>();
            configurationServiceMock = new Mock<IConfigurationService>();
            appTrackerMock = new Mock<IAppTracker>();
            sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object);
            navigationServiceMock = new Mock<INavigationService>();
            appTrackerMock = new Mock<IAppTracker>();
        }

        [Fact]
        public void When_InitializeAsync_Expect_Has1LoopAnd1SelectedLoop()
        {
            var audioSource = new AudioSource()
            {
                Id = 0,
                Duration = 123456,
                Source = "/abc/def",
                FileName = "file.mp3",
                Type = AudioSourceType.LocalInternal
            };

            var loop = new Loop
            {
                Id = 0,
                Name = "MySession",
                StartPosition = 0.0,
                EndPosition = 0.8,
                IsDefault = true,
                Repititions = 1
            };

            var sessionViewModel = new SessionViewModel(new Session()
            {
                Id = 0,
                Name = "MySession",
                AudioSource = audioSource,
                AudioSourceId = audioSource.Id,
                Loops = new System.Collections.Generic.List<Loop>
                {
                    loop
                }
            }, dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            sessionDetailsViewModel.InitializeAsync(sessionViewModel);

            Assert.NotNull(sessionDetailsViewModel.Session);
            Assert.NotNull(sessionDetailsViewModel.Loops);
            Assert.NotEmpty(sessionDetailsViewModel.Loops);
            Assert.Single(sessionDetailsViewModel.Loops);
            Assert.NotNull(sessionDetailsViewModel.SelectedLoop);
        }

        [Fact]
        public void When_Delete1Loop_Expect_HasNoLoops()
        {
            var audioSource = new AudioSource()
            {
                Id = 0,
                Duration = 123456,
                Source = "/abc/def",
                FileName = "file.mp3",
                Type = AudioSourceType.LocalInternal
            };

            var loop = new Loop
            {
                Id = 0,
                Name = "MySession",
                StartPosition = 0.0,
                EndPosition = 0.8,
                IsDefault = true,
                Repititions = 1
            };

            var sessionViewModel = new SessionViewModel(new Session()
            {
                Id = 0,
                Name = "MySession",
                AudioSource = audioSource,
                AudioSourceId = audioSource.Id,
                Loops = new System.Collections.Generic.List<Loop>
                {
                    loop
                }
            }, dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            sessionDetailsViewModel.InitializeAsync(sessionViewModel);

            Assert.NotNull(sessionDetailsViewModel.Session);
            Assert.NotNull(sessionDetailsViewModel.Loops);
            Assert.NotEmpty(sessionDetailsViewModel.Loops);
            Assert.Single(sessionDetailsViewModel.Loops);
            Assert.NotNull(sessionDetailsViewModel.SelectedLoop);

            var loopViewModel = new LoopViewModel(loop);
            MessagingCenter.Send(loopViewModel, MessengerKeys.DeleteLoop, loop);
            Assert.Empty(sessionDetailsViewModel.Loops);
        }

        [Fact]
        public void When_DeleteNullLoop_Expect_ArgumentNullException()
        {
            var loop = new Loop
            {
                Id = 0,
                Name = "MySession",
                StartPosition = 0.0,
                EndPosition = 0.8,
                IsDefault = true,
                Repititions = 1
            };

            Assert.Throws<ArgumentNullException>(() => sessionDetailsViewModel.OnDeleteLoop(new LoopViewModel(loop), null));
        }
    }
}
