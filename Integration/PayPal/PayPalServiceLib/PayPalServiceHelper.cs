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

		private PayPalServiceHelper()
		{
		}

		public static PayPalPersonalData GetAccountInfo(PayPalPermissionsGranted securityData )
		{
			var payPalServiceHelper = new PayPalServiceHelper();
			var info = payPalServiceHelper.GetAccountInfoInternal(securityData);
			return info;
		}

		private PayPalAccountStatusInfo GetVerifiedStatus( string userFirstName, string userLastName, string userEMail)
		{
			return PayPalAdaptiveAccountsServiceHelper.GetVerifiedStatus(userFirstName, userLastName, userEMail);			
		}

		private PayPalPersonalData GetAccountInfoInternal( PayPalPermissionsGranted securityData )
		{
			return PayPalPermissionServiceHelper.GetAccountInfo( securityData );				
		}

		public static PayPalTransactionsList GetTransactionData(PayPalRequestInfo reqInfo)
		{			
			return new PayPalServicePaymentsProHelper().GetTransactionData( reqInfo );
		}

        public static GetRequestPermissionsUrlResponse GetRequestPermissionsUrl(string callback)
		{
			return PayPalPermissionServiceHelper.GetRequestPermissionsUrl(callback);
		}

		public static PayPalPermissionsGranted GetAccessToken( string requestToken, string verificationCode )
		{
			return PayPalPermissionServiceHelper.GetAccessToken(requestToken, verificationCode);
		}
	}
}