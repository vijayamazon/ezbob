namespace UIAutomationTests.Tests.Application.Broker {
    using System;
    using Ezbob.Database;
    using NUnit.Framework;
    using UIAutomationTests.Core;
    using UIAutomationTests.Tests.Shared;
    using OpenQA.Selenium;
    using System.Collections.Generic;
    using System.Threading;
    using log4net;
    using TestRailModels.Automation;

    //using System.ComponentModel;

    class BrokerTests : WebTestBase {
        private static readonly ILog log = LogManager.GetLogger(typeof(BrokerTests));
        #region Application: Broker / Broker Wizard

        [Test]
        [Category("1202")]
        public void TestCase1202() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                //Steps 1-15
                brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Verification agains DB.
                SharedDBClass dbAccess = new SharedDBClass(EnvironmentConfig);
                SafeReader SR = dbAccess.oDB.GetFirst("UIAT_BrokerRegistrationValidation", CommandSpecies.StoredProcedure, new QueryParameter[] { new QueryParameter("brokerMail", brokerMail) });

                actionBot.WriteToLog("Begin assert: Firm name in DB match inserted value.");
                Assert.AreEqual("SomeCompany", (string)SR["FirmName"]);
                actionBot.WriteToLog("Positively asserted." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Broker name in DB match inserted value.");
                Assert.AreEqual("BrokerName", (string)SR["ContactName"]);
                actionBot.WriteToLog("Positively asserted." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Broker's e-mail address in DB match inserted value.");
                Assert.AreEqual(brokerMail, (string)SR["ContactEmail"]);
                actionBot.WriteToLog("Positively asserted." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Broker's mobile phone number in DB match inserted value.");
                Assert.AreEqual("01111111111", (string)SR["ContactMobile"]);
                actionBot.WriteToLog("Positively asserted." + Environment.NewLine);

                //actionBot.WriteToLog("Begin assert: Monthly application count in DB match inserted value.");
                //Assert.AreEqual(123, (int)SR["EstimatedMonthlyApplicationCount"]);
                //actionBot.WriteToLog("Positively asserted." + Environment.NewLine);

                //actionBot.WriteToLog("Begin assert: Monthly client amount in DB match inserted value.");
                //Assert.AreEqual(123.0m, (decimal)SR["EstimatedMonthlyClientAmount"]);
                //actionBot.WriteToLog("Positively asserted." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Customer origin match value inserted to DB.");
                Assert.AreEqual(BrandConfig.GetString("Brand_url").Split('.')[1], (string)SR["OriginName"]);
                actionBot.WriteToLog("Positively asserted." + Environment.NewLine);

                //Log off broker to finish session.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("7969")]
        public void TestCase7969() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2962")]
        public void TestCase2962() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1966")]
        public void TestCase1966() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2036")]
        public void TestCase2036() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                //Step 1 - Navigate to broker's sign up page.
                string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("Brand_url"), IsRunLocal, BrandConfig.GetString("BrokerSignupHost"));
                Driver.Navigate().GoToUrl(url);
                actionBot.WriteToLog("Nevigate to url: " + url);

                //Step 3 - Click the terms and conditions link.
                actionBot.Click(By.XPath("//label[@for='AgreeToTerms']/a"), "(agree to terms link button)");
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Terms and conditions dialog shuld be displayed.");
                SharedServiceClass.ElementIsClickable(Driver, By.CssSelector("div.ui-helper-clearfix > div.ui-dialog-buttonset > button"));
                actionBot.WriteToLog("Positively asserted: Terms and conditions dialog is displayed." + Environment.NewLine);

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("2037")]
        public void TestCase2037() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    //Precondition 3 - Navigate to broker's sign up page.
                string url = String.Concat(EnvironmentConfig.GetString("ENV_address"), BrandConfig.GetString("Brand_url"), IsRunLocal, BrandConfig.GetString("BrokerSignupHost"));
                    Driver.Navigate().GoToUrl(url);
                    actionBot.WriteToLog("Nevigate to url: " + url);

                //Step 3 - Click the privacy policy link.
                actionBot.Click(By.XPath("//label[@for='AgreeToPrivacyPolicy']/a"), "(privacy policy link button)");
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.SwitchToWindow(2, "(privacy policy window)");
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Verify web address contains the sub-string: 'privacy-and'");
                SharedServiceClass.WebAddressContains(Driver, "privacy-and", 20);
                actionBot.WriteToLog("Positively asserted: Web address contains the sub-string." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Privacy policy is opened in a new tab.");
                Assert.AreEqual(SharedServiceClass.ElementIsVisible(Driver, By.CssSelector(BrandConfig.GetString("C2037_Privacy_selector"))).Text, BrandConfig.GetString("C2037_Privacy_content"));
                actionBot.WriteToLog("Positively asserted: Privacy policy opened as expected." + Environment.NewLine);

                Driver.Close();
                actionBot.SwitchToWindow(1, "(back to Ezbob/Everline application window)");
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        #endregion
        #region Application: Broker / Lead Wizard

        [Test]
        [Category("1351")]
        public void TestCase1351() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Steps 3-7 - Add a new client to broker and click 'Send'.
                string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadSendInvitation"));

                actionBot.WriteToLog("Begin assert: New client is added to the client list with status 'Application not started'.");
                Assert.AreEqual("Application not started", SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("tr.odd > td.grid-item-Status")).Text);
                actionBot.WriteToLog("Positively asserted: Client with correct status exists." + Environment.NewLine);

                //Log off broker to finish session.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("1967")]
        public void TestCase1967() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Steps 3-7 - Add a new client to broker and click 'Send'.
                string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadSendInvitation"));

                //Step 8 - Log out of the broker account.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                //Step 9 - Navigate to inbox.
                GmailAPI.GmailOps gmailApi = new GmailAPI.GmailOps();

                actionBot.WriteToLog("Begin assert: Verify e-mail 'broker lead invitation' exists in e-mail.");
                Assert.IsTrue(gmailApi.CheckIncomingMessages(BrandConfig.GetString("Check_Incoming_Messages"), leadMail));
                actionBot.WriteToLog("Positively asserted: e-mail 'broker lead invitation' was found in inbox." + Environment.NewLine);

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("6032")]
        public void TestCase6032() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Steps 3-7 - Add a new client to broker and click 'Send'.
                string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadSendInvitation"));

                //Step 8 - Log out of the broker account.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                //Step 9 - Navigate to inbox.
                actionBot.WriteToLog("Begin assert: Using Gmail API to extract wizard link from sent e-mail.");
                GmailAPI.GmailOps gmailApi = new GmailAPI.GmailOps();
                string sentLink = gmailApi.ExtactLinkFromMessage(BrandConfig.GetString("Check_Incoming_Messages"), leadMail, String.Concat(EnvironmentConfig.GetString("ENV_name"), BrandConfig.GetString("Brand_url")));
                actionBot.WriteToLog("Positively asserted: Wizard link sussessfully extracted from e-mail." + Environment.NewLine);

                //Step 10 - Navigate on extracted link.
                Driver.Navigate().GoToUrl(sentLink);
                actionBot.WriteToLog("Nevigate to url: " + sentLink + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Verify e-mail address in wizard is: " + leadMail);
                Assert.AreEqual(SharedServiceClass.ElementIsVisible(Driver, By.Id("Email")).GetAttribute("value"), leadMail);
                actionBot.WriteToLog("Positively asserted: e-mail addresses matched." + Environment.NewLine);

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("1352")]
        public void TestCase1352() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Steps 3-7 - Add a new client to broker.
                string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                actionBot.WriteToLog("Begin assert: Verify e-mail address in wizard is: " + leadMail);
                Assert.AreEqual(SharedServiceClass.ElementIsVisible(Driver, By.Id("Email")).GetAttribute("value"), leadMail);
                actionBot.WriteToLog("Positively asserted: e-mail addresses matched." + Environment.NewLine);

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("3110")]
        public void TestCase3110() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition - Prepare a registered broker account.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                    //Precondition 1 - Prepare a registered broker account with at least one lead: C1352
                    string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                actionBot.WriteToLog("Begin assert: Verify e-mail address in wizard is: " + leadMail);
                Assert.AreEqual(SharedServiceClass.ElementIsVisible(Driver, By.Id("Email")).GetAttribute("value"), leadMail);
                actionBot.WriteToLog("Positively asserted: e-mail addresses matched." + Environment.NewLine);

                //Step 1 - Click "Finish later" button.
                //actionBot.ClickAssert(By.CssSelector("div.broker-finish-inner > div.button-container > button.button.btn-green.clean-btn"), By.CssSelector("tr.odd > td.grid-item-Status"), "(wizard finish later button)");
                actionBot.Click(By.CssSelector("div.broker-finish-inner > div.button-container > button.button.btn-green.clean-btn"), "(wizard finish later button)");
                actionBot.WriteToLog(Environment.NewLine);
               
                SharedServiceClass.WaitForAjaxReady(Driver);
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("Begin assert: New client is added to the client list with status 'Application not started'.");
                Assert.AreEqual("Application not started", SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("tr.odd > td.grid-item-Status")).Text);
                actionBot.WriteToLog("Positively asserted: Client with correct status exists." + Environment.NewLine);

                //Log off broker to finish session.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("1353")]
        public void TestCase1353() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Step 3-7 - Add a new client: Fill.
                string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);

                //Step 8 - Complete customer's Wizard step 1: C3
                wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "BrokerFillLead", leadMail, "123123", 2, "asd", "1000");

                //Step 9 - Complete customer's Wizard step 2: C1380
                wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "BrokerFillLead", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "6", "1111111111", "2222222222", true);

                //Step 10 - Complete customer's Wizard step 3: C91
                wizardShared.PerformWizardStepThree(logHeader + " - PerformWizardStepThree", "Entrepreneur", "15", "123");

                //Step 11 - Complete customer's Wizard step 4: C788 - TODO: replaced by EKM account, file upload problem needs to be resolved.
                //Wizard Step 4 - add EKM account
                wizardShared.PerformWizardStepFour(logHeader + " - PerformWizardStepFour", "BrokerFillLead", By.CssSelector("a.marketplace-button-account-EKM"), By.Id("ekm_login"), "ezbob", By.Id("ekm_password"), "ezekmshop2013", By.Id("ekm_link_account_button"));

                SharedServiceClass.WaitForAjaxReady(Driver);
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("Begin assert: 'Customer has been created' notification is displayed.");
                Assert.IsTrue(SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.notification_green > div.innerText > span.alert-msg")).Text.Contains("has been created."));
                actionBot.WriteToLog("Positively asserted: Correct notificztion has been displayed." + Environment.NewLine);

                //Log off broker to finish session.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("7472")]
        public void TestCase7472() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Step 3-7 - Add a new client: Fill, e-mail according to format: C4520
                string leadMail = "test+lead_" + DateTime.Now.Ticks + "+bds-afg@ezbob.com";
                brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);

                //Step 8 - Complete customer's Wizard step 1: C3
                wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "BrokerFillLead", leadMail, "123123", 2, "asd", "1000");

                //Step 9 - Complete customer's Wizard step 2: C26
                wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "BrokerFillLead", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "2", "1111111111", "2222222222", true);

                //Step 10 - Complete customer's Wizard step 3: C91
                wizardShared.PerformWizardStepThree(logHeader + " - PerformWizardStepThree", "Entrepreneur", "15", "123");

                //Step 11-12 - Complete customer's Wizard step 4: C4537 (Link minimum required marketplace's for approval: C4530)
                //Wizard Step 4 - add paypal account
                wizardShared.PerformWizardStepFourPayPal(logHeader + " - PerformWizardStepFour", "BrokerFillLead","liat@ibai.co.il", "1q2w3e4r");

                SharedServiceClass.WaitForAjaxReady(Driver);
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("Begin assert: 'Customer has been created' notification is displayed.");
                Assert.IsTrue(SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.notification_green > div.innerText > span.alert-msg")).Text.Contains("has been created."));
                actionBot.WriteToLog("Positively asserted: Correct notificztion has been displayed." + Environment.NewLine);

                //Log off broker to finish session.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("7473")]
        public void TestCase7473() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Step 3-7 - Add a new client: Fill, e-mail according to format: C4544
                string leadMail = "test+lead_" + DateTime.Now.Ticks + "+bds-afg@ezbob.com";
                brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);

                //Step 8 - Complete customer's Wizard step 1: C3
                wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "BrokerFillLead", leadMail, "123123", 2, "asd", "1000");

                //Step 9 - Complete customer's Wizard step 2: C1380
                wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "BrokerFillLead", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "6", "1111111111", "2222222222", true);

                //Step 10 - Complete customer's Wizard step 3: C91
                wizardShared.PerformWizardStepThree(logHeader + " - PerformWizardStepThree", "Entrepreneur", "15", "123");

                //Step 11-12 - Complete customer's Wizard step 4: C4537 (Link minimum required marketplace's for approval: C4530)
                //Wizard Step 4 - add paypal account
                wizardShared.PerformWizardStepFourPayPal(logHeader + " - PerformWizardStepFour", "BrokerFillLead", "liat@ibai.co.il", "1q2w3e4r");

                SharedServiceClass.WaitForAjaxReady(Driver);
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("Begin assert: 'Customer has been created' notification is displayed.");
                Assert.IsTrue(SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.notification_green > div.innerText > span.alert-msg")).Text.Contains("has been created."));
                actionBot.WriteToLog("Positively asserted: Correct notificztion has been displayed." + Environment.NewLine);

                //Log off broker to finish session.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("3109")]
        public void TestCase3109() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                    //Precondition 2 - Add a new client: Fill: C1352.
                    string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);

                //Step 1 - Complete customer's Wizard step 1: C3
                wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "BrokerFillLead", leadMail, "123123", 2, "asd", "1000");

                //Step 2 - Complete customer's Wizard step 2: C1380
                wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "BrokerFillLead", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "6", "1111111111", "2222222222", true);
                
                SharedServiceClass.WaitForAjaxReady(Driver);
                actionBot.WriteToLog(Environment.NewLine);

                //Step 3 - Click "Finish later" button.
                //actionBot.ClickAssert(By.CssSelector("div.broker-finish-inner > div.button-container > button.button.btn-green.clean-btn"),By.CssSelector("tr.odd > td.grid-item-Status"), "(wizard finish later button)");
                actionBot.Click(By.CssSelector("div.broker-finish-inner > div.button-container > button.button.btn-green.clean-btn"), "(wizard finish later button)");
                actionBot.WriteToLog(Environment.NewLine);

                actionBot.WriteToLog("Begin assert: New client is added to the client list with status 'Personal details'.");
                Assert.AreEqual("Personal Details", SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("tr.odd > td.grid-item-Status")).Text);
                actionBot.WriteToLog("Positively asserted: Client with correct status exists." + Environment.NewLine);

                //Log off broker to finish session.
                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        #endregion
        #region Application: Broker / Broker Dashboard / Marketing materials
        [Test]
        [Category("2040")]
         public void TestCase2040() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2041")]
        public void TestCase2041() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2042")]
        public void TestCase2042() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Broker Dashboard / Terms and conditions
        [Test]
        [Category("2043")]
        public void TestCase2043() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2039")]
        public void TestCase2039() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Broker Dashboard / Widget

        [Test]
        [Category("1354")]
        public void TestCase1354() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 1 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                    //Precondition 2 - Broker account is linked to approved customer: C7473.

                    //Insert email according to manual decision backdoor format: C4544.
                    string leadMail = "test+lead_" + DateTime.Now.Ticks + "+bds-afg@ezbob.com";
                    brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                    WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);

                    //Complete customer's Wizard step 1: C3
                    wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "BrokerFillLead", leadMail, "123123", 2, "asd", "1000");

                    //Complete customer's Wizard step 2: C1380
                    wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "BrokerFillLead", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "6", "1111111111", "2222222222", true);

                    //Complete customer's Wizard step 3: C91
                    wizardShared.PerformWizardStepThree(logHeader + " - PerformWizardStepThree", "Entrepreneur", "15", "123");

                    //Complete customer's Wizard step 4: C4537 (Link minimum required marketplace's for approval: C4530)
                    //Wizard Step 4 - add paypal account
                    wizardShared.PerformWizardStepFourPayPal(logHeader + " - PerformWizardStepFour", "BrokerFillLead", "liat@ibai.co.il", "1q2w3e4r");

                //Step 2 - Commission widget display the following as disabled.
                //Step 2.1 - Commission: Unlinked.
                actionBot.WriteToLog("Begin assert: Commission field is empty.");
                IWebElement widget = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.not_linked_bank > div.d-widgets > div.d-widget"));
                Assert.AreEqual("---", widget.FindElement(By.CssSelector("dd.dashes")).Text);
                actionBot.WriteToLog("Positively asserted: Commission field is empty." + Environment.NewLine);

                //Step 2.2 - Approved: Linked broker entitled for full commission.
                actionBot.WriteToLog("Begin assert: Approved ammount  field is empty.");
                Assert.IsTrue(decimal.Parse(widget.FindElement(By.CssSelector("dd.broker-approved")).Text.Substring(1)) == 0.0m);
                actionBot.WriteToLog("Positively asserted: Approved ammount  field is empty." + Environment.NewLine);

                brokerShared.BrokerLogOff(logHeader + "BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);

            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("1355")]
        public void TestCase1355() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 2 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                    //Precondition 3 - Broker account is linked to approved customer: C7472.

                    //Step 3-7 - Add a new client: Fill, mail according to format: C4520
                    string leadMail = "test+lead_" + DateTime.Now.Ticks + "+bds-afg@ezbob.com";
                    brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                    WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Complete customer's Wizard step 1: C3
                    wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "BrokerFillLead", leadMail, "123123", 2, "asd", "1000");

                    //Complete customer's Wizard step 2: C26
                    wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "BrokerFillLead", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "2", "1111111111", "2222222222", true);

                    //Complete customer's Wizard step 3: C91
                    wizardShared.PerformWizardStepThree(logHeader + " - PerformWizardStepThree", "Entrepreneur", "15", "123");

                    //Complete customer's Wizard step 4: C4537 (Link minimum required marketplace's for approval: C4530)
                    //Wizard Step 4 - add paypal account
                    wizardShared.PerformWizardStepFourPayPal(logHeader + " - PerformWizardStepFour", "BrokerFillLead", "liat@ibai.co.il", "1q2w3e4r");

                    //Log off broker, log into customer, take loan and log off customer.
                    brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");
                    CustomerShared customerShared = new CustomerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    customerShared.CustomerLogIn(logHeader + " - CustomerLogIn", true, leadMail);
                    customerShared.CustomerTakeLoan(logHeader + " - CustomerTakeLoan", "LeadFName", "LeadLName", "00000000", "00", "00", "00", 'P', "CardHolderName", "Visa", "4111111111111111", DateTime.UtcNow.AddYears(1).ToString("MM/yy"), "111");
                    customerShared.CustomerLogOff(logHeader + " - CustomerLogOff");

                //Step 1 - Browse to broker dashboard.
                brokerShared.BrokerLogIn(logHeader + " - BrokerLogIn", brokerMail);

                //Step 2 - Commission widget display the following as disabled.
                //Step 2.1 - Commission: Unlinked.
                actionBot.WriteToLog("Begin assert: Commission field is empty.");
                IWebElement widget = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.not_linked_bank > div.d-widgets > div.d-widget"));
                Assert.AreEqual("---", widget.FindElement(By.CssSelector("dd.dashes")).Text);
                actionBot.WriteToLog("Positively asserted: Commission field is empty." + Environment.NewLine);

                //Step 2.2 - Approved: Linked broker entitled for full commission.
                actionBot.WriteToLog("Begin assert: Approved ammount  field is not empty.");
                Assert.IsTrue(decimal.Parse(widget.FindElement(By.CssSelector("dd.broker-approved")).Text.Substring(1)) != 0.0m);
                actionBot.WriteToLog("Positively asserted: Approved ammount  field is not empty." + Environment.NewLine);

                brokerShared.BrokerLogOff(logHeader + "BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }


        [Test]
        [Category("1357")]
        public void TestCase1357() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 2 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                //Step 2 - Commission widget display the following as disabled.
                //Step 2.1 - Commission: Unlinked.
                actionBot.WriteToLog("Begin assert: Commission field is unlinked.");
                IWebElement widget = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.not_linked_bank > div.d-widgets > div.d-widget"));
                Assert.AreEqual("---", widget.FindElement(By.CssSelector("dd.dashes")).Text);
                actionBot.WriteToLog("Positively asserted: Commission field is unlinked." + Environment.NewLine);

                //Step 2.2 - Button: Link bank account (Enabled).
                actionBot.WriteToLog("Begin assert: Link bank account button is enabled.");
                SharedServiceClass.ElementIsClickable(Driver, By.CssSelector("button.button.btn-green.pull-right.btn-wide.add-bank.ev-btn-org"));
                actionBot.WriteToLog("Positively asserted: Link bank account button is enabled." + Environment.NewLine);

                //Step 3 - Prepare a valid bank account number and sort code.
                brokerShared.BrokerAddBankAccount(logHeader + " - BrokerAddBankAccount", "20115636", "62", "10", "00", 'P');

                //Step 4 (Assert) - Widget displayed in linked state.
                actionBot.WriteToLog("Begin assert: Widget displayed in linked state.");
                IWebElement widgetLinkedAccount = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.linked_bank > div.d-widgets > div.d-widget"));
                Assert.IsTrue(decimal.Parse(widgetLinkedAccount.FindElement(By.CssSelector("dd.broker-commission")).Text.Substring(1)) == 0.0m);
                actionBot.WriteToLog("Positively asserted: Widget displayed in linked state." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Approved ammount  field equals zero.");
                Assert.IsTrue(decimal.Parse(widgetLinkedAccount.FindElement(By.CssSelector("dd.broker-approved")).Text.Substring(1)) == 0.0m);
                actionBot.WriteToLog("Positively asserted: Approved ammount  field equals zero." + Environment.NewLine);

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("1358")]
        public void TestCase1358() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1359")]
        public void TestCase1359() {
            bool result = this.ExecuteTest((logHeader) => {
                if (!EnvironmentConfig.BaseName.Equals("UIAutomationTests.configs.Enviorment.QA") && !EnvironmentConfig.BaseName.Equals("UIAutomationTests.configs.Enviorment.Dev")) {
                    log.Error(logHeader + " -  This test can only run in QA or Dev enviorment due to changes it makes to configuration tables in DB.");
                    Assert.IsTrue(false);
                }

                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                    BrokerShared brokerShared = new BrokerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    //Precondition 2 - Prepare a registered broker account: C1202.
                    string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                    brokerShared.CreateNewBrokerAccount(logHeader + " - CreateNewBrokerAccount", "SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456");

                    //Precondition 3 - Broker account is linked to customer: C1353.
                    string leadMail = "test+lead_" + DateTime.Now.Ticks + "+bds-afg@ezbob.com";
                    brokerShared.BrokerLeadEnrolment(logHeader + " - BrokerLeadEnrolment", "LeadFName", "LeadLName", leadMail, By.Id("LeadFillWizard"));

                    WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);

                    //Complete customer's Wizard step 1: C3
                    wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "BrokerFillLead", leadMail, "123123", 2, "asd", "1000");

                    //Complete customer's Wizard step 2: C1380
                    wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "BrokerFillLead", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "6", "1111111111", "2222222222", true);

                    //Complete customer's Wizard step 3: C91
                    wizardShared.PerformWizardStepThree(logHeader + " - PerformWizardStepThree", "Entrepreneur", "15", "123");

                    //Complete customer's Wizard step 4: C788 - TODO: replaced by EKM account, file upload problem needs to be resolved.
                    //Wizard Step 4 - add EKM account
                    wizardShared.PerformWizardStepFour(logHeader + " - PerformWizardStepFour", "BrokerFillLead", By.CssSelector("a.marketplace-button-account-EKM"), By.Id("ekm_login"), "ezbob", By.Id("ekm_password"), "ezekmshop2013", By.Id("ekm_link_account_button"));

                    //Bank account linked: C1357
                    brokerShared.BrokerAddBankAccount(logHeader + " - BrokerAddBankAccount", "20115636", "62", "10", "00", 'P');

                    //Log off broker, log into customer, take loan and log off customer.
                    brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");
                    CustomerShared customerShared = new CustomerShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                    customerShared.CustomerLogIn(logHeader + " - CustomerLogIn", true, leadMail);
                    customerShared.CustomerTakeLoan(logHeader + " - CustomerTakeLoan", "LeadFName", "LeadLName", "00000000", "00", "00", "00", 'P', "CardHolderName", "Visa", "4111111111111111", DateTime.UtcNow.AddYears(1).ToString("MM/yy"), "111");
                    customerShared.CustomerLogOff(logHeader + " - CustomerLogOff");

                //Step 1 - Browse to broker dashboard.
                brokerShared.BrokerLogIn(logHeader + " - BrokerLogIn", brokerMail);

                //Log into DB. Execute query to run Pacnet, and then query to extract Pacnet validation status.
                actionBot.WriteToLog("(Begin DB query)");
                SharedDBClass dbAccess = new SharedDBClass(EnvironmentConfig);
                dbAccess.oDB.ExecuteNonQuery("UIAT_InitiatePacnetPaymentViaContrab", CommandSpecies.StoredProcedure);
                //Thread.Sleep awaits for DB SP to finish execution.
                actionBot.Sleep(75000);
                SafeReader SR = dbAccess.oDB.GetFirst("UIAT_PacnetStatusValidation", CommandSpecies.StoredProcedure, new QueryParameter[] { new QueryParameter("brokerMail", brokerMail) });
                actionBot.WriteToLog("(End DB query)");

                //Step 2 - Commission widget display the following as disabled.
                //Step 2.1 - Commission: Linked.
                actionBot.WriteToLog("Begin assert: Commission field is not empty. And DB value compares to to UIs.");
                IWebElement widget = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.linked_bank > div.d-widgets > div.d-widget"));
                decimal comission = decimal.Parse(widget.FindElement(By.CssSelector("dd.broker-commission")).Text.Substring(1));
                decimal issued = decimal.Parse(widget.FindElement(By.CssSelector("dd.broker-approved")).Text.Substring(1));
                Assert.IsTrue((comission != 0.0m) && (comission == (decimal)SR["CommissionAmount"]));
                actionBot.WriteToLog("Positively asserted: Commission field is not empty. And DB value compares to to UIs." + Environment.NewLine);

                //Step 2.2 - Approved: Linked broker entitled for full commission.
                actionBot.WriteToLog("Begin assert: Approved ammount  field is not empty. And DB value compares to to UIs.");
                Assert.IsTrue((issued != 0.0m) && (issued == (decimal)SR["LoanAmount"]));
                actionBot.WriteToLog("Positively asserted: Approved ammount  field is not empty. And DB value compares to to UIs." + Environment.NewLine);

                //Step 3 - Compare ui results to DB.
                actionBot.WriteToLog("Begin assert: CardInfoID is not null in DB.");
                Assert.IsTrue((decimal?)SR["CardInfoID"] != null);
                actionBot.WriteToLog("Positively asserted: CardInfoID is not null in DB." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: PaidDate is not null in DB.");
                Assert.IsTrue((DateTime?)SR["PaidDate"] != null);
                actionBot.WriteToLog("Positively asserted: PaidDate is not null in DB." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: TrackingNumber is not null in DB.");
                Assert.IsTrue((decimal?)SR["TrackingNumber"] != null);
                actionBot.WriteToLog("Positively asserted: TrackingNumber is not null in DB." + Environment.NewLine);

                actionBot.WriteToLog("Begin assert: Status is not null in DB.");
                Assert.IsTrue(string.IsNullOrEmpty((string)SR["Status"]) == false);
                actionBot.WriteToLog("Positively asserted: Status is not null in DB." + Environment.NewLine);

                brokerShared.BrokerLogOff(logHeader + " - BrokerLogOff");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("1360")]
        public void TestCase1360() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        #endregion
        #region Application: Broker / Broker Dashboard / Log In
        [Test]
        [Category("2032")]
        public void TestCase2032() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2033")]
        public void TestCase2033() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2034")]
        public void TestCase2034() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Commission / Fee: Automatic Payments

        [Test]
        [Category("1364")]
        public void TestCase1364() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1376")]
        public void TestCase1376() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1362")]
        public void TestCase1362() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1361")]
        public void TestCase1361() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1365")]
        public void TestCase1365() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false); }); Assert.IsTrue(result); }

        [Test]
        [Category("1367")]
        public void TestCase1367() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1363")]
        public void TestCase1363() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1368")]
        public void TestCase1368() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4190")]
        public void TestCase4190() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4191")]
        public void TestCase4191() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4541")]
        public void TestCase4541() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Commission / Fee: Default values

        [Test]
        [Category("1441")]
        public void TestCase1441() {
            bool result = this.ExecuteTest((logHeader) => {
                DateTime testStartTime = DateTime.UtcNow;
                actionBot.WriteToLog("Begin test: " + logHeader + Environment.NewLine);

                WizardShared wizardShared = new WizardShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                string customerMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";

                //Precondition 1 - Prepare a registered non broker account with no loan: C4543
                //Complete customer's Wizard step 1: C3
                wizardShared.PerformWizardStepOne(logHeader + " - PerformWizardStepOne", "ClientSignup", customerMail, "123123", 2, "asd", "1000");

                //Complete customer's Wizard step 2: C26
                wizardShared.PerformWizardStepTwo(logHeader + " - PerformWizardStepTwo", "ClientSignup", "LeadFName", "LeadLName", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "2", "1111111111", "2222222222", true);

                //Complete customer's Wizard step 3: C91
                wizardShared.PerformWizardStepThree(logHeader + " - PerformWizardStepThree", "Entrepreneur", "15", "123");

                //Complete customer's Wizard step 4: C788 - TODO: replaced by EKM account, file upload problem needs to be resolved.
                //Wizard Step 4 - add EKM account
                wizardShared.PerformWizardStepFour(logHeader + " - PerformWizardStepFour", "BrokerFillLead", By.CssSelector("a.marketplace-button-account-EKM"), By.Id("ekm_login"), "ezbob", By.Id("ekm_password"), "ezekmshop2013", By.Id("ekm_link_account_button"));


                //UnderWriterShared newUW = new UnderWriterShared(Driver, EnvironmentConfig, BrandConfig, actionBot);
                //newUW.LogIn("admin", "123456");
                //newUW.FindCustomer("test+client_635862269583123148@ezbob.com");

                actionBot.WriteToLog("End test: " + logHeader + ". Test duration: " + ((TimeSpan)(DateTime.UtcNow - testStartTime)).ToString(@"hh\:mm\:ss") + Environment.NewLine);
            });
            Assert.IsTrue(result);
        }

        [Test]
        [Category("1369")]
        public void TestCase1369() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1370")]
        public void TestCase1370() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1384")]
        public void TestCase1384() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1385")]
        public void TestCase1385() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1373")]
        public void TestCase1373() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1372")]
        public void TestCase1372() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1374")]
        public void TestCase1374() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1375")]
        public void TestCase1375() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Commission / Fee: Value manual overrid
        [Test]
        [Category("1377")]
        public void TestCase1377() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1378")]
        public void TestCase1378() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1389")]
        public void TestCase1389() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1379")]
        public void TestCase1379() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("3211")]
        public void TestCase3211() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1381")]
        public void TestCase1381() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Commission / Fee: Automatic invoice email / Condition for invoice
        [Test]
        [Category("4180")]
        public void TestCase4180() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4181")]
        public void TestCase4181() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4182")]
        public void TestCase4182() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4185")]
        public void TestCase4185() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4218")]
        public void TestCase4218() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4220")]
        public void TestCase4220() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Commission / Fee: Automatic invoice email / Invoice content
        [Test]
        [Category("4183")]
        public void TestCase4183() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4184")]
        public void TestCase4184() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4187")]
        public void TestCase4187() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4222")]
        public void TestCase4222() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4221")]
        public void TestCase4221() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4223")]
        public void TestCase4223() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4224")]
        public void TestCase4224() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Update Broker Lead
        [Test]
        [Category("1687")]
        public void TestCase1687() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1688")]
        public void TestCase1688() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1689")]
        public void TestCase1689() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("1690")]
        public void TestCase1690() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Lead/Client details / Personal details
        [Test]
        [Category("2394")]
        public void TestCase2394() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2396")]
        public void TestCase2396() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2395")]
        public void TestCase2395() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2398")]
        public void TestCase2398() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2397")]
        public void TestCase2397() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2423")]
        public void TestCase2423() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2983")]
        public void TestCase2983() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Lead/Client details / Director details
        [Test]
        [Category("2412")]
        public void TestCase2412() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2413")]
        public void TestCase2413() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2414")]
        public void TestCase2414() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2530")]
        public void TestCase2530() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2415")]
        public void TestCase2415() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2420")]
        public void TestCase2420() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2529")]
        public void TestCase2529() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2957")]
        public void TestCase2957() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Lead/Client details / Client files
        [Test]
        [Category("2416")]
        public void TestCase2416() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2417")]
        public void TestCase2417() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2418")]
        public void TestCase2418() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2419")]
        public void TestCase2419() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2541")]
        public void TestCase2541() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2955")]
        public void TestCase2955() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2956")]
        public void TestCase2956() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Lead/Client details / CRM events
        [Test]
        [Category("2422")]
        public void TestCase2422() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2421")]
        public void TestCase2421() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("3150")]
        public void TestCase3150() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2954")]
        public void TestCase2954() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2958")]
        public void TestCase2958() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
        #region Application: Broker / Lead/Client details / Button display
        [Test]
        [Category("3106")]
        public void TestCase3106() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2537")]
        public void TestCase2537() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("2393")]
        public void TestCase2393() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }

        [Test]
        [Category("4225")]
        public void TestCase4225() { bool result = this.ExecuteTest((logHeader) => { Assert.IsTrue(false);}); Assert.IsTrue(result); }
        #endregion
    }
}
