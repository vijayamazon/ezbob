using NUnit.Framework;

namespace Deveel.Web.Zoho
{
    [TestFixture]
    public class ZohoConvertLeadResponseTest
    {
        [Test]
        public void ParseSuccessResult()
        {
            const string response = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><success><Contact param=\"id\">664178000000079005</Contact></success>";
            var r = new ZohoConvertLeadResponse("Leads", "convertLead", response);
            Assert.That(r.IsError, Is.False);
            Assert.That(r.Id, Is.Not.Empty);
        }
    }
}