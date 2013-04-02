using System.Security.Policy;
using EZBob.DatabaseLib.Common;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore
{
	public class EbayServiceConnectionInfo
	{
		private readonly IServiceEndPointFactory _ServiceEndPointFactory;

		public EbayServiceConnectionInfo( ServiceEndPointType endPointType, IServiceEndPointFactory serviceEndPointFactory )
		{
			_ServiceEndPointFactory = serviceEndPointFactory;
			EndPointType = endPointType;
		}

		public ServiceProviderDataInfoDevId DevId { get; set; }
		public ServiceProviderDataInfoAppId AppId { get; set; }
		public ServiceProviderDataInfoCertId CertId { get; set; }

		public Url GetEndPoint( IEbayServiceProvider serviceProvider )
		{
			return _ServiceEndPointFactory.Create( serviceProvider.ServiceType, EndPointType );
		}

		public ServiceEndPointType EndPointType { get; private set; }

		public ServiceProviderDataInfoRuName RuName { get; set; }
	}
}