namespace UIAutomationTests.Core
{
    using System;
    using System.IO;
    using System.Reflection;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.IE;
    using OpenQA.Selenium.Safari;
    using TestStack.Seleno.Configuration;
    using TestStack.Seleno.Configuration.WebServers;

    public static class GetBrowserWebDriver
    {

        public const int DriverTimeOut = 30;
        public static string SiteRootUrl = "www.ezbob.com";
        public static IWebDriver ChromeDriver { get; set; }
        public static IWebDriver FirefoxDriver { get; set; }
        public static IWebDriver IEDriver { get; set; }
        public static IWebDriver InternetExplorerDriver { get; set; }
        public static IWebDriver SafariDriver { get; set; }

        public static IWebDriver GetWebDriverForBrowser(Browser browser)
        {
            IWebDriver driver = null;
            var path = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;
            switch (browser)
            {
                case Browser.Chrome:
                    if (ChromeDriver == null)
                    {
                        ChromeDriver = new ChromeDriver(path + @"\WebDrivers");
                    }
                    driver = ChromeDriver;
                    break;

                case Browser.Firefox:
                    if (FirefoxDriver == null)
                    {
                        FirefoxDriver = new ChromeDriver(path + @"\WebDrivers");
                    }
                driver = FirefoxDriver;
                    break;

                case Browser.IE:
                    if (InternetExplorerDriver == null)
                    {
                        InternetExplorerDriver = new ChromeDriver(path + @"\WebDrivers");
                    }
                driver =  InternetExplorerDriver;
                    break;

                case Browser.Safari:
                    if (SafariDriver == null)
                    {
                        SafariDriver = new ChromeDriver(path + @"\WebDrivers");
                    }
                    driver = SafariDriver;
                    break;
            }

            if (driver != null)
            {
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(DriverTimeOut));
            }

            return driver;
        }


    }
}
