// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Autofac;
using Emka3.PracticeLooper.Services;
using Emka3.PracticeLooper.Services.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;

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

            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().SingleInstance();
        }
    }
}
