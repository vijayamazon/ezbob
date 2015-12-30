﻿namespace UIAutomationTests.Tests.Shared {
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using System;
    using System.Linq;
    using log4net;
    using UIAutomationTests.Core;

    static class SharedServiceClass {
        private static readonly ILog log = LogManager.GetLogger(typeof(SharedServiceClass));
        const int MAX_WAIT_TIME = 120;

        public static IWebElement ElementIsVisible(IWebDriver Driver, By locator, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditionsExtention.ElementIsVisible(locator));
        }

        public static IWebElement ElementToBeClickable(IWebDriver Driver, By locator, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditionsExtention.ElementToBeClickable(locator));
        }

        public static bool ClickAssert(IWebDriver Driver, By locator, By assertElement, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.ClickAssert(locator, assertElement));
        }

        public static SelectElement SelectIsVisible(IWebDriver Driver, By locator, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<SelectElement>(ExpectedConditionsExtention.SelectIsVisible(locator));
        }

        public static bool TryElementClick(IWebDriver Driver, By locator, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.TryElementClick(locator));
        }

        public static IWebElement ElementToBeClickableAdvanced(IWebDriver Driver, By locator, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditionsExtention.ElementToBeClickableAdvanced(locator));
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

    public static class ExpectedConditionsExtention {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExpectedConditionsExtention));

        public static Func<IWebDriver, IWebElement> ElementToBeClickableAdvanced(By byElement) {
            return (driver) => {
                IWebElement element = driver.FindElement(byElement);
                return (element != null && element.Displayed && element.Enabled) ? element : null;
            };
        }

        public static Func<IWebDriver, bool> TryElementClick(By locator) {
            return (driver) => {
                try {
                    IWebElement element = ExpectedConditions.ElementToBeClickable(locator).Invoke(driver);
                    element.Click();
                    return true;
                } catch { }
                return false;
            };
        }

        public static Func<IWebDriver, string> LastWindowName(int windowCount) {
            return (driver) => (driver.WindowHandles.Count() == windowCount) ? driver.WindowHandles.Last().ToString() : null;
        }

        public static Func<IWebDriver, bool> WebAddressContains(string partialUrl) {
            return (driver) => driver.Url.Contains(partialUrl);
        }

        public static Func<IWebDriver, bool> WaitForAjaxReady(IJavaScriptExecutor jsExe) {
            return (Func<IWebDriver, bool>)(driver => {
                try {
                    if ((bool)jsExe.ExecuteScript("return jQuery.active===0")) {
                        log.Debug("jQuery finished execution.");
                        return true;
                    }
                    log.Debug("jQuery still running.");
                    return false;
                } catch (Exception ex) {
                    log.Debug("jQuery JavaScript produced an error.");
                    return false;
                }
            });
        }

        public static Func<IWebDriver, bool> WaitForAjaxReady2() {
            return (driver) => driver.FindElements(By.CssSelector(".waiting, .tb-loading")).Count == 0;
        }

        public static Func<IWebDriver, bool> WaitForBlockUiOff() {
            //return (driver) => (driver.FindElements(By.CssSelector(".blockUI")).Count == 0);//&&(driver.FindElements(By.CssSelector(".automation-popup")).Count==0);
            return (Func<IWebDriver, bool>)(driver => {
                try {
                    if (driver.FindElements(By.CssSelector(".blockUI")).Count==0) {
                        log.Debug("blockUI finished execution.");
                        return true;
                    }
                    log.Debug("blockUI still running.");
                    return false;
                } catch (Exception ex) {
                    log.Debug("blockUI produced an error.");
                    return false;
                }
            });
        }

        //public static Func<IWebDriver, bool> WaitForAjaxReady4(IJavaScriptExecutor jsExe) {
        //    return (driver) => (bool)jsExe.ExecuteScript("$(window).on('load',function(){return true;}");
        //}

        public static Func<IWebDriver, IWebElement> ElementToBeClickable(By locator) {
            return (Func<IWebDriver, IWebElement>)(driver => {
                try {
                    IWebElement element = driver.FindElement(locator);
                    if (element.Enabled && element.Displayed && (!element.Size.IsEmpty || !element.Location.IsEmpty)) {
                        log.Debug("Element: '" + locator.ToString() + "' is clickable.");
                        return element;
                    }
                    log.Debug("Element: '" + locator.ToString() + "' is not found.");
                    return null;
                } catch (Exception ex) {//StaleElementReferenceException ex , NoSuchElementException ex
                    log.Debug("Element: '" + locator.ToString() + "' is not found.");
                    return null;
                }
            });
        }

        public static Func<IWebDriver, bool> ClickAssert(By locator,By assertLocator) {
            return (Func<IWebDriver, bool>)(driver => {
                try {
                    IWebElement element = driver.FindElement(locator);
                    if (element.Enabled && element.Displayed && (!element.Size.IsEmpty || !element.Location.IsEmpty)) {
                        log.Debug("Element: '" + locator.ToString() + "' is clickable.");
                        element.Click();
                        IWebElement assertElement = SharedServiceClass.ElementIsVisible(driver,assertLocator,30);
                        if (assertElement.Displayed && (!assertElement.Size.IsEmpty || !assertElement.Location.IsEmpty)) {
                            log.Debug("Element: '" + locator.ToString() + "' has been clicked and asserted: '" + assertLocator.ToString() + "'.");
                            return true;
                        }
                        log.Debug("Assert failed for: '" + assertLocator.ToString() + "'.");
                        return false;
                    }
                    log.Debug("Element: '" + locator.ToString() + "' is not found/clickable.");
                    return false;
                } catch (Exception ex) {//StaleElementReferenceException ex , NoSuchElementException ex
                    log.Debug("Assert failed for: '" + assertLocator.ToString() + "'.");
                    return false;
                }
            });
        }

        public static Func<IWebDriver, IWebElement> ElementIsVisible(By locator) {
            return (Func<IWebDriver, IWebElement>)(driver => {
                try {
                    IWebElement element = driver.FindElement(locator);
                    if (element.Displayed && (!element.Size.IsEmpty || !element.Location.IsEmpty)) {
                        log.Debug("Element: '" + locator.ToString() + "' is visible.");
                        return element;
                    }
                    log.Debug("Element: '" + locator.ToString() + "' is not found.");
                    return null;
                } catch (Exception ex) {//StaleElementReferenceException ex , NoSuchElementException ex
                    log.Debug("Element: '" + locator.ToString() + "' is not found.");
                    return null;
                }
            });
        }

        public static Func<IWebDriver, SelectElement> SelectIsVisible(By locator) {
            return (Func<IWebDriver, SelectElement>)(driver => {
                try {
                    IWebElement element = driver.FindElement(locator);
                    if (element.Displayed && (!element.Size.IsEmpty || !element.Location.IsEmpty)) {
                        log.Debug("SelectElement: '" + locator.ToString() + "' is visible.");
                        return new SelectElement(element);
                    }
                    log.Debug("SelectElement: '" + locator.ToString() + "' is not found.");
                    return null;
                } catch (Exception ex) {//StaleElementReferenceException ex , NoSuchElementException ex
                    log.Debug("SelectElement: '" + locator.ToString() + "' is not found.");
                    return null;
                }
            });
        }
    }
}
