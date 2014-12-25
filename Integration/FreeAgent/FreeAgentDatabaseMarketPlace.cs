namespace FreeAgent {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class FreeAgentDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public FreeAgentDatabaseMarketPlace()
			: base(new FreeAgentServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new FreeAgentRetrieveDataHelper(helper, this);
		}
	}
}
