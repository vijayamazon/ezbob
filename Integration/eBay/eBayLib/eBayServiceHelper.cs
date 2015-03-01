using System.Security.Policy;
using EzBob.eBayLib.Config;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayLib
{
	public class eBayServiceHelper
	{
		private readonly EbayServiceDataProvider _DataProvider;

		public eBayServiceHelper(IEbayMarketplaceTypeConnection connectionInfo)
		{
			var dataInfoProduction = CreateConnection(connectionInfo);
			_DataProvider = new EbayServiceDataProvider(dataInfoProduction);
		}

		public string CreateSessionId(string ruName)
		{
			return _DataProvider.CreateSessionId(ruName);
		}

		public Url CreateUrl(string sessionId, string ruName)
		{
			return _DataProvider.GenerateUrl(sessionId, ruName);
		}

		public string FetchToken(string sessionId)
		{
			return _DataProvider.FetchToken(sessionId);
		}

		public static EbayServiceConnectionInfo CreateConnection(IEbayMarketplaceTypeConnection connectionInfo)
		{
			var serviceEndPointFactory = new ServiceEndPointFactory();

			return new EbayServiceConnectionInfo(connectionInfo.ServiceType, serviceEndPointFactory)
			{
				DevId = new ServiceProviderDataInfoDevId(connectionInfo.DevId),
				AppId = new ServiceProviderDataInfoAppId(connectionInfo.AppId),
				CertId = new ServiceProviderDataInfoCertId(connectionInfo.CertId),
			};
		}

		public bool ValidateAccount(eBayServiceLib.eBaySecurityInfo eBaySecurityInfo)
		{
			return _DataProvider.ValidateAccount(eBaySecurityInfo);
		}
	}
}
