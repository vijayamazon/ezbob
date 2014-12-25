namespace YodleeLib.connector {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class YodleeDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public YodleeDatabaseMarketPlace()
			: base(new YodleeServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new YodleeRetriveDataHelper(helper, this);
		}
	}
}
