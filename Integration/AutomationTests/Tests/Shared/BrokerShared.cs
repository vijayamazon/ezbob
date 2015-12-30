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
            string logHeader,
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
            actionBot.WriteToLog("Begin method: " + logHeader);
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("BrokerSignupHost"));

            Driver.Navigate().GoToUrl(url);
            actionBot.WriteToLog(logHeader + " - " + "Nevigate to url: " + url);

            //IWebElement firmName = SharedServiceClass.ElementIsVisible(Driver, By.Id("FirmName"));
            //firmName.SendKeys(iFirmName);
            actionBot.SendKeys(By.Id("FirmName"), iFirmName, logHeader);

            //IWebElement contactName = Driver.FindElement(By.Id("ContactName"));
            //contactName.SendKeys(iContactName);
            actionBot.SendKeys(By.Id("ContactName"), iContactName, logHeader);
            //IWebElement contactEmail = Driver.FindElement(By.Id("ContactEmail"));
            //contactEmail.SendKeys(iContactEmail);
            actionBot.SendKeys(By.Id("ContactEmail"), iContactEmail, logHeader);
            //IWebElement contactMobile = Driver.FindElement(By.Id("ContactMobile"));
            //contactMobile.SendKeys(iContactMobile);
            actionBot.SendKeys(By.Id("ContactMobile"), iContactMobile, logHeader);
            //IWebElement generateMobileCode = Driver.FindElement(By.Id("generateMobileCode"));
            //generateMobileCode.Click();
            actionBot.Click(By.Id("generateMobileCode"), logHeader);

            //IWebElement mobileCode = SharedServiceClass.ElementIsVisible(Driver,By.Id("MobileCode"));//Driver.FindElement(By.Id("MobileCode")));
            //mobileCode.SendKeys(iMobileCode);
            actionBot.SendKeys(By.Id("MobileCode"), iMobileCode, logHeader);
            //IWebElement estimatedMonthlyAppCount = Driver.FindElement(By.Id("EstimatedMonthlyAppCount"));
            //estimatedMonthlyAppCount.SendKeys(iEstimatedMonthlyAppCount);
            actionBot.SendKeys(By.Id("EstimatedMonthlyAppCount"), iEstimatedMonthlyAppCount, logHeader);
            //IWebElement estimatedMonthlyClientAmount = Driver.FindElement(By.Id("EstimatedMonthlyClientAmount"));
            //estimatedMonthlyClientAmount.SendKeys(iEstimatedMonthlyClientAmount);
            actionBot.SendKeys(By.Id("EstimatedMonthlyClientAmount"), iEstimatedMonthlyClientAmount, logHeader);
            //IWebElement password = Driver.FindElement(By.Id("Password"));
            //password.SendKeys(iPassword);
            actionBot.SendKeys(By.Id("Password"), iPassword, logHeader);
            //IWebElement password2 = Driver.FindElement(By.Id("Password2"));
            //password2.SendKeys(iPassword);
            actionBot.SendKeys(By.Id("Password2"), iPassword, logHeader);
            if (iAgreeToTerms) {
                actionBot.Click(By.XPath("//label[@for='AgreeToTerms']"), logHeader);
                //    IWebElement agreeToTerms = Driver.FindElement(By.XPath("//label[@for='AgreeToTerms']"));
                //    agreeToTerms.Click();

            }

            if (iAgreeToPrivacyPolicy) {
                actionBot.Click(By.XPath("//label[@for='AgreeToPrivacyPolicy']"), logHeader);
                //    IWebElement agreeToPrivacyPolicy = Driver.FindElement(By.XPath("//label[@for='AgreeToPrivacyPolicy']"));
                //    agreeToPrivacyPolicy.Click();
            }

            //IWebElement signupBrokerButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("SignupBrokerButton"));
            //signupBrokerButton.Click();
            actionBot.Click(By.Id("SignupBrokerButton"), logHeader);

            SharedServiceClass.ElementIsVisible(Driver, By.Id("AddNewCustomer"));
            actionBot.WriteToLog(logHeader + " - " + By.Id("AddNewCustomer") + ". Assert element is visible.");

            //IWebElement logOff = Driver.FindElement(By.CssSelector("li.menu-btn.login.log-off > a"));
            //logOff.Click();
            actionBot.Click(By.CssSelector("li.menu-btn.login.log-off > a"), logHeader);
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        public void BrokerLogIn(string logHeader, string brokerMail) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            //SharedServiceClass.WaitForBlockUiOff(Driver);
            SharedServiceClass.WaitForAjaxReady(Driver);
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("BrokerLoginHost"));

            Driver.Navigate().GoToUrl(url);
            actionBot.WriteToLog(logHeader + " - " + "Nevigate to url: " + url);

            //IWebElement loginEmail = SharedServiceClass.ElementIsVisible(Driver, By.Id("LoginEmail"));
            //loginEmail.SendKeys(brokerMail);
            actionBot.SendKeys(By.Id("LoginEmail"), brokerMail, logHeader);

            //IWebElement loginPassword = Driver.FindElement(By.Id("LoginPassword"));
            //loginPassword.SendKeys("123456");
            actionBot.SendKeys(By.Id("LoginPassword"), "123456", logHeader);

            //IWebElement loginBrokerButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("LoginBrokerButton"));
            //loginBrokerButton.Click();
            actionBot.Click(By.Id("LoginBrokerButton"), logHeader);

            try {
                //IWebElement acceptButon = SharedServiceClass.ElementToBeClickable(Driver, By.Id("AcceptTermsButton"),2);
                //acceptButon.Click();
                actionBot.Click(By.Id("AcceptTermsButton"), logHeader, 2);
            } catch { }
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        public void BrokerLogOff(string logHeader) {
            SharedServiceClass.WaitForBlockUiOff(Driver);
            actionBot.Click(By.CssSelector("li.menu-btn.login.log-off > a.button"), logHeader);
        }

        /// <summary>
        /// Pre-condtion broker must be logged-in
        /// </summary>
        /// <returns>Newly created lead mail address</returns>
        public void BrokerLeadEnrolment(string logHeader, string fName, string lName, string leadEmail, By fillWizardMethod) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            //IWebElement addNewCustomer = SharedServiceClass.ElementToBeClickable(Driver, By.Id("AddNewCustomer"));
            //addNewCustomer.Click();
            actionBot.Click(By.Id("AddNewCustomer"), logHeader);

            lock (this.Locker) {
                //IWebElement leadFirstName = SharedServiceClass.ElementIsVisible(Driver, By.Id("LeadFirstName"));
                //leadFirstName.Click();
                //leadFirstName.SendKeys(fName);
                actionBot.SendKeys(By.Id("LeadFirstName"), fName, logHeader);
            }

            lock (this.Locker) {
                //IWebElement leadLastName = Driver.FindElement(By.Id("LeadLastName"));
                //leadLastName.Click();
                //leadLastName.SendKeys(lName);
                actionBot.SendKeys(By.Id("LeadLastName"), lName, logHeader);
            }

            lock (this.Locker) {
                //IWebElement leadMail = Driver.FindElement(By.Id("LeadEmail"));
                //leadMail.Click();
                //leadMail.SendKeys(leadEmail);
                actionBot.SendKeys(By.Id("LeadEmail"), leadEmail, logHeader);
            }

            actionBot.Click(fillWizardMethod, logHeader);
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        public void BrokerAddBankAccount(string logHeader,
            string accountNum,
            string sort1,
            string sort2,
            string sort3,
            char accType) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            //IWebElement addBank = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("button.button.btn-green.pull-right.btn-wide.add-bank.ev-btn-org"));
            //addBank.Click();
            actionBot.Click(By.CssSelector("button.button.btn-green.pull-right.btn-wide.add-bank.ev-btn-org"), logHeader);

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

            //IWebElement continueButton = SharedServiceClass.ElementToBeClickable(Driver, By.Id("broker_bank_details_continue_button"));
            //continueButton.Click();
            actionBot.Click(By.Id("broker_bank_details_continue_button"), logHeader);
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }
    }
}
