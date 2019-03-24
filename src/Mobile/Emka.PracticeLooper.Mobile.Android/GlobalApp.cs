// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Emka.PracticeLooper.Mobile.Droid.Common;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid
{
    internal static class GlobalApp
    {
        public static void Init()
        {
            Emka3.PracticeLooper.Mappings.Contracts.IResolver resolver = Factory.GetResolver();
            //resolver.Register(typeof(FilePicker), typeof(IFilePicker));
            resolver.Register(typeof(FileAudioPlayer), typeof(IAudioPlayer));
        }
    }
}
