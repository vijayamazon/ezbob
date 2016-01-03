namespace UIAutomationTests.Core {
    using log4net;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using UIAutomationTests.Tests.Shared;
    public class ActionBot : WebTestBase {
        private static readonly ILog log = LogManager.GetLogger(typeof(ActionBot));
        public ActionBot(IWebDriver Driver) {
            this.Driver = Driver;
        }

        public void Click(By locator, string description, int waitTime = 120) {
            SharedServiceClass.ElementToBeClickable(Driver, locator, waitTime).Click();
            log.Info(description + " - '" + locator.ToString() + "' - Click.");
        }

        public void ClickAssert(By locator, By assertLocator, string description, int waitTime = 120) {
            log.Info("ClickAssert start.");
            SharedServiceClass.ClickAssert(Driver, locator, assertLocator, waitTime);
            log.Info("ClickAssert finished.");
            log.Info(description + " - ClickAssert performed. '" + locator.ToString() + "' has been clicked. and '" + assertLocator.ToString()+"' asserted.");
        }

        public void SendKeys(By locator, string keys, string description, int waitTime = 120, bool isClear = true) {
            IWebElement element = SharedServiceClass.ElementIsVisible(Driver, locator, waitTime);
            if (isClear)
                element.Clear();
            element.SendKeys(keys);
            log.Info(description + " - '" + locator.ToString() + "' - SendKeys: '" + keys + "'.");
        }

        public void SelectByIndex(By locator, int index, string description, int waitTime = 120) {
            SharedServiceClass.SelectIsVisible(Driver, locator, waitTime).SelectByIndex(index);
            log.Info(description + " - '" + locator.ToString() + "' - SelectByIndex: '" + index + "'.");
        }

        public void SelectByValue(By locator, string value, string description, int waitTime = 120) {
            SharedServiceClass.SelectIsVisible(Driver, locator, waitTime).SelectByValue(value);
            log.Info(description + " - '" + locator.ToString() + "' - SelectByValue: '" + value + "'.");
        }

        public void SelectByText(By locator, string text, string description, int waitTime = 120) {
            SharedServiceClass.SelectIsVisible(Driver, locator, waitTime).SelectByText(text);
            log.Info(description + " - '" + locator.ToString() + "' - SelectByText: '" + text + "'.");
        }

        public void SwitchToWindow(int lastWindowIndex, string description) {
            Driver.SwitchTo().Window(SharedServiceClass.LastWindowName(Driver, lastWindowIndex));
            log.Info("Moving focust to window: " + description + ".");
        }

        public void WriteToLog(string description) {
            log.Info(description);
        }
    }
}
