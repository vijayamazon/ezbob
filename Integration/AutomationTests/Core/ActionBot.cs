namespace UIAutomationTests.Core {
    using OpenQA.Selenium;
    using UIAutomationTests.Tests.Shared;
    public class ActionBot : WebTestBase {
        public ActionBot(IWebDriver Driver) {
            this.Driver = Driver;
        }

        public bool Click(By locator, string description, int waitTime = 120) {
            //IWebElement a  = SharedServiceClass.WaitForElemetVisible();
            //Driver.FindElement(locator).Click();
            SharedServiceClass.ElementToBeClickable(Driver, locator, waitTime).Click();
            return true;
        }
        public bool SendKeys(By locator, string keys, string description, int waitTime = 120, bool isClear = true) {
            IWebElement element = SharedServiceClass.ElementIsVisible(Driver, locator, waitTime);
            if (isClear)
                element.Clear();
            element.SendKeys(keys);
            return true;
        }

        public bool SelectByIndex(By locator,int index, string description, int waitTime = 120) {
            //IWebElement a  = SharedServiceClass.WaitForElemetVisible();
            //Driver.FindElement(locator).Click();
            SharedServiceClass.SelectIsVisible(Driver, locator, waitTime).SelectByIndex(index);
            return true;
        }

        public bool SelectByValue(By locator, string value, string description, int waitTime = 120) {
            //IWebElement a  = SharedServiceClass.WaitForElemetVisible();
            //Driver.FindElement(locator).Click();
            SharedServiceClass.SelectIsVisible(Driver, locator, waitTime).SelectByValue(value);
            return true;
        }

        public bool SelectByText(By locator, string text, string description, int waitTime = 120) {
            //IWebElement a  = SharedServiceClass.WaitForElemetVisible();
            //Driver.FindElement(locator).Click();
            SharedServiceClass.SelectIsVisible(Driver, locator, waitTime).SelectByText(text);
            return true;
        }

        public bool WriteToLog(string description) {
            return true;
        }

        public bool Type(By locator, string text, string description) {
            Driver.FindElement(locator).Clear();
            Driver.FindElement(locator).SendKeys(text);
            return true;
        }
    }
}
