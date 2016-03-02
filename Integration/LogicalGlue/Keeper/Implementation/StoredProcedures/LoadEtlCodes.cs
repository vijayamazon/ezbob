namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadEtlCodes : ALogicalGlueStoredProc {
		public LoadEtlCodes(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters
	} // class LoadEtlCodes
} // namespace
