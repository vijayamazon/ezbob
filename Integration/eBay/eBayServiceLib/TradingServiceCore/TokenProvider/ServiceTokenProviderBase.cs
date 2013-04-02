using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.TokenProvider
{
	public abstract class ServiceTokenProviderBase : IServiceTokenProvider
	{
		public abstract ServiceProviderDataInfoToken Token { get; }

		bool HasData 
		{
			get { return Token != null && Token.HasData; }
		}
	}
}