using System.Collections.Generic;
using EzBob.Configuration;
using Moq;
using NUnit.Framework;

namespace MailApi
{

    [TestFixture]
    public class MailFixture
    {
        private Mail _mail;

        [SetUp]
        public void StartUp()
        {
            var config = new Mock<IMandrillConfig>();
            config.SetupGet(x => x.Key).Returns("ZpZX8rtjJMJYOCGFCA1uGg");
            config.SetupGet(x => x.BaseSecureUrl).Returns("https://mandrillapp.com/api/1.0/");
            config.SetupGet(x => x.SendTemplatePath).Returns("/messages/send-template.json");
            config.SetupGet(x => x.FinishWizardTemplateName).Returns("finishwizardtemplate");

            _mail = new Mail(config.Object);
        }

        [Test]
        public void TestSend()
        {
            var vars = new Dictionary<string, string>
                {
                    {"CUSTOMER_NAME", "Test for Nimrod K"}, 
                };
            var result = _mail.ForUnitTest("shubin_igor@ukr.net",vars);
            Assert.That(result == "OK");
        }
    }
}
