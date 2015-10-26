namespace Ezbob.Backend.Strategies.NewLoan.DAL {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	// TODO remove static, add :ILoanDAL, add as dependancy injection onn bus refactoring
	public static class LoanDAL {
		
		public static NL_Loans GetLoan(long loanID) {
			try {
				return Library.Instance.DB.FillFirst<NL_Loans>("NL_LoansGet", CommandSpecies.StoredProcedure, new QueryParameter("@loanID", loanID));
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Library.Instance.Log.Alert("{0}", e.Message);
			}
			return null;
		}

		public static List<NL_LoanHistory> GetLoanHistories(long loanID, DateTime stateDate) {
			try {
				return Library.Instance.DB.Fill<NL_LoanHistory>("NL_LoanHistoryGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID",loanID),
					new QueryParameter("@Now", stateDate)
				);
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Library.Instance.Log.Alert("{0}", e.Message);
			}
			return null;
		}
	}
}
