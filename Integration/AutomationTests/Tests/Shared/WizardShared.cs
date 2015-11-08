namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Resources;
    using System.Threading;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Support.Events;
    using OpenQA.Selenium.Support.Extensions;
    using OpenQA.Selenium.Support.UI;
    using TestStack.Seleno.Extensions;

    class WizardShared {

        private IWebDriver _Driver;
        private ResourceManager _EnvironmentConfig;
        private ResourceManager _BrandConfig;

        public WizardShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig) {
            this._Driver = Driver;
            this._EnvironmentConfig = EnvironmentConfig;
            this._BrandConfig = BrandConfig;
        }

        //This procedure is to replace OrangeMoney/Wizard test case 'C3'
        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepOne(bool isLogIn,
            string origin,
            string emailAdress,
             string password,
             int secretQuestion,
             string secAnswer,
             string reqAmmount) {

            if (isLogIn) {
                string url = this._BrandConfig.GetString("WizardHost");//Step 1

                this._Driver.Navigate().GoToUrl(url);//Step 2
            }

            IWebElement email = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("Email"));//Step 3
            email.SendKeys(emailAdress);

            IWebElement signupPass1 = this._Driver.FindElement(By.Id("signupPass1"));//Step 4
            signupPass1.SendKeys(password);

            IWebElement signupPass2 = this._Driver.FindElement(By.Id("signupPass2"));//Step 5
            signupPass2.SendKeys(password);

            SelectElement secrertQuestion = new SelectElement(this._Driver.FindElement(By.Id("securityQuestion")));//Step 6
            secrertQuestion.SelectByIndex(secretQuestion);

            IWebElement securityAnswer = this._Driver.FindElement(By.Id("SecurityAnswer"));//Step 7
            securityAnswer.SendKeys(secAnswer);

            //IWebElement amount = this._Driver.FindElement(By.Id("amount"));
            //amount.SendKeys(reqAmmount);

            if (String.Equals(origin, "ClientSignup")) {//This code is ilrelevant in case accessed from Broker-lead-fill

                IWebElement mobilePhone = this._Driver.FindElement(By.Id("mobilePhone"));//Step 8
                mobilePhone.SendKeys("01111111111");

                IWebElement generateMobileCode = this._Driver.FindElement(By.Id("generateMobileCode"));//Step 9
                generateMobileCode.Click();

                IWebElement mobileCode = this._Driver.FindElement(By.Id("mobileCode"));//Step 10
                mobileCode.SendKeys("222222");
            }

            IWebElement signupSubmitButton = this._Driver.FindElement(By.Id("signupSubmitButton"));//Step 11
            signupSubmitButton.Click();
        }

        //This procedure is to replace OrangeMoney/Wizard test case 'C1380'
        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        public void PerformWizardStepTwo(string origin,
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

            IWebElement firstName = SharedServiceClass.ElementIsVisible(this._Driver, By.Id("FirstName"));//Step 1
            firstName.SendKeys(personName);

            IWebElement surname = this._Driver.FindElement(By.Id("Surname"));//Step 2
            surname.SendKeys(personSurename);

            IWebElement formRadioCtrl;//Step 3
            switch (char.ToUpper(gender)) {
                case 'F':
                    formRadioCtrl = this._Driver.FindElement(By.XPath("//label[@for='FormRadioCtrl_F']"));
                    break;
                default:
                    formRadioCtrl = this._Driver.FindElement(By.XPath("//label[@for='FormRadioCtrl_M']"));
                    break;
            }
            formRadioCtrl.Click();

            SelectElement dateOfBirthDay = new SelectElement(this._Driver.FindElement(By.Id("DateOfBirthDay")));//Step 4
            dateOfBirthDay.SelectByValue(dobDay);

            SelectElement dateOfBirthMonth = new SelectElement(this._Driver.FindElement(By.Id("DateOfBirthMonth")));
            dateOfBirthMonth.SelectByText(dobMonth);

            SelectElement dateOfBirthYear = new SelectElement(this._Driver.FindElement(By.Id("DateOfBirthYear")));
            dateOfBirthYear.SelectByValue(dobYear);

            SelectElement maritalStatus = new SelectElement(this._Driver.FindElement(By.Id("MaritalStatus")));//Step 5
            maritalStatus.SelectByValue(marStatus);

            IWebElement addAddressInput = this._Driver.FindElement(By.CssSelector("input.addAddressInput"));//Step 6
            addAddressInput.SendKeys(postCode);

            IWebElement addAddressButton = this._Driver.FindElement(By.CssSelector("input.addAddress"));//Step 7
            addAddressButton.Click();

            IWebElement matchingAddressList = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("ul.matchingAddressList > li"));//Step 8
            matchingAddressList.Click();

            IWebElement postCodeBtnOk = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("button.postCodeBtnOk"));//Step 9
            postCodeBtnOk.Click();

            SelectElement timeAtAddress = new SelectElement(this._Driver.FindElement(By.Id("TimeAtAddress")));//Step 10
            timeAtAddress.SelectByValue(addressTime);

            SelectElement propertyStatus = new SelectElement(this._Driver.FindElement(By.Id("PropertyStatus")));//Step 11
            propertyStatus.SelectByValue(resStatus);

            if (String.Equals(origin, "BrokerFillLead")) {

                IWebElement mobilePhone = this._Driver.FindElement(By.Id("MobilePhone")); //This code is only implemented in case accessed from Broker-lead-fill
                mobilePhone.SendKeys(phone);
            }

            IWebElement dayTimePhone = this._Driver.FindElement(By.Id("DayTimePhone"));//Step 12
            dayTimePhone.SendKeys(phone2);

            if (agreeTerms) {//Step 14
                IWebElement consentToSearch = this._Driver.FindElement(By.XPath("//label[@for='ConsentToSearch']"));
                consentToSearch.Click();
            }

            IWebElement personInfoContinueBtn = this._Driver.FindElement(By.Id("personInfoContinueBtn"));//Step 15
            personInfoContinueBtn.Click();
        }

        //This procedure is to replace OrangeMoney/Wizard test case 'C91'
        public void PerformWizardStepThree(string businessType, bool isSmallBusiness, string indType, string revenue) {

            SelectElement typeOfBusiness = SharedServiceClass.SelectIsVisible(this._Driver, By.Id("TypeOfBusiness"));//Step 1
            typeOfBusiness.SelectByValue(businessType);

            //if (isSmallBusiness) {//Removed in new wizard verssion
            //    IWebElement bussinesSquare = this._Driver.FindElement(By.CssSelector("i.fa.fa-square-o"));
            //    bussinesSquare.Click();
            //}

            SelectElement industryType = new SelectElement(this._Driver.FindElement(By.Id("IndustryType")));//Step 2
            industryType.SelectByValue(indType);

            IWebElement overallTurnOver = this._Driver.FindElement(By.Id("OverallTurnOver"));//Step 3
            overallTurnOver.SendKeys(revenue);

            IWebElement companyContinueBtn = this._Driver.FindElement(By.Id("companyContinueBtn"));//Step 4
            companyContinueBtn.Click();

            try {
                IWebElement targets = SharedServiceClass.ElementToBeClickable(this._Driver, By.CssSelector("div.ui-dialog > div.ui-dialog-content > ul.targets > li"), 25);
                SharedServiceClass.WaitForBlockUiOff(this._Driver);
                targets.Click();

                IWebElement button = this._Driver.FindElement(By.CssSelector("button.button.btn-green.btnTargetOk.ev-btn-org"));
                button.Click();
            } catch { }
        }

        //origin - BrokerFillLead: when broker fills lead's wizard; ClientSignup: when accessing from main wizard page.
        //This procedure is to replace OrangeMoney/Wizard test case 'C778' - Steps 1-4 have been replaced.
        public void PerformWizardStepFour(string origin,
            string marketplace,
            string accLogin,
            string loginVal,
            string accPass,
            string passVal,
            string accBtn) {

            //SharedServiceClass.WaitForAjaxReady(this._Driver);
            IWebElement showMore = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("link_account_see_more_less"));
            //SharedServiceClass.ScrollIntoView(this._Driver, showMore);
            SharedServiceClass.WaitForBlockUiOff(this._Driver);
            showMore.Click();

            IWebElement marketplaceButton = SharedServiceClass.ElementIsVisible(this._Driver, By.CssSelector(marketplace));
            marketplaceButton.Click();

            if (String.Equals("a.marketplace-button-account-paypal", marketplace)) {

                IWebElement paypalContinueBtn = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("paypalContinueBtn"));
                paypalContinueBtn.Click();

                this._Driver.SwitchTo().Window(SharedServiceClass.LastWindowName(this._Driver, 2));

                SharedServiceClass.WebAddressContains(this._Driver, "webscr?cmd", 20);
            }

            IWebElement loginField = this._Driver.FindElement(By.Id(accLogin));
            loginField.SendKeys(loginVal);

            IWebElement passwordField = this._Driver.FindElement(By.Id(accPass));
            passwordField.SendKeys(passVal);

            IWebElement linkAccounts = this._Driver.FindElement(By.Id(accBtn));
            linkAccounts.Click();

            if (String.Equals("a.marketplace-button-account-paypal", marketplace)) {
                IWebElement grantPermission = SharedServiceClass.ElementToBeClickable(this._Driver, By.Name("grant.x"));
                grantPermission.Click();

                this._Driver.SwitchTo().Window(SharedServiceClass.LastWindowName(this._Driver, 1));
            }

            SharedServiceClass.WaitForBlockUiOff(this._Driver);
            IWebElement finishWizard = SharedServiceClass.ElementToBeClickable(this._Driver, By.Id("finish-wizard"));//Step 5
            finishWizard.Click();
        }
    }
}
