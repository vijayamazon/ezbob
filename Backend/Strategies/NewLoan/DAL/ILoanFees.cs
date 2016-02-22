namespace Ezbob.Backend.Strategies.NewLoan.DAL {
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public interface ILoanFees {

		List<NL_LoanFees> GetLoanFees(long loanID);
	}
}