namespace EzBob.PayPalServiceLib
{
	using System;
	using System.Collections.Generic;
	using CommonLib;
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

		public static RequestsCounterData GetTransactionData(PayPalRequestInfo reqInfo, Func<List<PayPalTransactionItem>, bool> action)
		{			
			return new PayPalServicePaymentsProHelper().GetTransactionData( reqInfo, action);
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