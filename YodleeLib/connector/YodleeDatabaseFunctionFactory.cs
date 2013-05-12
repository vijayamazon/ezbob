using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace YodleeLib.connector
{
    public class YodleeDatabaseFunctionFactory : IDatabaseFunctionFactory<YodleeDatabaseFunctionType>
    {
        public IDatabaseFunction Create(YodleeDatabaseFunctionType type)
        {
            return YodleeDatabaseFunctionStorage.Instance.GetFunction(type);
        }

        public IDatabaseFunction GetById(Guid id)
        {
            return YodleeDatabaseFunctionStorage.Instance.GetFunctionById(id);
        }

        public IEnumerable<IDatabaseFunction> GetAll()
        {
            return YodleeDatabaseFunctionStorage.Instance.AllFunctions();
        }
    }
}