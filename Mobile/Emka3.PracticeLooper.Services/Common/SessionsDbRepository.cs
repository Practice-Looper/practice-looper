// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
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
        private bool initialized = false;
        private Lazy<SQLiteConnection> lazyInitializer;
        private readonly IDbInitializer<SQLiteConnection> dbInitializer;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor
        public SessionsDbRepository(IConfigurationService configurationService, IDbInitializer<SQLiteConnection> dbInitializer)
        {
            this.configurationService = configurationService;
            this.dbInitializer = dbInitializer;
        }
        #endregion

        #region Properties

        public SQLiteConnection Database => lazyInitializer.Value;
        #endregion

        #region Methods
        public async Task InitAsync()
        {
            lazyInitializer = new Lazy<SQLiteConnection>(() =>
            {
                return dbInitializer.Initialize(configurationService.LocalPath, configurationService.GetValue("DbName"));
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

        public async Task DeleteAsync(Session item)
        {
            await Task.Run(() => Delete(item));
        }

        public async Task<List<Session>> GetAllItemsAsync()
        {
            return await Task.Run(GetAllItems);
        }

        public async Task<Session> GetByIdAsync(int id)
        {
            return await Task.Run(() => GetById(id));
        }

        public async Task<int> SaveAsync(Session item)
        {
            return await Task.Run(() => Save(item));
        }

        public void Delete(Session item)
        {
            Database.Delete(item, true);
        }

        public int Save(Session item)
        {
            item.Loops.First().Session = item;
            Database.Insert(item.AudioSource);
            Database.InsertAll(item.Loops, true);
            Database.InsertWithChildren(item, true);
            return item.Id;
        }

        public Session GetById(int id)
        {
            return Database.GetWithChildren<Session>(id);
        }

        public List<Session> GetAllItems()
        {
            return Database.GetAllWithChildren<Session>(recursive: true);
        }

        public void Update(Session item)
        {
            Database.UpdateWithChildren(item);
        }

        public async Task UpdateAsync(Session item)
        {
            await Task.Run(() => Update(item));
        }
        #endregion
    }
}
