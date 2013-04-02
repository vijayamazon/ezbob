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

		public MarketplaceInfoTest(string displayName, Guid internalId, string description = null)
		{
			DisplayName = displayName;
			InternalId = internalId;
			Description = description;
		}

		public string DisplayName { get; private set; }
		public Guid InternalId { get; private set; }
		public string Description { get; private set; }
	}
}