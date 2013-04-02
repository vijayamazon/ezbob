using System.Security.Policy;

namespace EzBob.PayPalServiceLib.Common
{
	public class ServiceUrlsInfo
	{
		public ServiceUrlsInfo( string serviceEndpoint, string redirectUrl )
		{
			ServiceEndPoint = serviceEndpoint;
			RedirectUrl = redirectUrl;
		}

		public string ServiceEndPoint { get; private set; }
		public string RedirectUrl { get; private set; }
	}
}