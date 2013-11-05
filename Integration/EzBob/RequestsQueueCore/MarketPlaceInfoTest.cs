using System;
using EzBob.CommonLib;

namespace EzBob.RequestsQueueCore
{
	internal class MarketplaceInfoTest : IMarketplaceServiceInfo
	{
		public MarketplaceInfoTest(string displayName)
			:this(displayName, Guid.NewGuid())
		{
			DisplayName = displayName;
		}

		public MarketplaceInfoTest(string displayName, Guid internalId, string description = null, bool isPaymentAccount = false)
		{
			DisplayName = displayName;
			InternalId = internalId;
			Description = description;
			IsPaymentAccount = isPaymentAccount;
		}

		public string DisplayName { get; private set; }
		public Guid InternalId { get; private set; }
		public string Description { get; private set; }
		public bool IsPaymentAccount { get; private set; }
	}
}