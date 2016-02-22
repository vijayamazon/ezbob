namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class ALogicalGlueStoredProc : AStoredProcedure {
		protected ALogicalGlueStoredProc(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		/// <summary>
		/// Returns the name of the stored procedure.
		/// Stored procedure name is "LogicalGlue" + current class name.
		/// </summary>
		/// <returns>SP name.</returns>
		protected override string GetName() {
			return "LogicalGlue" + GetType().Name;
		} // GetName
	} // class ALogicalGlueStoredProc
} // namespace
