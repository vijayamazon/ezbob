namespace MailApi
{
	using System;
	using Model;
	using System.Collections.Generic;
	using NUnit.Framework;

	[TestFixture]
    public class MailFixture
    {
        private Mail _mail;
        private string _to;
        private string _subject;

        [SetUp]
        public void StartUp()
        {
			_mail = new Mail("nNAb_KZhxEqLCyzEGOWvlg");
	        _to = "dev@ezbob.com";
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

			var attachments = new List<attachment>();
			byte[] pdfdata = System.IO.File.ReadAllBytes(@"c:\ezbob\test-data\vat-return\a0213.pdf");
			string content = Convert.ToBase64String(pdfdata);
			attachments.Add(new attachment
			{
				type = "application/pdf",
				name = "test.pdf",
				content = content
			});

            var result = _mail.Send(vars, _to, "Greeting", "Thank you for registering with EZBOB!","", attachments);
            Assert.That(result == "OK");
        }

        [Test]
        public void SendMultiplyMessage()
        {
            var vars = new Dictionary<string, string>
                {
                    {"email", "dev@ezbob.com"}, 
                    {"EmailSubject", _subject}, 
                    {"ConfirmEmailAddress", "https://app.ezbob.com/confirm/90a9cd47-f84e-420e-820c-a1fc010fce11"}, 
                };

            var result = _mail.Send(vars, "dev@ezbob.com;dev+01@ezbob.com", "Greeting", "Thank you for registering with EZBOB!");
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
            Console.Out.Write(result);
        }
    }
}
