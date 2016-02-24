﻿namespace UIAutomationTests.Core {
    using System;
    using System.IO;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.IE;
    using TestRailModels.Automation;

    public class WebDriverManager {


        public const int DriverTimeOut = 30;
        public IWebDriver ChromeDriver { get; set; }
        public IWebDriver FirefoxDriver { get; set; }
        public IWebDriver IEDriver { get; set; }
        public IWebDriver InternetExplorerDriver { get; set; }
        public IWebDriver SafariDriver { get; set; }

        private static WebDriverManager instance;
        private WebDriverManager() { }
                
        public static WebDriverManager Instance {
            get { return instance ?? (instance = new WebDriverManager()); }
        }


        public IWebDriver GetWebDriverForBrowser(AutomationModels.Browser browser) {
            IWebDriver driver = null;
            string path = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;
            switch (browser) {
                case AutomationModels.Browser.Chrome:
                    if (ChromeDriver == null) {
                        ChromeDriver = new ChromeDriver(path + @"\WebDrivers");
                    }
                    driver = ChromeDriver;
                    break;

                case AutomationModels.Browser.Firefox:
                    if (FirefoxDriver == null) {
                        FirefoxProfile profile = new FirefoxProfile();
                        profile.SetPreference("webdriver.firefox.bin", "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe");
                        FirefoxDriver = new FirefoxDriver(profile);
                    }
                    driver = FirefoxDriver;
                    break;

                case AutomationModels.Browser.IE:
                    if (InternetExplorerDriver == null) {
                        InternetExplorerDriver = new InternetExplorerDriver(path + @"\WebDrivers");
                    }
                    driver = InternetExplorerDriver;
                    break;

                case AutomationModels.Browser.Safari:
                    if (SafariDriver == null) {
                        SafariDriver = new ChromeDriver(path + @"E:\WebDrivers");
                    }
                    driver = SafariDriver;
                    break;
            }

            if (driver != null) {
                //  driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(DriverTimeOut));
                driver.Manage().Window.Maximize();
            }

            return driver;
        }


    }
}
