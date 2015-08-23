namespace UIAutomationTests.Core
{
    using System;
    using System.IO;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;

    public static class GetBrowserWebDriver
    {

        public const int DriverTimeOut = 30;
        public static IWebDriver ChromeDriver { get; set; }
        public static IWebDriver FirefoxDriver { get; set; }
        public static IWebDriver IEDriver { get; set; }
        public static IWebDriver InternetExplorerDriver { get; set; }
        public static IWebDriver SafariDriver { get; set; }

        public static IWebDriver GetWebDriverForBrowser(AutomationEnums browser)
        {
            IWebDriver driver = null;
            //var path = Directory.GetParent(Directory.GetParent(System.Environment.CurrentDirectory).FullName).FullName;
            switch (browser)
            {
                case AutomationEnums.Chrome:
                    if (ChromeDriver == null)
                    {
                        ChromeDriver = new ChromeDriver(@"E:\WebDrivers");
                    }
                    driver = ChromeDriver;
                    break;

                case AutomationEnums.Firefox:
                    if (FirefoxDriver == null)
                    {
                        FirefoxDriver = new FirefoxDriver();
                    }
                driver = FirefoxDriver;
                    break;

                case AutomationEnums.IE:
                    if (InternetExplorerDriver == null)
                    {
                        InternetExplorerDriver = new ChromeDriver(@"E:\WebDrivers");
                    }
                driver =  InternetExplorerDriver;
                    break;

                case AutomationEnums.Safari:
                    if (SafariDriver == null)
                    {
                        SafariDriver = new ChromeDriver(@"E:\WebDrivers");
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
