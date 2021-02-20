// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

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
        Mock<Common.IFilePicker> filePickerMock;
        Mock<IConnectivityService> connectivityServiceMock;
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        Mock<IConfigurationService> configurationServiceMock;
        Mock<IAudioPlayer> audioPlayerMock;
        Mock<IFeatureRegistry> featureRegistryMock;
        List<IAudioPlayer> audioPlayers;
        List<Session> sessions;
        List<AudioSource> audioSources;
        List<Loop> loops;
        StringLocalizer localizer;
        List<SessionViewModel> sessionViewModels;
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
            audioPlayerMock = new Mock<IAudioPlayer>();
            featureRegistryMock = new Mock<IFeatureRegistry>();
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

            sessionViewModels = new List<SessionViewModel>
            {
                new SessionViewModel(sessions.First(), dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object),
                new SessionViewModel(sessions.Last(), dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object)
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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

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
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);
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
                audioPlayers,
                featureRegistryMock.Object);

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
        public async Task When_PickSourceSpotify_AuthorizationFails_Expect_ShowsDialog()
        {
            var tcs = new TaskCompletionSource<bool>();
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.Spotify));
            spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(false);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(false))
                .Verifiable();
            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_CouldNotConnectToSpotify")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))))
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            await tcs.Task;

            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Once);
            dialogServiceMock
                .Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_CouldNotConnectToSpotify")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_PickSourceSpotify_SpotifyAuthorized_Expect_NavigateToSpotifySearchView()
        {
            var tcs = new TaskCompletionSource<bool>();
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.Spotify));
            spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(true);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);
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
                audioPlayers,
                featureRegistryMock.Object);

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
        public async Task When_Droid_PickSourceSpotify_SpotifyNotInstalled_Expect_ShowsDialog()
        {
            var tcs = new TaskCompletionSource<bool>();
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.Spotify));
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(false);
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.Droid);

            dialogServiceMock
                .Setup(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))))
                .Callback(() => { Task.Run(() => tcs.SetResult(true)); })
                .Returns(Task.FromResult(false))
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            await tcs.Task;

            spotifyLoaderMock.Verify(s => s.IsSpotifyInstalled(), Times.Once);
            dialogServiceMock
                .Verify(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_Droid_PickSourceSpotify_SpotifyInstalled_Expect_NoDialog()
        {
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.Spotify));
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            spotifyLoaderMock.Setup(s => s.InstallSpotify()).Verifiable();
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<CurrentPlatform>(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.Droid);
            dialogServiceMock
                .Setup(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))))
                .Returns(Task.FromResult(false))
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            spotifyLoaderMock.Verify(s => s.IsSpotifyInstalled(), Times.Once);
            spotifyLoaderMock.Verify(s => s.InstallSpotify(), Times.Never);
            dialogServiceMock
                .Verify(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_iOS_PickSourceSpotify_SpotifyNotInstalled_NoSpotifyInstallationDialogShown()
        {
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);
            sourcePickerMock.Setup(s => s.SelectFileSource()).Returns(Task.FromResult(AudioSourceType.Spotify));
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            spotifyLoaderMock.Setup(s => s.InstallSpotify())
                .Verifiable();

            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(false);
            dialogServiceMock
                .Setup(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))))
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PickSourceCommand.Execute(null);

            spotifyLoaderMock.Verify(s => s.IsSpotifyInstalled(), Times.Never);
            spotifyLoaderMock.Verify(s => s.InstallSpotify(), Times.Never);
            dialogServiceMock
                .Verify(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))), Times.Never);
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
                audioPlayers,
                featureRegistryMock.Object);

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

            sessionsRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Session>())).Returns(Task.FromResult(1));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);

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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

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
                audioPlayers,
                featureRegistryMock.Object);

            mainViewModel.Pause();
            audioPlayerMock.Verify(s => s.Pause(default), Times.Once);
        }

        [Test()]
        public void When_CurrentPlayerNull_Expect_CanNotExecutePlayCommand()
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
                audioPlayers,
                featureRegistryMock.Object);

            var canExecutePlayCommand = mainViewModel.PlayCommand.CanExecute(null);
            Assert.IsNull(mainViewModel.CurrentAudioPlayer);
            Assert.IsFalse(canExecutePlayCommand);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_PlayerPlaying_Expect_PausesAndShowsAd()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(true);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.Setup(a => a.Pause(It.Is<bool>(b => b))).Callback((bool b) =>
            {
                audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
                audioPlayerMock.Raise(a => a.PlayStatusChanged += null, null, false);
                tcs.SetResult(true);
            });

            audioPlayers.Add(audioPlayerMock.Object);
            interstitialAdMock
                .Setup(a => a.ShowAdAsync())
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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;
            Assert.IsFalse(mainViewModel.IsPlaying);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_NotPlaying_Expect_PlayerInitializes()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock
                .Setup(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null))
                .Returns(Task.CompletedTask)
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Verifiable();

            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);

            audioPlayers.Add(audioPlayerMock.Object);

            interstitialAdMock
                .Setup(a => a.ShowAdAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(true);

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;
            audioPlayerMock.Verify(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null), Times.Once);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_FileNotFoundException_Expect_ShowsDialogButNoDelete()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock
                .Setup(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null))
                .Throws(new FileNotFoundException())
                .Verifiable();

            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);

            dialogServiceMock
                .Setup(d => d.ShowConfirmAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_FileNotFound")), It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")), It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))))
                .Returns(Task.FromResult(false))
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Verifiable();

            audioPlayers.Add(audioPlayerMock.Object);

            interstitialAdMock
                .Setup(a => a.ShowAdAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(true);

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;
            audioPlayerMock.Verify(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null), Times.Once);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
            dialogServiceMock
               .Verify(d => d.ShowConfirmAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_FileNotFound")), It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")), It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_FileNotFoundException_Expect_ShowsDialogAndDeletes()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock
                .Setup(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null))
                .Throws(new FileNotFoundException())
                .Verifiable();

            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);

            dialogServiceMock
                .Setup(d => d.ShowConfirmAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_FileNotFound")), It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")), It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))))
                .Returns(Task.FromResult(true))
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Verifiable();

            audioPlayers.Add(audioPlayerMock.Object);

            interstitialAdMock
                .Setup(a => a.ShowAdAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(true);

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;

            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(1));
            audioPlayerMock.Verify(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null), Times.Once);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
            dialogServiceMock
               .Verify(d => d.ShowConfirmAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_FileNotFound")), It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")), It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_Exception_Expect_ShowsDialog()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);

            audioPlayerMock
                .Setup(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null))
                .Throws(new Exception())
                .Verifiable();

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_General")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))))
                .Returns(Task.FromResult(true))
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Verifiable();

            audioPlayers.Add(audioPlayerMock.Object);

            interstitialAdMock
                .Setup(a => a.ShowAdAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(true);

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
                audioPlayers,
                featureRegistryMock.Object);

            mainViewModel.IsPlaying = false;
            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;

            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(2));
            audioPlayerMock.Verify(a => a.InitAsync(It.Is<Loop>(l => l == loops.First()), false, null), Times.Once);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
            dialogServiceMock
               .Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_General")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_PremiumChecked_Expect_NoSpotifyPremiumStateCheck()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(true);

            audioPlayerMock
                .Setup(a => a.PlayAsync())
                .Returns(Task.CompletedTask)
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Verifiable();

            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);

            audioPlayers.Add(audioPlayerMock.Object);
            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(true);

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;
            audioPlayerMock.Verify(a => a.PlayAsync(), Times.Once);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
            spotifyApiServiceMock.Verify(s => s.IsPremiumUser(), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_ChecksSpotifyPremiumStateOk_Expect_PlaysSong()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(true);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);

            audioPlayerMock
                .Setup(a => a.PlayAsync())
                .Returns(Task.CompletedTask)
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Verifiable();

            audioPlayers.Add(audioPlayerMock.Object);
            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(false);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;
            audioPlayerMock.Verify(a => a.PlayAsync(), Times.Once);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_ChecksSpotifyPremiumStateOk_NoPremium_Expect_DoesntPlaySong()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(true);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);

            audioPlayerMock
                .Setup(a => a.PlayAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            audioPlayers.Add(audioPlayerMock.Object);
            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(false);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, false)));

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;
            audioPlayerMock.Verify(a => a.PlayAsync(), Times.Never);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_ChecksSpotifyPremiumStateNotOk_Expect_DoesntPlaySong()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(true);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(true);
            audioPlayerMock
                .Setup(a => a.PlayAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            audioPlayers.Add(audioPlayerMock.Object);
            spotifyApiServiceMock.SetupGet(s => s.UserPremiumCheckSuccessful).Returns(false);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Callback(() =>
                {
                    tcs.SetResult(true);
                })
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.NotFound, false)));

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            await tcs.Task;
            audioPlayerMock.Verify(a => a.PlayAsync(), Times.Never);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExcutePlayCommand_SpotifyNotInstalled_Expect_DoesntPlaySong()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            //audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(false);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            //audioPlayerMock.SetupGet(a => a.Initialized).Returns(true);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            configurationServiceMock.Setup(c => c.GetValue<bool>(It.Is<string>(s => s == PreferenceKeys.IsSpotifyInstalled), false)).Returns(false);
            audioPlayerMock
                .Setup(a => a.PlayAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.PlayCommand.Execute(null);
            audioPlayerMock.Verify(a => a.PlayAsync(), Times.Never);
            interstitialAdMock.Verify(a => a.ShowAdAsync(), Times.Never);
            spotifyLoaderMock.Verify(s => s.IsSpotifyInstalled(), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_Droid_HandleSpotifyPlayerInitialization_SpotifyNotInstalled_Expect_Dialog()
        {
            var tcs = new TaskCompletionSource<bool>();
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.Droid);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));

            dialogServiceMock
                .Setup(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))))
                .Callback(() => { Task.Run(() => tcs.SetResult(true)); })
                .Returns(Task.FromResult(false))
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            var result = await mainViewModel.HandleSpotifyPlayerInitialization();
            await tcs.Task;
            Assert.False(result);
            dialogServiceMock
                .Verify(d => d.ShowConfirmAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Caption_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Hint_Content_SpotifyMissing")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Ok"))), Times.Once);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_Droid_HandleSpotifyPlayerInitialization_SpotifyInstalled_Expect_Initializes()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.Droid);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>(), false, null)).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            var result = await mainViewModel.HandleSpotifyPlayerInitialization();
            Assert.True(result);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
            audioPlayerMock.Verify(a => a.InitAsync(It.IsAny<Loop>(), false, null), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_iOS_HandleSpotifyPlayerInitialization_SpotifyInstalled_Expect_Initializes()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>(), false, null)).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            var result = await mainViewModel.HandleSpotifyPlayerInitialization();
            Assert.True(result);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
            audioPlayerMock.Verify(a => a.InitAsync(It.IsAny<Loop>(), false, null), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_iOS_HandleSpotifyPlayerInitialization_SpotifyNotInstalled_PalyerNotLoaded_Expect_LoadsPlayer()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));
            spotifyApiServiceMock.
                Setup(a => a.GetAvailableDevices())
                .Returns(Task.FromResult(new List<SpotifyDevice> { new SpotifyDevice("identifier", "Mobile Web Player", true, "Smartphone") }));

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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }
            var initTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.WebViewInit, (s) =>
            {
                Task.Run(() => { initTcs.SetResult(true); });
            });

            var loadTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyLoadWebPlayer, (s) =>
            {
                Task.Run(() => { loadTcs.SetResult(true); });
                MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, true);
            });

            var activateTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyActivatePlayer, s =>
            {
                Task.Run(() => { activateTcs.SetResult(true); });
                MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, true);
            });

            await mainViewModel.InitializeAsync(null);
            var result = await mainViewModel.HandleSpotifyPlayerInitialization();
            await initTcs.Task;
            await loadTcs.Task;
            await activateTcs.Task;

            Assert.True(result);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
            audioPlayerMock.Verify(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.Once);
            spotifyApiServiceMock.Verify(s => s.IsPremiumUser(), Times.Once);
            spotifyApiServiceMock.Verify(a => a.GetAvailableDevices(), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_iOS_HandleSpotifyPlayerInitialization_SpotifyNotInstalled_PalyerLoaded_Expect_DoesNotLoadPlayer()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));
            spotifyApiServiceMock.
                Setup(a => a.GetAvailableDevices())
                .Returns(Task.FromResult(new List<SpotifyDevice> { new SpotifyDevice("identifier", "Mobile Web Player", true, "Smartphone") }));

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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            var activateTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyActivatePlayer, s =>
            {
                Task.Run(() => { activateTcs.SetResult(true); });
                MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, true);
            });

            await mainViewModel.InitializeAsync(null);
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, true);
            var result = await mainViewModel.HandleSpotifyPlayerInitialization();
            await activateTcs.Task;

            Assert.True(result);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
            audioPlayerMock.Verify(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.Once);
            spotifyApiServiceMock.Verify(s => s.IsPremiumUser(), Times.Once);
            spotifyApiServiceMock.Verify(a => a.GetAvailableDevices(), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_iOS_HandleSpotifyPlayerInitialization_SpotifyNotInstalled_PalyerLoadedActivated_Expect_DoesNotActivatePlayer()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));
            spotifyApiServiceMock.
                Setup(a => a.GetAvailableDevices())
                .Returns(Task.FromResult(new List<SpotifyDevice> { new SpotifyDevice("identifier", "Mobile Web Player", true, "Smartphone") }));

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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, true);
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, true);
            var result = await mainViewModel.HandleSpotifyPlayerInitialization();

            Assert.True(result);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
            audioPlayerMock.Verify(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.Once);
            spotifyApiServiceMock.Verify(s => s.IsPremiumUser(), Times.Once);
            spotifyApiServiceMock.Verify(a => a.GetAvailableDevices(), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_iOS_HandleSpotifyPlayerInitialization_SpotifyNotInstalled_PalyerLoadedActivated_NoActiveDevice_Expect_DialogHint()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));
            spotifyApiServiceMock.
                Setup(a => a.GetAvailableDevices())
                .Returns(Task.FromResult(new List<SpotifyDevice>()));
            dialogServiceMock
                .Setup(dialogServiceMock => dialogServiceMock.ShowAlertAsync(localizer.GetLocalizedString("Error_Content_NoActivePlayer"), localizer.GetLocalizedString("Error_Caption")))
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, true);
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, true);
            var result = await mainViewModel.HandleSpotifyPlayerInitialization();

            Assert.False(result);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
            audioPlayerMock.Verify(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.Never);
            spotifyApiServiceMock.Verify(s => s.IsPremiumUser(), Times.Never);
            spotifyApiServiceMock.Verify(a => a.GetAvailableDevices(), Times.AtLeastOnce);
            dialogServiceMock
                .Verify(dialogServiceMock => dialogServiceMock.ShowAlertAsync(localizer.GetLocalizedString("Error_Content_NoActivePlayer"), localizer.GetLocalizedString("Error_Caption")), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_iOS_HandleSpotifyPlayerInitialization_SpotifyNotInstalled_PalyerLoadedNotActivated_NoActiveDevice_Expect_DialogHint()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(false);
            spotifyLoaderMock.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            audioPlayerMock.SetupGet(a => a.Initialized).Returns(false);
            audioPlayerMock.Setup(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock
                .Setup(s => s.IsPremiumUser())
                .Returns(Task.FromResult(Tuple.Create(System.Net.HttpStatusCode.OK, true)));
            spotifyApiServiceMock.
                Setup(a => a.GetAvailableDevices())
                .Returns(Task.FromResult(new List<SpotifyDevice>()));
            dialogServiceMock
                .Setup(dialogServiceMock => dialogServiceMock.ShowAlertAsync(localizer.GetLocalizedString("Error_Content_NoActivePlayer"), localizer.GetLocalizedString("Error_Caption")))
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            var activateTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyActivatePlayer, s =>
            {
                Task.Run(() => { activateTcs.SetResult(true); });
                MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, false);
            });

            await mainViewModel.InitializeAsync(null);
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, true);

            var result = await mainViewModel.HandleSpotifyPlayerInitialization();
            await activateTcs.Task;

            Assert.False(result);
            spotifyLoaderMock.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);
            audioPlayerMock.Verify(a => a.InitAsync(It.IsAny<Loop>(), true, It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.Never);
            spotifyApiServiceMock.Verify(s => s.IsPremiumUser(), Times.Never);
            spotifyApiServiceMock.Verify(a => a.GetAvailableDevices(), Times.AtLeastOnce);
            dialogServiceMock
                .Verify(dialogServiceMock => dialogServiceMock.ShowAlertAsync(localizer.GetLocalizedString("Error_Content_NoActivePlayer"), localizer.GetLocalizedString("Error_Caption")), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_UsesWebPlayer_PlaysSong_TogglesPlayerReload_Expect_Pauses()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);

            audioPlayerMock.SetupGet(a => a.Initialized).Returns(true);
            audioPlayerMock.Setup(a => a.Pause(It.Is<bool>(b => b)));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(true);
            audioPlayerMock.SetupGet(a => a.UsesWebPlayer).Returns(true);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);

            MessagingCenter.Send<object>(this, MessengerKeys.WebViewRefreshInitialized);

            audioPlayerMock.Verify(a => a.Pause(It.Is<bool>(b => b)), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_PlayerNull_TogglesPlayerReload_Expect_PauseNotInvoked()
        {
            configurationServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "Platform"), It.IsAny<CurrentPlatform>())).Returns(CurrentPlatform.iOS);

            audioPlayerMock.SetupGet(a => a.Initialized).Returns(true);
            audioPlayerMock.Setup(a => a.Pause(It.Is<bool>(b => b)));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(true);
            audioPlayerMock.SetupGet(a => a.UsesWebPlayer).Returns(true);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);

            MessagingCenter.Send<object>(this, MessengerKeys.WebViewRefreshInitialized);

            audioPlayerMock.Verify(a => a.Pause(It.Is<bool>(b => b)), Times.Never);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_ExecuteToggleSpotifyWebPlayerCommand_TogglesPlayerReload_Expect_ChangesVisibility(bool visible)
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            mainViewModel.IsSpotifyWebPlayerVisible = visible;
            mainViewModel.ToggleSpotifyWebPlayerCommand.Execute(null);
            Assert.AreEqual(mainViewModel.IsSpotifyWebPlayerVisible, !visible);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_DeleteSession_NullArgument_Expect_ThrowsArgumentException()
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
                audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            Assert.ThrowsAsync<ArgumentException>(() => mainViewModel.ExecuteDeleteSessionCommandAsync(null));
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExecuteDeleteSessionCommand_PlayingSessionToDelete_Expect_Pauses()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            audioPlayerMock.Setup(a => a.Pause(true)).Callback(() => { Task.Run(() => tcs.SetResult(true)); }).Verifiable();
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            spotifyApiServiceMock.SetupGet(a => a.UserPremiumCheckSuccessful).Returns(true);
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecutePlayCommand(null);
            audioPlayerMock.Raise(a => a.PlayStatusChanged += null, null, true);
            await mainViewModel.ExecuteDeleteSessionCommandAsync(mainViewModel.CurrentSession);
            await tcs.Task;
            audioPlayerMock.Verify(a => a.Pause(true), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExecuteDeleteSessionCommand_LocalFile_Expect_DeletesFromDbAndFileSystem()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            spotifyApiServiceMock.SetupGet(a => a.UserPremiumCheckSuccessful).Returns(true);

            sessionsRepositoryMock.Setup(s => s.DeleteAsync(It.IsAny<Session>())).Returns(Task.CompletedTask);
            fileRepositoryMock
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .Callback(() => { tcs.SetResult(true); })
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecuteDeleteSessionCommandAsync(mainViewModel.Sessions[1]);
            await tcs.Task;
            sessionsRepositoryMock.Verify(s => s.DeleteAsync(It.IsAny<Session>()), Times.Once);
            fileRepositoryMock.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Once);
            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(1));
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExecuteDeleteSessionCommand_SpotifySession_Expect_DeletesFromDb()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            spotifyApiServiceMock.SetupGet(a => a.UserPremiumCheckSuccessful).Returns(true);

            sessionsRepositoryMock
                .Setup(s => s.DeleteAsync(It.IsAny<Session>()))
                .Callback(() => { tcs.SetResult(true); })
                .Returns(Task.CompletedTask);

            fileRepositoryMock
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecuteDeleteSessionCommandAsync(mainViewModel.Sessions[0]);
            await tcs.Task;
            sessionsRepositoryMock.Verify(s => s.DeleteAsync(It.IsAny<Session>()), Times.Once);
            fileRepositoryMock.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(1));
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExecuteDeleteSessionCommand_Has2Sessions_Expect_CurrentSessionIsNotNull()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecuteDeleteSessionCommandAsync(mainViewModel.Sessions[0]);
            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(1));
            Assert.NotNull(mainViewModel.CurrentSession);
            Assert.AreEqual(sessionViewModels[1].Session.Id, mainViewModel.CurrentSession.Session.Id);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExecuteDeleteSessionCommand_Has1Session_Expect_CurrentSessionIsNull_LastLoopAndLastSessionCleared()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessions.RemoveAt(1);
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(sessions.First().AudioSource.Type);
            audioPlayers.Add(audioPlayerMock.Object);

            configurationServiceMock
                .Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.LastSession), It.Is<int>(i => i == -1)))
                .Returns(sessions.First().Id)
                .Verifiable();
            configurationServiceMock
                .Setup(c => c.ClearValue(It.Is<string>(s => s == PreferenceKeys.LastSession)))
                .Verifiable();
            configurationServiceMock
                .Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.LastLoop), It.Is<int>(i => i == -1)))
                .Returns(sessions.First().Loops.First().Id)
                .Verifiable();
            configurationServiceMock
                .Setup(c => c.ClearValue(It.Is<string>(s => s == PreferenceKeys.LastLoop)))
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.InitAudioPlayer(false);
            await mainViewModel.ExecuteDeleteSessionCommandAsync(mainViewModel.Sessions.First());

            Assert.That(mainViewModel.Sessions, Has.Count.EqualTo(0));
            Assert.Null(mainViewModel.CurrentSession);
            Assert.Null(mainViewModel.CurrentLoop);

            configurationServiceMock
               .Verify(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.LastSession), It.Is<int>(i => i == -1)), Times.Once);
            configurationServiceMock
                .Verify(c => c.ClearValue(It.Is<string>(s => s == PreferenceKeys.LastSession)), Times.Once);
            configurationServiceMock
                .Verify(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.LastLoop), It.Is<int>(i => i == -1)), Times.Once);
            configurationServiceMock
                .Verify(c => c.ClearValue(It.Is<string>(s => s == PreferenceKeys.LastLoop)), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_ExecuteDeleteSessionCommand_ThrowsException_Expect_DialogShown()
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            sessionsRepositoryMock.Setup(s => s.DeleteAsync(It.IsAny<Session>())).Throws(new Exception());
            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_CouldNotDeleteSong")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))))
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecuteDeleteSessionCommandAsync(mainViewModel.Sessions[0]);
            dialogServiceMock
                .Verify(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_CouldNotDeleteSong")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_CurrentSessionIsNull_Expect_CanNotExecuteAddLoopCommand()
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            var canExecute = mainViewModel.CanExecuteAddNewLoopCommand(null);
            Assert.IsFalse(canExecute);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_CurrentSessionIsNotNull_Expect_CanExecuteAddLoopCommand()
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            var canExecute = mainViewModel.CanExecuteAddNewLoopCommand(null);
            Assert.IsTrue(canExecute);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_AddNewLoop_UserEntersNoName_Expect_NameIs1Loop()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(sessions.First().AudioSource.Type);
            audioPlayers.Add(audioPlayerMock.Object);
            dialogServiceMock
                .Setup(d => d.ShowPromptAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Prompt_Caption_NewLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Prompt_Content_NewLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Save")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == string.Format(localizer.GetLocalizedString("Prompt_Content_NewLoop_NamePlaceholder"), sessions[0].Loops.Count)),
                    250))
                .Returns(Task.FromResult(string.Empty))
                .Verifiable();

            loopsRepositoryMock
                .Setup(l => l.GetByIdAsync(It.Is<int>(id => id == sessions[0].Loops.First().Id)))
                .Returns(Task.FromResult(sessions[0].Loops.First()));

            loopsRepositoryMock
                .Setup(l => l.SaveAsync(It.IsAny<Loop>()))
                .Returns(Task.FromResult(2));

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecuteAddNewLoopCommand(null);
            Assert.That(mainViewModel.CurrentSession.Session.Loops, Has.Count.EqualTo(2));
            Assert.NotNull(mainViewModel.CurrentLoop);
            Assert.AreEqual(mainViewModel.CurrentLoop.SessionId, mainViewModel.CurrentSession.Session.Id);
            Assert.AreEqual(mainViewModel.CurrentLoop.Name, "1. Loop");
            Assert.AreEqual(mainViewModel.CurrentLoop.Id, 2);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_AddNewLoop_UserEntersMyLoopName_Expect_NameIsMyLoop()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(sessions.First().AudioSource.Type);
            audioPlayers.Add(audioPlayerMock.Object);
            dialogServiceMock
                .Setup(d => d.ShowPromptAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Prompt_Caption_NewLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Prompt_Content_NewLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Save")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == string.Format(localizer.GetLocalizedString("Prompt_Content_NewLoop_NamePlaceholder"), sessions[0].Loops.Count)),
                    250))
                .Returns(Task.FromResult("MyLoop"));

            loopsRepositoryMock
                .Setup(l => l.GetByIdAsync(It.Is<int>(id => id == sessions[0].Loops.First().Id)))
                .Returns(Task.FromResult(sessions[0].Loops.First()));

            loopsRepositoryMock
                .Setup(l => l.SaveAsync(It.IsAny<Loop>()))
                .Returns(Task.FromResult(2));

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecuteAddNewLoopCommand(null);
            Assert.That(mainViewModel.CurrentSession.Session.Loops, Has.Count.EqualTo(2));
            Assert.NotNull(mainViewModel.CurrentLoop);
            Assert.AreEqual(mainViewModel.CurrentLoop.SessionId, mainViewModel.CurrentSession.Session.Id);
            Assert.AreEqual(mainViewModel.CurrentLoop.Name, "MyLoop");
            Assert.AreEqual(mainViewModel.CurrentLoop.Id, 2);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_AddNewLoop_ExceptionThrown_Expect_DialogShown()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(sessions.First().AudioSource.Type);
            audioPlayers.Add(audioPlayerMock.Object);

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_CouldNotCreateLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            dialogServiceMock
                .Setup(d => d.ShowPromptAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Prompt_Caption_NewLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Prompt_Content_NewLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Save")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Cancel")),
                    It.Is<string>(s => s == string.Format(localizer.GetLocalizedString("Prompt_Content_NewLoop_NamePlaceholder"), sessions[0].Loops.Count)),
                    250))
                .Throws(new Exception());

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecuteAddNewLoopCommand(null);
            dialogServiceMock
                .Verify(d => d.ShowAlertAsync(
                    It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_CouldNotCreateLoop")),
                    It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_InitAudioPlayer_IsPlaying_Expect_Pauses()
        {
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            spotifyApiServiceMock.SetupGet(a => a.UserPremiumCheckSuccessful).Returns(true);
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(sessions.First().AudioSource.Type);
            audioPlayers.Add(audioPlayerMock.Object);

            audioPlayerMock.Setup(a => a.Pause(false)).Verifiable();

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecutePlayCommand(null);
            audioPlayerMock.Raise(a => a.PlayStatusChanged += null, null, true);
            mainViewModel.LoadAudioPlayer();
            audioPlayerMock.Verify(a => a.Pause(false), Times.Once);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_InitAudioPlayer_CurrentSessionNotNull_Expect_AllValuesSet()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(sessions.First().AudioSource.Type);
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.InitAudioPlayer(false);
            Assert.NotNull(mainViewModel.CurrentAudioPlayer);
            Assert.AreEqual(mainViewModel.CurrentAudioPlayer, audioPlayerMock.Object);
            Assert.AreEqual(mainViewModel.CurrentLoop, mainViewModel.CurrentSession.Session.Loops.First());
            Assert.AreEqual(mainViewModel.MinimumValue, mainViewModel.CurrentLoop.StartPosition);
            Assert.AreEqual(mainViewModel.MaximumValue, mainViewModel.CurrentLoop.EndPosition);
            Assert.AreEqual(mainViewModel.SongDuration, TimeSpan.FromMilliseconds(mainViewModel.CurrentSession.Session.AudioSource.Duration * 1000).ToString(@"mm\:ss"));
            Assert.AreEqual(mainViewModel.CurrentSongTime, TimeSpan.FromMilliseconds(mainViewModel.CurrentSession.Session.AudioSource.Duration * 1000 * mainViewModel.MinimumValue).ToString(@"mm\:ss"));

        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_InitAudioPlayer_CurrentSessionNull_Expect_DefaultValues()
        {
            audioPlayerMock.SetupGet(a => a.Types).Returns(sessions.First().AudioSource.Type);
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.LoadAudioPlayer();
            Assert.Null(mainViewModel.CurrentAudioPlayer);
            Assert.Null(mainViewModel.CurrentLoop);
            Assert.AreEqual(mainViewModel.MinimumValue, 0.0);
            Assert.AreEqual(mainViewModel.MaximumValue, 1.0);
            Assert.AreEqual(mainViewModel.SongDuration, TimeSpan.FromMilliseconds(0.0).ToString(@"mm\:ss"));
            Assert.AreEqual(mainViewModel.CurrentSongTime, TimeSpan.FromMilliseconds(0.0).ToString(@"mm\:ss"));
        }

        [TestCase(true)]
        [TestCase(false)]
        [Apartment(ApartmentState.STA)]
        public async Task When_PlayStatusChangedRaised_Expect_IsPlayingChanges(bool isPlaying)
        {
            var tcs = new TaskCompletionSource<bool>();
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecutePlayCommand(null);
            audioPlayerMock.Raise(a => a.PlayStatusChanged += null, null, isPlaying);
            await Task.Delay(500);
            Assert.AreEqual(mainViewModel.IsPlaying, isPlaying);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_CurrentTimePositionChangedRaised_Expect_CurrentTimeSetCorrectly([Range(0.0, 1.0, 0.01)] double currentPosition)
        {
            var tcs = new TaskCompletionSource<bool>();
            spotifyLoaderMock.Setup(s => s.IsSpotifyInstalled()).Returns(true);
            spotifyApiServiceMock.SetupGet(a => a.UserPremiumCheckSuccessful).Returns(true);
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock
                .Setup(a => a.GetCurrentPosition(It.IsAny<Action<double>>()))
                .Callback((Action<double> a) =>
                {
                    a.Invoke(currentPosition);
                    tcs.SetResult(true);
                });

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecutePlayCommand(null);
            audioPlayerMock.Raise(a => a.CurrentTimePositionChanged += null, null, EventArgs.Empty);
            await tcs.Task;
            Assert.AreEqual(mainViewModel.CurrentSongTime, TimeSpan.FromMilliseconds(currentPosition).ToString(@"mm\:ss"));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_LoopChanged_DifferentLoop_Expect_CurrentLoopIsReceivedLoop()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
            var tcs = new TaskCompletionSource<bool>();
            sessions.First().Loops.Add(loops.Last());
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);

            var sessionViewModel = new SessionViewModel(sessions.First(), dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            await sessionDetailsViewModel.InitializeAsync(sessionViewModel);

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            sessionDetailsViewModel.SelectedLoop = sessionDetailsViewModel.Loops.Last();
            Assert.AreEqual(mainViewModel.CurrentLoop, mainViewModel.CurrentSession.Session.Loops.Last());
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_LoopChanged_SameLoopAsCurrent_Expect_CurrentLoopNotChanged()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
            var tcs = new TaskCompletionSource<bool>();
            sessions.First().Loops.Add(loops.Last());
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);

            var sessionViewModel = new SessionViewModel(sessions.First(), dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            await sessionDetailsViewModel.InitializeAsync(sessionViewModel);

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            sessionDetailsViewModel.SelectedLoop = sessionDetailsViewModel.Loops.First();
            Assert.AreEqual(mainViewModel.CurrentLoop, mainViewModel.CurrentSession.Session.Loops.First());
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_LoopDeleted_PlayerIsPlaying_Expect_PlayerPauses()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
            var tcs = new TaskCompletionSource<bool>();
            loops.First().Session = sessions.First();
            loops.Last().Session = sessions.First();
            sessions.First().Loops.Add(loops.Last());
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(true);
            audioPlayerMock.Setup(a => a.Pause(false)).Verifiable();

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await sessionDetailsViewModel.InitializeAsync(sessionViewModels.First());

            sessionDetailsViewModel.Loops.Last().DeleteCommand.Execute(null);
            audioPlayerMock.Verify(a => a.Pause(false), Times.Once);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_LoopDeleted_DeletedLoopNotEqualCurrentLoop_Expect_LoopDeleted()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
            var tcs = new TaskCompletionSource<bool>();
            sessions.First().Loops.Add(loops.Last());
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            loopsRepositoryMock
                .Setup(lr => lr.DeleteAsync(It.Is<Loop>(l => l.Id == sessions.First().Loops.Last().Id)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sessionViewModel = new SessionViewModel(sessions.First(), dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            await sessionDetailsViewModel.InitializeAsync(sessionViewModel);

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            sessionDetailsViewModel.Loops.Last().DeleteCommand.Execute(null);
            Assert.That(mainViewModel.CurrentSession.Session.Loops, Has.Count.EqualTo(1));
            loopsRepositoryMock
                .Verify(lr => lr.DeleteAsync(It.Is<Loop>(l => l.Id == sessions.First().Loops.Last().Id)), Times.Once);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_LoopDeleted_LoopIsDefaultLoop_Expect_LoopNotDeleted()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
            var tcs = new TaskCompletionSource<bool>();
            sessions.First().Loops.Add(loops.Last());
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayers.Add(audioPlayerMock.Object);
            loopsRepositoryMock
                .Setup(lr => lr.DeleteAsync(It.Is<Loop>(l => l.Id == sessions.First().Loops.First().Id)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sessionViewModel = new SessionViewModel(sessions.First(), dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            await sessionDetailsViewModel.InitializeAsync(sessionViewModel);

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            sessionDetailsViewModel.Loops.First().DeleteCommand.Execute(null);
            Assert.That(mainViewModel.CurrentSession.Session.Loops, Has.Count.EqualTo(2));
            loopsRepositoryMock
                .Verify(lr => lr.DeleteAsync(It.Is<Loop>(l => l.Id == sessions.First().Loops.First().Id)), Times.Never);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_LoopDeleted_PlayerIsPlaying_Expect_PlayerContinuesPlayingAfterDeletion()
        {
            var sessionDetailsViewModel = new SessionDetailsViewModel(configurationServiceMock.Object, dialogServiceMock.Object);
            var tcs = new TaskCompletionSource<bool>();
            sessions.First().Loops.Add(loops.Last());
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.IsPlaying).Returns(true);
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.Setup(a => a.Pause(false)).Verifiable();
            audioPlayerMock.Setup(a => a.InitAsync(It.Is<Loop>(l => l.Id == loops.First().Id), false, null)).Verifiable();
            audioPlayerMock.Setup(a => a.PlayAsync()).Returns(Task.CompletedTask).Verifiable();
            audioPlayers.Add(audioPlayerMock.Object);

            var sessionViewModel = new SessionViewModel(sessions.First(), dialogServiceMock.Object, loggerMock.Object, navigationServiceMock.Object, appTrackerMock.Object);

            await sessionDetailsViewModel.InitializeAsync(sessionViewModel);

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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.ExecutePlayCommand(null);
            sessionDetailsViewModel.Loops.Last().DeleteCommand.Execute(null);
            Assert.AreEqual(mainViewModel.CurrentLoop.Id, loops.First().Id);
            Assert.That(mainViewModel.CurrentSession.Session.Loops, Has.Count.EqualTo(1));
            audioPlayerMock.Verify(a => a.Pause(false), Times.Once);
            audioPlayerMock.Verify(a => a.InitAsync(It.Is<Loop>(l => l.Id == loops.First().Id), false, null), Times.Once);
            audioPlayerMock.Verify(a => a.PlayAsync(), Times.Once);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_NavigateToSettings_Expect_NavigateToSettingsCommandExecuted()
        {
            var tcs = new TaskCompletionSource<bool>();
            navigationServiceMock
                .Setup(n => n.NavigateToAsync<SettingsViewModel>())
                .Callback(() => { tcs.SetResult(true); })
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.NavigateToSettingsCommand.Execute(null);
            await tcs.Task;
            navigationServiceMock
                .Verify(n => n.NavigateToAsync<SettingsViewModel>(), Times.Once);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_UpdateMinimumValue_Expect_CurrentLoopHasNewValue([Range(0.0, 1.0, 0.1)] double newMinimumValue)
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.InitAudioPlayer(false);
            mainViewModel.MinimumValue = newMinimumValue;
            mainViewModel.UpdateLoopStartPosition();
            Assert.AreEqual(mainViewModel.CurrentLoop.StartPosition, newMinimumValue);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_UpdateMaximumValue_Expect_CurrentLoopHasNewValue([Range(0.0, 1.0, 0.1)] double newMaximumValue)
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.InitAudioPlayer(false);
            mainViewModel.MaximumValue = newMaximumValue;
            mainViewModel.UpdateLoopEndPosition();
            Assert.AreEqual(mainViewModel.CurrentLoop.EndPosition, newMaximumValue);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_SpotifyDisconnects_Expect_PausesPlayer()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.Spotify);
            audioPlayerMock.Setup(a => a.Pause(true)).Verifiable();
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            await mainViewModel.InitAudioPlayer(false);
            spotifyLoaderMock.Raise(s => s.Disconnected += null, null, EventArgs.Empty);
            audioPlayerMock.Verify(a => a.Pause(true), Times.Once);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_SpotifyDisconnects_LocalAudioPlayer_Expect_DontPausesPlayer()
        {
            sessionsRepositoryMock.Setup(s => s.GetAllItemsAsync()).Returns(Task.FromResult(sessions));
            audioPlayerMock.SetupGet(a => a.Types).Returns(AudioSourceType.LocalExternal | AudioSourceType.LocalInternal);
            audioPlayerMock.Setup(a => a.Pause(true)).Verifiable();
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.CurrentSession = mainViewModel.Sessions.Last();
            await mainViewModel.InitAudioPlayer(false);
            spotifyLoaderMock.Raise(s => s.Disconnected += null, null, EventArgs.Empty);
            audioPlayerMock.Verify(a => a.Pause(true), Times.Never);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_MinimumValueSetInvalidValue_Expect_HasMinimum()
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.MinimumValue = -1;
            Assert.AreEqual(mainViewModel.Minimum, 0);
            Assert.AreEqual(mainViewModel.MinimumValue, mainViewModel.Minimum);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_MaximumValueSetInvalidValue_Expect_HasMaximum()
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
               audioPlayers,
                featureRegistryMock.Object);

            await mainViewModel.InitializeAsync(null);
            mainViewModel.MaximumValue = 1.3;
            Assert.AreEqual(mainViewModel.Maximum, 1);
            Assert.AreEqual(mainViewModel.MaximumValue, mainViewModel.Maximum);
        }

        [Test()]
        [Apartment(ApartmentState.STA)]
        public async Task When_WebViewCommandsInvoked_Expect_MessagesSent()
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
                audioPlayers,
                featureRegistryMock.Object);

            if (mainViewModel.UiContext == null)
            {
                mainViewModel.UiContext = SynchronizationContext.Current;
            }

            await mainViewModel.InitializeAsync(null);
            var refreshTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<object>(this, MessengerKeys.WebViewRefreshInitialized, (sender) =>
            {
                Task.Run(() => refreshTcs.SetResult(true));
            });

            var goBackTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<object>(this, MessengerKeys.WebViewGoBackToggled, (sender) =>
            {
                Task.Run(() => goBackTcs.SetResult(true));
            });

            var goForwardTcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<object>(this, MessengerKeys.WebViewGoForwardToggled, (sender) =>
            {
                Task.Run(() => goForwardTcs.SetResult(true));
            });

            mainViewModel.SpotifyWebPlayeRefreshCommand.Execute(null);
            mainViewModel.SpotifyWebPlayerGoBackCommand.Execute(null);
            mainViewModel.SpotifyWebPlayerGoForwardCommand.Execute(null);

            var refreshResult = await refreshTcs.Task;
            var goBackResult = await goBackTcs.Task;
            var goForwardResult = await goForwardTcs.Task;

            Assert.True(refreshResult);
            Assert.True(goBackResult);
            Assert.True(goForwardResult);
        }

        private MainViewModel CreateDefault(IInterstitialAd interstitialAd,
            IRepository<Session> sessionsRepository,
            IRepository<Loop> loopsRepository,
            IDialogService dialogService,
            IFileRepository fileRepository,
            ISourcePicker sourcePicker,
            ISpotifyLoader spotifyLoader,
            ISpotifyApiService spotifyApiService,
            Common.IFilePicker filePicker,
            IConnectivityService connectivityService,
            INavigationService navigationService,
            ILogger logger,
            IAppTracker appTracker,
            IConfigurationService configurationService,
            IEnumerable<IAudioPlayer> audioPlayers,
            IFeatureRegistry featureRegistry)
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
                audioPlayers,
                featureRegistry);
        }
    }
}
