using System;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;

namespace PayPoint
{
    public class PayPointDatabaseMarketPlace : DatabaseMarketplaceBase<PayPointDatabaseMarketPlace, PayPointDatabaseFunctionType>
    {
        public PayPointDatabaseMarketPlace()
            : base(new PayPointServiceInfo())
        {
        }

        public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
        {
            return new PayPointRetriveDataHelper( helper, this );
        }

        public override IDatabaseFunctionFactory<PayPointDatabaseFunctionType> FunctionFactory
        {
            get { return new PayPointDatabaseFunctionFactory(); }
        }
    }
}
