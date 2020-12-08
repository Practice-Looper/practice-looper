// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Autofac;
using Emka3.PracticeLooper.Services.Common;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using SQLite;
using Emka3.PracticeLooper.Config.Feature;
using Emka3.PracticeLooper.Config.Contracts;

namespace Emka3.PracticeLooper.Mappings.Common
{
    /// <summary>
    /// Common App services.
    /// </summary>
    static class CommonMappings
    {
        /// <summary>
        /// Initializes the <see cref="CommonMappings"/> instance.
        /// </summary>
        public static void Register(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterType<FeatureRegistry>().As<IFeatureRegistry>().SingleInstance();
            builder.RegisterType<AppCenterLogger>().As<ILogger>().SingleInstance();
            builder.RegisterType<AppCenterTracker>().As<IAppTracker>().SingleInstance();
            builder.RegisterType<SQLiteDbInitializer>().As<IDbInitializer<SQLiteConnection>>();
            builder.RegisterType<SessionsDbRepository>().As<IRepository<Session>>();
            builder.RegisterType<LoopsRepository>().As<IRepository<Loop>>();
            builder.RegisterType<SecurePracticeLooperStorage>().As<ISecureRepository>();
            builder.RegisterType<SpotifyTokenStorage>().As<ITokenStorage>().SingleInstance();
            builder.RegisterType<AudioFileLoader>().As<IAudioFileLoader>();
        }
    }
}
