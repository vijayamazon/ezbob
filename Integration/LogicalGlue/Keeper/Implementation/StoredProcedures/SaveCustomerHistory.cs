namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	class SaveCustomerHistory : ALogicalGlueStoredProc {
		public SaveCustomerHistory(long responseID, AConnection db, ASafeLog log) : base(db, log) {
			ResponseID = responseID;
		} // constructor

		public override bool HasValidParameters() {
			return ResponseID > 0;
		} // HasValidParameters

		public long ResponseID { get; set; }
	} // class SaveCustomerHistory
} // namespace
