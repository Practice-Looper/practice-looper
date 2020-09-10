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
            using var db = InitDbConnection();
            if (!initialized)
            {
                if (!db.TableMappings.Any(m => m.MappedType.Name == typeof(Session).Name))
                {
                    await Task.Run(() => db.CreateTable<Session>(CreateFlags.None));
                }

                if (!db.TableMappings.Any(m => m.MappedType.Name == typeof(AudioSource).Name))
                {
                    await Task.Run(() => db.CreateTable<AudioSource>(CreateFlags.None));
                }

                if (!db.TableMappings.Any(m => m.MappedType.Name == typeof(Loop).Name))
                {
                    await Task.Run(() => db.CreateTable<Loop>(CreateFlags.None));
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
            using var db = InitDbConnection();   
            db.Delete(item, true);
        }

        public int Save(Session item)
        {
            using var db = InitDbConnection();
            item.Loops.First().Session = item;
            db.Insert(item.AudioSource);
            db.InsertAll(item.Loops, true);
            db.InsertWithChildren(item, true);
            return item.Id;
        }

        public Session GetById(int id)
        {
            using var db = InitDbConnection();
            return db.GetWithChildren<Session>(id);
        }

        public List<Session> GetAllItems()
        {
            using var db = InitDbConnection();
            return db.GetAllWithChildren<Session>(recursive: true);
        }

        public void Update(Session item)
        {
            using var db = InitDbConnection();
            db.UpdateWithChildren(item);
        }

        public async Task UpdateAsync(Session item)
        {
            await Task.Run(() => Update(item));
        }

        private SQLiteConnection InitDbConnection()
        {
            return dbInitializer.Initialize(configurationService.GetValue(PreferenceKeys.InternalStoragePath), configurationService.GetValue("DbName"));
        }
        #endregion
    }
}
