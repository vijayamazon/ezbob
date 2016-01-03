//namespace UIAutomationTests.Tests.Application.OrangeMoney
//{
//    using System;
//    using System.Runtime.CompilerServices;
//    using NUnit.Framework;
//    using OpenQA.Selenium;
//    using OpenQA.Selenium.Support.UI;
//    using UIAutomationTests.Core;

//    [TestFixture]
//    public class OrangeMoneyTests : WebTestBase
//    {

//        [Test]
//        [Category("2")]
//        public void Dummy2() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("6")]
//        public void Dummy6() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("3")]
//        public void CreateAccountTest()
//        {

//            bool result = this.ExecuteTest<object>(() =>
//            {
//                string url = EnvironmentConfig.GetString("WizardHost");

//                Driver.Navigate()
//                    .GoToUrl(url);

//                IWebElement emailBox = Driver.FindElement(By.Id("Email"));
//                emailBox.SendKeys("dor+" + DateTime.Now.Ticks + "@ezbob.com");

//                IWebElement passwordBox = Driver.FindElement(By.Id("signupPass1"));
//                passwordBox.SendKeys("dor2015");

//                IWebElement confirmPasswordBox = Driver.FindElement(By.Id("signupPass2"));
//                confirmPasswordBox.SendKeys("dor2015");

//                SelectElement secrertQuestion = new SelectElement(Driver.FindElement(By.Id("securityQuestion")));
//                secrertQuestion.SelectByIndex(1);

//                IWebElement SecurityAnswer = Driver.FindElement(By.Id("SecurityAnswer"));
//                SecurityAnswer.SendKeys("AAA");

//                IWebElement requestAmount = Driver.FindElement(By.Id("amount"));
//                requestAmount.SendKeys("1000");

//                //var mobilePhone = BrowserHost.instance.Application.Browser.FindElement(By.Id("mobilePhone"));
//                //mobilePhone.SendKeys("01111111111");

//                //var generateMobileCodeButton = BrowserHost.instance.Application.Browser.FindElement(By.Id("generateMobileCode"));
//                //generateMobileCodeButton.Click();

//                //var mobileCode = BrowserHost.instance.Application.Browser.FindElement(By.Id("mobileCode"));
//                //mobileCode.SendKeys("222222");

//                //var signupSubmitButton = BrowserHost.instance.Application.Browser.FindElement(By.Id("signupSubmitButton"));
//                //signupSubmitButton.Click();

//                //Thread.Sleep(5000);
//                //Assert.IsTrue(BrowserHost.instance.Application.Browser.FindElement(By.Id("FirstName")) != null);
//                return null;
//            });
//        }

//        [Test]
//        [Category("5")]
//        public void Dummy5() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("7")]
//        public void Dummy7() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("22")]
//        public void Dummy22() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("1686")]
//        public void Dummy1686() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("86")]
//        public void Dummy86() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("9")]
//        public void Dummy9() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("87")]
//        public void Dummy87() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("10")]
//        public void Dummy10() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("11")]
//        public void Dummy11() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("12")]
//        public void Dummy12() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("13")]
//        public void Dummy13() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("88")]
//        public void Dummy88() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("16")]
//        public void Dummy16() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("17")]
//        public void Dummy17() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("18")]
//        public void Dummy18() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("56")]
//        public void Dummy56() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("89")]
//        public void Dummy89() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("20")]
//        public void Dummy20() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("21")]
//        public void Dummy21() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("24")]
//        public void Dummy24() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("25")]
//        public void Dummy25() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("23")]
//        public void Dummy23() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("27")]
//        public void Dummy27() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("28")]
//        public void Dummy28() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("29")]
//        public void Dummy29() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("26")]
//        public void Dummy26() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("1380")]
//        public void Dummy1380() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("65")]
//        public void Dummy65() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("67")]
//        public void Dummy67() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("66")]
//        public void Dummy66() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("48")]
//        public void Dummy48() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("49")]
//        public void Dummy49() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("50")]
//        public void Dummy50() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("52")]
//        public void Dummy52() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("51")]
//        public void Dummy51() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("53")]
//        public void Dummy53() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("55")]
//        public void Dummy55() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("58")]
//        public void Dummy58() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("68")]
//        public void Dummy68() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("59")]
//        public void Dummy59() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("61")]
//        public void Dummy61() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("62")]
//        public void Dummy62() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("63")]
//        public void Dummy63() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("64")]
//        public void Dummy64() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("93")]
//        public void Dummy93() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("562")]
//        public void Dummy562() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("563")]
//        public void Dummy563() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("566")]
//        public void Dummy566() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("94")]
//        public void Dummy94() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("95")]
//        public void Dummy95() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("97")]
//        public void Dummy97() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("99")]
//        public void Dummy99() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("100")]
//        public void Dummy100() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("101")]
//        public void Dummy101() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("102")]
//        public void Dummy102() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("210")]
//        public void Dummy210() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("211")]
//        public void Dummy211() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("212")]
//        public void Dummy212() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("214")]
//        public void Dummy214() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("215")]
//        public void Dummy215() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("216")]
//        public void Dummy216() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("92")]
//        public void Dummy92() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("236")]
//        public void Dummy236() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("237")]
//        public void Dummy237() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("238")]
//        public void Dummy238() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("239")]
//        public void Dummy239() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("240")]
//        public void Dummy240() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("241")]
//        public void Dummy241() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("242")]
//        public void Dummy242() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("91")]
//        public void Dummy91() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("374")]
//        public void Dummy374() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("375")]
//        public void Dummy375() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("4537")]
//        public void Dummy4537() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("788")]
//        public void Dummy788() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("4530")]
//        public void Dummy4530() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("5201")]
//        public void Dummy5201() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("5205")]
//        public void Dummy5205() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("5202")]
//        public void Dummy5202() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("5203")]
//        public void Dummy5203() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("5206")]
//        public void Dummy5206() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}

//        [Test]
//        [Category("5204")]
//        public void Dummy5204() { bool result = this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); Assert.IsTrue(result);}
//    }

//}
