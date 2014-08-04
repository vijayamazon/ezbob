using System;
using EzBob.CommonLib;

namespace EzBob.PayPalServiceLib
{
	public class PayPalServiceInfo : IMarketplaceServiceInfo
	{
		public string DisplayName
		{
			get { return "Pay Pal"; }
		}

		public Guid InternalId
		{
			get { return new Guid( "{3FA5E327-FCFD-483B-BA5A-DC1815747A28}" ); }
		}

		public string Description
		{
			get { return "PayPal"; }
		}

		public bool IsPaymentAccount
		{
			get { return true; }
		}
	}
}