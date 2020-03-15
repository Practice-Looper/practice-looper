// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace Emka3.PracticeLooper.Services.Common
{
    [Preserve(AllMembers = true)]
    public class SessionsDbRepository : IRepository<Session>
    {
        #region Fields

        readonly IConfigurationService configService;
        readonly string dbName;
        private bool initialized = false;
        public const SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLiteOpenFlags.SharedCache;

        private static Lazy<SQLiteConnection> lazyInitializer;
        #endregion

        #region Ctor
        public SessionsDbRepository()
        {
            configService = Factory.GetConfigService();
            dbName = configService.GetValue("DbName");
            InitAsync().SafeFireAndForget(false);
        }
        #endregion

        #region Properties

        private static SQLiteConnection Database => lazyInitializer.Value;
        #endregion

        #region Methods
        async Task InitAsync()
        {
            try
            {
                lazyInitializer = new Lazy<SQLiteConnection>(() =>
                {
                    return new SQLiteConnection(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName), Flags);
                });

                if (!initialized)
                {
                    if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Session).Name))
                    {
                        await Task.Run(() => Database.CreateTable<Session>(CreateFlags.None));
                    }

                    if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(AudioSource).Name))
                    {
                        await Task.Run(() => Database.CreateTable<AudioSource>(CreateFlags.None));
                    }

                    if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Loop).Name))
                    {
                        await Task.Run(() => Database.CreateTable<Loop>(CreateFlags.None));
                    }

                    initialized = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task DeleteAsync(Session item)
        {
            try
            {
                await Task.Run(() => Delete(item));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Session>> GetAllItemsAsync()
        {
            try
            {
                return await Task.Run(GetAllItems);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Session> GetByIdAsync(int id)
        {
            try
            {
                return await Task.Run(() => GetById(id));

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveAsync(Session item)
        {
            try
            {
                await Task.Run(() => Save(item));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(Session item)
        {
            try
            {
                Database.Delete(item, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Save(Session item)
        {
            try
            {
                Database.InsertOrReplaceWithChildren(item, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Session GetById(int id)
        {
            try
            {
                return Database.GetWithChildren<Session>(id, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Session> GetAllItems()
        {
            try
            {
                return Database.GetAllWithChildren<Session>(recursive: true);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
