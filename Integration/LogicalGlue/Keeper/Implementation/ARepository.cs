namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal abstract class ARepository<TMember> : List<TMember> where TMember: class, new() {
		public virtual void Load() {
			Clear();

			LoadAllStoredProc.ForEachRowSafe(sr => Add(sr.Stuff<TMember>()));
		} // Load

		protected ARepository(AConnection db, ASafeLog log) {
			DB = db;
			Log = log;
		} // constructor

		protected abstract ALogicalGlueStoredProc LoadAllStoredProc { get; }

		protected AConnection DB { get; private set; }
		protected ASafeLog Log { get; private set; }
	} // class ARepository
} // namespace
