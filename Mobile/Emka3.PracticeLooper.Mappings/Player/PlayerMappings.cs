// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using Autofac;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Player;

namespace Emka3.PracticeLooper.Mappings.Player
{
    /// <summary>
    /// Audio player mappings
    /// </summary>
    public static class PlayerMappings
    {
        /// <summary>
        /// Initializes the <see cref="PlayerMappings"/> instance.
        /// </summary>
        public static void Register(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterType<PlayerTimer>().As<IPlayerTimer>();
        }
    }
}
