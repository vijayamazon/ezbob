using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib.Common;
using PayPal.Services.Private.AA;
using log4net;
using AdaptiveAccounts = PayPal.Platform.SDK.AdaptiveAccounts;

namespace EzBob.PayPalServiceLib
{
	public class PayPalAccountStatusInfo
	{
		public string AccountStatus { get; set; }

		public string AccountType { get; set; }
	}

	public class PayPalAdaptiveAccountsServiceHelper : PayPalServiceWrapperBase
	{
		private static readonly ILog _log = log4net.LogManager.GetLogger( typeof( PayPalAdaptiveAccountsServiceHelper ) );

		private PayPalAdaptiveAccountsServiceHelper(IPayPalConfig config)
			:base(config, PayPalServiceType.AdaptiveAccounts)
		{
			
		}

		private AdaptiveAccounts InternalCreateService( PayPalRermissionsGranted securityData = null, string scriptName = null )
		{			
			return new AdaptiveAccounts { APIProfile = GetProfile(securityData, scriptName) };
		}

		protected override string Endpoint
		{
			get { return AdaptiveAccounts.Endpoint; }
		}

		public static PayPalAccountStatusInfo GetVerifiedStatus( IPayPalConfig config, string userFirstName, string userLastName, string userEMail )
		{
			return new PayPalAdaptiveAccountsServiceHelper( config ).InternalGetVerifiedStatus( userFirstName, userLastName, userEMail );
		}

		private PayPalAccountStatusInfo InternalGetVerifiedStatus( string userFirstName, string userLastName, string userEMail )
		{
			var aa = InternalCreateService();

			var request = new GetVerifiedStatusRequest
			{
				requestEnvelope = GetRequestEnvelope(),
				emailAddress = userEMail,
				firstName = userFirstName,
				lastName = userLastName,
				matchCriteria = "NAME",
			};

			GetVerifiedStatusResponse response = aa.GetVerifiedStatus( request );

			if ( aa.isSuccess.ToUpper() == "FAILURE" )
			{
				_log.Error( "GetVerifiedStatus Failed" );
				_log.Error( aa.LastError.ErrorDetails );
				throw new PayPalException( aa.LastError.ErrorDetails );
			}

			return new PayPalAccountStatusInfo
				{
					AccountStatus = response.accountStatus,
					AccountType = response.userInfo == null ? string.Empty : response.userInfo.accountType
				};

		}

		private static RequestEnvelope GetRequestEnvelope()
		{
			return new RequestEnvelope { errorLanguage = "en_US" };

		}
	}
}