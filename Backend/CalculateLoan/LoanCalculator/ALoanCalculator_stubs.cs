namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System.Collections.Generic;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;

	public abstract partial class ALoanCalculator {
		// TODO: missing functionality

		/// <summary>
		/// Creates loan schedule by rescheduling time .............
		/// Also calculates loan plan.
		/// Schedule is stored in WorkingModel.Schedule ????????????????
		/// </summary>
		public virtual List<ScheduledItemWithAmountDue> RescheduleToIntervals(
			ReschedulingArgument reschedulingArgument,
			bool writeToLog = true
		) {
			return new RescheduleToIntervalsMethod(this, reschedulingArgument, writeToLog).Execute();
		} // RescheduleToIntervals
	} // class ALoanCalculator
} // namespace
