// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Model.Player;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Support.UI;

namespace Emka.PracticeLooper.Mobile.UITests.Tests.MainPage
{
    public class BasePage<T, W> : AppInitializer<T, W>
         where T : AppiumDriver<W>
         where W : IWebElement
    {
        private W slider;

        public BasePage() : base()
        {

        }

        protected override T GetDriver()
        {
            throw new NotImplementedException();
        }

        protected override void InitAppiumOptions(AppiumOptions appiumOptions)
        {
            throw new NotImplementedException();
        }

        [SetUp]
        public void SetupTest()
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            SkipTutorial();
        }

        public void Play()
        {
            var element = driver.FindElementByAccessibilityId("PlayButton");
            element.Click();

            Thread.Sleep(500);
        }

        public void Stop()
        {
            var element = driver.FindElementByAccessibilityId("PlayButton");
            element.Click();

            Thread.Sleep(500);
        }

        public bool IsLoopButtonVisible()
        {
            var element = driver.FindElementByAccessibilityId("AddBookmarkButton");
            return element.Displayed;
        }

        public bool IsSessionLoopsButtonVisible(string songName)
        {
            var elements = driver.FindElementsByAccessibilityId(songName);
            return elements.Count == 2;
        }


        public void OpenSpotifySearchPage()
        {
            var addButton = driver.FindElementByAccessibilityId("AddSongButton");
            addButton.Click();

            Thread.Sleep(500);

            var spotify = driver.FindElementByAccessibilityId("Spotify");
            spotify.Click();

            Thread.Sleep(500);
        }

        public bool IsSpotifySearchPageVisible()
        {
            return driver.FindElementByAccessibilityId("SearchBar").Displayed;
        }

        public void SearchSong(string songName) {
            
            if (IsAndroid)
            {
                // Android Search Bar is not accessible via "SearchBar", need "EditText"
                var element = driver.FindElementByClassName("android.widget.EditText");
                element.SendKeys(songName);
               
            }

            if (IsIos)
            {
                var element = driver.FindElementByAccessibilityId("SearchBar");
                element.SendKeys(songName);
            }

            Thread.Sleep(1000);
        }

        public void SelectSong(int id)
        {
            if (IsAndroid)
            {
                var elements = driver.FindElementsByClassName("android.view.View");
                elements[id].Click();
            }

            if (IsIos)
            {
                var elements = driver.FindElementsByXPath("//XCUIElementTypeCell");
                elements[id].Click();
            }

            Thread.Sleep(500);
        }

        public void CreateLoop(string loopName)
        {
            var addLoopButton = driver.FindElementByAccessibilityId("AddBookmarkButton");
            addLoopButton.Click();

            if (IsAndroid)
            {
                var element = driver.FindElementByClassName("android.widget.EditText");
                element.SendKeys(loopName);
            }

            if (IsIos)
            {
                var element = driver.FindElementByClassName("XCUIElementTypeTextField");
                element.SendKeys(loopName);
            }

            Thread.Sleep(200);
        }

        public void SaveLoop()
        {
            if (IsAndroid)
            {
                var element = driver.FindElementById("android:id/button1");
                element.Click();
            }

            if (IsIos)
            {
                var element = driver.FindElementByXPath(string.Format("//XCUIElementTypeButton[@label=\"{0}\"]", "Speichern"));
                element.Click();
            }

        }

        public void CancelLoop()
        {
            if (IsAndroid)
            {
                var element = driver.FindElementById("android:id/button2");
                element.Click();
            }

            if (IsIos)
            {
                var element = driver.FindElementByXPath(string.Format("//XCUIElementTypeButton[@label=\"{0}\"]", "Abbrechen"));
                element.Click();
            }

        }

        public void OpenSettings()
        {
            var element = driver.FindElementByAccessibilityId("SettingsButton");
            element.Click();

            Thread.Sleep(500);
        }

        public InAppPurchaseProduct GetProduct()
        {
            var name = driver.FindElementByAccessibilityId("ProductName").Text;
            var purchased = false;
            var purchasedPrice = string.Empty;
            try
            {
                var purchasedItem = driver.FindElementByAccessibilityId("ProductCheck");
                purchased = purchasedItem.Displayed;
            }
            catch (NoSuchElementException)
            {
                var purchasedPriceItem = driver.FindElementByAccessibilityId("ProductPrice");
                purchasedPrice = purchasedPriceItem.Text;
            }

            var product = new InAppPurchaseProduct
            {
                Name = name,
                ProductId = "id",
                Description = "Description",
                Image = "image.png",
                Purchased = purchased,
                LocalizedPrice = purchasedPrice,
                CurrencyCode = "de",
                Package = "Premium"
            };

            return product;
        }

        public void NavigateBack()
        {
            if (IsAndroid)
            {
                var element = driver.FindElementByClassName("android.widget.ImageButton");
                element.Click();
            }

            if (IsIos)
            {
                var element = driver.FindElementByClassName("XCUIElementTypeButton");
                element.Click();
            }

            Thread.Sleep(1000);
        }

        public void OpenLoops(string songName)
        {
            var elements = new List<W>(driver.FindElementsByAccessibilityId(songName));

            if (elements.Count != 2)
            {
                throw new InvalidSelectorException("Song has no Loop Button!");
            }
            elements[1].Click();

            Thread.Sleep(1000);
        }

        public bool IsSongInSessionList(string songName)
        {
            if (IsAndroid)
            {
                try
                {
                    var element = driver.FindElementByXPath(string.Format("//android.widget.TextView[@content-desc=\"{0}\"]", songName));
                    return element.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            }

            if (IsIos)
            {
                try
                {
                    var element = driver.FindElementByXPath(string.Format("//XCUIElementTypeStaticText[@label=\"{0}\"]", songName));
                    return element.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            }

            return false;

        }

        public void SelectSong(string songName)
        {
            var element = driver.FindElementByAccessibilityId(songName);
            element.Click();

            Thread.Sleep(500);
        }

        public void DeleteSong(string songName)
        {
            DeleteRow(songName);
        }

        public Loop SelectLoop(string loopName)
        {
            var loop = new Loop() { };
            var loopElements = new List<W>(driver.FindElementsByAccessibilityId(loopName));

            if (IsAndroid)
            {
                loop.Name = loopElements[0].Text;
                loop.StartPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[1].Text)).TotalSeconds;
                loop.EndPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[2].Text)).TotalSeconds;
                loopElements[0].Click();
            }

            if (IsIos)
            {
                loop.Name = loopElements[2].Text;
                loop.StartPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[0].Text)).TotalSeconds;
                loop.EndPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[1].Text)).TotalSeconds;
                loopElements[2].Click();
            }

            Thread.Sleep(500);

            return loop;
        }

        public Loop GetLoop(string loopName)
        {
            var loop = new Loop() { };
            var loopElements = new List<W>(driver.FindElementsByAccessibilityId(loopName));

            if (IsAndroid)
            {
                loop.Name = loopElements[0].Text;
                loop.StartPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[1].Text)).TotalSeconds;
                loop.EndPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[2].Text)).TotalSeconds;
            }

            if (IsIos)
            {
                loop.Name = loopElements[2].Text;
                loop.StartPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[0].Text)).TotalSeconds;
                loop.EndPosition = TimeSpan.Parse(string.Format("00:{0}", loopElements[1].Text)).TotalSeconds;
            }

            Thread.Sleep(1000);
            return loop;
        }

        public void DeleteLoop(string loopName)
        {
            DeleteRow(loopName);
        }

        private void DeleteRow(string rowName)
        {
            var row = driver.FindElementByAccessibilityId(rowName);

            if (IsAndroid)
            {
                var action = new TouchAction(driver);
                action.LongPress(row).Release().Perform();
                var element = driver.FindElementByClassName("android.widget.TextView");
                element.Click();
            }

            if (IsIos)
            {
                Dictionary<string, object> swipeLeft = new Dictionary<string, object>();
                swipeLeft.Add("element", row);
                swipeLeft.Add("velocity", 2500);
                swipeLeft.Add("direction", "left");
                ((IJavaScriptExecutor)driver).ExecuteScript("mobile: swipe", swipeLeft);

                TouchAction action = new TouchAction(driver);
                action.Press(driver.Manage().Window.Size.Width - 40, row.Location.Y).Perform();
            }

            Thread.Sleep(500);
        }


        public double GetSongDuration()
        {
            var element = driver.FindElementByAccessibilityId("SongDuration");
            var songDuration = element.Text;
            double seconds = TimeSpan.Parse(string.Format("00:{0}", songDuration)).TotalSeconds;
            return seconds;
        }

        public double GetCurrentSongTime()
        {
            var element = driver.FindElementByAccessibilityId("CurrentSongTime");
            var currentSongTime = element.Text;
            double seconds = TimeSpan.Parse(string.Format("00:{0}", currentSongTime)).TotalSeconds;
            return seconds;
        }

        public double GetLoopStartPosition()
        {
            var element = driver.FindElementByAccessibilityId("LoopStartPosition");
            var loopStartPosition = element.Text.TrimStart('[');
            double seconds = TimeSpan.Parse(string.Format("00:{0}", loopStartPosition)).TotalSeconds;
            return seconds;
        }

        public double GetLoopEndPosition()
        {
            var element = driver.FindElementByAccessibilityId("LoopEndPosition");
            var loopEndPosition = element.Text.TrimEnd(']');
            double seconds = TimeSpan.Parse(string.Format("00:{0}", loopEndPosition)).TotalSeconds;
            return seconds;
        }


        public bool IsDarkModeEnabled()
        {
            var toggle = driver.FindElementByAccessibilityId("SchemeSwitch");

            if (IsAndroid)
            {
                var toggleValue = toggle.GetAttribute("checked");
                return toggleValue.Equals("true");
            }

            if (IsIos)
            {
                var toggleValue = toggle.GetAttribute("value");
                return toggleValue.Equals("1");
            }

            return false;
        }
        
        public void EnableDarkmode()
        {
            if (!IsDarkModeEnabled())
            {
                var element = driver.FindElementByAccessibilityId("SchemeSwitch");
                element.Click();
            }

            Thread.Sleep(300);
        }

        public void DisableDarkmode()
        {
            if (IsDarkModeEnabled())
            {
                var element = driver.FindElementByAccessibilityId("SchemeSwitch");
                element.Click();
            }

            Thread.Sleep(300);
        }

        public void SetSlider(double lower, double upper)
        {
            
            if (IsAndroid)
            {
                slider = driver.FindElementByXPath("//android.view.ViewGroup[@content-desc='Slider_Container']");
            }

            if (IsIos)
            {
                slider = driver.FindElementByAccessibilityId("Slider");
            }

            var PixelPerSecond = (slider.Size.Width - 20) / GetSongDuration();
            var y = slider.Location.Y;
            var xLeft = slider.Location.X + 10;
            var xRight = slider.Location.X + slider.Size.Width - 10;

            foreach (var i in Enumerable.Range(0, 4))
            {
                var lowerDiff = lower - GetLoopStartPosition();
                if (lowerDiff != 0)
                {
                    var pixelToDragFromLeft = PixelPerSecond * lowerDiff;

                    if (IsAndroid)
                    {
                        TouchAction action = new TouchAction(driver);
                        action.Press(xLeft, y);
                        action.MoveTo(xLeft + Convert.ToInt32(pixelToDragFromLeft), y);
                        action.Release();
                        action.Perform();
                    }

                    if (IsIos)
                    {
                        var args = new Dictionary<string, object>();
                        args.Add("duration", 1);
                        args.Add("fromX", xLeft);
                        args.Add("fromY", y + slider.Size.Height / 2);
                        args.Add("toX", xLeft + Convert.ToInt32(pixelToDragFromLeft));
                        args.Add("toY", y + slider.Size.Height / 2);
                        ((IJavaScriptExecutor)driver).ExecuteScript("mobile: dragFromToForDuration", args);

                    }

                    xLeft += Convert.ToInt32(pixelToDragFromLeft);
                }
                else
                {
                    break;
                }
            }

            foreach (var i in Enumerable.Range(0, 4))
            {
                var upperDiff = upper - GetLoopEndPosition();
                if (upperDiff != 0)
                {
                    var pixelToDragFromRight = PixelPerSecond * upperDiff;

                    if (IsAndroid)
                    {
                        TouchAction action = new TouchAction(driver);
                        action.Press(xRight, y);
                        action.MoveTo(xRight + Convert.ToInt32(pixelToDragFromRight), y);
                        action.Release();
                        action.Perform();
                    }

                    if (IsIos)
                    {
                        var args = new Dictionary<string, object>();
                        args.Add("duration", 1);
                        args.Add("fromX", xRight);
                        args.Add("fromY", y + slider.Size.Height / 2);
                        args.Add("toX", xRight + Convert.ToInt32(pixelToDragFromRight));
                        args.Add("toY", y + slider.Size.Height / 2);
                        ((IJavaScriptExecutor)driver).ExecuteScript("mobile: dragFromToForDuration", args);
                    }

                    xRight += Convert.ToInt32(pixelToDragFromRight);
                }
                else
                {
                    break;
                }
            }

            Thread.Sleep(500);
        }

        public void LaunchApp()
        {
            driver.LaunchApp();
            SkipTutorial();
        }

        public void DeinstallSpotify()
        {
            if (IsAndroid)
            {
                driver.RemoveApp("com.spotify.music");
            }

            if (IsIos)
            {
                driver.RemoveApp("com.spotify.client");
            }

            Thread.Sleep(200);
        }  

        public void AcceptSpotifyInstallation()
        {
            if (IsAndroid)
            {
                var button = driver.FindElementById("android:id/button1");
                button.Click();

                Thread.Sleep(300);

                var install = driver.FindElementByXPath(string.Format("//android.widget.Button[@text=\"{0}\"]", "Installieren"));
                install.Click();

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(290));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(string.Format("//android.widget.Button[@text=\"{0}\"]", "Öffnen"))));
                driver.LaunchApp();
                SkipTutorial();
            }

            if (IsIos)
            {
                var button = driver.FindElementByAccessibilityId("OK");
                button.Click();

                Thread.Sleep(300);

                var install = driver.FindElementByAccessibilityId("neuer download");
                install.Click();

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(290));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("öffnen")));

                var finish = driver.FindElementByAccessibilityId("Fertig");
                finish.Click();
            }
        }

        public void LoginToSpotify()
        {
            OpenSpotifySearchPage();
            var credentials = JObject.Parse(File.ReadAllText("../../../Emka3.PracticeLooper.Config/credentials.json"));

            if (IsAndroid)
            {
                var login = driver.FindElementByXPath(string.Format("//android.widget.Button[@text=\"{0}\"]", "Anmelden"));
                login.Click();
                var fields = driver.FindElementsByClassName("android.widget.EditText");
                fields[0].SendKeys(credentials.GetValue("username").ToString());
                fields[1].SendKeys(credentials.GetValue("password").ToString());
                var signin = driver.FindElementByXPath(string.Format("//android.widget.Button[@text=\"{0}\"]", "ANMELDEN"));
                signin.Click();
            }

            if (IsIos)
            {
                try
                {
                    driver.SwitchTo().Alert().Accept();
                }
                catch (Exception)
                {
                    // nothing to do
                }
            }

            Thread.Sleep(300);
        }

        public void DeclineSpotifyInstallation()
        {
            if (IsAndroid)
            {
                var element = driver.FindElementById("android:id/button2");
                element.Click();
            }

            if (IsIos)
            {
                var element = driver.FindElementByAccessibilityId("Abbrechen");
                element.Click();
            }

            Thread.Sleep(300);
        }

        public void SkipTutorial()
        {
            var element = driver.FindElementByAccessibilityId("SkipTutorialButton");
            element.Click();

            Thread.Sleep(200);
        }

        public void SetLeftPicker(double seconds)
        {
            double actualStart = GetLoopStartPosition();
            double actualEnd = GetLoopEndPosition();

            var timeToSet = TimeSpan.FromSeconds(seconds);

            var actualMinutesStart = TimeSpan.FromSeconds(actualStart).Minutes;
            var minutesToDrag = Math.Abs(timeToSet.Minutes - actualMinutesStart);
            var minuteDirection = actualMinutesStart > timeToSet.Minutes ? 1 : -1;
            (int, int) minutesToSet = (minutesToDrag, minuteDirection);

            var actualSecondsStart = TimeSpan.FromSeconds(actualStart).Seconds;
            var secondsToDrag = Math.Abs(timeToSet.Seconds - actualSecondsStart);
            var secondDirection = actualSecondsStart > timeToSet.Seconds ? 1 : -1;

            if (actualStart + minutesToDrag * 60 > actualEnd - 5)
            {
                secondsToDrag = Math.Abs(TimeSpan.FromSeconds(actualEnd).Seconds - timeToSet.Seconds - 5);
                secondDirection = actualEnd - 5 > timeToSet.TotalSeconds ? 1 : -1;
            }

            (int, int) secondsToSet = (secondsToDrag, secondDirection);

            var picker = driver.FindElementByAccessibilityId("LoopStartPosition");
            picker.Click();
            SetPickerValue(minutesToSet, secondsToSet);

            Thread.Sleep(1000);
        }

        public void SetRightPicker(double seconds)
        {
            double actualStart = GetLoopStartPosition();
            double actualEnd = GetLoopEndPosition();

            var timeToSet = TimeSpan.FromSeconds(seconds);

            var actualMinutesEnd = TimeSpan.FromSeconds(actualEnd).Minutes;
            var minutesToDrag = Math.Abs(timeToSet.Minutes - actualMinutesEnd);
            var minuteDirection = actualMinutesEnd > timeToSet.Minutes ? 1 : -1;
            (int, int) minutesToSet = (minutesToDrag, minuteDirection);
            
            
            var actualSecondsEnd = TimeSpan.FromSeconds(actualEnd).Seconds;
            var secondsToDrag = Math.Abs(timeToSet.Seconds - actualSecondsEnd);
            var secondDirection = actualSecondsEnd > timeToSet.Seconds ? 1 : -1;

            if (actualEnd - minutesToDrag * 60 < actualStart + 5)
            {
                secondsToDrag = Math.Abs(timeToSet.Seconds - TimeSpan.FromSeconds(actualStart).Seconds - 5);
                secondDirection = actualStart + 5 > timeToSet.TotalSeconds ? 1 : -1;
            }

            (int, int) secondsToSet = (secondsToDrag, secondDirection);

            var picker = driver.FindElementByAccessibilityId("LoopEndPosition");
            picker.Click();

            SetPickerValue(minutesToSet, secondsToSet);

            Thread.Sleep(1000);
        }

        private void SetPickerValue((int, int) minutesToSet, (int, int) secondsToSet)
        {
            
            var x = driver.Manage().Window.Size.Width;
            var y = driver.Manage().Window.Size.Height;
            

            if (IsAndroid)
            {
                var yClickMinutes = (y / 2) + (minutesToSet.Item2 * y * 0.06);
                var yClickSeconds = (y / 2) + (secondsToSet.Item2 * y * 0.06);
                

                // Set Minutes
                if (minutesToSet.Item1 != 0)
                {
                    
                    foreach (var i in Enumerable.Range(1, minutesToSet.Item1))
                    {
                        TouchAction actionLeft = new TouchAction(driver);
                        actionLeft.Press(x / 4, y / 2);
                        actionLeft.MoveTo(x / 4, yClickMinutes);
                        actionLeft.Release();
                        actionLeft.Perform();
                    }
                }

                Thread.Sleep(1000);

                // Set seconds
                if (secondsToSet.Item1 != 0)
                {
                    
                    foreach (var i in Enumerable.Range(1, secondsToSet.Item1))
                    {
                        TouchAction actionRight = new TouchAction(driver);
                        actionRight.Press((x / 4) * 3, y / 2);
                        actionRight.MoveTo((x / 4) * 3, yClickSeconds);
                        actionRight.Release();
                        actionRight.Perform();
                    }
                }

                Thread.Sleep(500);

                TouchAction action = new TouchAction(driver);
                action.Press(x / 2, y * 0.7).Release().Perform();
            }

            if (IsIos)
            {
                var yClickMinutes = (y / 2) + (minutesToSet.Item2 * y * 0.045 * -1);
                var yClickSeconds = (y / 2) + (secondsToSet.Item2 * y * 0.045 * -1);

                // Set Minutes
                if (minutesToSet.Item1 != 0)
                {
                    var args = new Dictionary<string, object>();
                    args.Add("x", x / 4);
                    args.Add("y", yClickMinutes);

                    foreach (var i in Enumerable.Range(1, minutesToSet.Item1))
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("mobile: tap", args);
                        Thread.Sleep(300);
                    }
                }

                // Set seconds
                if (secondsToSet.Item1 != 0)
                {
                    var args = new Dictionary<string, object>();
                    args.Add("x", (x / 4) * 3);
                    args.Add("y", yClickSeconds);

                    foreach (var i in Enumerable.Range(1, secondsToSet.Item1))
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("mobile: tap", args);
                        Thread.Sleep(100);
                    }
                }

                Thread.Sleep(500);

                var close = new Dictionary<string, object>();
                close.Add("x", x / 2);
                close.Add("y", y * 0.64);

                ((IJavaScriptExecutor)driver).ExecuteScript("mobile: tap", close);
            }

            
        }

    }

}
