// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Globalization;
using SkiaSharp;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    internal static class ColorConstants
    {
        public const string BackgroundHexColor = "#FF46494D";
        public const string PrimaryHexColor = "#FF4EBC9A";
        public const string SecondaryHexColor = "#FF369598";

        public static Color Background = Color.FromHex(BackgroundHexColor);
        public static Color Primary = Color.FromHex(PrimaryHexColor);
        public static Color Secondary = Color.FromHex(SecondaryHexColor);

        public static uint ConvertToUint(string hexColor)
        {
            return UInt32.Parse(hexColor.Replace("#", ""), NumberStyles.HexNumber);
        }
    }
}
