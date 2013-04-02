using NUnit.Framework;
using ZohoCRM;

namespace EzBob.Tests.ZohoCRMTests
{
    [TestFixture]
    public class ZohoFixture
    {
        [Test]
        [Ignore]
        public void can_init_zoho()
        {
            var z = new Zoho();
            z.GenerateToken();
        }
    }
}