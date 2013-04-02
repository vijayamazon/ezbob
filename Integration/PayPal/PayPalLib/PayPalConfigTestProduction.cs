using EzBob.PayPalServiceLib.Common;

namespace EzBob.PayPal
{
	public class PayPalConfigTestProduction : PayPalConfigBase
	{
		public override string PPApplicationId
		{
			get { return "APP-8W341342JX273523H"; }
		}

		public override string ApiUsername
		{
			get { return "caroles_api1.ezbob.com"; }
		}

		public override string ApiPassword
		{
			get { return "8RYKUCVNC5PXWLPD"; }
		}

		public override string ApiSignature
		{
			get { return "AFcWxV21C7fd0v3bYYYRCpSSRl31Aoui-O9-IwfGEuUAgSnmws-LttVZ"; }
		}

		public override ServiceEndPointType ServiceType
		{
			get { return ServiceEndPointType.Production; }
		}
	}
}