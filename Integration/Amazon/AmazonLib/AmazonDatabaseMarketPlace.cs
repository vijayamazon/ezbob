namespace EzBob.AmazonLib {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EzBob.AmazonServiceLib;

	public class AmazonDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public AmazonDatabaseMarketPlace()
			: base(new AmazonServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new AmazonRetriveDataHelper(helper, this);
		}
	}
}
