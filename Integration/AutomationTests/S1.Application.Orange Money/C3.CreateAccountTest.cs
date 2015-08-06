﻿namespace UIAutomationTests.S1.Application.Orange_Money
{
    using System;
    using System.Threading;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    [TestFixture]
    public class CreateAccountTests {

        [Test]
        public void MustSucseedTest() {
            Assert.IsTrue( true);
        }

        [Test]
        public void MustFailTest()
        {
            Assert.IsTrue(false);
        }

        [Test]
        public void CreateAccountTest1() {
           
            //WebDriverWait wait = new WebDriverWait(BrowserHost.instance.Application.Browser, new TimeSpan(3000));
            //var cBoxOverlay = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Email")));
            
                Thread.Sleep(5000);

                var emailBox = BrowserHost.instance.Application.Browser.FindElement(By.Id("Email"));
                emailBox.SendKeys("dor+" + DateTime.Now.Ticks + "@ezbob.com");

                var passwordBox = BrowserHost.instance.Application.Browser.FindElement(By.Id("signupPass1"));
                passwordBox.SendKeys("dor2015");

                var confirmPasswordBox = BrowserHost.instance.Application.Browser.FindElement(By.Id("signupPass2"));
                confirmPasswordBox.SendKeys("dor2015");

                SelectElement secrertQuestion = new SelectElement(BrowserHost.instance.Application.Browser.FindElement(By.Id("securityQuestion")));
                secrertQuestion.SelectByIndex(1);

                var SecurityAnswer = BrowserHost.instance.Application.Browser.FindElement(By.Id("SecurityAnswer"));
                SecurityAnswer.SendKeys("AAA");

                var requestAmount = BrowserHost.instance.Application.Browser.FindElement(By.Id("amount"));
                requestAmount.SendKeys("1000");

                //var mobilePhone = BrowserHost.instance.Application.Browser.FindElement(By.Id("mobilePhone"));
                //mobilePhone.SendKeys("01111111111");

                //var generateMobileCodeButton = BrowserHost.instance.Application.Browser.FindElement(By.Id("generateMobileCode"));
                //generateMobileCodeButton.Click();

                //var mobileCode = BrowserHost.instance.Application.Browser.FindElement(By.Id("mobileCode"));
                //mobileCode.SendKeys("222222");

                //var signupSubmitButton = BrowserHost.instance.Application.Browser.FindElement(By.Id("signupSubmitButton"));
                //signupSubmitButton.Click();

                //Thread.Sleep(5000);
                //Assert.IsTrue(BrowserHost.instance.Application.Browser.FindElement(By.Id("FirstName")) != null);

        }
    }
}
