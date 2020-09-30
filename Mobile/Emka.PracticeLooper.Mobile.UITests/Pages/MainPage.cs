// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using Emka.PracticeLooper.Mobile.UITests.Common;
using Xamarin.UITest.Queries;
using System.Threading.Tasks;
using Emka.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Moq;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;
using Emka.PracticeLooper.Mobile.Common;
using System.Linq;

namespace Emka.PracticeLooper.Mobile.UITests.Pages
{
    public class MainPage : BasePage
    {
        readonly Query SettingsButton;
        readonly Query AddSongButton;
        readonly Query SpotifyButton;
        readonly Query PlayButton;
        readonly Query StopButton;
        readonly Query LoopStartPosition;
        readonly Query LoopEndPosition;
        readonly Query CurrentSongTime;
        readonly Query SongDuration;
        readonly Query Slider;
        readonly Query AddBookmarkButton;
        readonly Query AdmobBanner;
        readonly Query Ok;
        readonly Query Save;
        readonly Query Cancel;
        readonly Query ClosePicker;
        readonly Query MinutesPickerEngine;
        readonly Query SecondsPickerEngine;
        private readonly Mock<ILogger> loggerMock;
        private readonly IStringLocalizer stringLocalizer;

        protected override PlatformQuery Trait => new PlatformQuery
        {
             Android = x => x.Marked("SettingsButton"),
             iOS = x => x.Marked("SettingsButton")
        };

        public MainPage()
        {
            loggerMock = new Mock<ILogger>();
            stringLocalizer = new StringLocalizer(loggerMock.Object);

            SettingsButton = x => x.Marked("SettingsButton");
            AddSongButton = x => x.Marked("AddSongButton");
            SpotifyButton = x => x.Text("Spotify");
            LoopStartPosition = x => x.Marked("LoopStartPosition");
            LoopEndPosition = x => x.Marked("LoopEndPosition");
            CurrentSongTime = x => x.Marked("CurrentSongTime");
            SongDuration = x => x.Marked("SongDuration");
            Slider = x => x.Marked("Slider");
            ClosePicker = x => x.Marked("ClosePicker");
            AddBookmarkButton = x => x.Marked("AddBookmarkButton");
            PlayButton = x => x.Marked("PlayButton");
            StopButton = x => x.Marked("PlayButton");
            AdmobBanner = x => x.Marked("AdmobBanner");
            Ok = x => x.Text(stringLocalizer.GetLocalizedString("Ok"));
            Save = x => x.Text(stringLocalizer.GetLocalizedString("Save"));
            Cancel = x => x.Text(stringLocalizer.GetLocalizedString("Cancel"));
            MinutesPickerEngine = x => x.Class("crc645f5d5eaea4c07924.SfParentPicker").Child(0);
            SecondsPickerEngine = x => x.Class("crc645f5d5eaea4c07924.SfParentPicker").Child(1);
        }

        public MainPage GoToSettings()
        {
            app.Tap(SettingsButton);
            return this;
        }

        public MainPage AddNewSpotifiySession()
        {

            if (AppInitializer.IsLite)
            {
                app.WaitForElement(AdmobBanner);
                Task.Delay(3000).Wait();
                app.Tap(AddSongButton);
                CloseInterstitialAd();
            }
            else
            {
                app.Tap(AddSongButton);
            }

            app.WaitForElement(SpotifyButton);
            app.Tap(SpotifyButton);

            return this;
        }

        public MainPage Play()
        {
            app.Tap(PlayButton);
            System.Threading.Thread.Sleep(2000);
            var popup = app.Query(Ok).FirstOrDefault();
            if (popup != null)
            {
                app.Tap(popup.Id);
            }

            return this;
        }

        public MainPage Stop()
        {
            app.Tap(StopButton);

            if (AppInitializer.IsLite)
            {
                CloseInterstitialAd();
            }

            return this;
        }

        public string GetSongDuration()
        {
            return app.Query(SongDuration).First().Text;
        }

        public double GetSongDurationAsDouble()
        {
            var songDuration = GetSongDuration();
            double seconds = TimeSpan.Parse("00:" + songDuration).TotalSeconds;
            return seconds;
        }

        public string GetCurrentSongTime()
        {
            return app.Query(CurrentSongTime).First().Text;
        }

        public double GetCurrentSongTimeAsDouble()
        {
            var currentSongTime = GetCurrentSongTime();
            double seconds = TimeSpan.Parse("00:" + currentSongTime).TotalSeconds;
            return seconds;
        }

        public string GetLoopStartPosition()
        {
            return app.Query(LoopStartPosition).First().Text.TrimStart('[');
        }

        public double GetLoopStartPositionAsDouble()
        {
            var loopStartPosition = GetLoopStartPosition();
            double seconds = TimeSpan.Parse("00:" + loopStartPosition).TotalSeconds;
            return seconds;
        }

        public string GetLoopEndPosition()
        {
            return app.Query(LoopEndPosition).First().Text.TrimEnd(']');
        }

        public double GetLoopEndPositionAsDouble()
        {
            var loopEndPosition = GetLoopEndPosition();
            double seconds = TimeSpan.Parse("00:" + loopEndPosition).TotalSeconds;
            return seconds;
        }

        public MainPage AddBookmark(string name)
        {
            app.Tap(AddBookmarkButton);
            app.Query(x => x.Class("EditText").Invoke("setText", name));
            return this;
        }

        public void SaveBookmark() {
            app.Tap(Save);
        }

        public void CancelBookmark()
        {
            app.Tap(Cancel);
        }

        public void OpenBookmarks(string song)
        {
            var result = app.Query(x => x.Text(song).Sibling().Index(1)).First();
            app.Tap(result.Id);
            Task.Delay(1000).Wait();
        }

        public void SelectSession(string song)
        {
            app.Tap(x => x.Text(song));
        }

        public bool IsAdMobBannerVisible()
        {
            app.WaitForElement(AdmobBanner, "Admob Banner not visible!", TimeSpan.FromSeconds(5));
            var banner = app.Query(AdmobBanner);
            return banner.Length == 1;
        }

        public bool IsAddBookmarkButtonVisisble()
        {
            app.WaitForElement(AddBookmarkButton, "Bookmark Button not visisble", TimeSpan.FromSeconds(1));
            var button = app.Query(AddBookmarkButton);
            return button.Length == 1;
        }

        public bool IsOpenBookmarkButtonVisible(string song)
        {
            var result = app.Query(x => x.Text(song).Sibling());
            return result.Length == 2;
        }

        public void CloseInterstitialAd()
        {           
            var closeButton = app.Query(x => x.All().Marked("Interstitial close button")).FirstOrDefault();
            if (closeButton == null)
            {
                // Ad is not laoded
                return;
            }
            else
            {
                Task.Delay(5500).Wait();
                app.TapCoordinates(80, 240);

                Task.Delay(500).Wait();
                closeButton = app.Query(x => x.All().Marked("Interstitial close button")).FirstOrDefault();
                if (closeButton != null)
                {
                    app.TapCoordinates(1075, 78);
                }
                else
                {
                    return;
                }

                Task.Delay(500).Wait();
                closeButton = app.Query(x => x.All().Marked("Interstitial close button")).FirstOrDefault();
                if (closeButton != null)
                {
                    app.TapCoordinates(330, 1400);
                }
                else
                {
                    return;
                }

                Task.Delay(500).Wait();
                closeButton = app.Query(x => x.All().Marked("Interstitial close button")).FirstOrDefault();
                if (closeButton != null)
                {
                    app.TapCoordinates(280, 2135);
                }
                else
                {
                    return;
                }
            }
        }

        public void SetSlider(double lower, double upper)
        {
            var slider = app.Query(Slider).First();
            var PixelPerSecond = slider.Rect.Width / GetSongDurationAsDouble();

            var y = slider.Rect.CenterY;
            var xLeft = slider.Rect.CenterX - (slider.Rect.Width / 2);
            foreach (var i in Enumerable.Range(0,4))
            {   
                var lowerDiff = lower - GetLoopStartPositionAsDouble();
                if (lowerDiff != 0)
                {
                    var pixelToDragFromLeft = PixelPerSecond * lowerDiff;

                    app.DragCoordinates(xLeft, y, xLeft + Convert.ToInt64(pixelToDragFromLeft), y);
                    xLeft += Convert.ToInt64(pixelToDragFromLeft);
                }
            }

            Task.Delay(500).Wait();

            var xRight = slider.Rect.CenterX + (slider.Rect.Width / 2);
            foreach (var i in Enumerable.Range(0, 4))
            {
                var upperDiff = upper - GetLoopEndPositionAsDouble();
                if (upperDiff != 0)
                {
                    var pixelToDragFromRight = PixelPerSecond * upperDiff;
                    app.DragCoordinates(xRight - 1, y, xRight + Convert.ToInt64(pixelToDragFromRight), y);
                    xRight += Convert.ToInt64(pixelToDragFromRight);
                }
            }

        }

        public void SetLeftPicker(double seconds)
        {
            app.Tap(LoopStartPosition);
            var leftEngine = app.Query(MinutesPickerEngine).First();
            var rightEngine = app.Query(SecondsPickerEngine).First();

            var timeToSet = TimeSpan.FromSeconds(seconds);

            //Set Minutes
            var actualMinutes = TimeSpan.FromSeconds(GetLoopStartPositionAsDouble()).Minutes;
            var minutesToDrag = 0;
            var direction = 0;
            if (actualMinutes < timeToSet.Minutes)
            {
                minutesToDrag = timeToSet.Minutes - actualMinutes;
                direction = -1;
            }
            else
            {
                minutesToDrag = actualMinutes - timeToSet.Minutes;
                direction = 1;
            }

            if (minutesToDrag != 0)
            {
                foreach (var i in Enumerable.Range(0, minutesToDrag))
                {
                    app.DragCoordinates(leftEngine.Rect.X, leftEngine.Rect.Y, leftEngine.Rect.X, leftEngine.Rect.Y + (direction * 86));
                }
            }

            //Set Seconds
            var actualSeconds = TimeSpan.FromSeconds(GetLoopStartPositionAsDouble()).Seconds;
            var secondsToDrag = 0;

            if (actualSeconds < timeToSet.Seconds)
            {
                secondsToDrag = timeToSet.Seconds - actualSeconds;
                direction = -1;
            }
            else
            {
                secondsToDrag = actualSeconds - timeToSet.Seconds;
                direction = 1;
            }

            if (secondsToDrag != 0)
            {
                foreach (var i in Enumerable.Range(0, secondsToDrag))
                {
                    app.DragCoordinates(rightEngine.Rect.X, rightEngine.Rect.Y, rightEngine.Rect.X, rightEngine.Rect.Y + (direction * 86));
                }
            }

            app.Tap(ClosePicker);
        }

        public void SetRightPicker(double seconds)
        {
            app.Tap(LoopEndPosition);
            var leftEngine = app.Query(MinutesPickerEngine).First();
            var rightEngine = app.Query(SecondsPickerEngine).First();

            var timeToSet = TimeSpan.FromSeconds(seconds);

            //Set Minutes
            var actualMinutes = TimeSpan.FromSeconds(GetLoopEndPositionAsDouble()).Minutes;

            var minutesToDrag = 0;
            var direction = 0;
            if (actualMinutes < timeToSet.Minutes)
            {
                minutesToDrag = timeToSet.Minutes - actualMinutes;
                direction = -1;
            }
            else
            {
                minutesToDrag = actualMinutes - timeToSet.Minutes;
                direction = 1;
            }

            if (minutesToDrag != 0)
            {
                foreach (var i in Enumerable.Range(0, minutesToDrag))
                {
                    app.DragCoordinates(leftEngine.Rect.X, leftEngine.Rect.Y, leftEngine.Rect.X, leftEngine.Rect.Y + (direction * 86));
                }
            }

            //Set Seconds
            var actualSeconds = TimeSpan.FromSeconds(GetLoopEndPositionAsDouble()).Seconds;
            var secondsToDrag = 0;
            
            if (actualSeconds < timeToSet.Seconds)
            {
                secondsToDrag = timeToSet.Seconds - actualSeconds;
                direction = -1;
            }
            else
            {
                secondsToDrag = actualSeconds - timeToSet.Seconds;
                direction = 1;
            }

            if (secondsToDrag != 0)
            {
                foreach (var i in Enumerable.Range(0, secondsToDrag))
                {
                    app.DragCoordinates(rightEngine.Rect.X, rightEngine.Rect.Y, rightEngine.Rect.X, rightEngine.Rect.Y + (direction * 86));
                }
            }

            app.Tap(ClosePicker);
        }

    }
}
