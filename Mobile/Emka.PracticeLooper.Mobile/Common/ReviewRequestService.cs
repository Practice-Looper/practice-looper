// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Plugin.StoreReview;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class ReviewRequestService : IReviewRequestService
    {
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private readonly IAppTracker appTracker;
        int startsUntilReview = 0;
        bool neverAskedForReview;

        public ReviewRequestService(IConfigurationService configurationService, ILogger logger, IAppTracker appTracker)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appTracker = appTracker?? throw new ArgumentNullException(nameof(appTracker));
        }

        public async Task RequestReview()
        {
            try
            {
                // call for review for already in production versions. can be remove in next version
                neverAskedForReview = await configurationService.GetValueAsync<bool>(nameof(neverAskedForReview), true);

                if (VersionTracking.IsFirstLaunchEver || neverAskedForReview)
                {
                    await configurationService.SetValueAsync(nameof(startsUntilReview), 7, true);
                    await configurationService.SetValueAsync(nameof(neverAskedForReview), false, true);
                    return;
                }

                if (!VersionTracking.IsFirstLaunchEver && VersionTracking.IsFirstLaunchForCurrentVersion)
                {
                    var random = new Random();
                    var nextValue = random.Next(35, 50);
                    await configurationService.SetValueAsync(nameof(startsUntilReview), nextValue, true);
                    return;
                }

                startsUntilReview = await configurationService.GetValueAsync<int>(nameof(startsUntilReview));

                if (startsUntilReview > 0)
                {
                    startsUntilReview--;
                    await configurationService.SetValueAsync(nameof(startsUntilReview), startsUntilReview, true);
                    return;
                }

                if (startsUntilReview == 0)
                {
                    await appTracker.TrackAsync(Emka3.PracticeLooper.Model.Common.TrackerEvents.RequestRating, new Dictionary<string, string> { { "StartsUntilReview", $"{startsUntilReview}" } });
                    await configurationService.SetValueAsync(nameof(startsUntilReview), -1, true);
                    await CrossStoreReview.Current.RequestReview(false);
                }
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
            }
        }
    }
}
