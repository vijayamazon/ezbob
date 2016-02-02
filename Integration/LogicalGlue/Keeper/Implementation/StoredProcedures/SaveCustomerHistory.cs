namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
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

		public DateTime Now {
			get { return DateTime.UtcNow; }
			// ReSharper disable once ValueParameterNotUsed
			set { }
		} // Now
	} // class SaveCustomerHistory
} // namespace
