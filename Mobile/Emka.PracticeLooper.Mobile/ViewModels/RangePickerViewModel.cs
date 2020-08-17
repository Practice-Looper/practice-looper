// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class RangePickerViewModel : ViewModelBase
    {
        #region Fields
        private readonly IDialogService dialogService;
        private readonly TimeSpan fiveSecondsSpan = new TimeSpan(0, 0, 5);
        private readonly object locker = new object();
        #endregion

        #region Ctor
        public RangePickerViewModel(
            IDialogService dialogService,
            ILogger logger,
            INavigationService navigationService,
            IAppTracker appTracker)
            : base(navigationService, logger, appTracker)
        {
            this.dialogService = dialogService;
            Minutes = new RangeObservableCollection<object>();
            Time = new List<object>();
            SelectedTime = new ObservableCollection<object>();
            Headers = new ObservableCollection<string>
            {
                AppResources.Minutes, AppResources.Seconds
            };
        }
        #endregion

        #region Properties
        public List<object> Time { get; private set; }
        public RangeObservableCollection<object> Minutes { get; set; }
        public ObservableCollection<string> Headers { get; set; }
        public ObservableCollection<object> SelectedTime { get; private set; }

        public AudioSource AudioSource { get; set; }
        public Loop Loop { get; set; }
        public bool IsStartPosition { get; private set; }
        public int LengthHours { get; private set; }
        public int LengthMinutes { get; private set; }
        public int LengthSeconds { get; private set; }
        public double TotalSeconds { get; private set; }
        private int MinutesMinimum { get; set; }
        private int MinutesMaximum { get; set; }
        #endregion

        #region Methods
        public async override Task InitializeAsync(object parameter)
        {
            if (AudioSource == null || Loop == null)
            {
                throw new InvalidOperationException("AudioSource or Loop null reference");
            }

            if (parameter is bool isStartPosition)
            {
                IsStartPosition = isStartPosition;
            }

            LengthHours = TimeSpan.FromSeconds(AudioSource.Duration).Hours;
            LengthMinutes = TimeSpan.FromSeconds(AudioSource.Duration).Minutes;
            LengthSeconds = TimeSpan.FromSeconds(AudioSource.Duration).Seconds;
            TotalSeconds = TimeSpan.FromSeconds(AudioSource.Duration).TotalSeconds;

            if (LengthHours > 0)
            {
                await dialogService?.ShowAlertAsync(AppResources.Hint_Content_FileTooLong_Content, AppResources.Hint_Caption_FileTooLong_Caption);
                await Logger.LogErrorAsync(new Exception("File too long!"));
                return;
            }

            lock (locker)
            {
                Minutes.Clear();
                int minutesIndex;
                int secondsIndex;

                if (IsStartPosition)
                {
                    MinutesMaximum = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Subtract(fiveSecondsSpan).Minutes;
                    Minutes.InsertRange(Enumerable.Range(0, MinutesMaximum + 1).Select(i => i.ToString("D2")));
                    minutesIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition).Minutes;
                    secondsIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition).Seconds;
                }
                else
                {
                    MinutesMinimum = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition).Add(fiveSecondsSpan).Minutes;
                    Minutes.InsertRange(Enumerable.Range(MinutesMinimum, LengthMinutes - MinutesMinimum + 1).Select(i => i.ToString("D2")));
                    minutesIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Minutes;
                    secondsIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Seconds;
                }

                Time = new List<object> { Minutes, GetValidSeconds(minutesIndex.ToString("D2")) };
                SelectedTime = new ObservableCollection<object> { minutesIndex.ToString("D2"), secondsIndex.ToString("D2") };
            }
        }

        public RangeObservableCollection<object> GetValidSeconds(string minutes)
        {
            var result = new RangeObservableCollection<object>();
            var startSpan = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition);
            var currentMinuteSpan = TimeSpan.FromMinutes(int.Parse(minutes));

            if (IsStartPosition)
            {
                var nextValue = TimeSpan.FromMinutes(int.Parse(minutes)).Add(TimeSpan.FromMinutes(1)).TotalSeconds;
                var endValue = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).TotalSeconds;
                var seconds = endValue - nextValue >= 5 ? 59 : TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Subtract(fiveSecondsSpan).Seconds;
                result.InsertRange(Enumerable.Range(0, seconds + 1).Select(i => i.ToString("D2")));
                return result;
            }
            else
            {
                var nextValue = currentMinuteSpan.Add(TimeSpan.FromMinutes(1)).TotalSeconds;
                var endValue = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).TotalSeconds;
                var minSeconds = currentMinuteSpan.TotalSeconds - startSpan.TotalSeconds >= 5 ? 0 : startSpan.Add(fiveSecondsSpan).Seconds;
                var maxSeconds = nextValue > TotalSeconds ? TimeSpan.FromSeconds(AudioSource.Duration).Seconds : 59;
                result.InsertRange(Enumerable.Range(minSeconds, maxSeconds - minSeconds + 1).Select(i => i.ToString("D2")));
                return result;
            }
        }
    }
    #endregion
}
