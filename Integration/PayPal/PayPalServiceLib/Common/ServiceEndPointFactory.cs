namespace EzBob.PayPalServiceLib.Common
{
	using System;

	public enum PayPalServiceType
	{
		Permissions,
		WebServiceThreeToken,
		WebServiceCertificate,
		AdaptiveAccounts
	}

	public interface IServiceEndPointFactory
	{
		ServiceUrlsInfo Create( PayPalServiceType serviceType );
	}

	public class ServiceEndPointFactory : IServiceEndPointFactory
	{
		public ServiceUrlsInfo Create( PayPalServiceType serviceType )
		{
			string endPointType = ConfigManager.CurrentValues.Instance.PayPalServiceType;
			string endPoint = string.Empty;
			string redirect = string.Empty;

			switch (serviceType)
			{
				case PayPalServiceType.Permissions:
					{
						switch ( endPointType )
						{
							case "Production":
								endPoint = "https://svcs.paypal.com/";
								redirect = "https://www.paypal.com/webscr&cmd=";
								break;

							case "Sandbox":
								endPoint = "https://svcs.sandbox.paypal.com/";
								redirect = "https://www.sandbox.paypal.com/webscr&cmd=";
								break;

							default:
								throw new NotImplementedException();
						}
					}
					break;

				case PayPalServiceType.AdaptiveAccounts:
					{
						switch ( endPointType )
						{
							case "Production":
								endPoint = "https://svcs.paypal.com/";
								break;

							case "Sandbox":
								endPoint = "https://svcs.sandbox.paypal.com/";
								break;
						}
					}
					break;
				case PayPalServiceType.WebServiceThreeToken:
					{
						switch ( endPointType )
						{
							case "Production":
								endPoint = "https://api-3t.paypal.com/2.0/";								
								break;

							case "Sandbox":
								endPoint = "https://api.sandbox.paypal.com/2.0/";
								break;

							default:
								throw new NotImplementedException();
						}
					}
					break;

				case PayPalServiceType.WebServiceCertificate:
					{
						switch ( endPointType )
						{
							case "Production":
								endPoint = "https://api.paypal.com/2.0/";
								break;

							case "Sandbox":
								endPoint = "https://api.sandbox.paypal.com/2.0/";
								break;

							default:
								throw new NotImplementedException();
						}
					}
					break;

				default:
					throw new NotImplementedException();
			}

			return new ServiceUrlsInfo(endPoint, redirect );
		}
	}
}
