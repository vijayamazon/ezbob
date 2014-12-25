namespace EzBob.eBayLib {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EzBob.eBayServiceLib;

	public class eBayDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public eBayDatabaseMarketPlace()
			: base(new eBayServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new eBayRetriveDataHelper(helper, this);
		}
	}
}
