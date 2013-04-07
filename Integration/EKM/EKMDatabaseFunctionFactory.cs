using System;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace EKM
{
    public class EKMDatabaseFunctionFactory : IDatabaseFunctionFactory<EkmDatabaseFunctionType>
    {
        public IDatabaseFunction Create(EkmDatabaseFunctionType type)
        {
            return EKMDatabaseFunctionStorage.Instance.GetFunction(type);
        }

        public IDatabaseFunction GetById(Guid id)
        {
            return EKMDatabaseFunctionStorage.Instance.GetFunctionById(id);
        }
    }
}