namespace CompanyFiles {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class CompanyFilesDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public CompanyFilesDatabaseMarketPlace()
			: base(new CompanyFilesServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new CompanyFilesRetriveDataHelper(helper, this);
		}
	}
}
