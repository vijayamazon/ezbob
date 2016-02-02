namespace Ezbob.Backend.Strategies.NewLoan.DAL {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class LoanDAL :ILoanDAL {

		NL_Loans ILoanDAL.GetLoan(long loanID) {
			try {
				return Library.Instance.DB.FillFirst<NL_Loans>("NL_LoansGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", loanID));
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Library.Instance.Log.Alert("{0}", e.Message);
			}
			return null;
		}

		public List<NL_LoanHistory> GetLoanHistories(long loanID, DateTime stateDate) {
			try {
				return Library.Instance.DB.Fill<NL_LoanHistory>("NL_LoanHistoryGet", CommandSpecies.StoredProcedure,new QueryParameter("LoanID", loanID));
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Library.Instance.Log.Alert("{0}", e.Message);
			}
			return null;
		}
	}
}
