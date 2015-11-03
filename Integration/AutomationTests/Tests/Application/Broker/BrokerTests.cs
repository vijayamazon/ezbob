 ﻿namespace UIAutomationTests.Tests.Application.Broker {
      using System;
      using System.Threading;
      using NUnit.Framework;
      using UIAutomationTests.Core;
      using UIAutomationTests.Tests.Shared;
      using OpenQA.Selenium;
      using OpenQA.Selenium.Firefox;
      using OpenQA.Selenium.Interactions;
      using OpenQA.Selenium.Support.UI;

 ﻿    class BrokerTests : WebTestBase {

          [Test]
          [Category("1202")]
          public void TestCase1202() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account
                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("2962")]
          public void TestCase2962() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1966")]
          public void TestCase1966() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2036")]
          public void TestCase2036() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2037")]
          public void TestCase2037() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          #region Application: Broker / Lead Wizard

          [Test]
          [Category("1351")]
          public void TestCase1351() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account

                  newBroker.BrokerLogIn(brokerMail);//Step 1-2

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);//Steps 3-6

                  IWebElement leadSendInvitation = Driver.FindElement(By.Id("LeadSendInvitation"));//Step 7
                  leadSendInvitation.Click();

                  IWebElement statusList = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("tr.odd > td.grid-item-Status"));

                  Assert.AreEqual(statusList.Text, "Application not started");//Verify that lead status is "Application not started"

                  IWebElement logOff = Driver.FindElement(By.CssSelector("li.menu-btn.login.log-off > a"));
                  logOff.Click();
                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("1967")]
          public void TestCase1967() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account

                  newBroker.BrokerLogIn(brokerMail);//Step 1-2

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);//Steps 3-6

                  IWebElement leadSendInvitation = Driver.FindElement(By.Id("LeadSendInvitation"));//Step 7
                  leadSendInvitation.Click();

                  IWebElement statusList = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("tr.odd > td.grid-item-Status"));
                  Assert.AreEqual(statusList.Text, "Application not started");//Verify that lead status is "Application not started".

                  IWebElement logOff = Driver.FindElement(By.CssSelector("li.menu-btn.login.log-off > a"));
                  logOff.Click();

                  GmailAPI.GmailOps newApi = new GmailAPI.GmailOps(); //Step 9
                  Assert.IsTrue(newApi.CheckIncomingMessages(BrandConfig.GetString("Check_Incoming_Messages"), leadMail));
                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("6032")]
          public void TestCase6032() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account

                  newBroker.BrokerLogIn(brokerMail);//Step 1-2

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);//Steps 3-6

                  IWebElement leadSendInvitation = Driver.FindElement(By.Id("LeadSendInvitation"));//Step 7
                  leadSendInvitation.Click();

                  SharedServiceClass.WaitForBlockUiOff(Driver);
                  //Driver.Manage().Cookies.DeleteAllCookies();
                  IWebElement logOff = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("li.menu-btn.login.log-off > a"));//Strep 8
                  logOff.Click();

                  GmailAPI.GmailOps newApi = new GmailAPI.GmailOps(); //Step 9 
                  string sentLink = newApi.ExtactLinkFromMessage(BrandConfig.GetString("Check_Incoming_Messages"), leadMail, BrandConfig.GetString("Enviorment_url"));
                  Driver.Navigate().GoToUrl(sentLink);

                  IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));
                  Assert.AreEqual(email.GetAttribute("Value"), leadMail);
                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("1352")]
          public void TestCase1352() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account

                  newBroker.BrokerLogIn(brokerMail);//Step 1-2

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);//Steps 3-6

                  IWebElement leadFillWizard = Driver.FindElement(By.Id("LeadFillWizard"));//Step 7
                  leadFillWizard.Click();

                  IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));
                  Assert.AreEqual(email.GetAttribute("value"), leadMail);//Verify that lead's email is displayed in the Email address field.
                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("3110")]
          public void TestCase3110() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);//Preconditions
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account

                  newBroker.BrokerLogIn(brokerMail);//Step 1

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);//Preconditions

                  IWebElement leadSendInvitation = Driver.FindElement(By.Id("LeadSendInvitation"));//Preconditions
                  leadSendInvitation.Click();

                  IWebElement statusList = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("tr.odd td.grid-item-FirstName a.profileLink"));//Step 3
                  statusList.Click();

                  IWebElement leadFillWizard = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("div.section-lead-details > div.back-to-list-container > button.lead-fill-wizard.button"));
                  leadFillWizard.Click();

                  IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));
                  Assert.AreEqual(email.GetAttribute("value"), leadMail);//Verify that lead's email is displayed in the Email address field.

                  IWebElement buttonContainer = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("div.button-container > button"));
                  buttonContainer.Click();

                  statusList = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("tr.odd td.grid-item-Status"));

                  Assert.AreEqual(statusList.Text, "Application not started");//Verify that we've returned to the Broker dashboard

                  IWebElement logOff = Driver.FindElement(By.CssSelector("li.menu-btn.login.log-off > a"));
                  logOff.Click();
                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("1353")]
          public void TestCase1353() {
              bool result = this.ExecuteTest<object>(() => {

                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account

                  newBroker.BrokerLogIn(brokerMail);//Step 1-2

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);//Steps 3-6

                  IWebElement leadFillWizard = Driver.FindElement(By.Id("LeadFillWizard"));//Step 7
                  leadFillWizard.Click();

                  IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));
                  Assert.AreEqual(email.GetAttribute("value"), leadMail);//Verify that lead's email is displayed in the Email address field.

                  WizardShared newWizard = new WizardShared(Driver, EnvironmentConfig, BrandConfig);

                  //string clientMail = "test+client_" + DateTime.Now.Ticks + "@ezbob.com";
                  newWizard.PerformWizardStepOne(false,"BrokerFillLead", "", "123123", 2, "asd", "1000");//Step 8 (C3)

                  newWizard.PerformWizardStepTwo("BrokerFillLead", "", "", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "5", "01111111111", "02222222222", true);//Step 9 (C1380)

                  newWizard.PerformWizardStepThree("Entrepreneur", false, "15", "123");//Step 10 (C91)

                  //Wizard Step 4 - add EKM account
                  newWizard.PerformWizardStepFour("BrokerFillLead", "a.marketplace-button-account-EKM", "ekm_login", "ezbob", "ekm_password", "ezekmshop2013", "ekm_link_account_button");//Step 11 (C788)

                  IWebElement logOff = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("li.menu-btn.login.log-off > a"));
                  logOff.Click();
                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("3109")]
          public void TestCase3109() {
              bool result = this.ExecuteTest<object>(() => {

                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);//Precondition - create new broker account

                  newBroker.BrokerLogIn(brokerMail);

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);

                  IWebElement leadFillWizard = SharedServiceClass.ElementIsVisible(Driver, By.Id("LeadFillWizard"));
                  leadFillWizard.Click();

                  IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));
                  Assert.AreEqual(email.GetAttribute("value"), leadMail);//Verify that lead's email is displayed in the Email address field.

                  WizardShared newWizard = new WizardShared(Driver, EnvironmentConfig, BrandConfig);//Preconditions

                  //string clientMail = "test+client_" + DateTime.Now.Ticks + "@ezbob.com";
                  newWizard.PerformWizardStepOne(false,"BrokerFillLead", "", "123123", 2, "asd", "1000");//Step 1 (C3)

                  newWizard.PerformWizardStepTwo("BrokerFillLead", "", "", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "5", "01111111111", "02222222222", true);//Step 2 (C1380)

                  SharedServiceClass.WaitForBlockUiOff(Driver);

                  IWebElement finishLater = SharedServiceClass.ElementIsVisible(Driver, By.Id("wizard"));//Step 3
                  finishLater.FindElement(By.CssSelector("button.button.btn-green.clean-btn")).Click();

                  IWebElement statusList = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("tr.odd > td.grid-item-Status"));
                  Assert.AreEqual("Personal Details", statusList.Text);

                  IWebElement logOff = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("li.menu-btn.login.log-off > a"));
                  logOff.Click();

                  return null;
              });
              Assert.IsTrue(result);
          }

          #endregion

          [Test]
          [Category("2040")]
          public void TestCase2040() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2041")]
          public void TestCase2041() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2042")]
          public void TestCase2042() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2043")]
          public void TestCase2043() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2039")]
          public void TestCase2039() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          #region Application: Broker / Broker Dashboard / Widget

          [Test]
          [Category("1354")]
          public void TestCase1354() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);

                  newBroker.BrokerLogIn(brokerMail);

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "_bds-afD10@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);

                  IWebElement leadFillWizard = SharedServiceClass.ElementToBeClickable(Driver, By.Id("LeadFillWizard"));
                  leadFillWizard.Click();

                  IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));
                  Assert.AreEqual(email.GetAttribute("value"), leadMail);//Verify that lead's email is displayed in the Email address field.

                  WizardShared newWizard = new WizardShared(Driver, EnvironmentConfig, BrandConfig);

                  //string clientMail = "test+client_" + DateTime.Now.Ticks + "@ezbob.com";
                  newWizard.PerformWizardStepOne(false,"BrokerFillLead", "", "123123", 2, "asd", "1000");

                  newWizard.PerformWizardStepTwo("BrokerFillLead", "", "", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "5", "01111111111", "02222222222", true);

                  newWizard.PerformWizardStepThree("Entrepreneur", false, "15", "123");

                  //Wizard Step 4 - add paypal account
                  newWizard.PerformWizardStepFour("BrokerFillLead", "a.marketplace-button-account-paypal", "login_email", "liat@ibai.co.il", "login_password", "1q2w3e4r", "login.x");//Preconditions. At the end of this step broker dashboard is displayed - Step 1.

                  IWebElement widget = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.not_linked_bank > div.d-widgets > div.d-widget"));//Step 2

                  IWebElement widgetComission = widget.FindElement(By.CssSelector("dd.dashes"));//Step 2.1
                  Assert.AreEqual("---", widgetComission.Text);

                  IWebElement widgetIssued = widget.FindElement(By.CssSelector("dd.broker-approved"));//Step 2.2
                  Assert.AreEqual("£0.00", widgetIssued.Text);

                  IWebElement logOff = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("li.menu-btn.login.log-off > a"));
                  logOff.Click();

                  return null;
              });
              Assert.IsTrue(result);
          }

          [Test]
          [Category("1355")]
          public void TestCase1355() {
              bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);

                  newBroker.BrokerLogIn(brokerMail);

                  string leadMail = "test+lead_" + DateTime.Now.Ticks + "+bds-afd10@ezbob.com";
                  newBroker.BrokerLeadEnrolment("LeadFName", "LeadLName", leadMail);

                  IWebElement leadFillWizard = SharedServiceClass.ElementToBeClickable(Driver, By.Id("LeadFillWizard"));
                  leadFillWizard.Click();

                  IWebElement email = SharedServiceClass.ElementIsVisible(Driver, By.Id("Email"));
                  Assert.AreEqual(email.GetAttribute("value"), leadMail);//Verify that lead's email is displayed in the Email address field.

                  WizardShared newWizard = new WizardShared(Driver, EnvironmentConfig, BrandConfig);

                  //string clientMail = "test+client_" + DateTime.Now.Ticks + "@ezbob.com";
                  newWizard.PerformWizardStepOne(false,"BrokerFilllead", "", "123123", 2, "asd", "1000");

                  newWizard.PerformWizardStepTwo("BrokerFilllead", "", "", 'M', "2", "Mar.", "1921", "Single", "ab101ba", "3", "5", "01111111111", "02222222222", true);

                  newWizard.PerformWizardStepThree("Entrepreneur", false, "15", "123");

                  //Wizard Step 4 - add paypal account
                  newWizard.PerformWizardStepFour("BrokerFilllead", "a.marketplace-button-account-paypal", "login_email", "liat@ibai.co.il", "login_password", "1q2w3e4r", "login.x");//Preconditions. At the end of this step broker dashboard is displayed - Step 1.

                  //SharedServiceClass.WaitForAjaxReady(Driver);
                  //Driver.Manage().Cookies.DeleteAllCookies();
                  IWebElement logOffBroker = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("li.menu-btn.login.log-off > a.button"));
                  SharedServiceClass.WaitForBlockUiOff(Driver);
                  logOffBroker.Click();

                  CustomerShared newCustomer = new CustomerShared(Driver, EnvironmentConfig, BrandConfig);
                  newCustomer.CustomerLogIn(true, leadMail);
                  newCustomer.CustomerTakeLoan("LeadFName", "LeadLName", "00000000", "00", "00", "00", 'P', "CardHolderName", "Visa", "4111111111111111", DateTime.UtcNow.AddYears(1).ToString("MM/yy"), "111");

                  //SharedServiceClass.WaitForAjaxReady(Driver);
                  //Driver.Manage().Cookies.DeleteAllCookies();
                  SharedServiceClass.WaitForBlockUiOff(Driver);
                  IWebElement logOffCustomer = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("li.login > a"));
                  logOffCustomer.Click();

                  newBroker.BrokerLogIn(brokerMail);//Step 1

                  IWebElement widget = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("div.not_linked_bank > div.d-widgets > div.d-widget"));//Step 2

                  IWebElement widgetComission = widget.FindElement(By.CssSelector("dd.dashes"));//Step 2.1
                  Assert.AreEqual("---", widgetComission.Text);

                  IWebElement widgetIssued = widget.FindElement(By.CssSelector("dd.broker-approved"));//Step 2.2
                  Assert.IsTrue(decimal.Parse(widgetIssued.Text.Substring(1)) != 0.0m);

                  IWebElement logOff = SharedServiceClass.ElementToBeClickable(Driver, By.CssSelector("li.menu-btn.login.log-off > a.button"));
                  logOff.Click();

                  return null;
              });
              Assert.IsTrue(result);
          }


 ﻿        [Test]
 ﻿        [Category("1357")]
 ﻿        public void TestCase1357() {
 ﻿            bool result = this.ExecuteTest<object>(() => {
                  BrokerShared newBroker = new BrokerShared(Driver, EnvironmentConfig, BrandConfig);
                  string brokerMail = "test+broker_" + DateTime.Now.Ticks + "@ezbob.com";
                  newBroker.CreateNewBrokerAccount("SomeCompany", "BrokerName", brokerMail, "01111111111", "222222", "123", "123", "123456", true, true);

                  newBroker.BrokerLogIn(brokerMail);//Step 1

                  IWebElement widget = Driver.FindElement(By.CssSelector("div.not_linked_bank > div.d-widgets > div.d-widget"));//Step 2

                  IWebElement widgetComission = widget.FindElement(By.CssSelector("dd.dashes"));//Step 2.1
                  Assert.AreEqual("---", widgetComission.Text);

                  IWebElement widgetIssued = widget.FindElement(By.CssSelector("dd.broker-approved"));//Step 2.2
                  Assert.AreEqual("£0.00", widgetIssued.Text);

 ﻿                newBroker.BrokerAddBankAccount("20115636", "62", "10", "00", 'P');//Step 3

                 IWebElement widgetLinkedAccount = SharedServiceClass.ElementIsVisible(Driver, By.CssSelector("div.linked_bank > div.d-widgets > div.d-widget"));//Step 4

                 Assert.AreEqual("£0.00", widgetLinkedAccount.FindElement(By.CssSelector("dd.broker-commission")).Text);//Step 4.1

                 Assert.AreEqual("£0.00", widgetLinkedAccount.FindElement(By.CssSelector("dd.broker-approved")).Text);//Step 4.2
                 
                 return null;
 ﻿            });
              Assert.IsTrue(result);
 ﻿        }

          [Test]
          [Category("1358")]
          public void TestCase1358() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1359")]
          public void TestCase1359() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1360")]
          public void TestCase1360() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          #endregion

          [Test]
          [Category("2032")]
          public void TestCase2032() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2033")]
          public void TestCase2033() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2034")]
          public void TestCase2034() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1364")]
          public void TestCase1364() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1376")]
          public void TestCase1376() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1362")]
          public void TestCase1362() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1361")]
          public void TestCase1361() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1365")]
          public void TestCase1365() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1367")]
          public void TestCase1367() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1363")]
          public void TestCase1363() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1368")]
          public void TestCase1368() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4190")]
          public void TestCase4190() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4191")]
          public void TestCase4191() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4541")]
          public void TestCase4541() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1441")]
          public void TestCase1441() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1369")]
          public void TestCase1369() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1370")]
          public void TestCase1370() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1384")]
          public void TestCase1384() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1385")]
          public void TestCase1385() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1373")]
          public void TestCase1373() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1372")]
          public void TestCase1372() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1374")]
          public void TestCase1374() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1375")]
          public void TestCase1375() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1377")]
          public void TestCase1377() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1378")]
          public void TestCase1378() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1389")]
          public void TestCase1389() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1379")]
          public void TestCase1379() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("3211")]
          public void TestCase3211() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1381")]
          public void TestCase1381() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4180")]
          public void TestCase4180() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4181")]
          public void TestCase4181() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4182")]
          public void TestCase4182() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4185")]
          public void TestCase4185() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4218")]
          public void TestCase4218() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4220")]
          public void TestCase4220() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4183")]
          public void TestCase4183() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4184")]
          public void TestCase4184() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4187")]
          public void TestCase4187() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4222")]
          public void TestCase4222() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4221")]
          public void TestCase4221() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4223")]
          public void TestCase4223() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4224")]
          public void TestCase4224() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1687")]
          public void TestCase1687() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1688")]
          public void TestCase1688() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1689")]
          public void TestCase1689() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("1690")]
          public void TestCase1690() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2394")]
          public void TestCase2394() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2396")]
          public void TestCase2396() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2395")]
          public void TestCase2395() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2398")]
          public void TestCase2398() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2397")]
          public void TestCase2397() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2423")]
          public void TestCase2423() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2983")]
          public void TestCase2983() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2412")]
          public void TestCase2412() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2413")]
          public void TestCase2413() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2414")]
          public void TestCase2414() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2530")]
          public void TestCase2530() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2415")]
          public void TestCase2415() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2420")]
          public void TestCase2420() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2529")]
          public void TestCase2529() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2957")]
          public void TestCase2957() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2416")]
          public void TestCase2416() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2417")]
          public void TestCase2417() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2418")]
          public void TestCase2418() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2419")]
          public void TestCase2419() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2541")]
          public void TestCase2541() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2955")]
          public void TestCase2955() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2956")]
          public void TestCase2956() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2422")]
          public void TestCase2422() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2421")]
          public void TestCase2421() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("3150")]
          public void TestCase3150() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2954")]
          public void TestCase2954() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2958")]
          public void TestCase2958() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("3106")]
          public void TestCase3106() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2537")]
          public void TestCase2537() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("2393")]
          public void TestCase2393() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

          [Test]
          [Category("4225")]
          public void TestCase4225() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result); }

      }
  }
