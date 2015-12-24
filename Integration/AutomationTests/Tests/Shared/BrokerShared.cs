namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using OpenQA.Selenium;
    using UIAutomationTests.Core;

    class BrokerShared : WebTestBase {
        private readonly object Locker;

        public BrokerShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this.Driver = Driver;
            this.EnvironmentConfig = EnvironmentConfig;
            this.BrandConfig = BrandConfig;
            this.actionBot = new ActionBot(Driver);
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
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("BrokerSignupHost"));

            Driver.Navigate().GoToUrl(url);

            //IWebElement firmName = SharedServiceClass.ElementIsVisible(Driver, By.Id("FirmName"));
            //firmName.SendKeys(iFirmName);
            actionBot.SendKeys(By.Id("FirmName"), iFirmName, "");

            //IWebElement contactName = Driver.FindElement(By.Id("ContactName"));
            //contactName.SendKeys(iContactName);
            actionBot.SendKeys(By.Id("ContactName"), iContactName, "");
            //IWebElement contactEmail = Driver.FindElement(By.Id("ContactEmail"));
            //contactEmail.SendKeys(iContactEmail);
            actionBot.SendKeys(By.Id("ContactEmail"), iContactEmail, "");
            //IWebElement contactMobile = Driver.FindElement(By.Id("ContactMobile"));
            //contactMobile.SendKeys(iContactMobile);
            actionBot.SendKeys(By.Id("ContactMobile"), iContactMobile, "");
            //IWebElement generateMobileCode = Driver.FindElement(By.Id("generateMobileCode"));
            //generateMobileCode.Click();
            actionBot.Click(By.Id("generateMobileCode"), "");

            //IWebElement mobileCode = SharedServiceClass.ElementIsVisible(Driver,By.Id("MobileCode"));//Driver.FindElement(By.Id("MobileCode")));
            //mobileCode.SendKeys(iMobileCode);
            actionBot.SendKeys(By.Id("MobileCode"), iMobileCode, "");
            //IWebElement estimatedMonthlyAppCount = Driver.FindElement(By.Id("EstimatedMonthlyAppCount"));
            //estimatedMonthlyAppCount.SendKeys(iEstimatedMonthlyAppCount);
            actionBot.SendKeys(By.Id("EstimatedMonthlyAppCount"), iEstimatedMonthlyAppCount, "");
            //IWebElement estimatedMonthlyClientAmount = Driver.FindElement(By.Id("EstimatedMonthlyClientAmount"));
            //estimatedMonthlyClientAmount.SendKeys(iEstimatedMonthlyClientAmount);
            actionBot.SendKeys(By.Id("EstimatedMonthlyClientAmount"), iEstimatedMonthlyClientAmount, "");
            //IWebElement password = Driver.FindElement(By.Id("Password"));
            //password.SendKeys(iPassword);
            actionBot.SendKeys(By.Id("Password"), iPassword, "");
            //IWebElement password2 = Driver.FindElement(By.Id("Password2"));
            //password2.SendKeys(iPassword);
            actionBot.SendKeys(By.Id("Password2"), iPassword, "");
            if (iAgreeToTerms) {
                actionBot.Click(By.XPath("//label[@for='AgreeToTerms']"), "");
                //    IWebElement agreeToTerms = Driver.FindElement(By.XPath("//label[@for='AgreeToTerms']"));
                //    agreeToTerms.Click();

            }

            if (iAgreeToPrivacyPolicy) {
                actionBot.Click(By.XPath("//label[@for='AgreeToPrivacyPolicy']"), "");
                //    IWebElement agreeToPrivacyPolicy = Driver.FindElement(By.XPath("//label[@for='AgreeToPrivacyPolicy']"));
                //    agreeToPrivacyPolicy.Click();
            }

            //IWebElement signupBrokerButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("SignupBrokerButton"));
            //signupBrokerButton.Click();
            actionBot.Click(By.Id("SignupBrokerButton"), "");

            SharedServiceClass.ElementIsVisible(Driver, By.Id("AddNewCustomer"));

            //IWebElement logOff = Driver.FindElement(By.CssSelector("li.menu-btn.login.log-off > a"));
            //logOff.Click();
            actionBot.Click(By.CssSelector("li.menu-btn.login.log-off > a"), "");
        }

        public void BrokerLogIn(string brokerMail) {
            SharedServiceClass.WaitForBlockUiOff(Driver);
            SharedServiceClass.WaitForAjaxReady(Driver);
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("BrokerLoginHost"));

            Driver.Navigate().GoToUrl(url);

            //IWebElement loginEmail = SharedServiceClass.ElementIsVisible(Driver, By.Id("LoginEmail"));
            //loginEmail.SendKeys(brokerMail);
            actionBot.SendKeys(By.Id("LoginEmail"), brokerMail, "");

            //IWebElement loginPassword = Driver.FindElement(By.Id("LoginPassword"));
            //loginPassword.SendKeys("123456");
            actionBot.SendKeys(By.Id("LoginPassword"), "123456", "");

            //IWebElement loginBrokerButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("LoginBrokerButton"));
            //loginBrokerButton.Click();
            actionBot.Click(By.Id("LoginBrokerButton"), "");

            try {
                //IWebElement acceptButon = SharedServiceClass.ElementToBeClickable(Driver, By.Id("AcceptTermsButton"),2);
                //acceptButon.Click();
                actionBot.Click(By.Id("AcceptTermsButton"), "", 2);
            } catch { }
        }

        /// <summary>
        /// Pre-condtion broker must be logged-in
        /// </summary>
        /// <returns>Newly created lead mail address</returns>
        public void BrokerLeadEnrolment(string fName, string lName, string leadEmail) {

            //IWebElement addNewCustomer = SharedServiceClass.ElementToBeClickable(Driver, By.Id("AddNewCustomer"));
            //addNewCustomer.Click();
            actionBot.Click(By.Id("AddNewCustomer"), "");

            lock (this.Locker) {
                //IWebElement leadFirstName = SharedServiceClass.ElementIsVisible(Driver, By.Id("LeadFirstName"));
                //leadFirstName.Click();
                //leadFirstName.SendKeys(fName);
                actionBot.SendKeys(By.Id("LeadFirstName"), fName, "");
            }

            lock (this.Locker) {
                //IWebElement leadLastName = Driver.FindElement(By.Id("LeadLastName"));
                //leadLastName.Click();
                //leadLastName.SendKeys(lName);
                actionBot.SendKeys(By.Id("LeadLastName"), lName, "");
            }

            lock (this.Locker) {
                //IWebElement leadMail = Driver.FindElement(By.Id("LeadEmail"));
                //leadMail.Click();
                //leadMail.SendKeys(leadEmail);
                actionBot.SendKeys(By.Id("LeadEmail"), leadEmail, "");
            }

        }

        public void BrokerAddBankAccount(string accountNum,
            string sort1,
            string sort2,
            string sort3,
            char accType) {
            //IWebElement addBank = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("button.button.btn-green.pull-right.btn-wide.add-bank.ev-btn-org"));
            //addBank.Click();
            actionBot.Click(By.CssSelector("button.button.btn-green.pull-right.btn-wide.add-bank.ev-btn-org"), "");

            //IWebElement accountNumber = SharedServiceClass.ElementIsVisible(Driver, By.Id("AccountNumber"));
            //accountNumber.SendKeys(accountNum);
            actionBot.SendKeys(By.Id("AccountNumber"), accountNum, "");

            //IWebElement sortCode1 = Driver.FindElement(By.Id("SortCode1"));
            //sortCode1.SendKeys(sort1);
            actionBot.SendKeys(By.Id("SortCode1"), sort1, "");

            //IWebElement sortCode2 = Driver.FindElement(By.Id("SortCode2"));
            //sortCode2.SendKeys(sort2);
            actionBot.SendKeys(By.Id("SortCode2"), sort2, "");

            //IWebElement sortCode3 = Driver.FindElement(By.Id("SortCode3"));
            //sortCode3.SendKeys(sort3);
            actionBot.SendKeys(By.Id("SortCode3"), sort3, "");

            IWebElement accTypeRadio;
            switch (char.ToUpper(accType)) {
                case 'B':
                    accTypeRadio = SharedServiceClass.ElementToBeClickable(Driver,By.XPath("//label[@for='baBusiness']"));//Driver.FindElement(By.XPath("//label[@for='baBusiness']")));
                    break;
                default:
                    accTypeRadio = SharedServiceClass.ElementToBeClickable(Driver,By.XPath("//label[@for='baPersonal']"));//Driver.FindElement(By.XPath("//label[@for='baPersonal']"));
                    break;
            }
            accTypeRadio.Click();

            //IWebElement continueButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("broker_bank_details_continue_button"));
            //continueButton.Click();
            actionBot.Click(By.Id("broker_bank_details_continue_button"), "");
        }
    }
}
