namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	public abstract partial class ALoanCalculator {
		// TODO: missing functionality


		
		///// <exception cref="ArgumentNullException">The RepaymentIntervalTypes.Week  parameter is null. </exception>
		/*public virtual List<ScheduledItemWithAmountDue> bkp_RescheduleToIntervals(ReschedulingArgument reschedulingArgument) {

			List<ScheduledItemWithAmountDue> newItems = new List<ScheduledItemWithAmountDue>();

			if (reschedulingArgument.ReschedulingBalance != null) {

				WorkingModel.LoanAmount = (decimal)reschedulingArgument.ReschedulingBalance;

				// "loan maturity date" i.e. planned loan close date - today)/interval type

				if (reschedulingArgument.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month) {

					var intervalsNum = MiscUtils.DateDiffInMonths(reschedulingArgument.ReschedulingDate, reschedulingArgument.LoanCloseDate);

					Console.WriteLine("months intervals for balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}", reschedulingArgument.ReschedulingBalance, intervalsNum,
						reschedulingArgument.LoanCloseDate, reschedulingArgument.ReschedulingDate);

					decimal intervalPrincipal = (decimal)(reschedulingArgument.ReschedulingBalance / intervalsNum);

					WorkingModel.RepaymentCount = intervalsNum;

					for (int shPositions = 1; shPositions <= intervalsNum; shPositions++) {
						ScheduledItemWithAmountDue schi = new ScheduledItemWithAmountDue(shPositions, reschedulingArgument.ReschedulingDate.AddMonths(shPositions), intervalPrincipal, WorkingModel.MonthlyInterestRate, 14);
						newItems.Add(schi);
					}
				}

				if (reschedulingArgument.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Week) {

					var intervalsNum = MiscUtils.DateDiffInWeeks(reschedulingArgument.ReschedulingDate, reschedulingArgument.LoanCloseDate);

					Console.WriteLine("weeks intervals for balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}", reschedulingArgument.ReschedulingBalance, intervalsNum,
						reschedulingArgument.LoanCloseDate, reschedulingArgument.ReschedulingDate);

					decimal intervalPrincipal = (decimal)(reschedulingArgument.ReschedulingBalance / intervalsNum);
					decimal intervalInterestRate = 0;
					try {
						intervalInterestRate = WorkingModel.MonthlyInterestRate / UInt32.Parse(RepaymentIntervalTypes.Week.ToString());
					} catch (OverflowException overflowException) {
						Console.WriteLine(overflowException);
					}

					DateTime pdate = reschedulingArgument.ReschedulingDate;

					for (int shPositions = 1; shPositions <= intervalsNum; shPositions++) {
						newItems.Add(new ScheduledItemWithAmountDue(shPositions, pdate.AddDays(7), intervalPrincipal, intervalInterestRate, 14));
					}
				}
			}

			Console.WriteLine("------------------------WORKING MODEL--------------------");
			Console.WriteLine(WorkingModel.ToString());

			Console.WriteLine("------------------------NEW SCHEDULE ITEMS--------------------");
			newItems.ForEach(Console.WriteLine);

			return newItems;

		} */// RescheduleToIntervals

	} // class ALoanCalculator
} // namespace
