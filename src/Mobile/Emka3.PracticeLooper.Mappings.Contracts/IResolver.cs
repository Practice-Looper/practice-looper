// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;

namespace Emka3.PracticeLooper.Mappings.Contracts
{

    public interface IResolver
    {
        /// <summary>
        /// Builds the autofac container.
        /// </summary>
        void BuildContainer();

        /// <summary>
        /// Resolve instance registered in any IoC framework.
        /// </summary>
        /// <returns>Resolved instances.</returns>
        /// <typeparam name="T">Type to resolve.</typeparam>
        IEnumerable<T> ResolveAll<T>();

        /// <summary>
        /// Resolve instance registered in any IoC framework.
        /// </summary>
        /// <returns>Resolved instance.</returns>
        /// <typeparam name="T">Type to resolve.</typeparam>
        T Resolve<T>();

        /// <summary>
        /// Resolve instance registered in any IoC framework.
        /// </summary>
        /// <returns>The resolved instance.</returns>
        /// <param name="name">Name of instance to resolve.</param>
        /// <typeparam name="T">Type to resolve</typeparam>
        T ResolveNamed<T>(string name);

        /// <summary>
        /// Register the specified type and name.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="name">Name.</param>
        /// <typeparam name="T">The type parameter.</typeparam>
        void Register(Type type, Type typeInterface, string name = "");

        /// <summary>
        /// Registers an created instance.
        /// </summary>
        /// <param name="instance">Instance.</param>
        /// <param name="type">Type.</param>
        void RegisterInstance(object instance, Type type);
    }
}
