using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace Integration.Play {
	public class PlayDatabaseFunctionFactory : IDatabaseFunctionFactory<PlayDatabaseFunctionType> {
		public IDatabaseFunction Create(PlayDatabaseFunctionType type) {
			return PlayDatabaseFunctionStorage.Instance.GetFunction(type);
		} // Create

		public IDatabaseFunction GetById(Guid id) {
			return PlayDatabaseFunctionStorage.Instance.GetFunctionById(id);
		} // GetById

		public IEnumerable<IDatabaseFunction> GetAll() {
			return PlayDatabaseFunctionStorage.Instance.AllFunctions();
		} // GetAll
	} // class PlayDatabaseFunctionFactory
} // namespace