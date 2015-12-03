namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using OpenQA.Selenium;
    using UIAutomationTests.Core;

    class BrokerShared : WebTestBase {

        private readonly IWebDriver _Driver;
        private readonly ResourceManager _EnvironmentConfig;
        private readonly ResourceManager _BrandConfig;
        private readonly object Locker;
        //private static readonly object lockObject= new object();

        public BrokerShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this._Driver = Driver;
            this._EnvironmentConfig = EnvironmentConfig;
            this._BrandConfig = BrandConfig;
            this.Locker = new object();
        }
        /// <summary>
        /// This procedure is to replace broker test case 'C1202'
        /// </summary>
        /// <returns>Returns the newly generated broker Email</returns>
        public void CreateNewBrokerAccount(
            string iFirmName,
            string iContactName,
            string iContactEmail,
            string iContactMobile,
            string iMobileCode,
            string iEstimatedMonthlyAppCount,
            string iEstimatedMonthlyClientAmount,
            string iPassword,
            bool iAgreeToTerms,
            bool iAgreeToPrivacyPolicy
            ) {
            string url = String.Concat(this._EnvironmentConfig.GetString("ENV_address"),this._BrandConfig.GetString("BrokerSignupHost"));

            this._Driver.Navigate().GoToUrl(url);

            IWebElement firmName = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("FirmName"));
            firmName.SendKeys(iFirmName);

            IWebElement contactName = this._Driver.FindElement(By.Id("ContactName"));
            contactName.SendKeys(iContactName);

            IWebElement contactEmail = this._Driver.FindElement(By.Id("ContactEmail"));
            contactEmail.SendKeys(iContactEmail);

            IWebElement contactMobile = this._Driver.FindElement(By.Id("ContactMobile"));
            contactMobile.SendKeys(iContactMobile);

            IWebElement generateMobileCode = this._Driver.FindElement(By.Id("generateMobileCode"));
            generateMobileCode.Click();

            IWebElement mobileCode = this._Driver.FindElement(By.Id("MobileCode"));
            mobileCode.SendKeys(iMobileCode);

            IWebElement estimatedMonthlyAppCount = this._Driver.FindElement(By.Id("EstimatedMonthlyAppCount"));
            estimatedMonthlyAppCount.SendKeys(iEstimatedMonthlyAppCount);

            IWebElement estimatedMonthlyClientAmount = this._Driver.FindElement(By.Id("EstimatedMonthlyClientAmount"));
            estimatedMonthlyClientAmount.SendKeys(iEstimatedMonthlyClientAmount);

            IWebElement password = this._Driver.FindElement(By.Id("Password"));
            password.SendKeys(iPassword);

            IWebElement password2 = this._Driver.FindElement(By.Id("Password2"));
            password2.SendKeys(iPassword);

            if (iAgreeToTerms) {
                IWebElement agreeToTerms = this._Driver.FindElement(By.XPath("//label[@for='AgreeToTerms']"));
                agreeToTerms.Click();
            }

            if (iAgreeToPrivacyPolicy) {
                IWebElement agreeToPrivacyPolicy = this._Driver.FindElement(By.XPath("//label[@for='AgreeToPrivacyPolicy']"));
                agreeToPrivacyPolicy.Click();
            }

            IWebElement signupBrokerButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("SignupBrokerButton"));
            signupBrokerButton.Click();

            SharedServiceClass.ElementIsVisible(this._Driver, By.Id("AddNewCustomer"));

            IWebElement logOff = this._Driver.FindElement(By.CssSelector("li.menu-btn.login.log-off > a"));
            logOff.Click();
        }

        public void BrokerLogIn(string brokerMail) {
            SharedServiceClass.WaitForBlockUiOff(this._Driver);
            string url = String.Concat(this._EnvironmentConfig.GetString("ENV_address"), this._BrandConfig.GetString("BrokerLoginHost"));

            this._Driver.Navigate()
                .GoToUrl(url);

            IWebElement loginEmail = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("LoginEmail"));
            loginEmail.SendKeys(brokerMail);

            IWebElement loginPassword = this._Driver.FindElement(By.Id("LoginPassword"));
            loginPassword.SendKeys("123456");

            IWebElement loginBrokerButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("LoginBrokerButton"));
            loginBrokerButton.Click();

            try {
                IWebElement acceptButon = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("AcceptTermsButton"),2);
                acceptButon.Click();
            } catch {}
        }

        /// <summary>
        /// Pre-condtion broker must be logged-in
        /// </summary>
        /// <returns>Newly created lead mail address</returns>
        public void BrokerLeadEnrolment(string fName, string lName, string leadEmail) {

            IWebElement addNewCustomer = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("AddNewCustomer"));
            addNewCustomer.Click();

            lock (this.Locker) {
                IWebElement leadFirstName = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("LeadFirstName"));
                leadFirstName.Click();
                leadFirstName.SendKeys(fName);
            }

            lock (this.Locker) {
                IWebElement leadLastName = this._Driver.FindElement(By.Id("LeadLastName"));
                leadLastName.Click();
                leadLastName.SendKeys(lName);
            }

            lock (this.Locker) {
                IWebElement leadMail = this._Driver.FindElement(By.Id("LeadEmail"));
                leadMail.Click();
                leadMail.SendKeys(leadEmail);
            }

        }

        public void BrokerAddBankAccount(string accountNum,
            string sort1,
            string sort2,
            string sort3,
            char accType) {

            IWebElement addBank = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("button.button.btn-green.pull-right.btn-wide.add-bank.ev-btn-org"));
 ﻿           addBank.Click();

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

            IWebElement continueButton = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("broker_bank_details_continue_button"));
            continueButton.Click();
        }
    }
}
