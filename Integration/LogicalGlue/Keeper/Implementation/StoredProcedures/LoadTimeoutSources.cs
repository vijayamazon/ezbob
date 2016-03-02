namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadTimeoutSources : ALogicalGlueStoredProc {
		public LoadTimeoutSources(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters
	} // class LoadTimeoutSources
} // namespace
