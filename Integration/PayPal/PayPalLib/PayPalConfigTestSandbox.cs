using EzBob.PayPalServiceLib;
using EzBob.PayPalServiceLib.Common;

namespace EzBob.PayPal
{
	public class PayPalConfigTestSandbox : PayPalConfigBase
	{
		public override string PPApplicationId
		{
			get { return "APP-80W284485P519543T"; }
		}

		public override string ApiUsername
		{
			get { return "test_1330798552_biz_api1.gmail.com"; }
		}

		public override string ApiPassword
		{
			get { return "1330798576"; }
		}

		public override string ApiSignature
		{
			get { return "AE7EXJWc1StWpskzGiv6CfiP5WFtAti3ySlXlN3pGbkuMxoqTVXpOIoP"; }
		}

		public override ServiceEndPointType ServiceType
		{
			get { return ServiceEndPointType.Sandbox; }
		}

		public override int NumberOfRetries
		{
			get { return 10; }
		}
	}

}