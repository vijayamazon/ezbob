namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using System.Threading;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using UIAutomationTests.Core;

    class BrokerShared : WebTestBase {

        private IWebDriver _Driver;
        private ResourceManager _EnvironmentConfig;
        private ResourceManager _BrandConfig;

        public BrokerShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this._Driver = Driver;
            this._EnvironmentConfig = EnvironmentConfig;
            this._BrandConfig = BrandConfig;
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

            string url = this._BrandConfig.GetString("BrokerSignupHost");

            this._Driver.Navigate()
                .GoToUrl(url);

            IWebElement firmName = this._Driver.FindElement(By.Id("FirmName"));
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

            IWebElement signupBrokerButton = this._Driver.FindElement(By.Id("SignupBrokerButton"));
            signupBrokerButton.Click();

            Thread.Sleep(5000);
            //this._Driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.Parse("10"));
            Assert.IsTrue(this._Driver.FindElement(By.Id("AddNewCustomer")).Displayed);
            IWebElement logOff = this._Driver.FindElement(By.ClassName("log-off"));
            logOff.Click();
        }

        public void BrokerLogIn(string brokerMail) {

            string url = this._BrandConfig.GetString("BrokerLoginHost");
            this._Driver.Navigate()
                .GoToUrl(url);

            IWebElement loginEmail = this._Driver.FindElement(By.Id("LoginEmail"));
            loginEmail.SendKeys(brokerMail);

            IWebElement loginPassword = this._Driver.FindElement(By.Id("LoginPassword"));
            loginPassword.SendKeys("123456");

            IWebElement loginBrokerButton = this._Driver.FindElement(By.Id("LoginBrokerButton"));
            loginBrokerButton.Click();

            Thread.Sleep(2000);
            //this._Driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.Parse("10"));
            if (this._Driver.FindElement(By.ClassName("section-requestacceptterms")).Displayed == true) {//In case new terms screen appears.
                IWebElement acceptButon = this._Driver.FindElement(By.Id("AcceptTermsButton"));
                acceptButon.Click();
            }
        }

        /// <summary>
        /// Pre-condtion broker must be logged-in
        /// </summary>
        /// <returns>Newly created leed mail address</returns>
        public void BrokerLeedEnrolment(string fName, string lName, string leadEmail) {

            Thread.Sleep(7000);
            //this._Driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.Parse("10"));
            Assert.IsTrue(this._Driver.FindElement(By.Id("AddNewCustomer")) != null);

            IWebElement addNewCustomer = this._Driver.FindElement(By.Id("AddNewCustomer"));
            addNewCustomer.Click();

            Thread.Sleep(2000);
            //this._Driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.Parse("10"));
            Assert.IsTrue(this._Driver.FindElement(By.Id("LeadFirstName")) != null);

            IWebElement leadFirstName = this._Driver.FindElement(By.Id("LeadFirstName"));
            leadFirstName.SendKeys(fName);

            IWebElement leadLastName = this._Driver.FindElement(By.Id("LeadLastName"));
            leadLastName.SendKeys(lName);

            IWebElement leadMail = this._Driver.FindElement(By.Id("LeadEmail"));
            leadMail.SendKeys(leadEmail);
        }
    }
}
