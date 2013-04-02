using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap.bulkdataexchange;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.BulkData
{
	public class EbayBulkDataServiceProvider : EbayLargeMerchantServicesProviderBase<BulkDataExchangeServicePort>
	{
		public EbayBulkDataServiceProvider(EbayServiceConnectionInfo dataInfo) 
			: base(dataInfo)
		{
		}

		public override BulkDataExchangeServicePort GetService( string callProcedureName, ServiceVersion ver, IServiceTokenProvider tokenProvider )
		{
			var service = new BulkDataExchangeServicePort( ServiceType.ToString(), ver, tokenProvider );
			service.Url = Endpoint;
			return service;
		}

		public override EbayServiceType ServiceType
		{
			get { return EbayServiceType.BulkDataExchangeService; }
		}
	}
}
