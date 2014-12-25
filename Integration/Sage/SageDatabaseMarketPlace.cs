namespace Sage {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class SageDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public SageDatabaseMarketPlace()
			: base(new SageServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new SageRetrieveDataHelper(helper, this);
		}
	}
}
