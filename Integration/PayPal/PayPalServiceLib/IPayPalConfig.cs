using EzBob.PayPalServiceLib.Common;

namespace EzBob.PayPalServiceLib
{
	public enum PayPalDataFormat
	{
		SOAP11,
		NV,
		JSON,
		XML
	}

	public enum PayPalProfileType
	{
		ThreeToken,
		Certificate
	}

	public interface IPayPalTransactionSearchSettings
	{
		int MonthsBack { get; }
	}

	public interface IPayPalConfig
    {
		ServiceEndPointType ServiceType { get; }
		PayPalProfileType ApiAuthenticationMode { get; }
		PayPalDataFormat ApiRequestformat { get; }
		PayPalDataFormat ApiResponseformat { get; }        
		string PPApplicationId { get; }
		string ApiUsername { get; }
		string ApiPassword { get; }
		string ApiSignature { get; }

		bool TrustAll { get; }
		int NumberOfRetries { get; }
    }
}