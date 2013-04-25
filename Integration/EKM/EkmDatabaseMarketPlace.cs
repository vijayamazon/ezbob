using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;

namespace EKM
{
	public class EkmDatabaseMarketPlace : DatabaseMarketplaceBase<EkmDatabaseFunctionType>
    {
        public EkmDatabaseMarketPlace()
            : base(new EkmServiceInfo())
        {
        }

        public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
        {
            return new EkmRetriveDataHelper( helper, this );
        }

        public override IDatabaseFunctionFactory<EkmDatabaseFunctionType> FunctionFactory
        {
            get { return new EKMDatabaseFunctionFactory(); }
        }
    }
}
