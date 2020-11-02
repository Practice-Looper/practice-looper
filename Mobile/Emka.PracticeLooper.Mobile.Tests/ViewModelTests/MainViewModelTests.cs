// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture()]
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
        Mock<ISpotifyApiService> spotifyApiServiceMock;
        Mock<Mobile.Common.IFilePicker> filePickerMock;
        Mock<IConnectivityService> connectivityServiceMock;
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        Mock<IConfigurationService> configurationServiceMock;
        Mock<IInAppBillingService> inAppBillingServiceMock;
        Mock<IAudioPlayer> audioPlayerMock;
        List<IAudioPlayer> audioPlayers;
        List<Session> sessions;
        List<AudioSource> audioSources;
        List<Loop> loops;
        StringLocalizer localizer;

        public MainViewModelTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            interstitialAdMock = new Mock<IInterstitialAd>();
            sessionsRepositoryMock = new Mock<IRepository<Session>>();
            loopsRepositoryMock = new Mock<IRepository<Loop>>();
            dialogServiceMock = new Mock<IDialogService>();
            fileRepositoryMock = new Mock<IFileRepository>();
            sourcePickerMock = new Mock<ISourcePicker>();
            spotifyLoaderMock = new Mock<ISpotifyLoader>();
            spotifyApiServiceMock = new Mock<ISpotifyApiService>();
            filePickerMock = new Mock<Mobile.Common.IFilePicker>();
            connectivityServiceMock = new Mock<IConnectivityService>();
            navigationServiceMock = new Mock<INavigationService>();
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();
            configurationServiceMock = new Mock<IConfigurationService>();
            inAppBillingServiceMock = new Mock<IInAppBillingService>();
            audioPlayerMock = new Mock<IAudioPlayer>();
            localizer = new StringLocalizer(loggerMock.Object);
            audioPlayers = new List<IAudioPlayer>();
            audioSources = new List<AudioSource>
            {
                new AudioSource
                {
                    FileName = "file1",
                    Type = AudioSourceType.Spotify,
                    Source = "1234567890:666",
                    Duration = 60000 / 1000
                },

                new AudioSource
                {
                    FileName = "file2",
                    Type = AudioSourceType.LocalInternal,
                    Source = "file2.wav",
                    Duration = 120000 / 1000
                }
            };

            loops = new List<Loop>
            {
                new Loop {
                            Name = audioSources.First().FileName,
                            StartPosition = 0.0,
                            EndPosition = 0.5,
                            Repititions = 0,
                            IsDefault = true
                          },
                new Loop {
                            Name = audioSources.First().FileName,
                            StartPosition = 0.3,
                            EndPosition = 1.0,
                            Repititions = 0,
                            IsDefault = false
                          }
            };

            sessions = new List<Session>
            {
                 new Session
                    {   Id=0,
                        Name = audioSources.First().FileName,
                        AudioSource = audioSources.First(),
                        Loops = new List<Loop>
                        {
                            loops.First()
                        }
                    },
                 new Session
                    {   Id=1,
                        Name = audioSources.Last().FileName,
                        AudioSource = audioSources.Last(),
                        Loops = new List<Loop>
                        {
                            loops.Last()
                        }
                    }
            };
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_InitializeHasFastConnection_Expect_IsInitialized()
        {
            connectivityServiceMock.Setup(cs => cs.HasFastConnection()).Returns(true);
            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            await mainViewModel.InitializeAsync(null);
            Assert.IsEmpty(mainViewModel.Sessions);
            Assert.Null(mainViewModel.CurrentSession);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_InitializeHas2StoredSession_Expect_2SessionLoaded()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            await mainViewModel.InitializeAsync(null);
            Assert.IsNotEmpty(mainViewModel.Sessions);
            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(2));
            Assert.NotNull(mainViewModel.CurrentSession);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_Initialize_LastSelectionId1_Expect_CurrentSessionHasId1()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.LastSession), 0)).Returns(1);

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            await mainViewModel.InitializeAsync(null);
            Assert.NotNull(mainViewModel.CurrentSession);
            Assert.AreEqual(sessions[1], mainViewModel.CurrentSession.Session);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_InitializeHasSlowConnection_Expect_DialogShown()
        {
            connectivityServiceMock
                .Setup(cs => cs.HasFastConnection())
                .Returns(false);

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SlowConnection")), It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SlowConnection"))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            configurationServiceMock
                .Setup(c => c.SetValueAsync(It.Is<string>(s => s == PreferenceKeys.SlowConnectionWarning), It.Is<bool>(b => b == true), It.Is<bool>(b => b == true)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            Assert.IsEmpty(mainViewModel.Sessions);
            Assert.Null(mainViewModel.CurrentSession);

            dialogServiceMock
                .Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SlowConnection")), It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SlowConnection"))), Times.Once);

            configurationServiceMock
                .Verify(c => c.SetValueAsync(It.Is<string>(s => s == PreferenceKeys.SlowConnectionWarning), It.Is<bool>(b => b == true), It.Is<bool>(b => b == true)), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_InitializeHasSlowConnectionAndHintAlreadyShown_Expect_DialogNotShown()
        {
            connectivityServiceMock
                .Setup(cs => cs.HasFastConnection())
                .Returns(false);

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SlowConnection")), It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SlowConnection"))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            configurationServiceMock
                .Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.SlowConnectionWarning), false))
                .Returns(true)
                .Verifiable();

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            Assert.IsEmpty(mainViewModel.Sessions);
            Assert.Null(mainViewModel.CurrentSession);

            dialogServiceMock
                .Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SlowConnection")), It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SlowConnection"))), Times.Never);

            configurationServiceMock
                .Verify(c => c.SetValueAsync(It.Is<string>(s => s == PreferenceKeys.SlowConnectionWarning), It.Is<bool>(b => b == true), It.Is<bool>(b => b == true)), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_PickSourceSpotify_SpotifyNotAuthorized_Expect_AuthSpotify()
        {
            var tcs = new TaskCompletionSource<bool>();
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.Spotify));
            spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(false);
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Callback(() => { Task.Run(() => tcs.SetResult(true)); })
                .Returns(Task.FromResult(true))
                .Verifiable();

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            await tcs.Task;

            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_PickSourceSpotify_SpotifyAuthorized_Expect_NavigateToSpotifySearchView()
        {
            var tcs = new TaskCompletionSource<bool>();
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.Spotify));
            spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(true);
            navigationServiceMock
                .Setup(n => n.NavigateToAsync<SpotifySearchViewModel>())
                .Callback(() => { Task.Run(() => tcs.SetResult(true)); })
                .Returns(Task.CompletedTask)
                .Verifiable();

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            await tcs.Task;

            navigationServiceMock.Verify(n => n.NavigateToAsync<SpotifySearchViewModel>(), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_PickSourceLocal_UserCancelled_Expect_NoSessionCreated()
        {
            var tcs = new TaskCompletionSource<bool>();
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.LocalInternal));
            filePickerMock
                .Setup(f => f.ShowPicker())
                .Callback(() => { tcs.SetResult(true); })
                .Returns(Task.FromResult<AudioSource>(null));

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            await tcs.Task;
            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(0));
            filePickerMock
                .Verify(f => f.ShowPicker(), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_PickSourceLocal_UserSelectsFile_Expect_OneSessionCreated()
        {
            var audioSource = new AudioSource
            {
                Type = AudioSourceType.LocalInternal,
                Source = "source",
                Duration = 240,
                FileName = "audio.mp3"
            };

            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.LocalInternal));
            filePickerMock
                .Setup(f => f.ShowPicker())
                .Returns(Task.FromResult(audioSource));

            configurationServiceMock.Setup(c => c.GetSecureValue<bool>(It.Is<string>(s => s == PreferenceKeys.PremiumGeneral))).Returns(true);
            sessionsRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Session>())).Returns(Task.FromResult(1));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            //audioPlayerMock.Setup(a => a.PlayAsync())
            //    .Callback(() => { tcs.SetResult(true); })
            //    .Returns(Task.CompletedTask);
            //audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>())).Returns(Task.CompletedTask);

            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            while (mainViewModel.Sessions.Count == 0)
            {
                await Task.Delay(200);
            }

            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(1));
        }

        [Test()]
        public async Task When_HasNoSessionsAndCreatesSession_Expect_HasOneNewSession()
        {
            var audioSource = new AudioSource
            {
                Duration = 666,
                FileName = "fileName",
                Source = "/folder/subFolder",
                Type = AudioSourceType.LocalInternal
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
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            await mainViewModel.ExecuteCreateSessionCommandAsync(audioSource);
            Assert.NotNull(mainViewModel.CurrentSession);
            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(1));
            Assert.AreEqual(mainViewModel.CurrentSession.Session.AudioSource, audioSource);

            sessionsRepositoryMock.Verify(s => s.SaveAsync(It.IsAny<Session>()), Times.Once);
        }

        [Test()]
        public void When_CreateWithNullAudioSource_Expect_ThrowsArgumentNullException()
        {
            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            Assert.ThrowsAsync<ArgumentNullException>(() => mainViewModel.ExecuteCreateSessionCommandAsync(null));
        }

        [Test()]
        public void When_PauseInvoked_Expect_SpotifyDisconnects()
        {
            spotifyLoaderMock.Setup(s => s.Disconnect()).Callback(() => spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(true)).Verifiable();
            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            mainViewModel.Pause();
            spotifyLoaderMock.Verify(s => s.Disconnect(), Times.Once);
        }

        [Test()]
        public void When_PauseInvoked_Expect_PlayerPauses()
        {
            Assert.Ignore("Test in different test!!");
            audioPlayerMock.Setup(a => a.Pause(default)).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            mainViewModel = CreateDefault(interstitialAdMock.Object,
                sessionsRepositoryMock.Object,
                loopsRepositoryMock.Object,
                dialogServiceMock.Object,
                fileRepositoryMock.Object,
                sourcePickerMock.Object,
                spotifyLoaderMock.Object,
                spotifyApiServiceMock.Object,
                filePickerMock.Object,
                connectivityServiceMock.Object,
                navigationServiceMock.Object,
                loggerMock.Object,
                appTrackerMock.Object,
                configurationServiceMock.Object,
                inAppBillingServiceMock.Object,
                audioPlayers);

            mainViewModel.Pause();
            audioPlayerMock.Verify(s => s.Pause(default), Times.Once);
        }

        private MainViewModel CreateDefault(IInterstitialAd interstitialAd,
            IRepository<Session> sessionsRepository,
            IRepository<Loop> loopsRepository,
            IDialogService dialogService,
            IFileRepository fileRepository,
            ISourcePicker sourcePicker,
            ISpotifyLoader spotifyLoader,
            ISpotifyApiService spotifyApiService,
            Mobile.Common.IFilePicker filePicker,
            IConnectivityService connectivityService,
            INavigationService navigationService,
            ILogger logger,
            IAppTracker appTracker,
            IConfigurationService configurationService,
            IInAppBillingService inAppBillingService,
            IEnumerable<IAudioPlayer> audioPlayers)
        {
            return new MainViewModel(interstitialAd,
                sessionsRepository,
                loopsRepository,
                dialogService,
                fileRepository,
                sourcePicker,
                spotifyLoader,
                spotifyApiService,
                filePicker,
                connectivityService,
                navigationService,
                logger,
                appTracker,
                configurationService,
                inAppBillingService,
                audioPlayers);
        }
    }
}
