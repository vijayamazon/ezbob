using System;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;

namespace PayPoint
{
    public class PayPointDatabaseMarketPlace : DatabaseMarketplaceBase<PayPointDatabaseFunctionType>
    {
        public PayPointDatabaseMarketPlace()
            : base(new PayPointServiceInfo())
        {
        }

        public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
        {
            return new PayPointRetrieveDataHelper( helper, this );
        }

        public override IDatabaseFunctionFactory<PayPointDatabaseFunctionType> FunctionFactory
        {
            get { return new PayPointDatabaseFunctionFactory(); }
        }
    }
}
