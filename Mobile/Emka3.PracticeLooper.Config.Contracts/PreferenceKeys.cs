// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
namespace Emka3.PracticeLooper.Config.Contracts
{
    public static class PreferenceKeys
    {
        public static readonly string PremiumGeneral = "pl.premium.general";
        public static readonly string NrLoopChanged = nameof(NrLoopChanged);
        public static readonly string LastSession = nameof(LastSession);
        public static readonly string LastLoop = nameof(LastLoop);
        public static readonly string ColorScheme = nameof(ColorScheme);
        public static readonly string SlowConnectionWarning = nameof(SlowConnectionWarning);
        public static readonly string ExternalStoragePath = nameof(ExternalStoragePath);
        public static readonly string InternalStoragePath = nameof(InternalStoragePath);
        public static readonly string IsSpotifyInstalled = nameof(IsSpotifyInstalled);
    }
}
