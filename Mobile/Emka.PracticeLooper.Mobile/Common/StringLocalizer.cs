// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class StringLocalizer : IStringLocalizer
    {
        #region Fields
        private readonly ILogger logger;
        #endregion

        #region Ctor

        public StringLocalizer(ILogger logger)
        {
            this.logger = logger;
        }
        #endregion

        #region Methods

        public string GetLocalizedString(string key)
        {
            var localizedString = AppResources.ResourceManager?.GetString(key);
            if (string.IsNullOrWhiteSpace(localizedString))
            {
                logger.LogError(new System.Exception($"Missing localized string for {key}"));
                return "Missing content!";
            }

            return localizedString;
        }

        public async Task<string> GetLocalizedStringAsync(string key)
        {
            return await Task.Run(() => GetLocalizedString(key));
        }
        #endregion
    }
}
