namespace EzBob.PayPalServiceLib
{
	using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
	using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
	using PayPalDbLib.Models;
	using Common;

	public class PayPalServiceHelper
	{
		public static PayPalPersonalData GetAccountInfo(PayPalPermissionsGranted securityData )
		{
			var payPalServiceHelper = new PayPalServiceHelper();
			var info = payPalServiceHelper.GetAccountInfoInternal(securityData);
			return info;
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