using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace CompanyFiles
{
    public class CompanyFilesDatabaseFunctionFactory : IDatabaseFunctionFactory<CompanyFilesDatabaseFunctionType>
    {
        public IDatabaseFunction Create(CompanyFilesDatabaseFunctionType type)
        {
            return CompanyFilesDatabaseFunctionStorage.Instance.GetFunction(type);
        }

        public IDatabaseFunction GetById(Guid id)
        {
            return CompanyFilesDatabaseFunctionStorage.Instance.GetFunctionById(id);
        }

        public IEnumerable<IDatabaseFunction> GetAll()
        {
            return CompanyFilesDatabaseFunctionStorage.Instance.AllFunctions();
        }
    }
}