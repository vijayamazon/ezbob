namespace ExtractDataForLsa {
	using Ezbob.Database;
	using Ezbob.Logger;

	class RptLoansForLsaAgreementsBasePaths : AStoredProcedure {
		public RptLoansForLsaAgreementsBasePaths(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters
	} // class RptLoansForLsaAgreementsBasePaths
} // namespace
