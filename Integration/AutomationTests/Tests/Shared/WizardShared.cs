namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using OpenQA.Selenium;
    using UIAutomationTests.Core;

    class WizardShared : WebTestBase {
        private readonly object Locker;

        public WizardShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this.Driver = Driver;
            this.EnvironmentConfig = EnvironmentConfig;
            this.BrandConfig = BrandConfig;
            this.actionBot = new ActionBot(Driver);
            this.Locker = new object();
        }

        //This procedure is to replace OrangeMoney/Wizard test case 'C3'
        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepOne(string logHeader,
            string origin,
            string emailAdress,
             string password,
             int secretQuestion,
             string secAnswer,
             string reqAmmount) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            if (string.Equals("ClientSignup",origin)) {
                string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("WizardHost"));//Step 1

                Driver.Navigate().GoToUrl(url);//Step 2

                actionBot.WriteToLog(logHeader + " - " + "Nevigate to url: " + url);

                //IWebElement agreeToTerms = Driver.FindElement(By.XPath("//label[@for='AgreeToTerms']"));
                //agreeToTerms.Click();
            }

            //IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));//Step 3
            //email.SendKeys(emailAdress);
            actionBot.SendKeys(By.Id("Email"), emailAdress, logHeader, isClear: false);

            //IWebElement signupPass1 = Driver.FindElement(By.Id("signupPass1"));//Step 4
            //signupPass1.SendKeys(password);
            actionBot.SendKeys(By.Id("signupPass1"), password, logHeader);

            //IWebElement signupPass2 = Driver.FindElement(By.Id("signupPass2"));//Step 5
            //signupPass2.SendKeys(password);
            actionBot.SendKeys(By.Id("signupPass2"), password, logHeader);

            //SelectElement secrertQuestion = new SelectElement(Driver.FindElement(By.Id("securityQuestion")));//Step 6
            //secrertQuestion.SelectByIndex(secretQuestion);
            actionBot.SelectByIndex(By.Id("securityQuestion"), secretQuestion, logHeader);

            //IWebElement securityAnswer = Driver.FindElement(By.Id("SecurityAnswer"));//Step 7
            //securityAnswer.SendKeys(secAnswer);
            actionBot.SendKeys(By.Id("SecurityAnswer"), secAnswer, logHeader);

            //IWebElement amount = Driver.FindElement(By.Id("amount"));
            //amount.SendKeys(reqAmmount);

            if (String.Equals(origin, "ClientSignup")) {//This code is ilrelevant in case accessed from Broker-lead-fill

                //IWebElement mobilePhone = Driver.FindElement(By.Id("mobilePhone"));//Step 8
                //mobilePhone.SendKeys("1111111111");
                actionBot.SendKeys(By.Id("mobilePhone"), "1111111111", logHeader);

                //IWebElement generateMobileCode = Driver.FindElement(By.Id("generateMobileCode"));//Step 9
                //generateMobileCode.Click();
                actionBot.Click(By.Id("generateMobileCode"), logHeader);

                //IWebElement mobileCode = Driver.FindElement(By.Id("mobileCode"));//Step 10
                //mobileCode.SendKeys("222222");
                actionBot.SendKeys(By.Id("mobileCode"), "222222", logHeader);
            }

            //IWebElement signupSubmitButton = Driver.FindElement(By.Id("signupSubmitButton"));//Step 11
            //signupSubmitButton.Click();
            actionBot.Click(By.Id("signupSubmitButton"), logHeader);
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //This procedure is to replace OrangeMoney/Wizard test case 'C1380', 'C26'
        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepTwo(string logHeader,
            string origin,
            string personName,
             string personSurename,
             char gender,
             string dobDay,
             string dobMonth,
             string dobYear,
             string marStatus,
             string postCode,
             string addressTime,
             string resStatus,
             string phone,
            string phone2,
             bool agreeTerms) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            //SharedServiceClass.WaitForBlockUiOff(Driver);
            SharedServiceClass.WaitForAjaxReady(Driver);
            lock (this.Locker) {
                //IWebElement firstName = SharedServiceClass.ElementIsVisible(Driver, By.Id("FirstName"));//Step 1
                //firstName.Click();
                //firstName.SendKeys(personName);
                actionBot.SendKeys(By.Id("FirstName"), personName, logHeader, isClear: false);
            }

            lock (this.Locker) {
                //IWebElement surname = Driver.FindElement(By.Id("Surname"));//Step 2
                //surname.Click();
                //surname.SendKeys(personSurename);
                actionBot.SendKeys(By.Id("Surname"), personSurename, logHeader, isClear: false);
            }

            By formRadioCtrl;//Step 3
            switch (char.ToUpper(gender)) {
                case 'F':
                    formRadioCtrl = By.XPath("//label[@for='FormRadioCtrl_F']");//Driver.FindElement(By.XPath("//label[@for='FormRadioCtrl_F']"));
                    break;
                default:
                    formRadioCtrl = By.XPath("//label[@for='FormRadioCtrl_M']");//Driver.FindElement(By.XPath("//label[@for='FormRadioCtrl_M']"));
                    break;
            }
            actionBot.Click(formRadioCtrl, logHeader);

            //SelectElement dateOfBirthDay = new SelectElement(Driver.FindElement(By.Id("DateOfBirthDay")));//Step 4
            //dateOfBirthDay.SelectByValue(dobDay);
            actionBot.SelectByValue(By.Id("DateOfBirthDay"), dobDay, logHeader);

            //SelectElement dateOfBirthMonth = new SelectElement(Driver.FindElement(By.Id("DateOfBirthMonth")));
            //dateOfBirthMonth.SelectByText(dobMonth);
            actionBot.SelectByText(By.Id("DateOfBirthMonth"), dobMonth, logHeader);

            //SelectElement dateOfBirthYear = new SelectElement(Driver.FindElement(By.Id("DateOfBirthYear")));
            //dateOfBirthYear.SelectByValue(dobYear);
            actionBot.SelectByValue(By.Id("DateOfBirthYear"), dobYear, logHeader);

            //SelectElement maritalStatus = new SelectElement(Driver.FindElement(By.Id("MaritalStatus")));//Step 5
            //maritalStatus.SelectByValue(marStatus);
            actionBot.SelectByValue(By.Id("MaritalStatus"), marStatus, logHeader);

            //IWebElement addAddressInput = Driver.FindElement(By.CssSelector("input.addAddressInput"));//Step 6
            //addAddressInput.SendKeys(postCode);
            actionBot.SendKeys(By.CssSelector("input.addAddressInput"), postCode, logHeader);

            //IWebElement addAddressButton = Driver.FindElement(By.CssSelector("input.addAddress"));//Step 7
            //addAddressButton.Click();
            actionBot.Click(By.CssSelector("input.addAddress"), logHeader);

            //IWebElement matchingAddressList = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("ul.matchingAddressList > li"));//Step 8
            //matchingAddressList.Click();
            actionBot.Click(By.CssSelector("ul.matchingAddressList > li"), logHeader);

            //IWebElement postCodeBtnOk = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("button.postCodeBtnOk"));//Step 9
            //postCodeBtnOk.Click();
            actionBot.Click(By.CssSelector("button.postCodeBtnOk"), logHeader);

            //SelectElement timeAtAddress = new SelectElement(Driver.FindElement(By.Id("TimeAtAddress")));//Step 10
            //timeAtAddress.SelectByValue(addressTime);
            actionBot.SelectByValue(By.Id("TimeAtAddress"), addressTime, logHeader);

            //SelectElement propertyStatus = new SelectElement(Driver.FindElement(By.Id("PropertyStatus")));//Step 11
            //propertyStatus.SelectByValue(resStatus);
            actionBot.SelectByValue(By.Id("PropertyStatus"), resStatus, logHeader);

            if (String.Equals(origin, "BrokerFillLead")) {

                //IWebElement mobilePhone = Driver.FindElement(By.Id("MobilePhone")); //This code is only implemented in case accessed from Broker-lead-fill
                //mobilePhone.SendKeys(phone);
                actionBot.SendKeys(By.Id("MobilePhone"), phone, logHeader);
            }

            //IWebElement dayTimePhone = Driver.FindElement(By.Id("DayTimePhone"));//Step 12
            //dayTimePhone.SendKeys(phone2);
            actionBot.SendKeys(By.Id("DayTimePhone"), phone2, logHeader);

            if (agreeTerms) {//Step 14
                //IWebElement consentToSearch = Driver.FindElement(By.XPath("//label[@for='ConsentToSearch']"));
                //consentToSearch.Click();
                actionBot.Click(By.XPath("//label[@for='ConsentToSearch']"), logHeader);
            }

            //IWebElement personInfoContinueBtn = Driver.FindElement(By.Id("personInfoContinueBtn"));//Step 15
            //personInfoContinueBtn.Click();
            actionBot.Click(By.Id("personInfoContinueBtn"), logHeader);
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //This procedure is to replace OrangeMoney/Wizard test case 'C91'
        public void PerformWizardStepThree(string logHeader,
            string businessType,
            bool isSmallBusiness,
            string indType,
            string revenue) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            //SharedServiceClass.WaitForBlockUiOff(Driver);
            SharedServiceClass.WaitForAjaxReady(Driver);

            //SelectElement typeOfBusiness = SharedServiceClass.SelectIsVisible(Driver, By.Id("TypeOfBusiness"));//Step 1
            //typeOfBusiness.SelectByValue(businessType);
            actionBot.SelectByValue(By.Id("TypeOfBusiness"), businessType, logHeader);

            //if (isSmallBusiness) {//Removed in new wizard verssion
            //    IWebElement bussinesSquare = Driver.FindElement(By.CssSelector("i.fa.fa-square-o"));
            //    bussinesSquare.Click();
            //}

            //SelectElement industryType = new SelectElement(Driver.FindElement(By.Id("IndustryType")));//Step 2
            //industryType.SelectByValue(indType);
            actionBot.SelectByValue(By.Id("IndustryType"), indType, logHeader);

            //IWebElement overallTurnOver = Driver.FindElement(By.Id("OverallTurnOver"));//Step 3
            //overallTurnOver.SendKeys(revenue);
            actionBot.SendKeys(By.Id("OverallTurnOver"), revenue, logHeader);

            //IWebElement companyContinueBtn = Driver.FindElement(By.Id("companyContinueBtn"));//Step 4
            //companyContinueBtn.Click();
            actionBot.Click(By.Id("companyContinueBtn"), logHeader);

            try {
                //SharedServiceClass.WaitForAjaxReady(Driver);
                //IWebElement targets = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("div.ui-dialog > div.ui-dialog-content > ul.targets > li"), 25);
                SharedServiceClass.WaitForBlockUiOff(Driver);
                //targets.Click();
                actionBot.Click(By.CssSelector("div.ui-dialog > div.ui-dialog-content > ul.targets > li"), logHeader, 25);

                //IWebElement button = Driver.FindElement(By.CssSelector("button.button.btn-green.btnTargetOk.ev-btn-org"));
                //button.Click();
                actionBot.Click(By.CssSelector("button.button.btn-green.btnTargetOk.ev-btn-org"), logHeader);
            } catch { }
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        //This procedure is to replace OrangeMoney/Wizard test case 'C778' - Steps 1-4 have been replaced.
        public void PerformWizardStepFour(string logHeader,
            string origin,
            string marketplace,
            string accLogin,
            string loginVal,
            string accPass,
            string passVal,
            string accBtn) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            SharedServiceClass.WaitForBlockUiOff(Driver);
            //IWebElement showMore = SharedServiceClass.ElementToBeClickable(Driver, By.Id("link_account_see_more_less"));
            //SharedServiceClass.ScrollIntoView(Driver, showMore);
            //SharedServiceClass.WaitForBlockUiOff(Driver);
            //showMore.Click();
            actionBot.Click(By.Id("link_account_see_more_less"), logHeader);

            //Thread.Sleep(1000);
            //SharedServiceClass.WaitForBlockUiOff(Driver);
            //IWebElement marketplaceButton = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector(marketplace));
            //marketplaceButton.Click();
            //actionBot.Click(By.CssSelector(marketplace), "");
            By marketplacedAssert;
            switch (marketplace) {
                case "a.marketplace-button-account-paypal":
                    marketplacedAssert = By.Id("paypalContinueBtn");
                    break;
                default:
                    marketplacedAssert = By.Id(accLogin);
                    break;
            }
            actionBot.ClickAssert(By.CssSelector(marketplace),marketplacedAssert, logHeader);
            //SharedServiceClass.TryElementClick(Driver, By.CssSelector(marketplace));
            //actionBot.WriteToLog(logHeader + " - " + By.CssSelector(marketplace).ToString() + " - TryElementClick.");

            if (String.Equals("a.marketplace-button-account-paypal", marketplace)) {

                //IWebElement paypalContinueBtn = SharedServiceClass.ElementToBeClickable(Driver, By.Id("paypalContinueBtn"));
                //paypalContinueBtn.Click();
                actionBot.Click(By.Id("paypalContinueBtn"), logHeader);

                Driver.SwitchTo().Window(SharedServiceClass.LastWindowName(Driver, 2));

                SharedServiceClass.WebAddressContains(Driver, "webscr", 20);
            }

            //IWebElement loginField = Driver.FindElement(By.Id(accLogin));
            //loginField.SendKeys(loginVal);
            actionBot.SendKeys(By.Id(accLogin), loginVal, logHeader);

            //IWebElement passwordField = Driver.FindElement(By.Id(accPass));
            //passwordField.SendKeys(passVal);
            actionBot.SendKeys(By.Id(accPass), passVal, logHeader);

            //IWebElement linkAccounts = Driver.FindElement(By.Id(accBtn));
            //linkAccounts.Click();
            actionBot.Click(By.Id(accBtn), logHeader);

            if (String.Equals("a.marketplace-button-account-paypal", marketplace)) {
                //By.Name("grant.x")
                //IWebElement grantPermission = SharedServiceClass.ElementToBeClickable(Driver, By.XPath("//input[@name='grant.x']"));
                //grantPermission.Click();
                actionBot.Click(By.XPath("//input[@name='grant.x']"), logHeader);

                Driver.SwitchTo().Window(SharedServiceClass.LastWindowName(Driver, 1));
            }

            //SharedServiceClass.WaitForBlockUiOff(Driver);
            SharedServiceClass.WaitForAjaxReady(Driver);
            //IWebElement finishWizard = SharedServiceClass.ElementToBeClickable(Driver, By.Id("finish-wizard"));//Step 5
            //finishWizard.Click();
            By finishWizardAssert = By.Id("");
            switch (origin) {
                case "BrokerFillLead":
                    finishWizardAssert =By.Id("AddNewCustomer");
                    break;
                case "ClientSignup":
                    finishWizardAssert = By.XPath("//button[@ui-event-control-id='profile:request-processing-continue-popup-nodecision']");
                    break;
            }
            actionBot.ClickAssert(By.Id("finish-wizard"), finishWizardAssert, logHeader);

            if (String.Equals("ClientSignup", origin)) {
                //IWebElement continueToAccount = SharedServiceClass.ElementToBeClickable(Driver, By.XPath("//button[@ui-event-control-id='profile:request-processing-continue-popup-nodecision']"));
                //continueToAccount.Click();
                actionBot.Click(By.XPath("//button[@ui-event-control-id='profile:request-processing-continue-popup-nodecision']"), logHeader);
            }
            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }
    }
}
