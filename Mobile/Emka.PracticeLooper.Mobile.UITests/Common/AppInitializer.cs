// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Service;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Emka.PracticeLooper.Mobile.UITests
{
    public abstract class AppInitializer<T, W>
        where T : AppiumDriver<W>
        where W : IWebElement
    {
        protected abstract T GetDriver();
        protected abstract void InitAppiumOptions(AppiumOptions options);
        protected T driver;
        protected readonly AppiumOptions options;
        protected readonly Uri driverUri;
        private AppiumLocalService service;

        protected AppInitializer()
        {
            driverUri = new Uri("http://localhost:4723/wd/hub");
            options = new AppiumOptions();
        }

        [SetUp]
        public void SetUp() {
            /*service = new AppiumServiceBuilder()
                .WithAppiumJS(new FileInfo("/usr/local/lib/node_modules/appium/build/lib/main.js"))
                .WithIPAddress("127.0.0.1")
                .UsingPort(4723)
                .WithLogFile(new FileInfo("/Users/simonsymhoven/log.txt"))
                .Build();

            Environment.SetEnvironmentVariable("ANDROID_HOME", "/Users/simonsymhoven/Library/Android/sdk/");
            Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", "/Users/simonsymhoven/Library/Android/sdk/");
            Environment.SetEnvironmentVariable("JAVA_HOME", "/Library/Java/JavaVirtualMachines/adoptopenjdk-8.jdk/Contents/Home/");
            service.Start();
             */


            InitAppiumOptions(options);
            driver = GetDriver();
        }

        [TearDown]
        public void TearDown()
        {
            // service.Dispose();
            driver.Quit();
            Thread.Sleep(5000);
        }
       
        public bool IsAndroid => driver.Capabilities
            .GetCapability(MobileCapabilityType.PlatformName).Equals("Android");

        public bool IsIos => driver.Capabilities
            .GetCapability(MobileCapabilityType.PlatformName).Equals("iOS");
    }
}
