namespace ExtractDataForLsa {
	using Ezbob.Database;
	using Ezbob.Logger;

	class RptLoansForLsaSnailmails : AStoredProcedure {
		public RptLoansForLsaSnailmails(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters
	} // class RptLoansForLsaSnailmails
} // namespace
