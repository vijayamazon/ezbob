using System;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace EzBob.AmazonDbLib
{
	public class AmazonDatabaseFunctionFactory : IDatabaseFunctionFactory<AmazonDatabaseFunctionType>
	{
		public IDatabaseFunction Create( AmazonDatabaseFunctionType type )
		{
			return AmazonDatabaseFunctionStorage.Instance.GetFunction( type );
		}

		public IDatabaseFunction GetById(Guid id)
		{
			return AmazonDatabaseFunctionStorage.Instance.GetFunctionById( id );
		}
	}
}