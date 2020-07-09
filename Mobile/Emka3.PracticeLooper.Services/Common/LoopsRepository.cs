// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
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
    public class LoopsRepository : IRepository<Loop>
    {
        #region Fields
        private bool initialized = false;
        private Lazy<SQLiteConnection> lazyInitializer;
        private readonly IDbInitializer<SQLiteConnection> dbInitializer;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public LoopsRepository(IConfigurationService configurationService, IDbInitializer<SQLiteConnection> dbInitializer)
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
                return dbInitializer.Initialize(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), configurationService.GetValue("DbName"));
            });

            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Loop).Name))
                {
                    await Task.Run(() => Database.CreateTable<Loop>(CreateFlags.None));
                }

                initialized = true;
            }
        }

        public void Delete(Loop item)
        {
            Database.Delete(item);
        }

        public async Task DeleteAsync(Loop item)
        {
            await Task.Run(() => Delete(item));
        }

        public List<Loop> GetAllItems()
        {
            return Database.GetAllWithChildren<Loop>(recursive: false);
        }

        public async Task<List<Loop>> GetAllItemsAsync()
        {
            return await Task.Run(GetAllItems);
        }

        public Loop GetById(int id)
        {
            var loop = Database.Get<Loop>(id);
            if (loop != null)
            {
                var session = Database.GetWithChildren<Session>(loop.SessionId);
                loop.Session = session;
            }

            return loop;
        }

        public async Task<Loop> GetByIdAsync(int id)
        {
            return await Task.Run(() => GetById(id));
        }

        public int Save(Loop item)
        {
            return Database.Insert(item);
        }

        public async Task<int> SaveAsync(Loop item)
        {
            return await Task.Run(() => Save(item));
        }

        public void Update(Loop item)
        {
            Database.Update(item);
        }

        public async Task UpdateAsync(Loop item)
        {
            await Task.Run(() => Update(item));
        }
        #endregion
    }
}
