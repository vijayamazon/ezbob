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
        /// This procedure follows C1202.
        /// </summary>
        /// <returns></returns>
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

            //Step 4 - Click create an account.
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("BrokerSignupHost"));
            Driver.Navigate().GoToUrl(url);
            actionBot.WriteToLog("Nevigate to url: " + url);

            //Step 5 - Insert company name to the company name field and focus out.
            actionBot.SendKeys(By.Id("FirmName"), iFirmName, "(company name field)");

            //Step 6 - Insert contact person full name and focus out.
            actionBot.SendKeys(By.Id("ContactName"), iContactName, "(contact person full name)");

            //Step 7 - Insert a valid email address and focus out.
            actionBot.SendKeys(By.Id("ContactEmail"), iContactEmail, "(valid email addressma)");

            //Step 8 - Insert a valid number to the contact person mobile phone field and focus out. Mobile phone via work around is: 01111111111
            actionBot.SendKeys(By.Id("ContactMobile"), iContactMobile, "(contact person mobile phone)");

            //Work around is to replace Step 13 - Insert valid CAPTCHA and focus out
            //Work around - configuration change in Table: [ezbob].[ConfigurationVariables] Parameter: Name='BrokerForceCaptcha' Value='0'
            actionBot.Click(By.Id("generateMobileCode"), "(generate mobile code button)");

            //Mobile code via work around is: 222222
            actionBot.SendKeys(By.Id("MobileCode"), iMobileCode, "(valid mobile code)");

            //Step 9 - Insert any amount to the number of applications per month field and focus out.
            actionBot.SendKeys(By.Id("EstimatedMonthlyAppCount"), iEstimatedMonthlyAppCount, "(number of applications per month)");

            //Step 10 - Insert any amount to the value of credit per month field and focus out.
            actionBot.SendKeys(By.Id("EstimatedMonthlyClientAmount"), iEstimatedMonthlyClientAmount, "(value of credit per month)");

            //Step 11 - Insert a valid password to the password field and focus out.
            actionBot.SendKeys(By.Id("Password"), iPassword, "(password field)");

            //Step 12 - Insert the same password to the confirm password field and focus out.
            actionBot.SendKeys(By.Id("Password2"), iPassword, "(confirm password)");

            //Step 14 - Check all required checkboxe's.
            if (iAgreeToTerms)
                actionBot.Click(By.XPath("//label[@for='AgreeToTerms']"), "(agree to terms checkBox)");

            if (iAgreeToPrivacyPolicy)
                actionBot.Click(By.XPath("//label[@for='AgreeToPrivacyPolicy']"), "(agree to privacy policy checkBox)");

            //Step 15 - Click sign up.
            actionBot.Click(By.Id("SignupBrokerButton"), "(sign up button)");

            actionBot.WriteToLog("Begin assert: Verify broker dashboard is displayed.");
            SharedServiceClass.ElementIsVisible(Driver, By.Id("AddNewCustomer"));
            actionBot.WriteToLog("Positively asserted: Dashboard is displayed.");

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        public void BrokerLogIn(
            string logHeader,
            string brokerMail
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForAjaxReady(Driver);

            //Navigate to broker log-in page.
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("BrokerLoginHost"));
            Driver.Navigate().GoToUrl(url);
            actionBot.WriteToLog("Nevigate to url: " + url);

            //Insert a registered broker's email address and focus out.
            actionBot.SendKeys(By.Id("LoginEmail"), brokerMail, "(registered broker email address)");

            //Insert a valid password to the password field and focus out.
            actionBot.SendKeys(By.Id("LoginPassword"), "123456", "(password field)");

            //Click sign up.
            actionBot.Click(By.Id("LoginBrokerButton"), "(sign up button)");

            //Accept terms button only appears when terms are changed. TODO: check why this button shows up without terms change.
            try {
                actionBot.Click(By.Id("AcceptTermsButton"), "(accept terms button)", 2);
            } catch { }

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        public void BrokerLogOff(string logHeader
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForBlockUiOff(Driver);

            //Click log-off
            actionBot.Click(By.CssSelector("li.menu-btn.login.log-off > a.button"), "(Broker log-off button)");

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        /// <summary>
        /// This procedure follows C1352.
        /// </summary>
        /// <returns></returns>
        public void BrokerLeadEnrolment(
            string logHeader, 
            string fName,
            string lName, 
            string leadEmail,
            By fillWizardMethod
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            //Step 3 - Click add a new client.
            actionBot.Click(By.Id("AddNewCustomer"), "(add a new client button)");

            //TODO: remove a-sync locks. must be a workaround this problem.

            //Step 4 - Insert first name and focus out.
            lock (this.Locker)
                actionBot.SendKeys(By.Id("LeadFirstName"), fName, "(first name field)");

            //Step 5 - Insert surname and focus out.
            lock (this.Locker)
                actionBot.SendKeys(By.Id("LeadLastName"), lName, "(surname field)");

            //Step 6 - Insert prepared email and focus out.
            lock (this.Locker)
                actionBot.SendKeys(By.Id("LeadEmail"), leadEmail, "(valid lead e-mail)");

            //Step 7 - Click fill or send.
            actionBot.Click(fillWizardMethod, "(fill or send button)");

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
