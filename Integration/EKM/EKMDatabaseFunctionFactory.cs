using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace EKM
{
    public class EKMDatabaseFunctionFactory : IDatabaseFunctionFactory<EkmDatabaseFunctionType>
    {
        public IDatabaseFunction Create(EkmDatabaseFunctionType type)
        {
            return EkmDatabaseFunctionStorage.Instance.GetFunction(type);
        }

        public IDatabaseFunction GetById(Guid id)
        {
            return EkmDatabaseFunctionStorage.Instance.GetFunctionById(id);
        }

        public IEnumerable<IDatabaseFunction> GetAll()
        {
            return EkmDatabaseFunctionStorage.Instance.AllFunctions();
        }
    }
}