namespace ExtractDataForLsa {
	using Ezbob.Database;
	using Ezbob.Logger;

	class RptLoansForLsaEmails : AStoredProcedure {
		public RptLoansForLsaEmails(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters
	} // class RptLoansForLsaEmails
} // namespace
