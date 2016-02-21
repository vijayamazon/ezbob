namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using UIAutomationTests.Core;

    class WizardShared : WebTestBase {
        private readonly object Locker; // TODO to be removed

        public WizardShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig, ActionBot actionBot) {
            this.Driver = Driver;
            this.EnvironmentConfig = EnvironmentConfig;
            this.BrandConfig = BrandConfig;
            this.actionBot = actionBot;
            this.Locker = new object();
        }

        //This procedure follows C3.
        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepOne(
            string logHeader,
            string origin,
            string emailAdress,
            string password,
            int secretQuestion,
            string secAnswer,
            string reqAmmount
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            if (string.Equals("ClientSignup", origin)) {
                string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("Brand_url"), IsRunLocal, BrandConfig.GetString("WizardHost"));

                //Step 2 - Browse to OM app.
                Driver.Navigate().GoToUrl(url);
                actionBot.WriteToLog("Nevigate to url: " + url);

                //Step 3 - Insert a valid email address to the Email address field and focus out.
                actionBot.SendKeys(By.Id("Email"), emailAdress, "(valid email address)");
            }

            if (string.Equals("BrokerFillLead", origin)) {
                actionBot.WriteToLog("Begin assert: Verify e-mail address in wizard is: " + emailAdress);
                Assert.AreEqual(SharedServiceClass.ElementIsVisible(Driver, By.Id("Email")).GetAttribute("value"), emailAdress);
                actionBot.WriteToLog("Positively asserted: e-mail addresses matched.");
            }

            //Step 4 - Insert a valid password to the password field and focus out.
            actionBot.SendKeys(By.Id("signupPass1"), password, "(password field)");

            //Step 5 - Insert the same password to the confirm password field and focus out.
            actionBot.SendKeys(By.Id("signupPass2"), password, "(confirm password)");

            //Step 6 - Select secret question from the list.
            actionBot.SelectByIndex(By.Id("securityQuestion"), secretQuestion, "(secret question select)");

            //Step 7 - Insert answer in the Secret answer field and focus out.
            actionBot.SendKeys(By.Id("SecurityAnswer"), secAnswer, "(secret answer field)");

            //IWebElement amount = Driver.FindElement(By.Id("amount"));
            //amount.SendKeys(reqAmmount);

            if (String.Equals(origin, "ClientSignup")) {//This code is ilrelevant in case accessed from Broker-lead-fill

                //Step 8 - Insert number in the Mobile phone field and focus out.
                actionBot.SendKeys(By.Id("mobilePhone"), "1111111111", "(contact person mobile phone)");

                //Step 9 - Click send activation code.
                actionBot.Click(By.Id("generateMobileCode"), "(generate mobile code button)");

                //Step 10 - Insert the authentication code. Workaround is: 222222
                actionBot.SendKeys(By.Id("mobileCode"), "222222", "(valid mobile code)");
            }

            //Step 11 - Click continue.
            actionBot.Click(By.Id("signupSubmitButton"), "(continue button)");

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //This procedure follows C26.
        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepTwo(
             string logHeader,
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
             bool agreeTerms
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForAjaxReady(Driver);
            SharedServiceClass.WaitForBlockUiOff(Driver);

            //TODO: remove a-sync locks. must be a workaround this problem.
            if (string.Equals("ClientSignup", origin)) {
                //Step 1 - Insert first name and focus out.
                lock (this.Locker)
                    actionBot.SendKeys(By.Id("FirstName"), personName, "(first name field)");

                lock (this.Locker)
                    actionBot.SendKeys(By.Id("Surname"), personSurename, "(surname field)");
            }

            if (string.Equals("BrokerFillLead", origin)) {
                actionBot.WriteToLog("Begin assert: Verify first name in wizard is: " + personName);
                Assert.AreEqual(SharedServiceClass.ElementIsVisible(Driver, By.Id("FirstName")).GetAttribute("value"), personName);
                actionBot.WriteToLog("Positively asserted: first names matched.");

                actionBot.WriteToLog("Begin assert: Verify sure name in wizard is: " + personSurename);
                Assert.AreEqual(SharedServiceClass.ElementIsVisible(Driver, By.Id("Surname")).GetAttribute("value"), personSurename);
                actionBot.WriteToLog("Positively asserted: sure names matched.");
            }

            //Step 3 - Select gender and focus out.
            By formRadioCtrl;
            switch (char.ToUpper(gender)) {
                case 'F':
                    formRadioCtrl = By.XPath("//label[@for='FormRadioCtrl_F']");
                    break;
                default:
                    formRadioCtrl = By.XPath("//label[@for='FormRadioCtrl_M']");
                    break;
            }
            actionBot.MoveToElement(formRadioCtrl);
            actionBot.Click(formRadioCtrl, "(gender select button)");

            //Step 4 - 	Select date of birth and focus out.
            actionBot.MoveToElement(By.Id("DateOfBirthDay"));
            actionBot.SelectByValue(By.Id("DateOfBirthDay"), dobDay, "(date of birth - day select)");

            actionBot.SelectByText(By.Id("DateOfBirthMonth"), dobMonth, "(date of birth - month select)");

            actionBot.SelectByValue(By.Id("DateOfBirthYear"), dobYear, "(date of birth - year select)");

            //Step 5 - Select Marital status other and focus out.
            actionBot.SelectByValue(By.Id("MaritalStatus"), marStatus, "(marital status select)");

            //Step 6 - Insert post code.
            actionBot.SendKeys(By.CssSelector("input.addAddressInput"), postCode, "(post code field)");

            //Step 7 - Click Postcode lookup.
            actionBot.Click(By.CssSelector("input.addAddress"), "(postcode lookup button)");

            //Step 8 - Click the correct address.
            actionBot.Click(By.CssSelector("ul.matchingAddressList > li"), "(sellect address from list)");

            //Step 9 - Click OK.
            actionBot.Click(By.CssSelector("button.postCodeBtnOk"), "(address OK button)");

            //Step 10 - In the How long at this address field, select relevant property and focus out.
            actionBot.SelectByValue(By.Id("TimeAtAddress"), addressTime, "(how long at this address select)");

            //Step 11 - In the Residential status field, select relevant property and focus out.
            actionBot.SelectByValue(By.Id("PropertyStatus"), resStatus, "(residential status select)");

            //Step 12 - Insert valid format phone number in the Other contact number field and focus out.
            if (String.Equals(origin, "BrokerFillLead")) {
                actionBot.SendKeys(By.Id("MobilePhone"), phone, "(valid format mobile phone number)");
            }

            actionBot.SendKeys(By.Id("DayTimePhone"), phone2, "(valid format day time phone number)");

            //Step 13 - Check the TOS checkbox.
            if (agreeTerms)
                actionBot.Click(By.XPath("//label[@for='ConsentToSearch']"), "(terms and conditions checkBox)");

            //Step 14 - Click continue.
            actionBot.Click(By.Id("personInfoContinueBtn"), "(continue button)");

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //This procedure follows C91.
        public void PerformWizardStepThree(
            string logHeader,
            string businessType,
            string indType,
            string revenue
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForAjaxReady(Driver);

            //Step 1 - In the Type of Business field, select relevant property and focus out.
            actionBot.SelectByValue(By.Id("TypeOfBusiness"), businessType, "(type of business select)");

            //Step 2 - In the Type of Industry field, select relevant property and focus out.
            actionBot.SelectByValue(By.Id("IndustryType"), indType, "(type of industry select)");

            //Step 3 - Insert any amount to the Total annual revenue field and focus out.
            actionBot.SendKeys(By.Id("OverallTurnOver"), revenue, "(total annual revenue field)");

            //actionBot.MoveToElement(By.Id("companyContinueBtn"));
            actionBot.MoveToBottom();
            //Step 4 - Click continue.
            //actionBot.Click(By.Id("companyContinueBtn"), "(continue button)");
            actionBot.JqueryClick("#companyContinueBtn", "(continue button)");

            //TODO: find out when this dialog is displayed, and create more acurate scenario for it.
            try {
                SharedServiceClass.WaitForBlockUiOff(Driver);

                //Click the correct address.
                actionBot.Click(By.CssSelector("div.ui-dialog > div.ui-dialog-content > ul.targets > li"), "(sellect address from list)", 25);

                //Click OK.
                actionBot.Click(By.CssSelector("button.button.btn-green.btnTargetOk.ev-btn-org"), "(address OK button)");
            } catch { }

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //This procedure follows C4530. (PayPal sometimes replaced by other data sources - C4537)
        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepFour(
            string logHeader,
            string origin,
            By marketplace,
            By accLogin,
            string loginVal,
            By accPass,
            string passVal,
            By accBtn
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForBlockUiOff(Driver);

            actionBot.MoveToBottom();
            actionBot.Click(By.Id("link_account_see_more_less"), "(see full data source button)");

            //Step 1 - Click on relevant data source.
            //By marketplacedAssert;
            //switch (marketplace) {
            //    case "a.marketplace-button-account-paypal":
            //        marketplacedAssert = By.Id("paypalContinueBtn");
            //        break;
            //    default:
            //        marketplacedAssert = By.Id(accLogin);
            //        break;
            //}
            //actionBot.ClickAssert(By.CssSelector(marketplace), marketplacedAssert, "(data source button)");
            
            actionBot.Click(marketplace, "(data source button)");

            //Step 3 - Insert prepared credentials and click sign in.
            actionBot.SendKeys(accLogin, loginVal, "(data source login field)", 20);

            actionBot.SendKeys(accPass, passVal, "(data source password field)");

            actionBot.Click(accBtn, "(data source login button)");

            SharedServiceClass.WaitForAjaxReady(Driver);

            //Step 4 - Click complete.
            //By finishWizardAssert = By.Id("");
            //switch (origin) {
            //    case "BrokerFillLead":
            //        finishWizardAssert = By.Id("AddNewCustomer");
            //        break;
            //    case "ClientSignup":
            //        finishWizardAssert = By.XPath("//button[@ui-event-control-id='profile:request-processing-continue-popup-nodecision']");
            //        break;
            //}
            //actionBot.ClickAssert(By.Id("finish-wizard"), finishWizardAssert, "(complete button)");
            actionBot.Click(By.Id("finish-wizard"), "(complete button)");

            //Step 5 - Accept dialog.
            if (String.Equals("ClientSignup", origin)) {
                actionBot.Click(By.XPath("//button[@ui-event-control-id='profile:request-processing-continue-popup-nodecision']"), "(accept dialog button)");
            }

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepFourPayPal(
            string logHeader,
            string origin,
            string loginVal,
            string passVal
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForBlockUiOff(Driver);

            actionBot.Click(By.Id("link_account_see_more_less"), "(see full data source button)");
            SharedServiceClass.JqueryElementReady(Driver, ".marketplace-button-account-paypal");
            //Step 1 - Click on relevant data source.

            actionBot.Click(By.ClassName("marketplace-button-account-paypal"), "(data source button)");
            //actionBot.Click(By.CssSelector("a.marketplace-button-account-paypal"), "(data source button)");

            //PayPal Click continue.
            actionBot.Click(By.Id("paypalContinueBtn"), "(continue to PayPal button)");

            //Move focus to the pop-uped window.
            actionBot.SwitchToWindow(2, "(PayPal add account window)");

            //Verify the pop-up address is correct.
            actionBot.WriteToLog("Begin assert: Verify web address contains the sub-string: 'webscr'");
            SharedServiceClass.WebAddressContains(Driver, "webscr", 20);
            actionBot.WriteToLog("Positively asserted: Web address contains the sub-string.");

            //Step 3 - Insert prepared credentials and click sign in.
            try {
                actionBot.SendKeys(By.Id("login_email"), loginVal, "(data source login field)", 20);

                actionBot.SendKeys(By.Id("login_password"), passVal, "(data source password field)");

                actionBot.Click(By.Id("login.x"), "(data source login button)");
            } catch (Exception e) {
                actionBot.WriteToLog("Paypal is already in session. Moving to grant premission page automaticaly.");
            }

            actionBot.Click(By.Name("grant.x"), "(PayPal continue to Ezbob/Everline button)");

            //Move focus back to Ezbob/Everline window.
            actionBot.SwitchToWindow(1, "(back to Ezbob/Everline application window)");

            SharedServiceClass.WaitForAjaxReady(Driver);

            //Step 4 - Click complete.
            actionBot.Sleep(5000);
            actionBot.Click(By.Id("finish-wizard"), "(complete button)");

            //Step 5 - Accept dialog.
            if (String.Equals("ClientSignup", origin)) {
                actionBot.Click(By.XPath("//button[@ui-event-control-id='profile:request-processing-continue-popup-nodecision']"), "(accept dialog button)");
            }

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }
    }
}
