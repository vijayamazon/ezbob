namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AssignPaymentToLoan : AStrategy {
		public AssignPaymentToLoan(NL_Model nlModel) {
			NLModel = nlModel;
		} // constructor

		public override string Name { get { return "AssignPaymentToLoan"; } }

		public NL_Model NLModel { get; private set; }

		/// <summary>
		/// Strategy makes (and removes) assignment of payment to loan. 
		/// 
		/// Expected argument (input data) - NL_Model with Loan.LoanID,
		/// PaymentToAssign (logic payment to assign, in NL_Model).
		/// 
		/// Expected result: NL_Model containing: 
		///	1. PaymentAssignedToLoanFees - the list of existing fees, covered by the payment (in NL_Model).
		/// 2. PaymentAssignedToScheduleItems - the list of closest updated schedule items covered
		/// by the payment (in NL_Model).
		/// </summary>
		public override void Execute() {
			LoanState<NL_Model> stateStrategy = new LoanState<NL_Model>(
				new NL_Model(),
				NLModel.Loan.LoanID,
				NLModel.CustomerID,
				DateTime.UtcNow
			);
			stateStrategy.Execute();

			// TODO: choose proper calculator
			ALoanCalculator calculator = new LegacyLoanCalculator(stateStrategy.CalcModel);

			ConnectionWrapper pconn = DB.GetPersistent();
			pconn.BeginTransaction();

			// save schedulePayment1, schedulePayment2, feePayment

			pconn.Commit();

			// into catch
			//pconn.Rollback();
		} // Execute
	} // class AssignPaymentToLoan
} // namespace