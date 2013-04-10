using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace PayPoint
{
    public class PayPointDatabaseFunctionFactory : IDatabaseFunctionFactory<PayPointDatabaseFunctionType>
    {
        public IDatabaseFunction Create(PayPointDatabaseFunctionType type)
        {
            return PayPointDatabaseFunctionStorage.Instance.GetFunction(type);
        }

        public IDatabaseFunction GetById(Guid id)
        {
            return PayPointDatabaseFunctionStorage.Instance.GetFunctionById(id);
        }

        public IEnumerable<IDatabaseFunction> GetAll()
        {
            return PayPointDatabaseFunctionStorage.Instance.AllFunctions();
        }
    }
}