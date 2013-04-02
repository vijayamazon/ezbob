using System;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace EzBob.eBayDbLib
{
	public class eBayDatabaseFunctionFactory : IDatabaseFunctionFactory<eBayDatabaseFunctionType>
	{
		public  IDatabaseFunction Create( eBayDatabaseFunctionType type )
		{
			return eBayDatabaseFunctionStorage.Instance.GetFunction( type );
		}

		public IDatabaseFunction GetById(Guid id)
		{
			return eBayDatabaseFunctionStorage.Instance.GetFunctionById( id );
		}
	}
}