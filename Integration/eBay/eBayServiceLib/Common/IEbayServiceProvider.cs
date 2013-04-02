using System.Web.Services.Protocols;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;

namespace EzBob.eBayServiceLib.Common
{
	public interface IEbayServiceProvider
	{
		ServiceEndPointType EndPointType { get; }
		ServiceProviderDataInfoRuName RuName { get; }
		EbayServiceType ServiceType { get; }
	}

	public interface IEbayServiceProvider<out T> : IEbayServiceProvider
		//where T : SoapHttpClientProtocol
	{
		T GetService( string callProcedureName, ServiceVersion ver, IServiceTokenProvider tokenProvider );		
	}
}