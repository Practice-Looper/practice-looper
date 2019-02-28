// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Autofac;
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

            //builder.RegisterType<RemoteNotificationProxy>().As<IRemoteNotificationProxy>();
            //builder.RegisterType<UserServiceProxy>().Named<IUserServiceProxy>("UserServiceProxy");
            //builder.RegisterType<UserServiceProxyMock>().Named<IUserServiceProxy>("UserServiceProxyMock");
            //builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();
            //builder.RegisterType<HttpApiClient>().SingleInstance();
            builder.RegisterType<SpotifyAuthenticationService>().Named<IAuthenticationService>("SpotifyAuthenticationService");
        }
    }
}
