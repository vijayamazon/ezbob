using System.Security.Policy;
using System.Web;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Simple;

namespace EzBob.eBayServiceLib.TradingServiceCore.TokenProvider
{
	public class ServiceTokenProviderBySession : ServiceTokenProviderBase
	{
		private readonly IEbayServiceProvider _ServiceProvider;
		private readonly IServiceSignInUrlFactory _ServiceSignInUrlFactory;
		private readonly IWaitWebResponseProvider _WaitWebResponseProvider;
		private ServiceProviderDataInfoToken _Token;
		private ServiceProviderDataInfoSessionId _SessionId;

		public ServiceTokenProviderBySession( IEbayServiceProvider serviceProvider, IServiceSignInUrlFactory serviceSignInUrlFactory, IWaitWebResponseProvider waitWebResponseProvider )
		{
			_ServiceProvider = serviceProvider;
			_ServiceSignInUrlFactory = serviceSignInUrlFactory;
			_WaitWebResponseProvider = waitWebResponseProvider;
		}

		public override ServiceProviderDataInfoToken Token
		{
			get { return _Token ?? ( _Token = CreateToken() ); }
		}

		private ServiceProviderDataInfoSessionId SessionId
		{
			get
			{
				if ( _SessionId == null )
				{
					var s = new DataProviderSessionID( _ServiceProvider );
					_SessionId = s.GetSessionId( RuName ).SessionId;
				}
				return _SessionId;
			}
		}

		private ServiceProviderDataInfoRuName RuName
		{
			get { return _ServiceProvider.RuName; }
		}

		private ServiceProviderDataInfoToken CreateToken()
		{
			string sesionId = SessionId.Value;

			Url signInUrl = _ServiceSignInUrlFactory.Create( _ServiceProvider.EndPointType );
			string finalUrl = signInUrl.Value + "&RuName=" + RuName.Value + "&SessID=" + HttpUtility.UrlEncode( sesionId);

			var proc = System.Diagnostics.Process.Start( finalUrl );
			_WaitWebResponseProvider.WaitForResponse();						

			var t = new DataProviderFetchToken( _ServiceProvider );

			return t.GenerateToken( SessionId ).Token;
		}
	}
}