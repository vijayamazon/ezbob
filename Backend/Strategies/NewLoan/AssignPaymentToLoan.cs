namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AssignPaymentToLoan : AStrategy {

		public AssignPaymentToLoan(NL_Model nlModel) {

			NLModel = nlModel;

			this.userID = NLModel.UserID;
			
		}//constructor

		public override string Name { get { return "AssignPaymentToLoan"; } }

		/// <summary>
		/// Strategy makes (and remove) assignment of payment to loan. 
		/// 
		/// Expected argument (input data) - NL_Model with Loan.LoanID, PaymentToAssign (logic payment to assign, in NL_Model)
		/// 
		/// Expected result: NL_Model contaning: 
		///		1. PaymentAssignedToLoanFees - the list of existing fees, covered by the payment (in NL_Model)
		/// 	2. PaymentAssignedToScheduleItems - the list of closest updaid schedule items covered by the payment (in NL_Model)
		/// </summary>
		public override void Execute() {

			// loan loan state
			LoanState<NL_Model> stateStrategy = new LoanState<NL_Model>(new NL_Model(), NLModel.Loan.LoanID, NLModel.CustomerID, DateTime.UtcNow);
			stateStrategy.Execute();
			ALoanCalculator calculator = new LegacyLoanCalculator(stateStrategy.CalcModel);
		//	calculator.


			// check payment - validate before usage -  already assigned / to assign, to cancel, etc. 

			// distribution logic

			// results
			// payment covering whole schedule item(s) or fee(s) OR partial

			ConnectionWrapper pconn = DB.GetPersistent();
			pconn.BeginTransaction();

			NL_LoanSchedulePayments schedulePayment1 = new NL_LoanSchedulePayments() {LoanScheduleID = 1,PaymentID = NLModel.PaymentToAssign.PaymentID,InterestPaid = 20,PrincipalPaid = 5};
			NL_LoanSchedulePayments schedulePayment2 = new NL_LoanSchedulePayments() {LoanScheduleID = 1,InterestPaid = 20,PrincipalPaid = 5};

			NL_LoanFeePayments feePayment = new NL_LoanFeePayments() { LoanFeeID = 1, PaymentID = NLModel.PaymentToAssign.PaymentID, Amount = 7 };

			// save schedulePayment1, schedulePayment2, feePayment

			pconn.Commit();

			// into catch
			//pconn.Rollback();

		} // Execute

		private readonly int? userID;
		public NL_Model NLModel { get; private set; }

	} // class AssignPaymentToLoan
} // ns