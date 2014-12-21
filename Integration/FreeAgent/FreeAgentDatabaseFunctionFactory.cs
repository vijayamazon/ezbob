namespace FreeAgent {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.Functions;

	public class FreeAgentDatabaseFunctionFactory : IDatabaseFunctionFactory<FreeAgentDatabaseFunctionType> {
		public IDatabaseFunction Create(FreeAgentDatabaseFunctionType type) {
			return FreeAgentDatabaseFunctionStorage.Instance.GetFunction(type);
		}

		public IDatabaseFunction GetById(Guid id) {
			return FreeAgentDatabaseFunctionStorage.Instance.GetFunctionById(id);
		}

		public IEnumerable<IDatabaseFunction> GetAll() {
			return FreeAgentDatabaseFunctionStorage.Instance.AllFunctions();
		}
	}
}