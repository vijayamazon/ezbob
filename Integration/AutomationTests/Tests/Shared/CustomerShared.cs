namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using System.Threading;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using UIAutomationTests.Core;

    class CustomerShared : WebTestBase {

        private readonly IWebDriver _Driver;
        private readonly ResourceManager _EnvironmentConfig;
        private readonly ResourceManager _BrandConfig;

        public CustomerShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this._Driver = Driver;
            this._EnvironmentConfig = EnvironmentConfig;
            this._BrandConfig = BrandConfig;
        }

        public void CustomerLogIn(bool isFirstTime, string brokerMail) {
            SharedServiceClass.WaitForBlockUiOff(this._Driver);
            string url = String.Concat(this._EnvironmentConfig.GetString("ENV_address"), this._BrandConfig.GetString("CustomerLogIn"));
            this._Driver.Navigate().GoToUrl(url);

            IWebElement userName = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("UserName"));
            userName.SendKeys(brokerMail);

            IWebElement password = this._Driver.FindElement(By.Id("Password"));
            password.SendKeys("123123");

            IWebElement loginBrokerButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("loginSubmit"));
            loginBrokerButton.Click();

            if (isFirstTime) {
                IWebElement continueButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("div.automation-popup > div.automation-popup-content > div.alignright > button.button"));//By.CssSelector("div.automation-popup > div.automation-popup-content > div.alignright > button.button.btn-green.pull-right.automation-button.ev-btn-org")
                continueButton.Click();
            }
        }

        //Precondition: be loggedin to customer.
        public void CustomerTakeLoan(string fName,
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
            string securityCode) {

            SharedServiceClass.WaitForBlockUiOff(this._Driver);
            IWebElement chooseAmountBtn = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("button.button.btn-green.get-cash.ev-btn-org"));
            chooseAmountBtn.Click();

            IWebElement preAgreementTermsRead = this._Driver.FindElement(By.XPath("//label[@for='preAgreementTermsRead']"));
            preAgreementTermsRead.Click();

            IWebElement agreementTermsRead = this._Driver.FindElement(By.XPath("//label[@for='agreementTermsRead']"));
            agreementTermsRead.Click();

            IWebElement notInBankruptcy = this._Driver.FindElement(By.XPath("//label[@for='notInBankruptcy']"));
            notInBankruptcy.Click();

            SharedServiceClass.WaitForBlockUiOff(this._Driver);

            IWebElement confirmButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("button.ok-button.button.btn-green.ev-btn-org"));
            confirmButton.Click();

            IWebElement signedName = this._Driver.FindElement(By.Id("signedName"));
            signedName.SendKeys(fName + " " + lName);

            IWebElement nextButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("form.LoanLegal a.btn-continue.button.btn-green.ev-btn-org.submit"));
            nextButton.Click();

            //End of Step 1 - Choosing loas terms
            IWebElement accountNumber = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("AccountNumber"));
            accountNumber.SendKeys(accountNum);

            IWebElement sortCode1 = this._Driver.FindElement(By.Id("SortCode1"));
            sortCode1.SendKeys(sort1);

            IWebElement sortCode2 = this._Driver.FindElement(By.Id("SortCode2"));
            sortCode2.SendKeys(sort2);

            IWebElement sortCode3 = this._Driver.FindElement(By.Id("SortCode3"));
            sortCode3.SendKeys(sort3);

            IWebElement accTypeRadio;
            switch (char.ToUpper(accType)) {
                case 'B':
                    accTypeRadio = this._Driver.FindElement(By.XPath("//label[@for='baBusiness']"));
                    break;
                default:
                    accTypeRadio = this._Driver.FindElement(By.XPath("//label[@for='baPersonal']"));
                    break;
            }
            accTypeRadio.Click();

            IWebElement continueButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"));
            continueButton.Click();
            //Thread.Sleep(2000);
            SharedServiceClass.WaitForBlockUiOff(this._Driver);
            continueButton.Click();
            //End of step 2 - Entering bank details

            IWebElement customer = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("customer"));
            customer.SendKeys(cardHolderName);

            SelectElement cardTypeSelect = new SelectElement(this._Driver.FindElement(By.CssSelector("select.selectheight.form_field")));
            cardTypeSelect.SelectByValue(cardType);

            IWebElement cardNo = this._Driver.FindElement(By.Id("card_no"));
            cardNo.SendKeys(cardNumber);

            IWebElement expiry = this._Driver.FindElement(By.Id("expiry"));
            expiry.SendKeys(expDate);

            IWebElement cv2 = this._Driver.FindElement(By.Id("cv2"));
            cv2.SendKeys(securityCode);

            IWebElement confirmStep3Button = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("paypoint-submit"));
            confirmStep3Button.Click();

            IWebElement myAccountButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("pacnet-status-back-to-profile"));
            myAccountButton.Click();
            //End of step 3 - Get cash
        }
    }
}
