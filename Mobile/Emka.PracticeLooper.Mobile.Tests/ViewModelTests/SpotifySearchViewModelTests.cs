// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Moq;
using NUnit.Framework;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture]
    public class SpotifySearchViewModelTests
    {
        Mock<INavigationService> navigationServiceMock;
        Mock<ILogger> loggerMock;
        Mock<IAppTracker> appTrackerMock;
        Mock<ISpotifyApiService> spotifyApiMock;
        Mock<ISpotifyLoader> spotifyLoaderMock;
        Mock<IDialogService> dialogServiceMock;
        StringLocalizer localizer;
        List<SpotifyTrack> tracks;

        public SpotifySearchViewModelTests()
        {
            tracks = new List<SpotifyTrack>
            {
                new SpotifyTrack
                {
                    Name = "bbbbaAAaa",
                    Album = new SpotifyAlbum
                    {
                        Name = "Album1"
                    }
                },
                new SpotifyTrack
                {
                    Name = "aaaabbbbbb",
                    Album = new SpotifyAlbum
                    {
                        Name = "Album2"
                    }
                },
                new SpotifyTrack
                {
                    Name = "zzzxxxx",
                    Album = new SpotifyAlbum
                    {
                        Name = "Album3"
                    }
                }
            };
        }

        [SetUp]
        public void SetUp()
        {
            navigationServiceMock = new Mock<INavigationService>();
            loggerMock = new Mock<ILogger>();
            appTrackerMock = new Mock<IAppTracker>();
            spotifyApiMock = new Mock<ISpotifyApiService>();
            spotifyLoaderMock = new Mock<ISpotifyLoader>();
            dialogServiceMock = new Mock<IDialogService>();
            localizer = new StringLocalizer(loggerMock.Object);
        }

        [Test]
        public async Task When_InitSpotifyLoaderNotAuthorized_Expect_SpotifyLoaderInitInvoked()
        {
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Callback(() => { spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(true); })
                .ReturnsAsync(true)
                .Verifiable();

            navigationServiceMock
                .Setup(n => n.GoBackAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var spotifySearchVm = new SpotifySearchViewModel(spotifyApiMock.Object, spotifyLoaderMock.Object, navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, dialogServiceMock.Object);
            await spotifySearchVm.InitializeAsync(null);

            spotifyLoaderMock
                .Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Once);

            navigationServiceMock
                .Verify(n => n.GoBackAsync(), Times.Never);
        }

        [Test]
        public async Task When_InitSpotifyLoaderAuthorized_Expect_SpotifyLoaderInitNotInvoked()
        {
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Verifiable();

            spotifyLoaderMock
                .SetupGet(s => s.Authorized)
                .Returns(true);

            navigationServiceMock
                .Setup(n => n.GoBackAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var spotifySearchVm = new SpotifySearchViewModel(spotifyApiMock.Object, spotifyLoaderMock.Object, navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, dialogServiceMock.Object);
            await spotifySearchVm.InitializeAsync(null);

            spotifyLoaderMock
                .Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Never);

            navigationServiceMock
                .Verify(n => n.GoBackAsync(), Times.Never);
        }

        [Test]
        public async Task When_InitSpotifyLoaderReturnsFalse_Expect_ShowsDialogAndNavigatesBack()
        {
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Callback(() => { spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(false); })
                .ReturnsAsync(false)
                .Verifiable();

            navigationServiceMock
                .Setup(n => n.GoBackAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            dialogServiceMock
                .Setup(d => d.ShowAlertAsync(It.Is<string>(s => s == localizer.GetLocalizedString("Error_Content_CouldNotConnectToSpotify")), It.Is<string>(s => s == localizer.GetLocalizedString("Error_Caption"))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var spotifySearchVm = new SpotifySearchViewModel(spotifyApiMock.Object, spotifyLoaderMock.Object, navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, dialogServiceMock.Object);
            await spotifySearchVm.InitializeAsync(null);

            spotifyLoaderMock
                .Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Once);

            navigationServiceMock
                .Verify(n => n.GoBackAsync(), Times.Once);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_Search_Expect_SearchResults()
        {
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Callback(() => { spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(true); })
                .ReturnsAsync(true)
                .Verifiable();

            spotifyApiMock
                .Setup(s => s.SearchTrackByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string term, CancellationToken token) => { return tracks.Where(t => t.Name.Contains(term)).ToList(); });

            var spotifySearchVm = new SpotifySearchViewModel(spotifyApiMock.Object, spotifyLoaderMock.Object, navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, dialogServiceMock.Object);
            await spotifySearchVm.InitializeAsync(null);
            spotifySearchVm.SearchCommand.Execute("aa");
            await Task.Delay(2000);
            Assert.NotNull(spotifySearchVm.SearchResults);
            Assert.That(spotifySearchVm.SearchResults, Has.Count.EqualTo(2));


        }

        [Test]
        public async Task When_CreateSession_Expect_TrackAddedMessage()
        {
            var tcs = new TaskCompletionSource<bool>();
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Callback(() => { spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(true); })
                .ReturnsAsync(true)
                .Verifiable();

            var track = new SpotifyTrack
            {
                Id = "0",
                Duration = 60,
                Uri = "asdkasldjalsdjlasjdklasd",
                Name = "zzzxxxx",
                Album = new SpotifyAlbum
                {
                    Name = "Album3",
                    Artists = new List<SpotifyArtist>
                    {
                        new SpotifyArtist
                        {
                            Id = "0",
                            Name = "Cartman"
                        },
                        new SpotifyArtist
                        {
                            Id = "1",
                            Name = "Kenny"
                        }
                    },
                }
            };


            MessagingCenter.Subscribe<SpotifySearchViewModel, AudioSource>(this, MessengerKeys.NewTrackAdded, (sender, audioSorce) =>
            {
                tcs.SetResult(audioSorce.FileName == track.Name);
            });

            var spotifySearchVm = new SpotifySearchViewModel(spotifyApiMock.Object, spotifyLoaderMock.Object, navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, dialogServiceMock.Object);
            await spotifySearchVm.InitializeAsync(null);
            spotifySearchVm.CreateSessionCommand.Execute(track);
            await tcs.Task;
            Assert.IsTrue(tcs.Task.Result);

            navigationServiceMock
               .Setup(n => n.GoBackAsync())
               .Returns(Task.CompletedTask)
               .Verifiable();

            navigationServiceMock
                .Verify(n => n.GoBackAsync(), Times.Once);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task When_CreateSessionNullReference_Expect_ArgumentException()
        {
            spotifyLoaderMock
                .Setup(s => s.InitializeAsync(It.IsAny<string>()))
                .Callback(() => { spotifyLoaderMock.SetupGet(s => s.Authorized).Returns(true); })
                .ReturnsAsync(true)
                .Verifiable();
            var spotifySearchVm = new SpotifySearchViewModel(spotifyApiMock.Object, spotifyLoaderMock.Object, navigationServiceMock.Object, loggerMock.Object, appTrackerMock.Object, dialogServiceMock.Object);
            await spotifySearchVm.InitializeAsync(null);

            var ex = Assert.ThrowsAsync<ArgumentException>(() => spotifySearchVm.ExecuteCreateSessionCommand(null));
            Assert.NotNull(ex);
        }
    }
}
