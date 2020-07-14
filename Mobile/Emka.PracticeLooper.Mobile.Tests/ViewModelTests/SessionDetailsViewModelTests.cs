using System;
using System.Linq;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
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

        public SessionDetailsViewModelTests()
        {
            sessionDetailsViewModel = new SessionDetailsViewModel();
            loggerMock = new Mock<ILogger>();
            dialogServiceMock = new Mock<IDialogService>();
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
                Type = AudioSourceType.Local
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
            }, dialogServiceMock.Object, loggerMock.Object);

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
                Type = AudioSourceType.Local
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
            }, dialogServiceMock.Object, loggerMock.Object);

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
