// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface ILogger
    {
        void LogInfo(string message, IDictionary<string, string> properties = default);
        void LogWarning(string message);
        void LogError(Exception exception, IDictionary<string, string> properties = default);
        Task LogInfoAsync(string message, IDictionary<string, string> properties = default);
        Task LogWarningAsync(string message);
        Task LogErrorAsync(Exception exception, IDictionary<string, string> properties = default);
    }
}
