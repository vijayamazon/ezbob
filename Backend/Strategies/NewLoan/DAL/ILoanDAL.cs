namespace Ezbob.Backend.Strategies.NewLoan.DAL {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public interface ILoanDAL {

		 NL_Loans GetLoan(long loanID);

		List<NL_LoanHistory> GetLoanHistories(long loanID, DateTime stateDate);
	}
}
