﻿namespace ExtractDataForLsa {
	using Ezbob.Database;
	using Ezbob.Logger;

	class RptLoansForLsaCRM : AStoredProcedure {
		public RptLoansForLsaCRM(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters
	} // class RptLoansForLsaCRM
} // namespace
