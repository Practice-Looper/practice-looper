// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;
using Autofac;
using Emka3.PracticeLooper.Mappings.Common;
using Emka3.PracticeLooper.Mappings.Contracts;
using Emka3.PracticeLooper.Mappings.Player;
using Emka3.PracticeLooper.Mappings.Services;

namespace Emka3.PracticeLooper.Mappings
{
    /// <summary>
    /// Practice looper instances resolver.
    /// </summary>
    public class PracticeLooperResolver : IResolver
    {
        #region Fields
        ContainerBuilder builder;
        private static PracticeLooperResolver instance;
        #endregion Fields

        #region Ctor
        public PracticeLooperResolver()
        {
            builder = new ContainerBuilder();
            //WebServicesMappings.Register(builder);
            //ObserverMappings.Register(builder);
            //DataMappings.Register(builder);
            //ViewModelMappings.Register(builder);
            CommonMappings.Register(builder);
            ServicesMapping.Register(builder);
            PlayerMappings.Register(builder);
            builder.RegisterType<PracticeLooperResolver>().As<IResolver>();

        }
        #endregion

        #region Properties
        public static IResolver Instance
        {
            get
            {
                return instance ?? (instance = new PracticeLooperResolver());
            }
        }

        private static IContainer Container
        {
            get;
            set;
        }
        #endregion

        #region Methods
        public void BuildContainer()
        {
            Container = builder.Build();
        }

        /// <summary>
        /// Register the specified type and name.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="name">Name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Register(Type type, Type interfaceType, string name = "")
        {
            if (builder != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    builder.RegisterType(type).Named(name, interfaceType);
                }
                else
                {
                    builder.RegisterType(type).As(interfaceType);
                }
            }
        }
               
        public void RegisterSingleton(Type type, Type typeInterface, string name = "")
        {
            if (builder != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    builder.RegisterType(type).Named(name, typeInterface).SingleInstance();
                }
                else
                {
                    builder.RegisterType(type).As(typeInterface).SingleInstance();
                }
            }
        }

        /// <summary>
        /// Registers an created instance.
        /// </summary>
        /// <param name="instance">Instance.</param>
        /// <param name="type">Type.</param>
        public void RegisterInstance(object instance, Type type)
        {
            if (builder != null)
            {
                builder.RegisterInstance(instance).As(type);
            }
        }

        /// <summary>
        /// Resolve a reference.
        /// </summary>
        /// <returns>The resolved type.</returns>
        /// <typeparam name="T">Type to resolve.</typeparam>
        public T Resolve<T>()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                return (T)scope.Resolve(typeof(T));
            }
        }

        /// <summary>
        /// Resolves all implementations for a type.
        /// </summary>
        /// <returns>All instances.</returns>
        /// <typeparam name="T">The type to resolve.</typeparam>
        public IEnumerable<T> ResolveAll<T>()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                return scope.Resolve<IEnumerable<T>>();
            }
        }

        /// <summary>
        /// Resolves the named instance.
        /// </summary>
        /// <returns>The named instance.</returns>
        /// <param name="name">Name.</param>
        /// <typeparam name="T">The type to resolve.</typeparam>
        public T ResolveNamed<T>(string name)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                return Container.ResolveNamed<T>(name);
            }
        }
        #endregion
    }
}
