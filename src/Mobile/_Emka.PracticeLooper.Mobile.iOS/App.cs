// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.IO;
using System.Reflection;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.iOS
{
    public static class App
    {
        #region Properties
        public static IConfigurationService ConfigurationService { get; private set; }
        #endregion

        #region Methods
        public static void Init()
        {
            try
            {
                string jsonConfig;
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Emka.PracticeLooper.Mobile.iOS.App.config.json"))
                using (var reader = new StreamReader(stream))
                {
                    jsonConfig = reader.ReadToEnd();
                }

                ConfigurationService = Factory.GetResolver().Resolve<IConfigurationService>();
                ConfigurationService.Initialize(jsonConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}
