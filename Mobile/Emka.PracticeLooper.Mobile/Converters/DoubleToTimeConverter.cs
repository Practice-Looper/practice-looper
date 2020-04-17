// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Globalization;
using Emka.PracticeLooper.Mobile.ViewModels;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Converters
{
    public class DoubleToTimeConverter : IValueConverter
    {
        public DoubleToTimeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LoopViewModel loop && parameter is string position)
            {
                if (position == "Start")
                {
                    return FormatTime(loop.Loop.StartPosition * loop.Loop.Session.AudioSource.Duration * 1000);
                }

                return FormatTime(loop.Loop.EndPosition * loop.Loop.Session.AudioSource.Duration * 1000);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string FormatTime(double time)
        {
            return TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss");
        }
    }
}
