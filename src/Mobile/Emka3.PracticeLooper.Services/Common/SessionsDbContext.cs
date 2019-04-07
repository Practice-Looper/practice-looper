// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.IO;
using Emka3.PracticeLooper.Model.Player;
using Microsoft.EntityFrameworkCore;

namespace Emka3.PracticeLooper.Services.Common
{
    public class SessionsDbContext : DbContext
    {
        private readonly string dbName;

        public SessionsDbContext(string dbName)
        {
            this.dbName = dbName;
        }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<AudioSource> AudioSources { get; set; }
        public DbSet<Loop> Loops { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);
            Console.WriteLine(dbPath);
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>()
                .HasOne(s => s.AudioSource)
                .WithOne(a => a.Session)
                .HasForeignKey<AudioSource>(a => a.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Session>()
                .HasMany(s => s.Loops)
                .WithOne(l => l.Session)
                .HasPrincipalKey(l => l.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
