using System.Web.Services.Protocols;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore
{
	public abstract class EbayServiceProviderBase<T> : IEbayServiceProvider<T> 
		//where T : SoapHttpClientProtocol
	{
		protected EbayServiceProviderBase(EbayServiceConnectionInfo dataInfo)
		{
			DataInfo = dataInfo;
		}

		protected EbayServiceConnectionInfo DataInfo { get; private set; }

		public abstract T GetService(string callProcedureName, ServiceVersion ver, IServiceTokenProvider tokenProvider);

		public ServiceEndPointType EndPointType
		{
			get { return DataInfo.EndPointType; }
		}

		public ServiceProviderDataInfoRuName RuName
		{
			get { return DataInfo.RuName; }
		}

		public abstract EbayServiceType ServiceType { get; }
	}
}