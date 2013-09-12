using EzBob.CommonLib;
using EzBob.PayPalServiceLib;
using EzBob.PayPalServiceLib.Common;

namespace EzBob.PayPal
{
	public abstract class PayPalConfigBase : IPayPalConfig
	{
		public PayPalProfileType ApiAuthenticationMode
		{
			get { return PayPalProfileType.ThreeToken; }
		}

		public PayPalDataFormat ApiRequestformat
		{
			get { return PayPalDataFormat.SOAP11; }
		}

		public PayPalDataFormat ApiResponseformat
		{
			get { return PayPalDataFormat.SOAP11; }
		}

		public bool TrustAll
		{
			get { return true; }
		}

		public abstract string PPApplicationId { get; }
		public abstract string ApiUsername { get; }
		public abstract string ApiPassword { get; }
		public abstract string ApiSignature { get; }
		public abstract ServiceEndPointType ServiceType { get; }
		public abstract int NumberOfRetries { get; }
		public abstract int MaxAllowedFailures { get; }
	}
}