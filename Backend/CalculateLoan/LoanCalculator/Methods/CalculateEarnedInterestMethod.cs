namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Extensions;

	internal class CalculateEarnedInterestMethod : AWithActualDailyLoanStatusMethod {
		public CalculateEarnedInterestMethod(
			ALoanCalculator calculator,
			DateTime? startDate,
			DateTime? endDate,
			bool writeToLog
		) : base(calculator, writeToLog) {
			this.startDate = startDate;
			this.endDate = endDate;
		} // constructor

		public virtual decimal Execute() {
			WorkingModel.ValidateSchedule();

			DateTime firstDay = (this.startDate ?? WorkingModel.LoanIssueDate).Date;

			DateTime lastDay = (this.endDate ?? WorkingModel.LastScheduledDate).Date;

			DailyLoanStatus days = CreateActualDailyLoanStatus(lastDay);

			decimal earnedInterest = days.Days
				.Where(odls => firstDay <= odls.Date && odls.Date <= lastDay)
				.Sum(odls => odls.DailyInterest);

			if (WriteToLog) {
				days.AddScheduleNotes(WorkingModel);
				days.AddPaymentNotes(WorkingModel);
				days.AddNote(firstDay, "First earned interest period date.");
				days.AddNote(lastDay, "Last earned interest period date.");

				Log.Debug(
					"\n\n{5}.CalculateEarnedInterest - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nEarned interest between {3} and {4}:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\nLoanCalculator.CalculateEarnedInterest - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", earnedInterest.ToString("C2", Library.Instance.Culture)),
					days.ToFormattedString("\t\t"),
					firstDay.DateStr(),
					lastDay.DateStr(),
					Calculator.Name
				);
			} // if

			return earnedInterest;
		} // Execute

		private readonly DateTime? startDate;

		private readonly DateTime? endDate;
	} // class CalculateEarnedInterestMethod
} // namespace
