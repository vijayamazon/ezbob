namespace CompanyFiles
{
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;

	public class CompanyFilesRetriveDataHelper : MarketplaceRetrieveDataHelperBase<CompanyFilesDatabaseFunctionType>
	{
		public CompanyFilesRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<CompanyFilesDatabaseFunctionType> marketplace)
			: base(helper, marketplace)
		{
		}

		protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
		}

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
		{
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
		{
			return null;
		}
	}
}