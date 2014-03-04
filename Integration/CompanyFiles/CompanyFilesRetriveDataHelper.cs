using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;
using log4net;

namespace CompanyFiles
{
	public class CompanyFilesRetriveDataHelper : MarketplaceRetrieveDataHelperBase<CompanyFilesDatabaseFunctionType>
	{

		private static ILog log = LogManager.GetLogger(typeof(CompanyFilesRetriveDataHelper));

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