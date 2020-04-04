// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Microsoft.AppCenter.Analytics;

namespace Emka3.PracticeLooper.Services.Common
{
    [Preserve(AllMembers = true)]
    public class AppCenterTracker : IAppTracker
    {
        public void Track(TrackerEvents trackerEvent, IDictionary<string, string> properties)
        {
            Analytics.TrackEvent(trackerEvent.ToString(), properties);
        }

        public async Task TrackAsync(TrackerEvents trackerEvent, IDictionary<string, string> properties)
        {
            await Task.Run(() => Track(trackerEvent, properties));
        }
    }
}
