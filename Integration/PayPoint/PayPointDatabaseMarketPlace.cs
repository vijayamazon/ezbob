namespace PayPoint {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class PayPointDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public PayPointDatabaseMarketPlace()
			: base(new PayPointServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new PayPointRetrieveDataHelper(helper, this);
		}
	}
}
