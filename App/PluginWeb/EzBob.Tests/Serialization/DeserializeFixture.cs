using EzBob.CommonLib;
using NUnit.Framework;

namespace EzBob.Tests.Serialization
{
	using Ezbob.Utils.Serialization;

	[TestFixture]
    [Ignore]
    public class DeserializeFixture
    {
        [Test]
        public void can_deserialize_error_retrying_info()
        {
            var xml = "<ErrorRetryingInfo UseLastTimeOut=\"false\" MinorTimeoutInSeconds=\"60\" EnableRetrying=\"true\"><IterationSettings Index=\"1\" CountRequestsExpectError=\"10\" TimeOutAfterRetryingExpiredInMinutes=\"30\" /><IterationSettings Index=\"2\" CountRequestsExpectError=\"5\" TimeOutAfterRetryingExpiredInMinutes=\"0\" /></ErrorRetryingInfo>";
            var data = Serialized.Deserialize<ErrorRetryingInfo>(xml);
            Assert.That(data.Info, Is.Not.Null);
        }
    }
}