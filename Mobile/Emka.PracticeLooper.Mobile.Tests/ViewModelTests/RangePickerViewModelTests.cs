// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using NUnit.Framework;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture]
    public class RangePickerViewModelTests
    {

        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        Mock<IDialogService> dialogServiceMock;
        List<AudioSource> audioSources;
        List<Loop> loops;
        public RangePickerViewModelTests()
        {
            dialogServiceMock = new Mock<IDialogService>();
            navigationServiceMock = new Mock<INavigationService>();
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();
            audioSources = new List<AudioSource>
            {
                new AudioSource()
                {
                    Id = 0,
                    Duration = 60, // secs
                    Source = "/abc/def",
                    FileName = "file.mp3",
                    Type = AudioSourceType.LocalInternal
                },
                new AudioSource()
                {
                    Id = 1,
                    Duration = 175, // secs
                    Source = "/abc/def",
                    FileName = "file.mp3",
                    Type = AudioSourceType.LocalInternal
                },
                new AudioSource()
                {
                    Id = 2,
                    Duration = 330, // secs
                    Source = "/abc/def",
                    FileName = "file.mp3",
                    Type = AudioSourceType.LocalInternal
                }
            };

            loops = new List<Loop>
            {
                new Loop
                {
                    Id = 0,
                    Name = "MySession",
                    StartPosition = 0.0,
                    EndPosition = 1.0,
                    IsDefault = true,
                    Repititions = 1
                },
                new Loop
                {
                    Id = 1,
                    Name = "MySession",
                    StartPosition = 0.3,
                    EndPosition = 1.0,
                    IsDefault = true,
                    Repititions = 1
                },
                new Loop
                {
                    Id = 2,
                    Name = "MySession",
                    StartPosition = 0.2,
                    EndPosition = 0.7,
                    IsDefault = true,
                    Repititions = 1
                }
            };
        }

        [Test]
        public async Task When_InitializedForStartPosition_Expect_PicksStartPosition()
        {
            var vm = new RangePickerViewModel(dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);
            var audioSource = new AudioSource()
            {
                Id = 0,
                Duration = 60, // secs
                Source = "/abc/def",
                FileName = "file.mp3",
                Type = AudioSourceType.LocalInternal
            };

            var loop = new Loop
            {
                Id = 0,
                Name = "MySession",
                StartPosition = 0.0,
                EndPosition = 1.0,
                IsDefault = true,
                Repititions = 1
            };

            vm.AudioSource = audioSource;
            vm.Loop = loop;
            await vm.InitializeAsync(true);

            var minutesCollection = vm.Time[0] as RangeObservableCollection<object>;
            var secondsCollection = vm.Time[1] as RangeObservableCollection<object>;
            Assert.NotNull(vm.Loop);
            Assert.NotNull(vm.AudioSource);
            Assert.IsNotEmpty(vm.Time);
            Assert.IsNotEmpty(vm.SelectedTime);
            Assert.IsTrue(vm.IsStartPosition);
            Assert.AreEqual(minutesCollection.First(), "00");
            Assert.AreEqual(secondsCollection.First(), "00");
        }

        [Test]
        public async Task When_InitializedDurationGreater60Minutes_Expect_ShowDialog()
        {
            var tcs = new TaskCompletionSource<bool>();
            var vm = new RangePickerViewModel(dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);
            var audioSource = new AudioSource()
            {
                Id = 0,
                Duration = 3600, // secs
                Source = "/abc/def",
                FileName = "file.mp3",
                Type = AudioSourceType.LocalInternal
            };

            var loop = new Loop
            {
                Id = 0,
                Name = "MySession",
                StartPosition = 0.0,
                EndPosition = 1.0,
                IsDefault = true,
                Repititions = 1
            };

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            vm.AudioSource = audioSource;
            vm.Loop = loop;
            await vm.InitializeAsync(true);
        }

        [Test]
        public async Task When_InitializedForEndPosition_Expect_PicksEndPosition()
        {
            var vm = new RangePickerViewModel(dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);
            var audioSource = new AudioSource()
            {
                Id = 0,
                Duration = 60, // secs
                Source = "/abc/def",
                FileName = "file.mp3",
                Type = AudioSourceType.LocalInternal
            };

            var loop = new Loop
            {
                Id = 0,
                Name = "MySession",
                StartPosition = 0.0,
                EndPosition = 1.0,
                IsDefault = true,
                Repititions = 1
            };

            vm.AudioSource = audioSource;
            vm.Loop = loop;
            await vm.InitializeAsync(false);
            var minutesCollection = vm.Time[0] as RangeObservableCollection<object>;
            var secondsCollection = vm.Time[1] as RangeObservableCollection<object>;

            Assert.NotNull(vm.Loop);
            Assert.NotNull(vm.AudioSource);
            Assert.IsNotEmpty(vm.Time);
            Assert.IsNotEmpty(vm.SelectedTime);
            Assert.IsFalse(vm.IsStartPosition);
            Assert.AreEqual(minutesCollection.First(), "00");
            Assert.AreEqual(secondsCollection.First(), "00");
        }

        [Test, Combinatorial]
        public async Task When_Initialize_Expect_ValidSelection([Values(0, 1, 2)] int audioSourceId, [Values(0, 1, 2)] int loopId)
        {
            var vm = new RangePickerViewModel(dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            vm.AudioSource = audioSources[audioSourceId];
            vm.Loop = loops[loopId];
            await vm.InitializeAsync(true);

            var minutesIndex = TimeSpan.FromSeconds(audioSources[audioSourceId].Duration * loops[loopId].StartPosition).Minutes;
            var secondsIndex = TimeSpan.FromSeconds(audioSources[audioSourceId].Duration * loops[loopId].StartPosition).Seconds;
            Assert.AreEqual(vm.SelectedTime[0], minutesIndex.ToString("D2"));
            Assert.AreEqual(vm.SelectedTime[1], secondsIndex.ToString("D2"));
        }

        [Test, Combinatorial]
        public async Task When_GetSecondsStartValue_Expect_ValidLowerAndUpperBounds([Values(0, 1, 2)] int audioSourceId, [Values(0, 1, 2)] int loopId)
        {
            var vm = new RangePickerViewModel(dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            vm.AudioSource = audioSources[audioSourceId];
            vm.Loop = loops[loopId];
            await vm.InitializeAsync(true);

            var upperSeconds = TimeSpan.FromSeconds(audioSources[audioSourceId].Duration * loops[loopId].EndPosition).Subtract(TimeSpan.FromSeconds(5)).Seconds;
            var allMinutes = vm.Time[0] as RangeObservableCollection<object>;

            var lowerValidSeconds = vm.GetValidSeconds(allMinutes.First().ToString());
            var upperValidSeconds = vm.GetValidSeconds(allMinutes.Last().ToString());
            Assert.AreEqual(lowerValidSeconds.First().ToString(), "00");
            Assert.AreEqual(upperValidSeconds.Last().ToString(), upperSeconds.ToString("D2"));
        }

        [Test, Combinatorial]
        public async Task When_GetSecondsEndValue_Expect_ValidLowerAndUpperBounds([Values(0, 1, 2)] int audioSourceId, [Values(0, 1, 2)] int loopId)
        {
            var vm = new RangePickerViewModel(dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            vm.AudioSource = audioSources[audioSourceId];
            vm.Loop = loops[loopId];
            await vm.InitializeAsync(false);

            var lowerSeconds = TimeSpan.FromSeconds(audioSources[audioSourceId].Duration * loops[loopId].StartPosition).Add(TimeSpan.FromSeconds(5)).Seconds;
            var upperSeconds = TimeSpan.FromSeconds(audioSources[audioSourceId].Duration).Seconds;
            var allMinutes = vm.Time[0] as RangeObservableCollection<object>;

            var lowerValidSeconds = vm.GetValidSeconds(allMinutes.First().ToString());
            var upperValidSeconds = vm.GetValidSeconds(allMinutes.Last().ToString());
            Assert.AreEqual(lowerValidSeconds.First().ToString(), lowerSeconds.ToString("D2"));
            Assert.AreEqual(upperValidSeconds.Last().ToString(), upperSeconds.ToString("D2"));
        }
    }
}
