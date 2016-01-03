namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using UIAutomationTests.Core;

    class CustomerShared : WebTestBase {

        public CustomerShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this.Driver = Driver;
            this.EnvironmentConfig = EnvironmentConfig;
            this.BrandConfig = BrandConfig;
            this.actionBot = new ActionBot(Driver);
        }

        public void CustomerLogIn(string logHeader, bool isFirstTime, string brokerMail) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            SharedServiceClass.WaitForAjaxReady(Driver);
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("CustomerLogIn"));
            Driver.Navigate().GoToUrl(url);
            actionBot.WriteToLog(logHeader + " - " + "Nevigate to url: " + url);

            //IWebElement userName = SharedServiceClass.ElementIsVisible(Driver, By.Id("UserName"));
            //userName.SendKeys(brokerMail);
            actionBot.SendKeys(By.Id("UserName"), brokerMail, logHeader);

            //IWebElement password = Driver.FindElement(By.Id("Password"));
            //password.SendKeys("123123");
            actionBot.SendKeys(By.Id("Password"), "123123", logHeader);

            //IWebElement loginBrokerButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("loginSubmit"));
            //loginBrokerButton.Click();
            actionBot.Click(By.Id("loginSubmit"), logHeader);

            if (isFirstTime) {
                //IWebElement continueButton = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("div.automation-popup > div.automation-popup-content > div.alignright > button.button"));//By.CssSelector("div.automation-popup > div.automation-popup-content > div.alignright > button.button.btn-green.pull-right.automation-button.ev-btn-org")
                //continueButton.Click();
                actionBot.Click(By.CssSelector("div.automation-popup > div.automation-popup-content > div.alignright > button.button"), logHeader);
            }
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //Precondition: be loggedin to customer.
        public void CustomerTakeLoan(string logHeader,
            string fName,
            string lName,
            string accountNum,
            string sort1,
            string sort2,
            string sort3,
            char accType,
            string cardHolderName,
            string cardType,
            string cardNumber,
            string expDate,
            string securityCode,
            double? loanFraction = null) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            SharedServiceClass.WaitForAjaxReady(Driver);
            //IWebElement chooseAmountBtn = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("button.button.btn-green.get-cash.ev-btn-org"));
            //chooseAmountBtn.Click();
            //SharedServiceClass.TryElementClick(Driver, By.CssSelector("button.button.btn-green.get-cash.ev-btn-org"));
            //actionBot.WriteToLog(logHeader + " - " + By.CssSelector("button.button.btn-green.get-cash.ev-btn-org").ToString() + " - TryElementClick.");

            actionBot.ClickAssert(By.CssSelector("button.button.btn-green.get-cash.ev-btn-org"), By.XPath("//label[@for='preAgreementTermsRead']"), logHeader);


            //IWebElement preAgreementTermsRead = Driver.FindElement(By.XPath("//label[@for='preAgreementTermsRead']"));
            //preAgreementTermsRead.Click();
            actionBot.Click(By.XPath("//label[@for='preAgreementTermsRead']"), logHeader);

            if (loanFraction != null) {
                IWebElement rangeSelector = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("div.ui-slider-range.ui-widget-header.ui-slider-range-min"));//Driver.FindElement(By.CssSelector("div.ui-slider-range.ui-widget-header.ui-slider-range-min")));
                int clickCoordinate = (int)(rangeSelector.Size.Width * loanFraction);
                Actions moveAction = new Actions(Driver);
                moveAction.MoveToElement(rangeSelector, clickCoordinate, 0).Click().Build().Perform();
                actionBot.WriteToLog(logHeader + " - " + By.CssSelector("div.ui-slider-range.ui-widget-header.ui-slider-range-min").ToString() + " - Click was performed at " + loanFraction.ToString() + "fraction of the element.");
            }

            //IWebElement agreementTermsRead = Driver.FindElement(By.XPath("//label[@for='agreementTermsRead']"));
            //agreementTermsRead.Click();
            actionBot.Click(By.XPath("//label[@for='agreementTermsRead']"), logHeader);

            //IWebElement notInBankruptcy = Driver.FindElement(By.XPath("//label[@for='notInBankruptcy']"));
            //notInBankruptcy.Click();
            actionBot.Click(By.XPath("//label[@for='notInBankruptcy']"), logHeader);

            SharedServiceClass.WaitForBlockUiOff(Driver);

            //IWebElement confirmButton = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("button.ok-button.button.btn-green.ev-btn-org"));
            //confirmButton.Click();
            actionBot.Click(By.CssSelector("button.ok-button.button.btn-green.ev-btn-org"), logHeader);

            //IWebElement signedName = Driver.FindElement(By.Id("signedName"));
            //signedName.SendKeys(fName + " " + lName);
            actionBot.SendKeys(By.Id("signedName"), fName + " " + lName, logHeader);

            //IWebElement nextButton = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("form.LoanLegal a.btn-continue.button.btn-green.ev-btn-org.submit"));
            //nextButton.Click();
            actionBot.Click(By.CssSelector("form.LoanLegal a.btn-continue.button.btn-green.ev-btn-org.submit"), logHeader);

            //End of Step 1 - Choosing loas terms
            //IWebElement accountNumber = SharedServiceClass.ElementIsVisible(Driver, By.Id("AccountNumber"));
            //accountNumber.SendKeys(accountNum);
            actionBot.SendKeys(By.Id("AccountNumber"), accountNum, logHeader);

            //IWebElement sortCode1 = Driver.FindElement(By.Id("SortCode1"));
            //sortCode1.SendKeys(sort1);
            actionBot.SendKeys(By.Id("SortCode1"), sort1, logHeader);

            //IWebElement sortCode2 = Driver.FindElement(By.Id("SortCode2"));
            //sortCode2.SendKeys(sort2);
            actionBot.SendKeys(By.Id("SortCode2"), sort2, logHeader);

            //IWebElement sortCode3 = Driver.FindElement(By.Id("SortCode3"));
            //sortCode3.SendKeys(sort3);
            actionBot.SendKeys(By.Id("SortCode3"), sort3, logHeader);

            By accTypeRadio;
            switch (char.ToUpper(accType)) {
                case 'B':
                    accTypeRadio = By.XPath("//label[@for='baBusiness']");//Driver.FindElement(By.XPath("//label[@for='baBusiness']")));
                    break;
                default:
                    accTypeRadio = By.XPath("//label[@for='baPersonal']");//Driver.FindElement(By.XPath("//label[@for='baPersonal']"));
                    break;
            }
            actionBot.Click(accTypeRadio, logHeader);

            //IWebElement continueButton = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"));
            //continueButton.Click();
            actionBot.Click(By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"), logHeader);
            //Thread.Sleep(2000);
            SharedServiceClass.WaitForBlockUiOff(Driver);
            //continueButton.Click();
            //actionBot.Click(By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"), "");
            //End of step 2 - Entering bank details
            //SharedServiceClass.TryElementClick(Driver, By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"));
            //actionBot.WriteToLog(logHeader + " - " + By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org").ToString() + " - TryElementClick.");
            actionBot.Click(By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"), logHeader);

            //IWebElement customer = SharedServiceClass.ElementIsVisible(Driver, By.Id("customer"));
            //customer.SendKeys(cardHolderName);
            actionBot.SendKeys(By.Id("customer"), cardHolderName, logHeader);

            //SelectElement cardTypeSelect = new SelectElement(Driver.FindElement(By.CssSelector("select.selectheight.form_field")));
            //cardTypeSelect.SelectByValue(cardType);
            actionBot.SelectByValue(By.CssSelector("select.selectheight.form_field"), cardType, logHeader);

            //IWebElement cardNo = Driver.FindElement(By.Id("card_no"));
            //cardNo.SendKeys(cardNumber);
            actionBot.SendKeys(By.Id("card_no"), cardNumber, logHeader);

            //IWebElement expiry = Driver.FindElement(By.Id("expiry"));
            //expiry.SendKeys(expDate);
            actionBot.SendKeys(By.Id("expiry"), expDate, logHeader);

            //IWebElement cv2 = Driver.FindElement(By.Id("cv2"));
            //cv2.SendKeys(securityCode);
            actionBot.SendKeys(By.Id("cv2"), securityCode, logHeader);

            //IWebElement confirmStep3Button = SharedServiceClass.ElementToBeClickable(Driver, By.Id("paypoint-submit"));
            //confirmStep3Button.Click();
            actionBot.Click(By.Id("paypoint-submit"), logHeader);

            //IWebElement myAccountButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("pacnet-status-back-to-profile"));
            //myAccountButton.Click();
            actionBot.Click(By.Id("pacnet-status-back-to-profile"), logHeader);
            //End of step 3 - Get cash
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        public void CustomerLogOff(string logHeader) {
            SharedServiceClass.WaitForBlockUiOff(Driver);
            actionBot.Click(By.CssSelector("li.login > a"), logHeader);
        }
    }
}
