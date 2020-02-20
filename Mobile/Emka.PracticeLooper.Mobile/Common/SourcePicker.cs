// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Feature;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class SourcePicker : ISourcePicker
    {
        private readonly Page page;

        public SourcePicker()
        {
            page = Application.Current.MainPage;
        }

        public async Task<string> SelectFileSource()
        {
            if (FeatureRegistry.IsEnabled<IPremiumAudioPlayer>())
            {
                var playerNames = Factory.GetResolver().ResolveAll<IAudioPlayer>()?.Select(p => p.DisplayName);
                return await page.DisplayActionSheet("Select Source", "Cancel", null, playerNames.ToArray()).ConfigureAwait(false);
            }

            return "File";
        }
    }
}
