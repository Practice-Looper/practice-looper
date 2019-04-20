// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.iOS.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.iOS
{
    internal static class GlobalApp
    {
        public static void Init()
        {
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
            resolver.Register(typeof(FileAudioPlayer), typeof(IAudioPlayer));
            resolver.Register(typeof(AudioFileRepository), typeof(IFileRepository));
            ConfigurationService = Factory.GetConfigService();
            HasiCloud = CheckCloudAccess();
            //// GetUrlForUbiquityContainer is blocking, Apple recommends background thread or your UI will freeze
            //ThreadPool.QueueUserWorkItem(_ =>
            //{
            //    CheckingForiCloud = true;
            //    Console.WriteLine("Checking for iCloud");
            //    var uburl = NSFileManager.DefaultManager.GetUrlForUbiquityContainer(null);
            //    // OR instead of null you can specify "TEAMID.com.your-company.ApplicationName"

            //    if (uburl == null)
            //    {
            //        var HasiCloud = false;
            //        Console.WriteLine("Can't find iCloud container, check your provisioning profile and entitlements");
            //    }
            //    else
            //    { // iCloud enabled, store the NSURL for later use
            //        HasiCloud = true;
            //        var iCloudUrl = uburl;
            //        Console.WriteLine("yyy Yes iCloud! {0}", uburl.AbsoluteUrl);
            //    }
            //    CheckingForiCloud = false;
            //});
        }

        internal static IConfigurationService ConfigurationService { get; private set; }
        internal static bool CheckingForiCloud { get; set; }
        internal static bool HasiCloud { get; private set; }

        internal static bool CheckCloudAccess()
        {
            var cloudTask = Task.Run(() =>
            {
                var uburl = NSFileManager.DefaultManager.GetUrlForUbiquityContainer(null);
                // OR instead of null you can specify "TEAMID.com.your-company.ApplicationName"
                return uburl != null;
            });

            return cloudTask.Result;
        }
    }
}
