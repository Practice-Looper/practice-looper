// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;
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
            var names = new List<string>();
            if (FeatureRegistry.IsEnabled<IPremiumAudioPlayer>("Spotify"))
            {
                names.Add("Spotify");
            }

            if (names.Any())
            {
                names.Add("File");
                Array.Sort(names.ToArray(), StringComparer.InvariantCulture);
                return await page.DisplayActionSheet("Select Source", "Cancel", null, names.ToArray()).ConfigureAwait(false);
            }

            return "File";
        }
    }
}
