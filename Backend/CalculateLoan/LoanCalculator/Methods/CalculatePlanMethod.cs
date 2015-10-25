namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	internal class CalculatePlanMethod : AMethod {
		/// <exception cref="NoInitialDataException">Condition. </exception>
		//ALoanCalculator calculator, bool writeToLog) : base(calculator, writeToLog) {
		public CalculatePlanMethod(ALoanCalculator calculator, NL_Model loanModel)
			: base(calculator, false) {

			if (loanModel == null)
				throw new NoInitialDataException();

			if (loanModel.Loan == null)
				throw new NoInitialDataException();

			this.loanModel = loanModel;

			DailyLoanStatus = new DailyLoanStatus();
		} // constructor

		private readonly NL_Model loanModel;

		/*public CalculatePlanMethod(ALoanCalculator calculator): base(calculator) {

			
		} // constructor*/

		// TODO: revive
		public DailyLoanStatus DailyLoanStatus { get; private set; }

		// new version, with NL model
		/*public virtual void Execute() {

			NL_LoanHistory history = this.loanModel.Loan.LastHistory();

			if (history == null) {
				throw new NoInitialDataException();
			}

			DateTime firstInterestDay = this.loanModel.Loan.FirstHistory().EventTime.Date.AddDays(1);
			DateTime lastInterestDay = DateTime.UtcNow;

			var lastSchedule = this.loanModel.Loan.LastHistory().Schedule.OrderBy(s => s.PlannedDate).LastOrDefault();
			if (lastSchedule != null)
				lastInterestDay = lastSchedule.PlannedDate;

			// fill in loan period (since first interest day until last interest day) with "one day" loan status
			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				DailyLoanStatus.Add(new OneDayState(d, this.loanModel.Loan.LastHistory().Amount, DailyLoanStatus.LastDailyLoanStatus));

			// init prevTime by loan issue date
			DateTime prevTime = this.loanModel.Loan.FirstHistory().EventTime; // WorkingModel.LoanIssueTime;

			List<NL_LoanSchedules> schedule = new List<NL_LoanSchedules>();

			foreach (var h in this.loanModel.Loan.Histories) {
				schedule.AddRange(h.ActiveSchedule());
			}
			
			//for (int i = 0; i < schedule.Count; i++) {

			//	//ScheduledItem scheduleItem = WorkingModel.Schedule[i];
			//	NL_LoanSchedules scheduleItem = schedule[i];

			//	// decrease "one day" loan status entries by schedule planned Principal to be paid at schedule planned Date
			//	foreach (OneDayState dayState in DailyLoanStatus.Where(dd => dd.Date >= scheduleItem.PlannedDate)) {

			//		if (dayState.Date != scheduleItem.PlannedDate)
			//			dayState.OpenPrincipalForInterest -= scheduleItem.Principal;

			//		dayState.OpenPrincipalAfterRepayments -= scheduleItem.Principal;
			//	} 

			//	DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

			//	foreach (OneDayState cls in DailyLoanStatus.Where(cls => preScheduleEnd < cls.Date && cls.Date <= scheduleItem.Date)) {
			//		cls.DailyInterestRate = Calculator.CalculateDailyInterestRate( //GetDailyInterestRate(
			//			cls.Date,
			//			scheduleItem.InterestRate,
			//			false, // considerBadPeriods
			//			false, // considerFreezeInterestPeriod
			//			preScheduleEnd,
			//			scheduleItem.PlannedDate
			//		);
			//	} // for each

			//	prevTime = scheduleItem.PlannedDate;
			//} // for each scheduled payment

		} */// Execute

		public virtual void /* List<Repayment> */ _Execute() {
			// TODO: revive

			// checking Schedule.count >0 
			// checking order of Schedule item (by Date)
			//WorkingModel.ValidateSchedule();  // \ezbob\Backend\CalculateLoan\Models\LoanCalculatorModel.cs line 80-99

			/*DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.LastScheduledDate;

			// fill in loan period (since first interest day until last interest day) with "one day" loan status
			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				DailyLoanStatus.Add(new OneDayLoanStatus(d, WorkingModel.LoanAmount, DailyLoanStatus.LastDailyLoanStatus));

			// init prevTime by loan issue date
			DateTime prevTime = WorkingModel.LoanIssueTime;

			// 
			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				ScheduledItem scheduleItem = WorkingModel.Schedule[i];

				// decrease "one day" loan status entries by schedule planned Principal to be paid at schedule planned Date
				foreach (OneDayLoanStatus cls in DailyLoanStatus.Where(dd => dd.Date >= scheduleItem.Date)) {
					if (cls.Date != scheduleItem.Date)
						cls.OpenPrincipalForInterest -= scheduleItem.Principal;

					cls.OpenPrincipalAfterRepayments -= scheduleItem.Principal;
				} // for each

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (OneDayLoanStatus cls in DailyLoanStatus.Where(cls => preScheduleEnd < cls.Date && cls.Date <= scheduleItem.Date)) {
					cls.DailyInterestRate = Calculator.GetDailyInterestRate(
						cls.Date,
						scheduleItem.InterestRate,
						false, // considerBadPeriods
						false, // considerFreezeInterestPeriod
						preScheduleEnd,
						scheduleItem.Date
					);
				} // for each

				prevTime = scheduleItem.Date;
			} // for each scheduled payment

			var result = new List<Repayment>(
				WorkingModel.Schedule.Select(sp => new Repayment(sp.Date, sp.Principal, 0, 0))
			);

			prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < result.Count; i++) {
				Repayment r = result[i];

				DateTime dt = prevTime; // This assignment is to prevent "access to modified closure" warning.

				r.Interest = DailyLoanStatus.Where(cls => dt < cls.Date && cls.Date <= r.Time).Sum(cls => cls.DailyInterest);

				prevTime = r.Time;
			} // for

			foreach (Fee fee in WorkingModel.Fees) {
				if (fee.FeeType == FeeTypes.SetupFee)
					continue;

				Repayment scheduledPaymentToPayFee =
					result.FirstOrDefault(sp => sp.Date >= fee.AssignDate) ??
					result.Last();

				if (scheduledPaymentToPayFee == null) { // Should never happen but just in case...
					Library.Instance.Log.Alert(
						"{0}.CreatePlan: failed to find a scheduled payment to pay a fee.\n\tFee is: {1}." +
						"\n\tLoan calculator model:\n{2}",
						Calculator.Name,
						fee,
						WorkingModel
					);

					continue;
				} // if

				scheduledPaymentToPayFee.Fees += fee.Amount;
			} // for each fee

			if (WriteToLog) {
				DailyLoanStatus.AddScheduleNotes(WorkingModel);

				Log.Debug(
					"\n\n{3}.CreatePlan - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nLoan plan:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\n{3}.CreatePlan - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", result),
					DailyLoanStatus.ToFormattedString("\t\t"),
					Calculator.Name
				);
			} // if

			return result;*/

		} // Execute
	} // class CalculatePlanMethod
} // namespace
