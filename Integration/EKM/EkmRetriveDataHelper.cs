using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;

namespace EKM
{
    public class EkmRetriveDataHelper : MarketplaceRetrieveDataHelperBase<EKMDatabaseFunctionType>
    {
        public EkmRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<EKMDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {
            
        }

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            
            //retreive data from ekm api
            //calculate agregated data
            //store orders
        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {
            //store agregated

        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
            //retrive secure data
            return null;
        }
    }
}