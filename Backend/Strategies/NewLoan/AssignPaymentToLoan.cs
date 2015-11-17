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

		//public void SetScheduleCloseDatesFromPayments() {
		//	ValidateSchedule();

		//	var qsp = new Queue<ScheduledItem>();

		//	foreach (ScheduledItem s in Schedule) {
		//		s.ClearRepayments();
		//		qsp.Enqueue(s);
		//	} // for each

		//	if (Repayments.Count < 1)
		//		return;

		//	ScheduledItem curSchedule = qsp.Dequeue();

		//	for (var i = 0; i < Repayments.Count; i++) {
		//		Repayment curRepayment = Repayments[i];

		//		decimal currentPaidPrincipal = curRepayment.Principal;

		//		while (currentPaidPrincipal > 0) {
		//			currentPaidPrincipal = curSchedule.AddPrincipalRepayment(currentPaidPrincipal, curRepayment.Date);

		//			if (curSchedule.ClosedDate.HasValue) {
		//				if (qsp.Count > 0)
		//					curSchedule = qsp.Dequeue();

		//				else {
		//					curSchedule = null;
		//					break;
		//				} // if
		//			} // if
		//		} // while

		//		if (curSchedule == null)
		//			break;
		//	} // for each repayment
		//} // SetScheduleCloseDatesFromPayments

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
            NL_AddLog(LogType.Info, "Strategy Start", this.NLModel, null, null, null);
            try {
			/*
			 * TODO
			 * 
			 * var stateStrategy = new GetLoanDBState(
				new NL_Model(NLModel.CustomerID),
				NLModel.Loan.LoanID,
		NLModel.CustomerID,
				DateTime.UtcNow
			);
			stateStrategy.Execute();*/

			// TODO: choose proper calculator
			// ALoanCalculator calculator = new LegacyLoanCalculator(stateStrategy.CalcModel);

			ConnectionWrapper pconn = DB.GetPersistent();
			pconn.BeginTransaction();

			// save schedulePayment1, schedulePayment2, feePayment

			pconn.Commit();

			// into catch
			//pconn.Rollback();
            NL_AddLog(LogType.Info, "Strategy End",null,this.NLModel, null, null);
            }
            catch (Exception ex)
            {
                NL_AddLog(LogType.Error, "Strategy Faild", this.NLModel,null, ex.ToString(), ex.StackTrace);
            }
		} 
            // Execute
	} // class AssignPaymentToLoan
} // namespace