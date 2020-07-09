// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Autofac;
using Emka3.PracticeLooper.Services.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Emka3.PracticeLooper.Services.Rest;

namespace Emka3.PracticeLooper.Mappings.Services
{
    public static class ServicesMapping
    {
        /// <summary>
        /// Register the specified builder.
        /// </summary>
        /// <param name="builder">Builder.</param>
        public static void Register(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Register(c => new HttpApiClient(Config.Factory.GetConfigService().GetValue("SpotifyClientApiUri"), c.Resolve<ITokenStorage>())).As<IHttpApiClient>().SingleInstance();
            builder.RegisterType<SpotifyApiService>().As<ISpotifyApiService>().SingleInstance();
        }
    }
}
