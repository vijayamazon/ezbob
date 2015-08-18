namespace UIAutomationTests.Tests
{
    using System;
    using System.Reflection;
    using System.Threading;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.IE;
    using OpenQA.Selenium.Safari;
    using OpenQA.Selenium.Support.UI;
    using UIAutomationTests.Core;
    
    [TestFixture]
    public class UITests : TestBase
    {


        [Test]
        [Category("C1")]
        public void MustSucseedTest() {
            Assert.IsTrue( true);
        }

        [Test]
        [Category("C2")]
        public void MustFailTest()
        {
            Assert.IsTrue(false);
        }

        public static Browser IsBrowserChrome(string caseId) {
            return Browser.Chrome; 
        }

        [Test]
        [Category("C3")]
        public void CreateAccountTest() {

            MethodBase method = MethodBase.GetCurrentMethod();
            var category = ((NUnit.Framework.CategoryAttribute)(method.GetCustomAttributes(typeof(CategoryAttribute), true)[0])).Name;

            this.ExecuteFaultHandledOperation<object>(category, () => {
                
                Driver.Navigate()
                    .GoToUrl("https://app.ezbob.com/Customer/Wizard#SignUp");

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
