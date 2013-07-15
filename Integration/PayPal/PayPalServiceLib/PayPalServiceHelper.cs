using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib.Common;
using log4net;

namespace EzBob.PayPalServiceLib
{
	public class PayPalServiceHelper
	{
		private static readonly ILog _log = log4net.LogManager.GetLogger( typeof( PayPalServiceHelper ) );
		private readonly IPayPalConfig _Config;

		private PayPalServiceHelper(IPayPalConfig config)
		{
			_Config = config;
		}

		public static PayPalPersonalData GetAccountInfo(IPayPalConfig config, PayPalRermissionsGranted securityData )
		{
			var payPalServiceHelper = new PayPalServiceHelper(config);
			var info = payPalServiceHelper.GetAccountInfoInternal(securityData);
			return info;
		}

		private PayPalAccountStatusInfo GetVerifiedStatus( string userFirstName, string userLastName, string userEMail)
		{
			return PayPalAdaptiveAccountsServiceHelper.GetVerifiedStatus(_Config, userFirstName, userLastName, userEMail);			
		}

		private PayPalPersonalData GetAccountInfoInternal( PayPalRermissionsGranted securityData )
		{
			return PayPalPermissionServiceHelper.GetAccountInfo( _Config, securityData );				
		}

		public static PayPalTransactionsList GetTransactionData( IPayPalConfig config, PayPalRequestInfo reqInfo )
		{			
			return new PayPalServicePaymentsProHelper( config ).GetTransactionData( reqInfo );
		}

		public static string GetRequestPermissionsUrl( IPayPalConfig config, string callback )
		{
			return PayPalPermissionServiceHelper.GetRequestPermissionsUrl(config, callback);
		}

		public static PayPalRermissionsGranted GetAccessToken( IPayPalConfig config, string requestToken, string verificationCode )
		{
			return PayPalPermissionServiceHelper.GetAccessToken(config, requestToken, verificationCode);
		}
	}
}