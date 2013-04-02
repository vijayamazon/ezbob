using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.TokenProvider
{
	public interface IServiceTokenProvider
	{
		ServiceProviderDataInfoToken Token { get; }		
	}
}