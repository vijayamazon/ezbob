namespace EKM {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class EkmDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public EkmDatabaseMarketPlace()
			: base(new EkmServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new EkmRetriveDataHelper(helper, this);
		}
	}
}
