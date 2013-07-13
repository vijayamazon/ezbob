namespace Sage
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.Functions;

	public class SageDatabaseFunctionFactory : IDatabaseFunctionFactory<SageDatabaseFunctionType>
    {
		public IDatabaseFunction Create(SageDatabaseFunctionType type)
        {
			return SageDatabaseFunctionStorage.Instance.GetFunction(type);
        }

        public IDatabaseFunction GetById(Guid id)
        {
			return SageDatabaseFunctionStorage.Instance.GetFunctionById(id);
        }

        public IEnumerable<IDatabaseFunction> GetAll()
        {
			return SageDatabaseFunctionStorage.Instance.AllFunctions();
        }
    }
}