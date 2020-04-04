// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Microsoft.AppCenter.Crashes;

namespace Emka3.PracticeLooper.Services.Common
{
    [Preserve(AllMembers = true)]
    public class AppCenterLogger : ILogger
    {
        public void LogError(Exception exception, IDictionary<string, string> properties = default)
        {
            Crashes.TrackError(exception, properties);
        }

        public async Task LogErrorAsync(Exception exception, IDictionary<string, string> properties = default)
        {
            await Task.Run(() => LogError(exception, properties));
        }

        public void LogInfo(string message, IDictionary<string, string> properties = default)
        {
            
        }

        public Task LogInfoAsync(string message, IDictionary<string, string> properties = default)
        {
            return Task.CompletedTask;
        }

        public void LogWarning(string message)
        {

        }

        public Task LogWarningAsync(string message)
        {
            return Task.CompletedTask;
        }
    }
}
