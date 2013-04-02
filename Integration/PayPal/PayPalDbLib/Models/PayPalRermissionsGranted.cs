using System.Collections;
using EZBob.DatabaseLib.Common;

namespace EzBob.PayPalDbLib.Models
{
	public class PayPalSecurityData : IMarketPlaceSecurityInfo
	{
		public PayPalRermissionsGranted RermissionsGranted { get; set; }
		/// <summary>
		/// user e-Mail
		/// </summary>
		public string UserId { get; set; }

	}

	public class PayPalRermissionsGranted
	{
		public PayPalRermissionsGranted()
		{
		}

		public PayPalRermissionsGranted( string verificationCode, string requestToken, string accessToken, string tokenSecret )
		{
			VerificationCode = verificationCode;
			RequestToken = requestToken;
			AccessToken = accessToken;
			TokenSecret = tokenSecret;
		}

		public string TokenSecret { get; set; }
		public string VerificationCode { get; set; }
		public string RequestToken { get; set; }
		public string AccessToken { get; set; }
	}
}