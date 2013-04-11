using System;
using System.Collections.Generic;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EzBob.CommonLib;

namespace EzBob.RequestsQueueCore
{
	internal class DatabaseMarketplaceTest : DatabaseMarketplaceBaseBase
	{
		public DatabaseMarketplaceTest( IMarketplaceServiceInfo marketplaceSeriveInfo ) 
			: base(marketplaceSeriveInfo)
		{
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
		{
			throw new NotImplementedException();
		}

		public override IDatabaseFunction GetDatabaseFunctionById(Guid id)
		{
			throw new NotImplementedException();
		}

	    public override IEnumerable<IDatabaseFunction> DatabaseFunctionList
	    {
	        get { throw new NotImplementedException(); }
	    }
	}
}