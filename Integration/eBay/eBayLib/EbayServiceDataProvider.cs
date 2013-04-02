using System.Security.Policy;
using System.Web;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Simple;

namespace EzBob.eBayLib
{
	class EbayServiceDataProvider
	{
		private readonly EbayServiceConnectionInfo _Info;
		private readonly IEbayServiceProvider _ServiceProvider;

		public EbayServiceDataProvider(EbayServiceConnectionInfo info)
		{
			_Info = info;
			_ServiceProvider = new EbayTradingServiceProvider( info );			
		}

		public string CreateSessionId()
		{
			var s = new DataProviderSessionID( _ServiceProvider );
			var rez = s.GetSessionId( _Info.RuName );
			return rez.SessionId.Value;
		}

		public Url GenerateUrl( string sessionId )
		{
			IServiceSignInUrlFactory signUrlFactory = new ServiceSignInUrlFactory();
			Url signInUrl = signUrlFactory.Create( _ServiceProvider.EndPointType );
			string finalUrl = signInUrl.Value + "&RuName=" + _Info.RuName.Value + "&SessID=" + HttpUtility.UrlEncode( sessionId );
			return new Url( finalUrl );
		}

		public string FetchToken( string sessionId )
		{
			var t = new DataProviderFetchToken( _ServiceProvider );

			var rez = t.GenerateToken( new ServiceProviderDataInfoSessionId( sessionId ) );

			return rez.Token.Value;
		}

	}
}