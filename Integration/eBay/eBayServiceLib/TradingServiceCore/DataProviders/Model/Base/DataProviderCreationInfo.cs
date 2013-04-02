using EzBob.eBayLib.Config;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base
{
	public class DataProviderCreationInfo
	{
		public DataProviderCreationInfo( IEbayServiceProvider serviceProvider, IServiceTokenProvider serviceTokenProvider = null, IEbayMarketplaceSettings settings = null )
		{
			ServiceTokenProvider = serviceTokenProvider;
			ServiceProvider = serviceProvider;
			Settings = settings;
		}

		public IServiceTokenProvider ServiceTokenProvider { get; set; }

		public IEbayServiceProvider ServiceProvider { get; set; }

		public IEbayMarketplaceSettings Settings { get; set; }
	}
}