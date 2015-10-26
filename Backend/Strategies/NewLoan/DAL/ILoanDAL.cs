namespace Ezbob.Backend.Strategies.NewLoan.DAL {
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public interface ILoanDAL {

		 NL_Loans GetLoan(long loanID);
	}
}
