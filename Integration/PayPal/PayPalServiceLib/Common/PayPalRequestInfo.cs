using System;
using EzBob.CommonLib;
using EzBob.PayPalDbLib.Models;

namespace EzBob.PayPalServiceLib.Common
{
	public class PayPalRequestInfo
	{
		public PayPalSecurityData SecurityInfo { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public ErrorRetryingInfo ErrorRetryingInfo { get; set; }

		public int OpenTimeOutInMinutes { get; set; }

		public int SendTimeoutInMinutes { get; set; }

		public int DaysPerRequest { get; set; }

	}
	
}