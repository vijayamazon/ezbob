namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class APRCalculator {
		public double Calculate(
			decimal amount,
			IEnumerable<LoanScheduleItem> monthlyRepayments,
			decimal setupFee = 0M,
			DateTime? currentDate = null
		) {
			bool accuracyAchieved;
			int iterationCount;
			double lastHitAccuracy;

			return Calculate(
				amount,
				monthlyRepayments,
				setupFee,
				currentDate,
				out accuracyAchieved,
				out iterationCount,
				out lastHitAccuracy
			);
		} // Calculate

		public double Calculate(
			decimal amount,
			IEnumerable<LoanScheduleItem> monthlyRepayments,
			decimal setupFee,
			DateTime? currentDate,
			out bool accuracyAchieved,
			out int iterationCount,
			out double lastHitAccuracy
		) {
			accuracyAchieved = false;
			lastHitAccuracy = -1;

			this.date = currentDate ?? DateTime.Today; // TODO: DateTime.UtcNow.Date if schedules have UTC date.

			double x = 1.0;

			List<LoanScheduleItem> payments = monthlyRepayments.ToList();

			int? accuracyReachedOn = null;

			for (iterationCount = 0; iterationCount < 10000; iterationCount++) {
				double f_x = f(x, amount - setupFee, payments);

				lastHitAccuracy = Math.Abs(f_x);

				if (lastHitAccuracy < 1e-6) {
					accuracyAchieved = true;
					accuracyReachedOn = iterationCount;
					break;
				} // if

				double f_prime_x = f_prime(x, payments);

				double dx = f_x / f_prime_x;

				x -= dx;
			} // for

			double apr = Math.Round(x * 100, 2);

			log.Debug(
				"On {0} APR = {1} (after {2}) with loan amount of {3} and setup fee of {4}.\nAmount due dates:\n\t{5}",
				this.date.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				apr,
				accuracyReachedOn.HasValue
					? "accuracy reached on iteration " + accuracyReachedOn.Value
					: "max iterations limit reached",
				amount.ToString("C2", CultureInfo.InvariantCulture),
				setupFee.ToString("C2", CultureInfo.InvariantCulture),
				string.Join("\n\t", payments.Select(
					sch => string.Format(
						"{0} on {1}, days count = {2}",
						sch.AmountDue.ToString("C2", CultureInfo.InvariantCulture),
						sch.Date.ToString("MMM dd yyyy", CultureInfo.InvariantCulture),
						(sch.Date - this.date).Days
					)
				))
			);

			return apr;
		} // Calculate

		private double f(double x, decimal credit, List<LoanScheduleItem> monthlyRepayments) {
			return
				-(double)credit + (
					from monthlyRepayment in monthlyRepayments
					let t_i = (monthlyRepayment.Date - this.date).Days / 365.0
					select ((double)monthlyRepayment.AmountDue) * Math.Pow(1 + x, -t_i)
				).Sum();
		} // f

		private double f_prime(double x, List<LoanScheduleItem> monthlyRepayments) {
			return (
				from monthlyRepayment in monthlyRepayments
				let t_i = (monthlyRepayment.Date - this.date).Days / 365.00
				select -t_i * ((double)monthlyRepayment.AmountDue) * Math.Pow(1 + x, -t_i + 1)
			).Sum();
		} // f_prime

		private DateTime date;
		private static readonly ASafeLog log = new SafeILog(typeof(APRCalculator));
	} // class APRCalculator
} // namespace
