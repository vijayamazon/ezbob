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
        private string _to;
        private string _subject;


        [SetUp]
        public void StartUp()
        {
            var config = new Mock<IMandrillConfig>();
            config.SetupGet(x => x.Key).Returns("Z95NpOsNNMy4LMLMH9mUjw");
            config.SetupGet(x => x.Enable).Returns(true);
            config.SetupGet(x => x.From).Returns("ezbob-fixture@ezbob.com");
            _mail = new Mail(config.Object);
            _to = "shubin_igor@ukr.net";
            _subject = "Thank you for registering with EZBOB!";
        }

        [Test]
        public void SendMessage()
        {
            var vars = new Dictionary<string, string>
                {
                    {"email", _to}, 
                    {"EmailSubject", _subject}, 
                    {"ConfirmEmailAddress", "https://app.ezbob.com/confirm/90a9cd47-f84e-420e-820c-a1fc010fce11"}, 
                };

            var result = _mail.Send(vars, "shubin_igor@ukr.net", "Greeting", "Thank you for registering with EZBOB!");
            Assert.That(result == "OK");
        }

        [Test]
        public void RenderTemplate()
        {
            var vars = new Dictionary<string, string>
                {
                    {"email", _to}, 
                    {"EmailSubject", _subject}, 
                    {"ConfirmEmailAddress", "https://app.ezbob.com/confirm/90a9cd47-f84e-420e-820c-a1fc010fce11"}, 
                };
            var result = _mail.GetRenderedTemplate(vars, "Greeting");
            Assert.That(result != null);
            System.Console.Out.Write(result);
        }

        [Test]
        public void SendMessageWithoutTemplate()
        {
            var text = string
                .Format("There is simple row message for {0}. \n <h1><u>And a bit html</u></h1>",typeof(MailFixture));

            var result = _mail.Send(_to, text, "There is not enough funds!");
            Assert.That(result == "OK");
        }
    }
}
