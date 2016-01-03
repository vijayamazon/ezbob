namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Linq;
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

			for (iterationCount = 0; iterationCount < 10000; iterationCount++) {
				double f_x = f(x, amount - setupFee, payments);

				lastHitAccuracy = Math.Abs(f_x);

				if (lastHitAccuracy < 1e-6) {
					accuracyAchieved = true;
					break;
				} // if

				double f_prime_x = f_prime(x, payments);

				double dx = f_x / f_prime_x;

				x -= dx;
			} // for

			return Math.Round(x * 100, 2);
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
	} // class APRCalculator
} // namespace
