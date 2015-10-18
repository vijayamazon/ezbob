namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Resources;
    using System.Threading;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

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
        //origin - BrokerFillLeed: when broker fills leed's wizard; ClientSignup: when accessing from main wizard page.
        public void ImplementWizardStepOne(string origin,
            string emailAdress,
             string password,
             int secretQuestion,
             string secAnswer,
             string reqAmmount) { 
 
            //string url = this._BrandConfig.GetString("WizardHost");//Step 1
 
            //this._Driver.Navigate().GoToUrl(url);//Step 2
 
             IWebElement email = this._Driver.FindElement(By.Id("Email"));//Step 3
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
 
            if (String.Equals(origin, "ClientSignup")) {//This code is ilrelevant in case accessed from Broker-leed-fill
 
                IWebElement mobilePhone = this._Driver.FindElement(By.Id("mobilePhone")); //Step 8
                mobilePhone.SendKeys("01111111111");
 
                IWebElement generateMobileCode = this._Driver.FindElement(By.Id("generateMobileCode")); //Step 9
                generateMobileCode.Click();

                IWebElement mobileCode = this._Driver.FindElement(By.Id("mobileCode")); //Step 10
                mobileCode.SendKeys("222222");
            }
 
             IWebElement signupSubmitButton = this._Driver.FindElement(By.Id("signupSubmitButton"));//Step 11
             signupSubmitButton.Click();
 
             IWebElement assertion = this._Driver.FindElement(By.ClassName("editor-label-with-checkbox"));
             Assert.IsTrue(string.Equals(assertion.Text, "I have read and accept the Terms & Conditions."));//Assertion of wizard step 2
         }
 
         //This procedure is to replace OrangeMoney/Wizard test case 'C1380'
        //origin - BrokerFillLeed: when broker fills leed's wizard; ClientSignup: when accessing from main wizard page.
        public void ImplementWizardStepTwo(string origin,
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
 
             IWebElement firstName = this._Driver.FindElement(By.Id("FirstName"));//Step 1
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
 
             IWebElement addAddressInput = this._Driver.FindElement(By.ClassName("addAddressInput"));//Step 6
             addAddressInput.SendKeys(postCode);
 
             IWebElement addAddressButton = this._Driver.FindElement(By.ClassName("addAddress"));//Step 7
             addAddressButton.Click();
 
             Thread.Sleep(2000);
             IReadOnlyCollection<IWebElement> matchingAddressList = this._Driver.FindElement(By.ClassName("matchingAddressList")).FindElements(By.TagName("li"));//Step 8
             Assert.IsTrue(matchingAddressList.Any());
             matchingAddressList.First().Click();
 
             IWebElement postCodeBtnOk = this._Driver.FindElement(By.ClassName("postCodeBtnOk"));//Step 9
             postCodeBtnOk.Click();
 
             SelectElement timeAtAddress = new SelectElement(this._Driver.FindElement(By.Id("TimeAtAddress")));//Step 10
             timeAtAddress.SelectByValue(addressTime);
 
             SelectElement propertyStatus = new SelectElement(this._Driver.FindElement(By.Id("PropertyStatus")));//Step 11
             propertyStatus.SelectByValue(resStatus);
 
            if (String.Equals(origin, "BrokerFillLeed")) {

                IWebElement mobilePhone = this._Driver.FindElement(By.Id("MobilePhone")); //This code is only implemented in case accessed from Broker-leed-fill
                mobilePhone.SendKeys(phone);
            }

             IWebElement dayTimePhone = this._Driver.FindElement(By.Id("DayTimePhone"));//Step 12
            dayTimePhone.SendKeys(phone2);
 
             if (agreeTerms) {//Step 14
                 IWebElement consentToSearch = this._Driver.FindElement(By.XPath("//label[@for='ConsentToSearch']"));
                 consentToSearch.Click();
             }
 
             IWebElement continueButton = this._Driver.FindElement(By.LinkText("Next"));//Step 15
             continueButton.Click();
 
            //TODO: lack of id in HTML prevents assertion. Remove comment and preform assertion checks after commit/release.
            //Thread.Sleep(5000);
            //IWebElement assertion = this._Driver.FindElement(By.Id("page-rendered-bussines-information"));
            //Assert.IsTrue(string.Equals(assertion.Text, "Business details"));
         }
 
         //This procedure is to replace OrangeMoney/Wizard test case 'C91'
         public void ImplementWizardStepThree(string businessType, bool isSmallBusiness,string indType,string revenue) {
 
             SelectElement typeOfBusiness = new SelectElement(this._Driver.FindElement(By.Id("TypeOfBusiness")));//Step 1
             typeOfBusiness.SelectByValue(businessType);
 
             //if (isSmallBusiness) {//Removed in new wizard verssion
             //    IWebElement bussinesSquare = this._Driver.FindElement(By.CssSelector("i.fa.fa-square-o"));
             //    bussinesSquare.Click();
             //}
 
             SelectElement industryType = new SelectElement(this._Driver.FindElement(By.Id("IndustryType")));//Step 2
             industryType.SelectByValue(indType);
 
             IWebElement overallTurnOver = this._Driver.FindElement(By.Id("OverallTurnOver"));//Step 3
             overallTurnOver.SendKeys(revenue);
 
             IWebElement continueButton = this._Driver.FindElement(By.LinkText("Next"));//Step 4
             continueButton.Click();
 
             Thread.Sleep(25000);
             try {
                 if (this._Driver.FindElement(By.Id("ui-dialog-title-2")) != null && string.Equals(this._Driver.FindElement(By.Id("ui-dialog-title-2")).Text, "Select company")) {
                     IWebElement targets = this._Driver.FindElement(By.CssSelector("ul.targets > li"));
                     targets.Click();
 
                     IWebElement button = this._Driver.FindElement(By.CssSelector("button.button.btn-green.btnTargetOk.ev-btn-org"));
                     button.Click();
                 }
             } catch (Exception ex) {
             }
 
            Thread.Sleep(25000);
             Assert.AreEqual("We base our loan decisions on the business information you provide. Please let us have at least one of the following. The more you link or upload the better your chances of being accepted.", this._Driver.FindElement(By.CssSelector("div.entry_message.wizard_message")).Text);
         }
 
        //origin - BrokerFillLeed: when broker fills leed's wizard; ClientSignup: when accessing from main wizard page.
        //This procedure is to replace OrangeMoney/Wizard test case 'C778'
        public void ImplementWizardStepFour(string origin,
            string marketplace,
            string accLogin,
            string loginVal,
            string accPass,
            string passVal,
            string accBtn) {
 
             //IWebElement financialDocuments = this._Driver.FindElement(By.CssSelector("a.marketplace-button-account-CompanyFiles"));
             //financialDocuments.Click();
 
             //IWebElement DandD = this._Driver.FindElement(By.Id("companyFilesUploadZone"));
             //DandD.SendKeys(@"C:\Users\mishas\Desktop\permission.html");
             //DandD.Click();
 
            IWebElement showMore = this._Driver.FindElement(By.Id("link_account_see_more_less"));//Steps 1-4 where changed due to technical missfits
             showMore.Click();
 
            IWebElement marketplaceButton = this._Driver.FindElement(By.CssSelector(marketplace));
            marketplaceButton.Click();

            if (String.Equals("a.marketplace-button-account-paypal", marketplace)) {

                bool looper;
                do {
                    looper = false;
                    IWebElement paypalContinueBtn = this._Driver.FindElement(By.Id("paypalContinueBtn"));
                    paypalContinueBtn.Click();

                    Thread.Sleep(7000);
                    this._Driver.SwitchTo().Window(this._Driver.WindowHandles.Last());

                    if (this._Driver.Url.Contains("webscr?cmd") == false) {
                        looper = true;
                        this._Driver.Close();
                        Thread.Sleep(100);
                        this._Driver.SwitchTo().Window(this._Driver.WindowHandles.Last());
                    }
                } while (looper);
            }
 
             IWebElement loginField = this._Driver.FindElement(By.Id(accLogin));
             loginField.SendKeys(loginVal);
 
             IWebElement passwordField = this._Driver.FindElement(By.Id(accPass));
             passwordField.SendKeys(passVal);
 
            IWebElement linkAccounts = this._Driver.FindElement(By.Id(accBtn));
             linkAccounts.Click();
 
            if (String.Equals("a.marketplace-button-account-paypal", marketplace)) {
                IWebElement grantPermission= this._Driver.FindElement(By.Name("grant.x"));
                grantPermission.Click();

                Thread.Sleep(9000);
                this._Driver.SwitchTo().Window(this._Driver.WindowHandles.Last());
            }

             Thread.Sleep(2500);
             Assert.AreEqual("We base our loan decisions on the business information you provide. Please let us have at least one of the following. The more you link or upload the better your chances of being accepted.", this._Driver.FindElement(By.CssSelector("div.entry_message.wizard_message")).Text);
 
             IWebElement completeButton = this._Driver.FindElement(By.LinkText("Complete"));//Step 5
             completeButton.Click();
 
             Thread.Sleep(25000);
 
            if (String.Equals(origin, "BrokerFillLeed")) { //This code is relevant only if accessed from Broker-leed-fill
                Assert.AreEqual("Link your bank account and receive commission automatically.", this._Driver.FindElement(By.CssSelector("div.not_linked_bank > div.profile-headlines > h5.header-message")).Text);
            }

            if (String.Equals(origin, "ClientSignup")) { //This code is relevant only if accessed from Wizard/Signup
                Assert.AreEqual("Your business loan application is complete.", this._Driver.FindElement(By.CssSelector("div.automation-popup-content > div.automation-header"))
                    .Text); //Assertion
 
                IWebElement nextButton = this._Driver.FindElement(By.CssSelector("button.button.btn-green.pull-right.automation-button.ev-btn-org")); //Verify Dashboard is displayed
                nextButton.Click();

                Assert.AreEqual("We are currently processing your application.", this._Driver.FindElement(By.CssSelector("h2.header-message.hm_green"))
                    .Text); //Assertion
            }
         }
     }
 }
