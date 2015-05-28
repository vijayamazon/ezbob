namespace Ezbob.Backend.Strategies.NewLoan {
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class AssignPaymentToLoan : AStrategy {
		public AssignPaymentToLoan(int userID, NL_Model nlModel) {
			this.userID = userID;
			NLModel = nlModel;
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

			// check loan

			// check payment - validate before usage - isActive, already assigned / to assign, to cancel, etc. 

			// distribution logic

			// results
			// payment covering whole schedule item(s) or fee(s) OR partial

			NLModel.PaymentAssignedToScheduleItems.Add(new NL_LoanSchedulePayments() { LoanScheduleID = 1, PaymentID = NLModel.PaymentToAssign.PaymentID, InterestPaid = 20, PrincipalPaid = 5  });
			NLModel.PaymentAssignedToScheduleItems.Add(new NL_LoanSchedulePayments() { LoanScheduleID = 1, InterestPaid = 20, PrincipalPaid = 5 });

			NLModel.PaymentAssignedToLoanFees.Add(new NL_LoanFeePayments() { LoanFeeID = 1, PaymentID = NLModel.PaymentToAssign.PaymentID, Amount = 7 });

		} // Execute

		private readonly int userID;
		public NL_Model NLModel { get; private set; }

	} // class AssignPaymentToLoan
} // ns