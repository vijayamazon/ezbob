namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using System.Threading;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using UIAutomationTests.Core;

    class CustomerShared : WebTestBase {

        private IWebDriver _Driver;
        private ResourceManager _EnvironmentConfig;
        private ResourceManager _BrandConfig;

        public CustomerShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this._Driver = Driver;
            this._EnvironmentConfig = EnvironmentConfig;
            this._BrandConfig = BrandConfig;
        }

        public void CustomerLogIn(bool isFirstTime, string brokerMail) {
            string url = this._BrandConfig.GetString("CustomerLogIn");
            this._Driver.Navigate()
                .GoToUrl(url);

            IWebElement userName = this._Driver.FindElement(By.Id("UserName"));
            userName.SendKeys(brokerMail);

            IWebElement password = this._Driver.FindElement(By.Id("Password"));
            password.SendKeys("123123");

            IWebElement loginBrokerButton = this._Driver.FindElement(By.Id("loginSubmit"));
            loginBrokerButton.Click();

            if (isFirstTime) {
                Thread.Sleep(25000);
                IWebElement continueButton = this._Driver.FindElement(By.CssSelector("div.automation-popup > div.automation-popup-content > div.alignright > button.button"));
                continueButton.Click();
            }

            Assert.IsTrue(this._Driver.FindElement(By.CssSelector("h2.header-message.hm_green")).Text.Contains("Click Choose Amount and choose the exact amount you need."));
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
            IWebElement chooseAmountBtn = this._Driver.FindElement(By.CssSelector("button.button.btn-green.get-cash.ev-btn-org"));
            chooseAmountBtn.Click();

            IWebElement preAgreementTermsRead = this._Driver.FindElement(By.XPath("//label[@for='preAgreementTermsRead']"));
            preAgreementTermsRead.Click();

            IWebElement agreementTermsRead = this._Driver.FindElement(By.XPath("//label[@for='agreementTermsRead']"));
            agreementTermsRead.Click();

            IWebElement notInBankruptcy = this._Driver.FindElement(By.XPath("//label[@for='notInBankruptcy']"));
            notInBankruptcy.Click();

            IWebElement confirmButton = this._Driver.FindElement(By.CssSelector("button.ok-button.button.btn-green.ev-btn-org"));
            confirmButton.Click();

            IWebElement signedName = this._Driver.FindElement(By.Id("signedName"));
            signedName.SendKeys(fName + " " + lName);

            IWebElement nextButton = this._Driver.FindElement(By.LinkText("Next"));
            nextButton.Click();

            //End of Step 1 - Choosing loas terms
            IWebElement accountNumber = this._Driver.FindElement(By.Id("AccountNumber"));
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

            IWebElement continueButton = this._Driver.FindElement(By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"));
            continueButton.Click();
            Thread.Sleep(1000);
            continueButton.Click();
            //End of step 2 - Entering bank details

            Thread.Sleep(10000);
            IWebElement customer = this._Driver.FindElement(By.Id("customer"));
            customer.SendKeys(cardHolderName);

            SelectElement cardTypeSelect = new SelectElement(this._Driver.FindElement(By.CssSelector("select.selectheight.form_field")));
            cardTypeSelect.SelectByValue(cardType);

            IWebElement cardNo = this._Driver.FindElement(By.Id("card_no"));
            cardNo.SendKeys(cardNumber);

            IWebElement expiry = this._Driver.FindElement(By.Id("expiry"));
            expiry.SendKeys(expDate);

            IWebElement cv2 = this._Driver.FindElement(By.Id("cv2"));
            cv2.SendKeys(securityCode);

            IWebElement confirmStep3Button = this._Driver.FindElement(By.Id("paypoint-submit"));
            confirmStep3Button.Click();

            IWebElement myAccountButton = this._Driver.FindElement(By.Id("pacnet-status-back-to-profile"));
            myAccountButton.Click();
            //End of step 3 - Get cash
        }
    }
}
