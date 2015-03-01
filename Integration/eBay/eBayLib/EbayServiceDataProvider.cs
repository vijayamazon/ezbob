namespace EzBob.eBayLib
{
	using System;
	using System.Linq;
	using Config;
	using StructureMap;
	using eBayServiceLib;
	using eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant;
	using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
	using eBayServiceLib.TradingServiceCore.TokenProvider;
	using System.Security.Policy;
	using System.Web;
	using EzBob.eBayServiceLib.Common;
	using EzBob.eBayServiceLib.TradingServiceCore;
	using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
	using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Simple;

	class EbayServiceDataProvider
	{
		private readonly IEbayServiceProvider _ServiceProvider;

		public EbayServiceDataProvider(EbayServiceConnectionInfo info)
		{
			_ServiceProvider = new EbayTradingServiceProvider( info );
		}

		public string CreateSessionId(string ruName)
		{
			var s = new DataProviderSessionID( _ServiceProvider );
			var rez = s.GetSessionId(new ServiceProviderDataInfoRuName(ruName));
			return rez.SessionId.Value;
		}

		public Url GenerateUrl(string sessionId, string ruName)
		{
			IServiceSignInUrlFactory signUrlFactory = new ServiceSignInUrlFactory();
			Url signInUrl = signUrlFactory.Create( _ServiceProvider.EndPointType );
			string finalUrl = signInUrl.Value + "&RuName=" + ruName + "&SessID=" + HttpUtility.UrlEncode( sessionId );
			return new Url( finalUrl );
		}

		public string FetchToken( string sessionId )
		{
			var t = new DataProviderFetchToken( _ServiceProvider );

			var rez = t.GenerateToken( new ServiceProviderDataInfoSessionId( sessionId ) );

			return rez.Token.Value;
		}

		/// <summary>
		/// Checking if during account info retrieve there is an exception with following data:
		/// ErrorCode: "332"
		/// LongMessage: "Your account has not been activated yet. Accounts are not accessible until an actual debit or credit has first been posted to the account, even though you may have already filled out our account creation form."
		/// SeverityCode: Error
		/// ShortMessage: "Your account has not been created."
		/// If it is returns false else true
		/// </summary>
		/// <param name="eBaySecurityInfo"></param>
		/// <returns>false if account is not activated</returns>
		public bool ValidateAccount(eBaySecurityInfo eBaySecurityInfo)
		{
			var creationInfo = CreateProviderCreationInfo(eBaySecurityInfo);
			var getAccount = new DataProviderGetAccount(creationInfo);
			try
			{
				getAccount.GetAccount();
			}
			catch (Exception ex)
			{
				var inner = ex.InnerException;
				if (inner != null && typeof(FailServiceRequestException) == inner.GetType())
				{
					var errorCodes = ((FailServiceRequestException)inner).ResultInfoError.Errors.Select(x => new{ x.ErrorCode, x.LongMessage} );
					if (errorCodes.Any(x => x.ErrorCode == "332"))
					{
						return false;
					}
				}
			}

			return true;
		}

		private DataProviderCreationInfo CreateProviderCreationInfo(eBaySecurityInfo securityInfo)
		{
			var ebayConnectionInfo = ObjectFactory.GetInstance<IEbayMarketplaceTypeConnection>();
			var connectionInfo = eBayServiceHelper.CreateConnection( ebayConnectionInfo );;
			
			IServiceTokenProvider serviceTokenProvider = new ServiceTokenProviderCustom(securityInfo.Token);
			IEbayServiceProvider serviceProvider = new EbayTradingServiceProvider(connectionInfo);

			return new DataProviderCreationInfo(serviceProvider)
			{
				ServiceTokenProvider = serviceTokenProvider,
				Settings = ObjectFactory.GetInstance<IEbayMarketplaceSettings>()
			};

		}
	}
}