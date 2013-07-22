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
            config.SetupGet(x => x.Key).Returns("Z95NpOsNNMy4LMLMH9mUjw");
            config.SetupGet(x => x.Enable).Returns(true);
            _mail = new Mail(config.Object);
        }

        [Test]
        public void SendMessage()
        {
            var vars = new Dictionary<string, string>
                {
                    {"email", "shubin_igor@ukr.net"}, 
                    {"EmailSubject", "Thank you for registering with EZBOB!"}, 
                    {"emailCC", ""}, 
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
                    {"email", "shubin_igor@ukr.net"}, 
                    {"EmailSubject", "Thank you for registering with EZBOB!"}, 
                    {"emailCC", ""}, 
                    {"ConfirmEmailAddress", "https://app.ezbob.com/confirm/90a9cd47-f84e-420e-820c-a1fc010fce11"}, 
                };
            var result = _mail.GetRenderedTemplate(vars, "Greeting");
            Assert.That(result != null);
            System.Console.Out.Write(result);
        }
    }
}
