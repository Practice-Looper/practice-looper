// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace Emka3.PracticeLooper.Services.Common
{
    [Preserve(AllMembers = true)]
    public class LoopsRepository : RepositoryBase, IRepository<Loop>
    {
        #region Ctor

        public LoopsRepository()
        {
        }
        #endregion

        #region Methods
        public async Task InitAsync()
        {
            lazyInitializer = new Lazy<SQLiteConnection>(() =>
            {
                return new SQLiteConnection(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName), Flags);
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
            try
            {
                Database.Delete(item);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteAsync(Loop item)
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

        public List<Loop> GetAllItems()
        {
            try
            {
                return Database.GetAllWithChildren<Loop>(recursive: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Loop>> GetAllItemsAsync()
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

        public Loop GetById(int id)
        {
            try
            {
                var loop = Database.Get<Loop>(id);
                if (loop != null)
                {
                    var session = Database.GetWithChildren<Session>(loop.SessionId);
                    loop.Session = session;
                }

                return loop;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Loop> GetByIdAsync(int id)
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

        public int Save(Loop item)
        {
            try
            {
               return Database.Insert(item);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> SaveAsync(Loop item)
        {
            try
            {
                return await Task.Run(() => Save(item));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Update(Loop item)
        {
            try
            {
                Database.Update(item);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateAsync(Loop item)
        {
            try
            {
                await Task.Run(() => Update(item));
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
