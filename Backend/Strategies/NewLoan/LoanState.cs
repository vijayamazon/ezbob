namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EZBob.DatabaseLib.Model.Database.Loans;

	//public enum BadCustomerStatuses {
	//	// name = Id in [dbo].[CustomerStatuses]
	//	Default = 1,
	//	Bad = 7,
	//	WriteOff = 8,
	//	DebtManagement = 9,
	//	Liquidation = 24,
	//	Bankruptcy = 26,
	//} // enum CustomerStatus

	/// <summary>
	/// Transform Loan or NL_Model to LoanCalculatorModel
	/// </summary>
	/// <typeparam name="T">Loan or NL_Model</typeparam>
	public class LoanState<T> : AStrategy {

		public LoanState(T t, int loanID, DateTime? stateDate) {

			this.loanID = loanID;

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
				this.tNLLoan = null;

			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
				this.tLoan = null;
			}

			StateDate = stateDate ?? DateTime.UtcNow;
		}

		public override string Name { get { return "LoanState"; } }

		public DateTime StateDate { get; set; }

		public override void Execute() {
			

		}

	

		private Loan tLoan;
		private readonly NL_Model tNLLoan;
		private readonly int loanID;

		private int customerID;

		// result
		public NL_Model CalcModel;

	}
}