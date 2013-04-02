using System.Web.Services.Protocols;
using EzBob.eBayServiceLib.TradingServiceCore;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic
{
	public abstract class EbayLargeMerchantServicesProviderBase<T> : EbayServiceProviderBase<T> 
		//where T : SoapHttpClientProtocol
	{
		protected EbayLargeMerchantServicesProviderBase(EbayServiceConnectionInfo dataInfo) 
			: base(dataInfo)
		{
			Endpoint = DataInfo.GetEndPoint( this ).Value;
		}
		
		protected string Endpoint { get; private set; }		
	}
}