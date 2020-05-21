// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
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
    public class SessionsDbRepository : RepositoryBase, IRepository<Session>
    {
        #region Ctor
        public SessionsDbRepository()
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

        public async Task<int> SaveAsync(Session item)
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

        public int Save(Session item)
        {
            try
            {
                item.Loops.First().Session = item;
                Database.Insert(item.AudioSource);
                Database.InsertAll(item.Loops, true);
                Database.InsertWithChildren(item, true);
                return item.Id;
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
                return Database.GetWithChildren<Session>(id);
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

        public void Update(Session item)
        {
            try
            {
                Database.UpdateWithChildren(item);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateAsync(Session item)
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
