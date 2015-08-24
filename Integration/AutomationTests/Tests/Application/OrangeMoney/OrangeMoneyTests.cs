namespace UIAutomationTests.Tests.Application.OrangeMoney
{
    using System;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using UIAutomationTests.Core;

    [TestFixture]
    public class OrangeMoneyTests : TestBase
    {
        [Test]
        [Category("3")]
        public void CreateAccountTest() {

            this.ExecuteTest<object>(() => {
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
[Test][Category("6")]public void DummyC6(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("5")]public void DummyC5(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("7")]public void DummyC7(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("22")]public void DummyC22(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("1686")]public void DummyC1686(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("86")]public void DummyC86(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("9")]public void DummyC9(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("87")]public void DummyC87(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("10")]public void DummyC10(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("11")]public void DummyC11(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("12")]public void DummyC12(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("13")]public void DummyC13(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("88")]public void DummyC88(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("16")]public void DummyC16(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("17")]public void DummyC17(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("18")]public void DummyC18(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("56")]public void DummyC56(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("89")]public void DummyC89(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("20")]public void DummyC20(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("21")]public void DummyC21(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("24")]public void DummyC24(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("25")]public void DummyC25(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("23D")]public void DummyC23(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("27")]public void DummyC27(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("28")]public void DummyC28(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("29")]public void DummyC29(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("26")]public void DummyC26(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("1380")]public void DummyC1380(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("65")]public void DummyC65(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("67")]public void DummyC67(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("66")]public void DummyC66(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("48")]public void DummyC48(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("49")]public void DummyC49(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("50")]public void DummyC50(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("52")]public void DummyC52(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("51")]public void DummyC51(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("53")]public void DummyC53(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("55")]public void DummyC55(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("58")]public void DummyC58(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("68")]public void DummyC68(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("59")]public void DummyC59(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("61")]public void DummyC61(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("62")]public void DummyC62(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("63")]public void DummyC63(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("64")]public void DummyC64(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("93")]public void DummyC93(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("562")]public void DummyC562(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("563")]public void DummyC563(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("566")]public void DummyC566(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("94")]public void DummyC94(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("95")]public void DummyC95(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("97")]public void DummyC97(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("99")]public void DummyC99(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("100")]public void DummyC100(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("101")]public void DummyC101(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("102")]public void DummyC102(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("210")]public void DummyC210(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("211")]public void DummyC211(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("212")]public void DummyC212(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("214")]public void DummyC214(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("215")]public void DummyC215(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("216")]public void DummyC216(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("92")]public void DummyC92(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("236")]public void DummyC236(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("237")]public void DummyC237(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("238")]public void DummyC238(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("239")]public void DummyC239(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("240")]public void DummyC240(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("241")]public void DummyC241(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("242")]public void DummyC242(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("91")]public void DummyC91(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("374")]public void DummyC374(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("375")]public void DummyC375(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("4537")]public void DummyC4537(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("788")]public void DummyC788(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}
[Test][Category("4530")]public void DummyC4530(){{this.ExecuteTest<object>(() => {Assert.IsTrue(false); return null;});}}

    }

}
