// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using NUnit.Framework;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture()]
    public class SessionDetailsViewModelTests
    {
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
            navigationServiceMock = new Mock<INavigationService>();
            appTrackerMock = new Mock<IAppTracker>();
        }

        [Test()]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task When_InitializeAsync_Expect_Has1LoopAnd1SelectedLoop()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
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

            await sessionDetailsViewModel.InitializeAsync(sessionViewModel);

            Assert.NotNull(sessionDetailsViewModel.Session);
            Assert.NotNull(sessionDetailsViewModel.Loops);
            Assert.IsNotEmpty(sessionDetailsViewModel.Loops);
            Assert.That(sessionDetailsViewModel.Loops, Has.Count.EqualTo(1));
            Assert.NotNull(sessionDetailsViewModel.SelectedLoop);
        }

        [Test()]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task When_Delete1Loop_Expect_HasNoLoops()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
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

            await sessionDetailsViewModel.InitializeAsync(sessionViewModel);

            Assert.NotNull(sessionDetailsViewModel.Session);
            Assert.NotNull(sessionDetailsViewModel.Loops);
            Assert.IsNotEmpty(sessionDetailsViewModel.Loops);
            Assert.That(sessionDetailsViewModel.Loops, Has.Count.EqualTo(1));
            Assert.NotNull(sessionDetailsViewModel.SelectedLoop);

            var loopViewModel = new LoopViewModel(loop);
            MessagingCenter.Send(loopViewModel, MessengerKeys.DeleteLoop, loop);
            Assert.IsEmpty(sessionDetailsViewModel.Loops);
        }

        [Test()]
        public void When_DeleteNullLoop_Expect_ArgumentNullException()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
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
