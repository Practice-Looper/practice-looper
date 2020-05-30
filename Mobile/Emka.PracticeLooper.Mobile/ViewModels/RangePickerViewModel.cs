// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
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
        private readonly IDialogService dialogService;
        private readonly ILogger logger;
        private readonly TimeSpan fiveSecondsSpan = new TimeSpan(0, 0, 5);
        #region Properties

        private ObservableCollection<object> time;
        private ObservableCollection<string> selectedTime;
        #endregion

        #region Ctor
        public RangePickerViewModel(AudioSource audioSource, Loop loop, bool isStartPosition, IDialogService dialogService, ILogger logger)
        {
            AudioSource = audioSource;
            Loop = loop;
            IsStartPosition = isStartPosition;
            this.dialogService = dialogService;
            this.logger = logger;
            Hours = new RangeObservableCollection<string>();
            Minutes = new RangeObservableCollection<string>();

            Seconds = new RangeObservableCollection<string>();

            Headers = new ObservableCollection<string>
            {
                "Minutes", "Seconds"
            };

            Time = new ObservableCollection<object>
            {
                Minutes, Seconds
            };

            SelectedTime = new ObservableCollection<string>();
        }
        #endregion

        #region Properties
        public ObservableCollection<object> Time { get => time; set => time = value; }
        public RangeObservableCollection<string> Hours { get; set; }
        public RangeObservableCollection<string> Minutes { get; set; }
        public RangeObservableCollection<string> Seconds { get; set; }
        public ObservableCollection<string> Headers { get; set; }
        public ObservableCollection<string> SelectedTime
        {
            get => selectedTime;
            set
            {
                selectedTime = value;
                NotifyPropertyChanged();
            }
        }

        public AudioSource AudioSource { get; }
        public Loop Loop { get; }
        public bool IsStartPosition { get; }
        public int LengthHours { get; private set; }
        public int LengthMinutes { get; private set; }
        public int LengthSeconds { get; private set; }
        public double TotalSeconds { get; private set; }
        private int MinutesMinimum { get; set; }
        private int MinutesMaximum { get; set; }
        private int SecondsMinimum { get; set; }
        private int SecondsMaximum { get; set; }
        #endregion

        #region Methods
        public async override Task InitializeAsync(object parameter)
        {
            if (AudioSource != null)
            {
                // total time

                LengthHours = TimeSpan.FromSeconds(AudioSource.Duration).Hours;
                LengthMinutes = TimeSpan.FromSeconds(AudioSource.Duration).Minutes;
                LengthSeconds = TimeSpan.FromSeconds(AudioSource.Duration).Seconds;
                TotalSeconds = TimeSpan.FromSeconds(AudioSource.Duration).TotalSeconds;

                if (LengthHours > 0)
                {
                    await dialogService?.ShowAlertAsync(AppResources.Hint_Content_FileTooLong_Content, AppResources.Hint_Caption_FileTooLong_Caption);
                    await logger.LogErrorAsync(new Exception("File too long!"));
                    return;
                }

                if (IsStartPosition)
                {
                    MinutesMaximum = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Subtract(fiveSecondsSpan).Minutes;
                    Minutes.InsertRange(Enumerable.Range(MinutesMinimum, MinutesMaximum + 1).Select(i => i.ToString("D2")));
                    var minutesIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition).Minutes;
                    var secondsIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition).Seconds;
                    UpdateSeconds(minutesIndex.ToString("D2"), secondsIndex.ToString("D2"));
                }
                else
                {
                    MinutesMinimum = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition).Add(fiveSecondsSpan).Minutes;
                    Minutes.InsertRange(Enumerable.Range(MinutesMinimum, LengthMinutes + 1).Select(i => i.ToString("D2")));
                    var minutesIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Minutes;
                    var secondsIndex = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Seconds;
                    UpdateSeconds(minutesIndex.ToString("D2"), secondsIndex.ToString("D2"));
                }
            }
        }

        public void UpdateSeconds(string minutes, string seconds)
        {
            if (SelectedTime.Any() && minutes == SelectedTime[0])
            {
                return;
            }

            if (IsStartPosition)
            {
                Seconds.Clear();
                if (Minutes.IndexOf(minutes) == Minutes.Count - 1)
                {
                    // create minutes and seconds ranges
                    SecondsMaximum = TimeSpan.FromSeconds(AudioSource.Duration * Loop.EndPosition).Subtract(fiveSecondsSpan).Seconds;
                    Seconds.InsertRange(Enumerable.Range(SecondsMinimum, SecondsMaximum + 1).Select(i => i.ToString("D2")));
                    seconds = SecondsMinimum.ToString("D2");
                }
                else
                {
                    // adds 60 seconds if the song is not shorter in total length
                    var maxValue = TotalSeconds < 60 ? TotalSeconds : 60;
                    Seconds.InsertRange(Enumerable.Range(SecondsMinimum, (int)maxValue).Select(i => i.ToString("D2")));
                }
            }
            else
            {
                Seconds.Clear();
                if (Minutes.IndexOf(minutes) == 0)
                {
                    var maxValue = TotalSeconds < 60 ? TotalSeconds : 60;
                    Seconds.InsertRange(Enumerable.Range(SecondsMinimum, (int)maxValue - 5).Select(i => i.ToString("D2")));
                    seconds = SecondsMinimum.ToString("D2");
                }
                else if (Minutes.IndexOf(minutes) == Minutes.Count - 1)
                {
                    SecondsMaximum = TimeSpan.FromSeconds(AudioSource.Duration).Seconds;
                    SecondsMinimum = TimeSpan.FromSeconds(AudioSource.Duration * Loop.StartPosition).Add(fiveSecondsSpan).Seconds;
                    Seconds.InsertRange(Enumerable.Range(0, SecondsMaximum + 1).Select(i => i.ToString("D2")));
                }
                else
                {
                    Seconds.InsertRange(Enumerable.Range(0, 60).Select(i => i.ToString("D2")));
                }
            }

            Time.RemoveAt(1);
            Time.Add(Seconds);
            SelectedTime = new ObservableCollection<string> { minutes, seconds };
        }
    }
    #endregion
}
