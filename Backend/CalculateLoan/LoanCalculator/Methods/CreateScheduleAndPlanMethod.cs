namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	internal class CreateScheduleAndPlanMethod : AMethod {
		public CreateScheduleAndPlanMethod(
			ALoanCalculator calculator,
			NL_Model loanToCreateModel,
			bool writeToLog
		) : base(calculator, writeToLog) {
			this.loanToCreateModel = loanToCreateModel;
			this.offerFees = GetOfferFees();
		} // constructor

		public List<ScheduledItemWithAmountDue> Execute() {
			new CreateScheduleMethod(Calculator, this.loanToCreateModel).Execute();

			var method = new CalculatePlanMethod(Calculator, false);

			List<Repayment> payments = method.Execute();

			var result = new List<ScheduledItemWithAmountDue>();

			// TODO: revive

			/*

			for (int i = 0; i < payments.Count; i++) {
				ScheduledItem si = WorkingModel.Schedule[i];
				Repayment payment = payments[i];

				result.Add(new ScheduledItemWithAmountDue(
					i + 1,
					si.Date,
					payment.Principal,
					si.InterestRate,
					payment.Interest,
					payment.Fees
				));
			} // for each

			if (WriteToLog) {
				Log.Debug(
					"\n\n{3}.CreateScheduleAndPlan - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nSchedule + plan:\n\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\n{3}.CreateScheduleAndPlan - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t", result),
					method.DailyLoanStatus.ToFormattedString("\t\t"),
					Calculator.Name
				);
			} // if

			*/

			return result;
		} // Execute

		private List<OfferFee> GetOfferFees() {
			var result = new List<OfferFee>();

			if ((loanToCreateModel.Fees != null) && (loanToCreateModel.Fees.Count > 0)) {
				foreach (var ff in loanToCreateModel.Fees) {
					NL_OfferFees assignedFee = ff.OfferFees;

					if (assignedFee == null)
						continue;

					var offerFee = new OfferFee(
						(FeeTypes)assignedFee.LoanFeeTypeID,
						WorkingModel.LoanHistory.Last().Amount,
						assignedFee.Percent,
						assignedFee.AbsoluteAmount,
						assignedFee.OneTimePartPercent,
						assignedFee.DistributedPartPercent
					);

					if (offerFee.Amount > 0)
						this.offerFees.Add(offerFee);
				} // for each fee
			} // if

			return result;
		} // GetOfferFees

		private readonly NL_Model loanToCreateModel;
		private readonly List<OfferFee> offerFees;
	} // class CreateScheduleAndPlanMethod
} // namespace
