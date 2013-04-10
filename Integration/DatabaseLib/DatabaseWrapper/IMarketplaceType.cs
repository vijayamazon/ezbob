using System;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public interface IMarketplaceType : IMarketplaceInfo
	{
		IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper);

		IDatabaseFunction GetDatabaseFunctionById(Guid id);
	}
}