// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Common;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Service.Tests
{
    public class RepositoryTests
    {
        protected readonly Mock<IConfigurationService> configServiceMock;
        const string DbName = "PracticeLooperTests.db3";
        protected List<Session> sessions;
        protected List<AudioSource> audioSources;
        protected List<Loop> loops;

        public RepositoryTests()
        {
            SQLitePCL.Batteries_V2.Init();
            configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == PreferenceKeys.InternalStoragePath))).Returns(Path.Combine(Directory.GetCurrentDirectory()));
            configServiceMock.Setup(c => c.GetValue(It.Is<string>(s => s == "DbName"))).Returns(DbName);

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
                    {
                        Name = audioSources.First().FileName,
                        AudioSource = audioSources.First(),
                        Loops = new List<Loop>
                        {
                            loops.First()
                        }
                    },
                 new Session
                    {
                        Name = audioSources.Last().FileName,
                        AudioSource = audioSources.Last(),
                        Loops = new List<Loop>
                        {
                            loops.Last()
                        }
                    }
            };
        }

        [SetUp()]
        public void CommonSetup()
        {
            DeleteDb();
        }

        protected void DeleteDb()
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), DbName);
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    [TestFixture()]
    public class SessionsDbRepositoryTests : RepositoryTests
    {
        private SessionsDbRepository sessionsDbRepository;

        [SetUp]
        public async Task Setup()
        {
            sessionsDbRepository = new SessionsDbRepository(configServiceMock.Object, new SQLiteDbInitializer());
            await sessionsDbRepository.InitAsync();
        }

        [Test()]
        public async Task When_HasNoSessions_Add2Sessions_Expect_Has2Sessions()
        {
            try
            {

                await sessionsDbRepository.SaveAsync(sessions.First());
                await sessionsDbRepository.SaveAsync(sessions.Last());

                var storedSessions = await sessionsDbRepository.GetAllItemsAsync();
                Assert.NotNull(storedSessions);
                Assert.That(storedSessions, Has.Count.EqualTo(2));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        [Test()]
        public async Task When_Has2Sessions_Delete1Session_Expect_Has1Session()
        {
            await sessionsDbRepository.SaveAsync(sessions.First());
            await sessionsDbRepository.SaveAsync(sessions.Last());

            var storedSessions = await sessionsDbRepository.GetAllItemsAsync();
            await sessionsDbRepository.DeleteAsync(storedSessions.First());
            storedSessions = await sessionsDbRepository.GetAllItemsAsync();
            Assert.NotNull(storedSessions);
            Assert.That(storedSessions, Has.Count.EqualTo(1));
        }
    }
}
