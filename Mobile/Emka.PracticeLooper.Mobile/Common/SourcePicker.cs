// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class SourcePicker : ISourcePicker
    {
        private readonly Page page;

        public SourcePicker(Page page)
        {
            this.page = page;
        }

        public async Task<string> SelectFileSource()
        {
            return await page.DisplayActionSheet("Select Source", "Cancel", null, "File", "Spotify").ConfigureAwait(false);
        }
    }
}
