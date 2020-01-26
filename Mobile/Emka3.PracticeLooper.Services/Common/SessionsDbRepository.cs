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
using Microsoft.EntityFrameworkCore;

namespace Emka3.PracticeLooper.Services.Common
{
    public class SessionsDbRepository : IRepository<Session>
    {
        readonly IConfigurationService configService;
        readonly string dbName;
        #region Ctor
        public SessionsDbRepository()
        {
            this.configService = Factory.GetConfigService();
            dbName = configService.GetValue("DbName");
            //Init();
        }

        public void Init()
        {
            using (var dbContext = new SessionsDbContext(dbName))
            {
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }
        }
        #endregion

        #region Methods
        public async Task DeleteAsync(Session item)
        {
            try
            {
                using (var dbContext = new SessionsDbContext(dbName))
                {
                    dbContext.Sessions.Remove(item);
                    await dbContext.SaveChangesAsync();
                }
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
                using (var dbContext = new SessionsDbContext(dbName))
                {
                    return await dbContext.Sessions.Include(s => s.AudioSource).Include(s => s.Loops).ToListAsync();
                }
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
                using (var dbContext = new SessionsDbContext(dbName))
                {
                    return await dbContext.Sessions.Include(s => s.AudioSource).Include(a => a.Loops).FirstOrDefaultAsync(s => s.Id == id);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> SafeAsync(Session item)
        {
            try
            {
                using (var dbContext = new SessionsDbContext(dbName))
                {
                    var exists = dbContext.Sessions.Any(s => s.Id == item.Id);
                    if (exists)
                    {
                        dbContext.Update(item);
                    }
                    else
                    {
                        await dbContext.Sessions.AddAsync(item);
                    }

                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
