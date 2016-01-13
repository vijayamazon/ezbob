﻿namespace ExtractDataForLsa {
	using Ezbob.Database;
	using Ezbob.Logger;

	class RptLoansForLsaDirectors : AStoredProcedure {
		public RptLoansForLsaDirectors(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters
	} // class RptLoansForLsaDirectors
} // namespace
