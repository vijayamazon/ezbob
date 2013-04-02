using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.TokenProvider
{
	public class ServiceTokenProviderCustom : ServiceTokenProviderBase
	{
		private readonly string _Token;

		public ServiceTokenProviderCustom( string token )
		{
			_Token = token;
		}

		public override ServiceProviderDataInfoToken Token
		{
			get { return new ServiceProviderDataInfoToken( _Token ); }
		}
	}
}