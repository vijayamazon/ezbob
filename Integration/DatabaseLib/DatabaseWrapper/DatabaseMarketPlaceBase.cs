namespace EZBob.DatabaseLib.DatabaseWrapper {
	using System;
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;

	public abstract class DatabaseMarketplaceBaseBase : IMarketplaceType {
		public string Description {
			get { return _MarketplaceSeriveInfo.Description; }
		}

		public string DisplayName {
			get { return _MarketplaceSeriveInfo.DisplayName; }
		}

		public Guid InternalId {
			get { return _MarketplaceSeriveInfo.InternalId; }
		}

		public bool IsPaymentAccount {
			get { return _MarketplaceSeriveInfo.IsPaymentAccount; }
		}

		public abstract IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper);

		protected DatabaseMarketplaceBaseBase(IMarketplaceServiceInfo marketplaceSeriveInfo) {
			_MarketplaceSeriveInfo = marketplaceSeriveInfo;
		}

		private readonly IMarketplaceServiceInfo _MarketplaceSeriveInfo;
	}
}