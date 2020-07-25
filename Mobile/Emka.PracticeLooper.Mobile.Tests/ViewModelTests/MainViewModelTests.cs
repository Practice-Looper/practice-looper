﻿// Copyright (C)  - All Rights Reserved
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
using Emka3.PracticeLooper.Services.Contracts.Player;
using Moq;
using Xunit;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    public class MainViewModelTests
    {
        MainViewModel mainViewModel;
        Mock<IInterstitialAd> interstitialAdMock;
        Mock<IRepository<Session>> sessionsRepositoryMock;
        Mock<IRepository<Loop>> loopsRepositoryMock;
        Mock<IDialogService> dialogServiceMock;
        Mock<IFileRepository> fileRepositoryMock;
        Mock<ISourcePicker> sourcePickerMock;
        Mock<ISpotifyLoader> spotifyLoaderMock;
        Mock<Common.IFilePicker> filePickerMock;
        Mock<IConnectivityService> connectivityServiceMock;
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        List<Session> sessions;

        public MainViewModelTests()
        {
            interstitialAdMock = new Mock<IInterstitialAd>();
            sessionsRepositoryMock = new Mock<IRepository<Session>>();
            loopsRepositoryMock = new Mock<IRepository<Loop>>();
            dialogServiceMock = new Mock<IDialogService>();
            fileRepositoryMock = new Mock<IFileRepository>();
            sourcePickerMock = new Mock<ISourcePicker>();
            spotifyLoaderMock = new Mock<ISpotifyLoader>();
            filePickerMock = new Mock<Common.IFilePicker>();
            connectivityServiceMock = new Mock<IConnectivityService>();
            navigationServiceMock = new Mock<INavigationService>();
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();

            sessions = new List<Session>();
        }

        [Fact]
        public async Task When_Initialize_Expect_IsInitialized()
        {
            connectivityServiceMock.Setup(cs => cs.HasFastConnection()).Returns(true);
            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object);

            await mainViewModel.InitializeAsync(null);
            Assert.Empty(mainViewModel.Sessions);
            Assert.Null(mainViewModel.CurrentSession);
        }

        [Fact]
        public async Task When_HasNoSessionsAndCreatesSession_Expect_HasOneNewSession()
        {
            var audioSource = new AudioSource
            {
                Duration = 666,
                FileName = "fileName",
                Source = "/folder/subFolder",
                Type = AudioSourceType.Local
            };

            sessionsRepositoryMock.Setup(sr => sr.SaveAsync(It.IsAny<Session>())).Callback((Session s) => { sessions.Add(s); }).ReturnsAsync(0);
            sessionsRepositoryMock.Setup(sr => sr.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => { return sessions.FirstOrDefault(s => s.Id == id); });

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object);

            await mainViewModel.ExecuteCreateSessionCommandAsync(audioSource);
            Assert.NotNull(mainViewModel.CurrentSession);
            Assert.Single(mainViewModel.Sessions);
            Assert.Equal(mainViewModel.CurrentSession.Session.AudioSource, audioSource);

            sessionsRepositoryMock.Verify(s => s.SaveAsync(It.IsAny<Session>()), Times.Once);
        }

        [Fact]
        public async Task When_CreateWithNullAudioSource_Expect_ThrowsArgumentNullException()
        {
            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => mainViewModel.ExecuteCreateSessionCommandAsync(null));
        }

        private MainViewModel CreateDefault(IInterstitialAd interstitialAd,
            IRepository<Session> sessionsRepository,
            IRepository<Loop> loopsRepository,
            IDialogService dialogService,
            IFileRepository fileRepository,
            ISourcePicker sourcePicker,
            ISpotifyLoader spotifyLoader,
            Mobile.Common.IFilePicker filePicker,
            IConnectivityService connectivityService,
            INavigationService navigationService,
            ILogger logger,
            IAppTracker appTracker)
        {
            return new MainViewModel(interstitialAd,
                sessionsRepository,
                loopsRepository,
                dialogService,
                fileRepository,
                sourcePicker,
                spotifyLoader,
                filePicker,
                connectivityService,
                navigationService,
                logger,
                appTracker);
        }
    }
}