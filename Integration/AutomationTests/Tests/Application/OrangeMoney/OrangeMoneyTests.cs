namespace UIAutomationTests.Tests.Application.OrangeMoney
{
    using System;
    using System.Runtime.CompilerServices;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using UIAutomationTests.Core;

    [TestFixture]
    public class OrangeMoneyTests : TestBase
    {

        [Test]
        [Category("2")]
        public void Dummy2() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("6")]
        public void Dummy6() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("3")]
        public void CreateAccountTest()
        {

            this.ExecuteTest<object>(() =>
            {
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

        [Test]
        [Category("5")]
        public void Dummy5() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("7")]
        public void Dummy7() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("22")]
        public void Dummy22() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("1686")]
        public void Dummy1686() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("86")]
        public void Dummy86() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("9")]
        public void Dummy9() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("87")]
        public void Dummy87() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("10")]
        public void Dummy10() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("11")]
        public void Dummy11() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("12")]
        public void Dummy12() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("13")]
        public void Dummy13() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("88")]
        public void Dummy88() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("16")]
        public void Dummy16() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("17")]
        public void Dummy17() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("18")]
        public void Dummy18() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("56")]
        public void Dummy56() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("89")]
        public void Dummy89() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("20")]
        public void Dummy20() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("21")]
        public void Dummy21() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("24")]
        public void Dummy24() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("25")]
        public void Dummy25() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("23")]
        public void Dummy23() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("27")]
        public void Dummy27() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("28")]
        public void Dummy28() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("29")]
        public void Dummy29() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("26")]
        public void Dummy26() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("1380")]
        public void Dummy1380() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("65")]
        public void Dummy65() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("67")]
        public void Dummy67() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("66")]
        public void Dummy66() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("48")]
        public void Dummy48() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("49")]
        public void Dummy49() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("50")]
        public void Dummy50() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("52")]
        public void Dummy52() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("51")]
        public void Dummy51() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("53")]
        public void Dummy53() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("55")]
        public void Dummy55() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("58")]
        public void Dummy58() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("68")]
        public void Dummy68() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("59")]
        public void Dummy59() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("61")]
        public void Dummy61() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("62")]
        public void Dummy62() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("63")]
        public void Dummy63() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("64")]
        public void Dummy64() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("93")]
        public void Dummy93() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("562")]
        public void Dummy562() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("563")]
        public void Dummy563() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("566")]
        public void Dummy566() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("94")]
        public void Dummy94() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("95")]
        public void Dummy95() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("97")]
        public void Dummy97() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("99")]
        public void Dummy99() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("100")]
        public void Dummy100() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("101")]
        public void Dummy101() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("102")]
        public void Dummy102() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("210")]
        public void Dummy210() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("211")]
        public void Dummy211() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("212")]
        public void Dummy212() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("214")]
        public void Dummy214() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("215")]
        public void Dummy215() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("216")]
        public void Dummy216() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("92")]
        public void Dummy92() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("236")]
        public void Dummy236() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("237")]
        public void Dummy237() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("238")]
        public void Dummy238() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("239")]
        public void Dummy239() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("240")]
        public void Dummy240() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("241")]
        public void Dummy241() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("242")]
        public void Dummy242() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("91")]
        public void Dummy91() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("374")]
        public void Dummy374() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("375")]
        public void Dummy375() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("4537")]
        public void Dummy4537() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("788")]
        public void Dummy788() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("4530")]
        public void Dummy4530() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("5201")]
        public void Dummy5201() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("5205")]
        public void Dummy5205() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("5202")]
        public void Dummy5202() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("5203")]
        public void Dummy5203() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("5206")]
        public void Dummy5206() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("5204")]
        public void Dummy5204() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }
    }

}
