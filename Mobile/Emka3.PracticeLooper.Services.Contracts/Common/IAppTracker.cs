// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Common;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IAppTracker
    {
        void Track(TrackerEvents trackerEvent, IDictionary<string, string> properties);
        Task TrackAsync(TrackerEvents trackerEvent, IDictionary<string, string> properties);
    }
}
