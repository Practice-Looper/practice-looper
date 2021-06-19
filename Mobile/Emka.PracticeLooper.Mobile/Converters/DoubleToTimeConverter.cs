// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Globalization;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Mappings.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Converters
{
    public class DoubleToTimeConverter : IValueConverter
    {
        private IResolver resolver;
        private ILogger logger;

        public DoubleToTimeConverter()
        {
            resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();
            logger = resolver?.Resolve<ILogger>();
        }

        /// <summary>
        /// Ctor for testing purposes.
        /// </summary>
        /// <param name="logger">logger</param>
        public DoubleToTimeConverter(ILogger logger)
        {
            this.logger = logger;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LoopViewModel loop && parameter is string position)
            {
                double? loopPosition = position == "Start" ? loop?.Loop?.StartPosition : loop?.Loop?.EndPosition;
                double? duration = loop?.Loop?.Session?.AudioSource?.Duration;

                if (loopPosition == null || duration == null)
                {
                    var properties = new Dictionary<string, string>
                    {
                        { "loop view model", loop?.ToString() },
                        { "loop", loop?.Loop?.ToString() },
                        { "loop start position", loop?.Loop?.StartPosition.ToString() },
                        { "loop end position", loop?.Loop?.EndPosition.ToString() },
                        { "loop session", loop?.Loop?.Session?.ToString() },
                        { "loop audio source", loop?.Loop?.Session?.AudioSource?.ToString() },
                        { "loop duration", loop?.Loop?.Session?.AudioSource?.Duration.ToString() },
                    };

                    logger.LogError(new Exception("failed to convert time"), properties);
                    return FormatTime(0);
                }

                var result = FormatTime(loopPosition.Value * duration.Value * 1000);
                return result;
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
