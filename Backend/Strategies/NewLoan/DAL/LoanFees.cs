namespace Ezbob.Backend.Strategies.NewLoan.DAL {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class LoanFees : ILoanFees {

		public List<NL_LoanFees> GetLoanFees(long loanID) {
			try {
				return Library.Instance.DB.Fill<NL_LoanFees>("NL_LoanFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", loanID));
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Library.Instance.Log.Alert("{0}", e.Message);
			}
			return null;
		}
	}
}
