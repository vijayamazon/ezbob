namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Extensions;

	internal class CalculateBalanceMethod : AWithActualDailyLoanStatusMethod {
		public CalculateBalanceMethod(
			ALoanCalculator calculator,
			DateTime today,
			bool writeToLog
		) : base(calculator, writeToLog) {
			this.today = today.Date;
		} // constructor

		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="WrongInstallmentOrderException">Condition. </exception>
		/// <exception cref="WrongFirstOpenPrincipalException">Condition. </exception>
		/// <exception cref="TooLateOpenPrincipalException">Condition. </exception>
		/// <exception cref="WrongOpenPrincipalOrderException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
		public virtual decimal Execute() {
			if (this.today <= WorkingModel.LoanIssueDate)
				return 0;

			DailyLoanStatus days = CreateActualDailyLoanStatus(this.today);

			decimal balance = days[this.today].CurrentBalance;

			if (WriteToLog) {
				days.AddScheduleNotes(WorkingModel);
				days.AddFeeNotes(WorkingModel);
				days.AddPaymentNotes(WorkingModel);
				days.AddNote(this.today, "Requested balance date.");

				Log.Debug(
					"\n\n{4}.CalculateBalance - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nBalance on {3}:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\n{4}.CalculateBalance - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", balance.ToString("C2", Library.Instance.Culture)),
					days.ToFormattedString("\t\t"), this.today.DateStr(),
					Calculator.Name
				);
			} // if

			return balance;
		} // Execute

		private readonly DateTime today;
	} // class CalculateBalanceMethod
} // namespace
