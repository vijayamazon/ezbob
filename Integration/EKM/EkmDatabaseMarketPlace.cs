﻿using System;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;

namespace EKM
{
	public class EkmDatabaseMarketPlace : DatabaseMarketplaceBase<EkmDatabaseMarketPlace, EKMDatabaseFunctionType>
    {
        public EkmDatabaseMarketPlace()
            : base(new EkmServiceInfo())
        {
        }

        public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
        {
            return new EkmRetriveDataHelper( helper, this );
        }

        public override IDatabaseFunctionFactory<EKMDatabaseFunctionType> FunctionFactory
        {
            get { return new EKMDatabaseFunctionFactory(); }
        }
    }
}
