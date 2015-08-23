namespace UIAutomationTests.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using UIAutomationTests.Core;

    [TestFixture]
    public class UITests : TestBase
    {


        [Test]
        [Category("1")]
        public void MustSucseedTest() {
            Assert.IsTrue( true);
        }

        [Test]
        [Category("2")]
        public void MustFailTest()
        {
            Assert.IsTrue(false);
        }

        public static AutomationEnums IsBrowserChrome(string caseId) {
            return AutomationEnums.Chrome; 
        }

        [Test]
        [Category("3")]
        public void CreateAccountTest() {

            //MethodBase method = MethodBase.GetCurrentMethod();
            //var category = ((CategoryAttribute)(method.GetCustomAttributes(typeof(CategoryAttribute), true)[0])).Name;
            var category = "3";

            if (category != null)
               this.ExecuteFaultHandledOperation<object>(ulong.Parse(category), () => {
                    var url = EnvironmentConfig.GetString("WizardHost");
                
                Driver.Navigate()
                    .GoToUrl(url);

                var emailBox = Driver.FindElement(By.Id("Email"));
                emailBox.SendKeys("dor+" + DateTime.Now.Ticks + "@ezbob.com");

                var passwordBox = Driver.FindElement(By.Id("signupPass1"));
                passwordBox.SendKeys("dor2015");

                var confirmPasswordBox = Driver.FindElement(By.Id("signupPass2"));
                confirmPasswordBox.SendKeys("dor2015");

                SelectElement secrertQuestion = new SelectElement(Driver.FindElement(By.Id("securityQuestion")));
                secrertQuestion.SelectByIndex(1);

                var SecurityAnswer = Driver.FindElement(By.Id("SecurityAnswer"));
                SecurityAnswer.SendKeys("AAA");

                var requestAmount = Driver.FindElement(By.Id("amount"));
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
                return null;
            });
        }
    }
}
