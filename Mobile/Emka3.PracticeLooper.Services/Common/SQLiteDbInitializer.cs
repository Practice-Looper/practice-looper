// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using SQLite;

namespace Emka3.PracticeLooper.Services.Common
{
    [Preserve(AllMembers = true)]
    public class SQLiteDbInitializer : IDbInitializer<SQLiteConnection>
    {
        #region Fields
        private const SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLiteOpenFlags.SharedCache;

        protected static Lazy<SQLiteConnection> lazyInitializer;
        #endregion

        #region Methods
        public SQLiteConnection Initialize(string dbPath, string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                throw new ArgumentNullException(nameof(dbPath));
            }

            if (string.IsNullOrWhiteSpace(dbName))
            {
                throw new ArgumentNullException(nameof(dbName));
            }

            var path = Path.Combine(dbPath, dbName);
            var connection = new SQLiteConnection(path, Flags);
            return connection;
        }

        public async Task<SQLiteConnection> InitializeAsync(string dbPath, string dbName)
        {
            return await Task.Run(() => Initialize(dbPath, dbName));
        }
        #endregion
    }
}
