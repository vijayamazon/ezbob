namespace UIAutomationTests.Core {
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using log4net;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Support.UI;
    using UIAutomationTests.Tests.Shared;
    public class ActionBot : WebTestBase {
        private static readonly ILog log = LogManager.GetLogger(typeof(ActionBot));
        public ActionBot(IWebDriver Driver) {
            this.Driver = Driver;
        }

        //Sleeps for predefined time period.
        public void Sleep(int millisecondsTimeout) {
            Thread.Sleep(millisecondsTimeout);
            log.Info("Sleep for: " + millisecondsTimeout/1000 + "seconds");
        }

        //Awaits an element to be clickable By locator, then clicks the element.
        public void Click(By locator, string description, int waitTime = 120) {
            SharedServiceClass.ElementIsClickable(Driver, locator, waitTime).Click();
            log.Info(description + " - '" + locator.ToString() + "' - Click.");
        }

        //Awaits an element to be clickable By locator,
        //clicks the element and than asserts By a following locator that the click action was initiated and performed correctly.
        public void ClickAssert(By locator, By assertLocator, string description, int waitTime = 120) {
            log.Debug("ClickAssert start.");
            SharedServiceClass.ClickAssert(Driver, locator, assertLocator, waitTime);
            log.Debug("ClickAssert finished.");
            log.Info(description + " - ClickAssert performed. '" + locator.ToString() + "' has been clicked. and '" + assertLocator.ToString() + "' asserted.");
        }

        //Awaits an element to be visible By locator, then sends keys string the element.
        public void SendKeys(By locator, string keys, string description, int waitTime = 120, bool isClear = true) {
            IWebElement element = SharedServiceClass.ElementIsVisible(Driver, locator, waitTime);
            if (isClear)
                element.Clear();
            element.SendKeys(keys);
            log.Info(description + " - '" + locator.ToString() + "' - SendKeys: '" + keys + "'.");
        }

        //Awaits a select element to be visible By locator, then selects by index.
        public void SelectByIndex(By locator, int index, string description, int waitTime = 120) {
            new SelectElement(SharedServiceClass.ElementIsVisible(Driver, locator, waitTime)).SelectByIndex(index);
            log.Info(description + " - '" + locator.ToString() + "' - SelectByIndex: '" + index + "'.");
        }

        //Awaits a select element to be visible By locator, then selects by value.
        public void SelectByValue(By locator, string value, string description, int waitTime = 120) {
            new SelectElement(SharedServiceClass.ElementIsVisible(Driver, locator, waitTime)).SelectByValue(value);
            log.Info(description + " - '" + locator.ToString() + "' - SelectByValue: '" + value + "'.");
        }

        //Awaits a select element to be visible By locator, then selects by text.
        public void SelectByText(By locator, string text, string description, int waitTime = 120) {
            new SelectElement(SharedServiceClass.ElementIsVisible(Driver, locator, waitTime)).SelectByText(text);
            log.Info(description + " - '" + locator.ToString() + "' - SelectByText: '" + text + "'.");
        }

        //Switches Driver's focus to the windowIndexes's opened browser instance.
        public void SwitchToWindow(int windowIndex, string description) {
            Driver.SwitchTo().Window(SharedServiceClass.LastWindowName(Driver, windowIndex));
            log.Info("Moving focust to window: " + description + ".");
        }

        //Writes the following description to the log.
        public void WriteToLog(string description) {
            log.Info(description);
        }
    }
}
