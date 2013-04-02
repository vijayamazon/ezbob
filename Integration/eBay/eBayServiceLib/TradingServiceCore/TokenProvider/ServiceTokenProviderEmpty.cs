using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.TokenProvider
{
	class ServiceTokenProviderEmpty : ServiceTokenProviderBase
	{
		public override ServiceProviderDataInfoToken Token
		{
			get { return new ServiceProviderDataInfoTokenEmpty(); }
		}
	}
}