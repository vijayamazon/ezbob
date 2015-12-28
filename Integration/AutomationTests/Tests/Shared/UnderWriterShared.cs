namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using UIAutomationTests.Core;

    class UnderWriterShared : WebTestBase {

        public UnderWriterShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this.Driver = Driver;
            this.EnvironmentConfig = EnvironmentConfig;
            this.BrandConfig = BrandConfig;
        }

        public void LogIn(string user, string pass) {
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("Brand_url"), "/Account/AdminLogOn");

            Driver.Navigate().GoToUrl(url);

            IWebElement userName = Driver.FindElement(By.Id("UserName"));
            userName.SendKeys(user);

            IWebElement password = Driver.FindElement(By.Id("Password"));
            password.SendKeys(pass);

            IWebElement logIn = Driver.FindElement(By.Id("loginSubmitBtn"));
            logIn.Click();
        }//test+client_635862269583123148@ezbob.com

        public void FindCustomer(string identifier) {

            SharedServiceClass.WaitForBlockUiOff(Driver);
            Actions sendKeyAction = new Actions(Driver);
            sendKeyAction.KeyDown(Keys.Control).SendKeys("g").Build().Perform();

            //IWebElement asd = Driver.FindElement(By.Id("go-to-template"));
            //IWebElement asdasd = asd.FindElement(By.CssSelector("input.goto-customerId.form-control.ui-autocomplete-input"));
            //asdasd.SendKeys(identifier);

            IWebElement idField = SharedServiceClass.ElementIsVisible(Driver, By.XPath("//div[@id='go-to-template']/input[@class='goto-customerId form-control ui-autocomplete-input']"));
            sendKeyAction.Click(idField).Build().Perform();
            sendKeyAction.SendKeys(idField, identifier).Build().Perform();
            sendKeyAction.SendKeys(idField,Keys.Down).Build().Perform();
            
            idField.Clear();
            idField.SendKeys(identifier);
            idField.SendKeys(Keys.Down);
            idField.SendKeys(Keys.Enter);

            IWebElement okButton = Driver.FindElement(By.CssSelector("div.ui-dialog-buttonpane.ui-widget-content.ui-helper-clearfix > div.ui-dialog-buttonset > button.ok-button.btn.btn-primary"));
            okButton.Click();
        }
        
    }
}
