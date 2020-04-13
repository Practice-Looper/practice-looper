// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka3.PracticeLooper.Config;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace Emka3.PracticeLooper.Services.Common
{
    public class RepositoryBase
    {
        #region Fields

        readonly IConfigurationService configService;
        protected readonly string dbName;
        protected bool initialized = false;
        protected const SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLiteOpenFlags.SharedCache;

        protected static Lazy<SQLiteConnection> lazyInitializer;
        #endregion

        #region Ctor

        public RepositoryBase()
        {
            configService = Factory.GetConfigService();
            dbName = configService.GetValue("DbName");
        }
        #endregion


        #region Properties

        protected static SQLiteConnection Database => lazyInitializer.Value;
        #endregion
    }
}
