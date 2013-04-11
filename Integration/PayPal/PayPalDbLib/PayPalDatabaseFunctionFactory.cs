using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace EzBob.PayPalDbLib
{
	public class PayPalDatabaseFunctionFactory : IDatabaseFunctionFactory<PayPalDatabaseFunctionType>
	{
		public IDatabaseFunction Create( PayPalDatabaseFunctionType type )
		{
			return PayPalDatabaseFunctionStorage.Instance.GetFunction( type );
		}

		public IDatabaseFunction GetById(Guid id)
		{
			return PayPalDatabaseFunctionStorage.Instance.GetFunctionById( id );
		}

        public IEnumerable<IDatabaseFunction> GetAll()
        {
            return PayPalDatabaseFunctionStorage.Instance.AllFunctions();
        }
	}
}