using System;
using EzBob.CommonLib;

namespace EzBob.eBayServiceLib
{
	public class eBayServiceInfo : IMarketplaceServiceInfo
	{
		public string DisplayName
		{
			get { return "eBay"; }
		}

		public Guid InternalId
		{
			get { return new Guid( "{A7120CB7-4C93-459B-9901-0E95E7281B59}" ); }
		}

		public string Description
		{
			get { return "ebay"; }
		}

		public bool IsPaymentAccount
		{
			get { return false; }
		}
	}
}
