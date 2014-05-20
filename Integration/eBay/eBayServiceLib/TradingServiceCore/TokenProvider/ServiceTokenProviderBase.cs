namespace EzBob.eBayServiceLib.TradingServiceCore.TokenProvider
{
	using DataInfos;

	public abstract class ServiceTokenProviderBase : IServiceTokenProvider
	{
		public abstract ServiceProviderDataInfoToken Token { get; }
	}
}