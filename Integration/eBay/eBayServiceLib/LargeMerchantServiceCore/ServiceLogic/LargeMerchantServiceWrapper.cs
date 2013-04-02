using System.Net;
using System.ServiceModel.Channels;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic
{
	public class LargeMerchantServiceWrapper
	{
		private readonly string _ServiceName;
		private readonly ServiceVersion _Version;
		private readonly string _Token;

		public LargeMerchantServiceWrapper( string serviceName, ServiceVersion version, IServiceTokenProvider tokenProvider )
		{
			_ServiceName = serviceName;
			_Version = version;
			_Token = tokenProvider.Token.Value;
		}

		public WebRequest CorrectRequest( string operationName, HttpWebRequest request )
		{
			ModifyHeaders(request.Headers, operationName);
			return request;
		}

		public HttpRequestMessageProperty CreateRequestMessageProperty( string operationName )
		{
			var p = new HttpRequestMessageProperty();

			ModifyHeaders(p.Headers, operationName);

			return p;
		}

		private void ModifyHeaders( WebHeaderCollection headersCollection, string operationName )
		{
			headersCollection.Set( "X-EBAY-SOA-OPERATION-NAME", operationName );
			headersCollection.Set( "X-EBAY-SOA-SECURITY-TOKEN", _Token );
			headersCollection.Set( "X-EBAY-SOA-REQUEST-DATA-FORMAT", "XML" );
			headersCollection.Set( "X-EBAY-SOA-RESPONSE-DATA-FORMAT", "XML" );
			headersCollection.Set( "X-EBAY-SOA-SERVICE-VERSION", _Version.Value );
			headersCollection.Set( "X-EBAY-SOA-SERVICE-NAME", _ServiceName );
			headersCollection.Set( "X-EBAY-SOA-MESSAGE-PROTOCOL", "SOAP12" );
		}
	}
}