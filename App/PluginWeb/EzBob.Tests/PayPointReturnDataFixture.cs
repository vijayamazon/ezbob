using NUnit.Framework;
using PaymentServices.PayPoint;

namespace EzBob.Tests
{
    [TestFixture]
    public class PayPointReturnDataFixture
    {
        [Test]
        public void parses_error_string()
        {
            var response = "?valid=false&trans_id=72034c29-72bb-4b29-b4ef-849fa15440202012-10-05_03:59:55&code=N&message=No {1} transaction found&resp_code=30";
            var data = new PayPointReturnData(response);

            Assert.That(data.HasError, Is.True);
            Assert.That(data.Code, Is.EqualTo("N"));
        }
    }
}