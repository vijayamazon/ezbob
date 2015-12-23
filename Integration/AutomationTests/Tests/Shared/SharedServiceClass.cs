namespace UIAutomationTests.Tests.Shared {
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using System;
    using System.Linq;
    using Google.Apis.Discovery;

    static class SharedServiceClass {

        const int MAX_WAIT_TIME = 120;

        public static IWebElement ElementIsVisible(IWebDriver Driver, By byElement, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditions.ElementIsVisible(byElement));
        }

        public static SelectElement SelectIsVisible(IWebDriver Driver, By byElement, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<SelectElement>(ExpectedConditionsExtention.SelectIsVisible(byElement));
        }

        public static IWebElement ElementToBeClickable(IWebDriver Driver, By byElement, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditions.ElementToBeClickable(byElement));
        }

        public static bool TryElementClick(IWebDriver Driver, By byElement, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.TryElementClick(byElement));
        }

        public static IWebElement ElementToBeClickableAdvanced(IWebDriver Driver, By byElement, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditionsExtention.ElementToBeClickableAdvanced(byElement));
        }

        public static string LastWindowName(IWebDriver Driver, int windowCount, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<string>(ExpectedConditionsExtention.LastWindowName(windowCount));
        }

        public static bool WebAddressContains(IWebDriver Driver, string partialUrl, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.WebAddressContains(partialUrl));
        }

        public static bool WaitForAjaxReady(IWebDriver Driver, int waitTime = MAX_WAIT_TIME) {
            IJavaScriptExecutor jsExe = Driver as IJavaScriptExecutor;
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.WaitForAjaxReady(jsExe));
        }

        public static bool WaitForBlockUiOff(IWebDriver Driver, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.WaitForBlockUiOff());
        }
    }

    public class ExpectedConditionsExtention {

        public static Func<IWebDriver, IWebElement> ElementToBeClickableAdvanced(By byElement) {
            return (driver) => {
                IWebElement element = driver.FindElement(byElement);
                return (element != null && element.Displayed && element.Enabled) ? element : null;
            };
        }

        public static Func<IWebDriver, bool> TryElementClick(By byElement) {
            return (driver) => {
                try {
                    IWebElement element = ExpectedConditions.ElementToBeClickable(byElement).Invoke(driver);
                    element.Click();
                    return true;
                } catch{}
                return false;
            };
        }

        public static Func<IWebDriver, SelectElement> SelectIsVisible(By byElement) {
            return (driver) => new SelectElement(driver.FindElement(byElement));
        }

        public static Func<IWebDriver, string> LastWindowName(int windowCount) {
            return (driver) => (driver.WindowHandles.Count() == windowCount) ? driver.WindowHandles.Last().ToString() : null;
        }

        public static Func<IWebDriver, bool> WebAddressContains(string partialUrl) {
            return (driver) => driver.Url.Contains(partialUrl);
        }

        public static Func<IWebDriver, bool> WaitForAjaxReady(IJavaScriptExecutor jsExe) {
            return (driver) => (bool)jsExe.ExecuteScript("return jQuery.active===0");
        }

        public static Func<IWebDriver, bool> WaitForAjaxReady2() {
            return (driver) => driver.FindElements(By.CssSelector(".waiting, .tb-loading")).Count == 0;
        }

        public static Func<IWebDriver, bool> WaitForBlockUiOff() {
            return (driver) => (driver.FindElements(By.CssSelector(".blockUI")).Count == 0);//&&(driver.FindElements(By.CssSelector(".automation-popup")).Count==0);
        }

        //public static Func<IWebDriver, bool> WaitForAjaxReady4(IJavaScriptExecutor jsExe) {
        //    return (driver) => (bool)jsExe.ExecuteScript("$(window).on('load',function(){return true;}");
        //}
    }
}
