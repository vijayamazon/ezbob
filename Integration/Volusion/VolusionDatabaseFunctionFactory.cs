﻿using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace Integration.Volusion
{
    public class VolusionDatabaseFunctionFactory : IDatabaseFunctionFactory<VolusionDatabaseFunctionType>
    {
        public IDatabaseFunction Create(VolusionDatabaseFunctionType type)
        {
            return VolusionDatabaseFunctionStorage.Instance.GetFunction(type);
        }

        public IDatabaseFunction GetById(Guid id)
        {
            return VolusionDatabaseFunctionStorage.Instance.GetFunctionById(id);
        }

        public IEnumerable<IDatabaseFunction> GetAll()
        {
            return VolusionDatabaseFunctionStorage.Instance.AllFunctions();
        }
    }
}