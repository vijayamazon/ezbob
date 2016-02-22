namespace UIAutomationTests.Tests.Shared {
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using System;
    using System.Linq;
    using System.Web.WebPages;
    using log4net;

    static class SharedServiceClass {
        private static readonly ILog log = LogManager.GetLogger(typeof(SharedServiceClass));
        const int MAX_WAIT_TIME = 120;

        //Awaits untill element is visible By locator, returns the element.
        public static IWebElement ElementIsVisible(IWebDriver Driver, By locator, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditionsExtention.ElementIsVisible(locator));
        }

        //Awaits untill element is clickable By locator, returns the element.
        public static IWebElement ElementIsClickable(IWebDriver Driver, By locator, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<IWebElement>(ExpectedConditionsExtention.ElementIsClickable(locator));
        }

        //Awaits an element to be clickable By locator,
        //clicks the element and than asserts By a following locator that the click action was initiated and performed correctly.
        public static bool ClickAssert(IWebDriver Driver, By locator, By assertElement, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.ClickAssert(locator, assertElement));
        }

        //Switches Driver's focus to the windowCount's opened browser instance.
        public static string LastWindowName(IWebDriver Driver, int windowCount, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<string>(ExpectedConditionsExtention.LastWindowName(windowCount));
        }

        //Check weather the current Driver's web address contains a sub-string repeatedly.
        public static bool WebAddressContains(IWebDriver Driver, string partialUrl, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.WebAddressContains(partialUrl));
        }

        //Waits untill Jquery.Active==0, then returns True.
        public static bool WaitForAjaxReady(IWebDriver Driver, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.WaitForAjaxReady(Driver as IJavaScriptExecutor));
        }

        //Waits untill count of elements containing the class name ".blockUI" is 0, then returns True.
        public static bool WaitForBlockUiOff(IWebDriver Driver, int waitTime = MAX_WAIT_TIME) {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.WaitForBlockUiOff());
        }

        public static bool WaitForJSElementReady(IWebDriver Driver, string locator, int waitTime = MAX_WAIT_TIME) {
            if (locator.IsEmpty()) {
                log.Debug("Selector is not supported fror Jquery validation.");
                return true;
            }
            IJavaScriptExecutor jsExe = Driver as IJavaScriptExecutor;
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTime)).Until<bool>(ExpectedConditionsExtention.WaitForJSElementReady(jsExe, locator));
        }


        //Awaits untill element is clickable By locator, returns the element.
        public static void JqueryElementReady(IWebDriver Driver, string selector, int waitTime = MAX_WAIT_TIME) {

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            bool flag = (bool)js.ExecuteScript("return typeof jQuery == 'undefined'");
            if (flag) {
                js.ExecuteScript("var jq = document.createElement('script');jq.src = '//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js';document.getElementsByTagName('head')[0].appendChild(jq);");
            }
            var wait = new WebDriverWait(Driver,new TimeSpan(waitTime));

            // Check if document is ready
            Func<IWebDriver, bool> readyCondition = driver => (bool)js.ExecuteScript("return $('" + selector + "').length > 0");
            wait.Until(readyCondition);
        }

        public static void JqueryClick(IWebDriver Driver, string locator, int waitTime = MAX_WAIT_TIME) {

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            bool flag = (bool)js.ExecuteScript("return typeof jQuery == 'undefined'");
            if (flag) {
                js.ExecuteScript("var jq = document.createElement('script');jq.src = '//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js';document.getElementsByTagName('head')[0].appendChild(jq);");
            }

            // Click element by lockator
            js.ExecuteScript("$('" + locator + "').first().click();");
        }

        public static void JquerySendKeys(IWebDriver Driver, string locator, string keys) {

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            bool flag = (bool)js.ExecuteScript("return typeof jQuery == 'undefined'");
            if (flag) {
                js.ExecuteScript("var jq = document.createElement('script');jq.src = '//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js';document.getElementsByTagName('head')[0].appendChild(jq);");
            }

            // Click element by lockator
            js.ExecuteScript("$('" + locator + "').first().val('" + keys + "');");
        }

        //public static string JQueryLocatorConverter(By locator) {
        //    string[] locComp = locator.ToString().Split(':').Select(s => s.Trim()).ToArray();

        //    switch (locComp[0]) {
        //        case "By.Id":
        //            return "#" + locComp[1];
        //            break;
        //        case "By.CssSelector":
        //            return locComp[1];
        //            break;
        //        default:
        //            return "";
        //    }
        //}
    }

    public static class ExpectedConditionsExtention {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExpectedConditionsExtention));

        //public static Func<IWebDriver, bool> WaitForAjaxReady2() {
        //    return (driver) => driver.FindElements(By.CssSelector(".waiting, .tb-loading")).Count == 0;
        //}

        public static Func<IWebDriver, string> LastWindowName(int windowCount) {
            return (Func<IWebDriver, string>)(driver => {
                try {
                    if (driver.WindowHandles.Count() == windowCount) {
                        log.Debug("WindowCount matches the value of: " + windowCount.ToString());
                        return driver.WindowHandles.Last().ToString();
                    }
                    log.Debug("WindowCount is: " + driver.WindowHandles.Count() + " while expected to be: " + windowCount.ToString());
                    return null;
                } catch (Exception ex) {
                    log.Debug("WindowCount is: " + driver.WindowHandles.Count() + " while expected to be: " + windowCount.ToString());
                    return null;
                }
            });
        }

        public static Func<IWebDriver, bool> WebAddressContains(string partialUrl) {
            return (Func<IWebDriver, bool>)(driver => {
                try {
                    if (driver.Url.Contains(partialUrl)) {
                        log.Debug("Web address contains the string: " + partialUrl);
                        return true;
                    }
                    log.Debug("Web address dosent contains the string: " + partialUrl);
                    return false;
                } catch (Exception ex) {
                    log.Debug("Web address dosent contains the string: " + partialUrl);
                    return false;
                }
            });
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

        public static Func<IWebDriver, bool> WaitForJSElementReady(IJavaScriptExecutor jsExe, string locator) {
            return (Func<IWebDriver, bool>)(driver => {
                try {
                    if ((bool)jsExe.ExecuteScript("var el=$('" + locator + "').get(0); if( $.css(el,'visibility') !== 'hidden' && $.css(el,'display') !== 'none' && $.css(el,'opacity') !== 0 && el.offsetWidth !== 0 && el.offsetHeight !== 0 ) {return true;} else {return false;}") == true) {
                        log.Debug("jQuery element ready.");
                        return true;
                    }
                    log.Debug("jQuery element is not ready.");
                    return false;
                } catch (Exception ex) {
                    log.Debug("jQuery element is not ready.");
                    return false;
                }
            });
        }

        public static Func<IWebDriver, bool> WaitForBlockUiOff() {
            return (Func<IWebDriver, bool>)(driver => {
                try {
                    if (driver.FindElements(By.CssSelector(".blockUI")).Count == 0) {
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

       //if (JQlocator.IsEmpty() || (bool)jsExe.ExecuteScript("var el=$('" + JQlocator + "').get(0); if( $.css(el,'visibility') !== 'hidden' && $.css(el,'display') !== 'none' && $.css(el,'opacity') !== 0 && el.offsetWidth !== 0 && el.offsetHeight !== 0 ) {return true;} else {return false;}") == true) {
       // if (!JQlocator.IsEmpty())
       //     log.Debug("JQuery element: '" + locator.ToString() + "' found.");

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
                } catch (Exception ex) {
                    log.Debug("Element: '" + locator.ToString() + "' is not found.");
                    return null;
                }
            });
        }
        public static Func<IWebDriver, IWebElement> ElementIsClickable(By locator) {
            return (Func<IWebDriver, IWebElement>)(driver => {
                try {
                    IWebElement element = driver.FindElement(locator);
                    if (element.Enabled && element.Displayed && (!element.Size.IsEmpty || !element.Location.IsEmpty)) {
                        log.Debug("Element: '" + locator.ToString() + "' is clickable.");
                        return element;
                    }
                    log.Debug("Element: '" + locator.ToString() + "' is not found.");
                    return null;
                } catch (Exception ex) {
                    log.Debug("Element: '" + locator.ToString() + "' is not found.");
                    return null;
                }
            });
        }

        public static Func<IWebDriver, bool> ClickAssert(By locator, By assertLocator) {
            return (Func<IWebDriver, bool>)(driver => {
                try {
                    IWebElement element = driver.FindElement(locator);
                    if (element.Enabled && element.Displayed && (!element.Size.IsEmpty || !element.Location.IsEmpty)) {
                        log.Debug("Element: '" + locator.ToString() + "' is clickable.");
                        element.Click();
                        IWebElement assertElement = SharedServiceClass.ElementIsVisible(driver, assertLocator, 30);
                        if (assertElement.Displayed && (!assertElement.Size.IsEmpty || !assertElement.Location.IsEmpty)) {
                            log.Debug("Element: '" + locator.ToString() + "' has been clicked and asserted: '" + assertLocator.ToString() + "'.");
                            return true;
                        }
                        log.Debug("Assert failed for: '" + assertLocator.ToString() + "'.");
                        return false;
                    }
                    log.Debug("Element: '" + locator.ToString() + "' is not found/clickable.");
                    return false;
                } catch (Exception ex) {
                    log.Debug("Assert failed for: '" + assertLocator.ToString() + "'.");
                    return false;
                }
            });
        }
    }
}
