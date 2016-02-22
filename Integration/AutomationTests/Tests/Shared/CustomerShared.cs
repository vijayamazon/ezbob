namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Resources;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using UIAutomationTests.Core;

    class CustomerShared : WebTestBase {

        public CustomerShared(IWebDriver Driver, ResourceManager EnvironmentConfig, ResourceManager BrandConfig, ActionBot actionBot) {
            this.Driver = Driver;
            this.EnvironmentConfig = EnvironmentConfig;
            this.BrandConfig = BrandConfig;
            this.actionBot = actionBot;
        }

        public void CustomerLogIn(
            string logHeader,
            bool isFirstTime,
            string brokerMail
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);
            SharedServiceClass.WaitForAjaxReady(Driver);
            string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("Brand_url"), IsRunLocal, BrandConfig.GetString("CustomerLogIn"));
            Driver.Navigate().GoToUrl(url);
            actionBot.WriteToLog("Nevigate to url: " + url);

            //Insert register customer's e-mail to the user name field.
            actionBot.SendKeys(By.Id("UserName"), brokerMail, "(user name field)");

            //Insert password to the password field. Default: '123456'
            actionBot.SendKeys(By.Id("Password"), "123123", "(password field)");

            //Click on submit button.
            actionBot.Click(By.Id("loginSubmit"), "(login button)");

            if (isFirstTime)
                actionBot.Click(By.CssSelector("div.automation-popup-content > div.alignright > button.button"), "(automation - accept terms button)");

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        //Precondition: be loggedin to customer.
        public void CustomerTakeLoan(
            string logHeader,
            string fName,
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
            string securityCode,
            double? loanFraction = null
            ) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForAjaxReady(Driver);

            //Click on the take loan button.
            //actionBot.ClickAssert(By.CssSelector("button.button.btn-green.get-cash.ev-btn-org"), By.XPath("//label[@for='preAgreementTermsRead']"), "(take loan button)");
            actionBot.Click(By.CssSelector("button.button.btn-green.get-cash.ev-btn-org"), "(take loan button)");

            if (loanFraction != null) {
                //Click on slider to vary the loan ammount.
                IWebElement rangeSelector = SharedServiceClass.ElementIsClickable(Driver, By.CssSelector("div.ui-slider-range.ui-widget-header.ui-slider-range-min"));//Driver.FindElement(By.CssSelector("div.ui-slider-range.ui-widget-header.ui-slider-range-min")));
                int clickCoordinate = (int)(rangeSelector.Size.Width * loanFraction);
                Actions moveAction = new Actions(Driver);
                moveAction.MoveToElement(rangeSelector, clickCoordinate, 0).Click().Build().Perform();
                actionBot.WriteToLog("Click was performed on loan ammount slider at " + (loanFraction*100).ToString() + "%.");
            }

            //Click on the accept pre-agreement terms button.
            //actionBot.Click(By.XPath("//label[@for='preAgreementTermsRead']"), "(pre-agreement terms button)");

            //Click on the contract terms read check box.
            //actionBot.Click(By.XPath("//label[@for='agreementTermsRead']"), "(contract terms read checkBox)");

            //Click on confirm the solvency representations and warranties.
            actionBot.Click(By.XPath("//label[@for='notInBankruptcy']"), "(solvency representations and warranties checkBox)");

            SharedServiceClass.WaitForBlockUiOff(Driver);

            //Confirn disclosures.
            actionBot.Click(By.CssSelector("button.ok-button.button.btn-green.ev-btn-org"), "(confirn disclosures button)");

            //Signe the agreement.
            actionBot.SendKeys(By.Id("signedName"), fName + " " + lName, "(signature field)");

            //Click next to accept the agreements.
            actionBot.Click(By.CssSelector("form.LoanLegal a.btn-continue.button.btn-green.ev-btn-org.submit"), "(accept agreement button)");

            //Insert account number sort codes.
            actionBot.SendKeys(By.Id("AccountNumber"), accountNum, "(account number field)");

            actionBot.SendKeys(By.Id("SortCode1"), sort1, "(sort code - part 1)");

            actionBot.SendKeys(By.Id("SortCode2"), sort2, "(sort code - part 2)");

            actionBot.SendKeys(By.Id("SortCode3"), sort3, "(sort code - part 3)");

            By accTypeRadio;
            string accTypeComment;
            switch (char.ToUpper(accType)) {
                case 'B':
                    accTypeRadio = By.XPath("//label[@for='baBusiness']");
                    accTypeComment = "(account type radioButton set to Bussines)";
                    break;
                default:
                    accTypeRadio = By.XPath("//label[@for='baPersonal']");
                    accTypeComment = "(account type radioButton set to Personal)";
                    break;
            }
            actionBot.Click(accTypeRadio, accTypeComment);

            //Click on connect bank continue button.
            actionBot.Click(By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"), "(connect bank continue button - first click)");

            SharedServiceClass.WaitForBlockUiOff(Driver);

            //Click on connect bank continue button. Second click. - Second click is to activate the backdoor.
            actionBot.Click(By.CssSelector("a.button.btn-green.connect-bank.ev-btn-org"), "(connect bank continue button - second click)");

            //Name of card holder.
            actionBot.SendKeys(By.Id("customer"), cardHolderName, "(card holder name field)");

            //Card type.
            actionBot.SelectByValue(By.CssSelector("select.selectheight.form_field"), cardType, "(card type select)");

            //Card number.
            actionBot.SendKeys(By.Id("card_no"), cardNumber, "(card number field)");

            //Card expire date.
            actionBot.SendKeys(By.Id("expiry"), expDate, "(card expire date field)");

            //Card CV2 code
            actionBot.SendKeys(By.Id("cv2"), securityCode, "(card CV2 code field)");

            //Submit debit card details
            actionBot.Click(By.Id("paypoint-submit"), "(submit debit card details button)");

            //Continue to customer's profile
            actionBot.Click(By.Id("pacnet-status-back-to-profile"), "(continue to customer's profile button)");

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }

        public void CustomerLogOff(string logHeader) {
            actionBot.WriteToLog("Begin method: " + logHeader);

            SharedServiceClass.WaitForBlockUiOff(Driver);

            //Click log-off
            actionBot.Click(By.CssSelector("li.login > a"), "(customer log-off button)");

            actionBot.WriteToLog("End method: " + logHeader + Environment.NewLine);
        }
    }
}
