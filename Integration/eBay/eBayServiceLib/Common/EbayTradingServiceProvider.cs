using EzBob.eBayServiceLib.TradingServiceCore;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.Common
{
	public class EbayTradingServiceProvider : EbayServiceProviderBase<eBayAPIInterfaceService>
	{
		public EbayTradingServiceProvider(EbayServiceConnectionInfo dataInfo) 
			: base(dataInfo)
		{
			
		}

		public override eBayAPIInterfaceService GetService( string callProcedureName, ServiceVersion ver, IServiceTokenProvider tokenProvider )
		{			
			// create the service object
			var service = new eBayAPIInterfaceService();
			
			// define what's needed to build the request url
			// to call the ebay soap api

			// endpoint can be either sandbox or production
			// make sure credentials are for that environment

			//string endpoint = "https://api.sandbox.ebay.com/wsapi";    // sandbox
			//string endpoint = "https://api.ebay.com/wsapi";               // production
			string endpoint = DataInfo.GetEndPoint( this ).Value;
			// credentials are hard-coded for this example
			string devId = DataInfo.DevId.Value;
			string appId = DataInfo.AppId.Value;
			string certId = DataInfo.CertId.Value;				

			// call name, site id, and version are required
			string callName = callProcedureName;
			string siteId = "3"; // UK

			// see: http://developer.ebay.com/devzone/xml/docs/WebHelp/ReleaseNotes.html
			string version = ver.Value;

			// build the request url
			var requestURL = string.Format("{0}?callname={1}&siteid={2}&appid={3}&version={4}&routing=default", endpoint, callName,
					                        siteId, appId, version);
			// assign the request url to the service object
			service.Url = requestURL;
				
			// add the token
			service.RequesterCredentials = new CustomSecurityHeaderType();

			if ( tokenProvider != null && tokenProvider.Token != null && tokenProvider.Token.HasData )
			{
				service.RequesterCredentials.eBayAuthToken = tokenProvider.Token.Value;
			}
			// add the three ids
			service.RequesterCredentials.Credentials = new UserIdPasswordType();
			service.RequesterCredentials.Credentials.AppId = appId;
			service.RequesterCredentials.Credentials.DevId = devId;
			service.RequesterCredentials.Credentials.AuthCert = certId;
			return service;
		}

		public override EbayServiceType ServiceType
		{
			get { return EbayServiceType.Trading; }
		}
	}
}