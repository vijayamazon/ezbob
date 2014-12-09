using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;

namespace PaymentServices.Calculators
{
	public class APRCalculator
	{
		private static DateTime _date = DateTime.Today;

		private static double f(double x, decimal credit, IEnumerable<LoanScheduleItem> monthlyRepayments)
		{
			return -(double)credit +
				   (from monthlyRepayment in monthlyRepayments
					let t_i = (monthlyRepayment.Date - _date).Days / 365.0
					select ((double)monthlyRepayment.AmountDue) * Math.Pow(1 + x, -t_i)).Sum();
		}

		private static double f_prime(double x, IEnumerable<LoanScheduleItem> monthlyRepayments)
		{
			return (from monthlyRepayment in monthlyRepayments
					let t_i = (monthlyRepayment.Date - _date).Days / 365.00
					select -t_i * ((double)monthlyRepayment.AmountDue) * Math.Pow(1 + x, -t_i + 1)).Sum();
		}

		public double Calculate(decimal amount, IEnumerable<LoanScheduleItem> monthlyRepayments, decimal setupFee = 0M,
								DateTime? date = null)
		{
			_date = date ?? DateTime.Today;

			var x = 1.0;
			for (var i = 0; i < 10000; i++)
			{
				var f_x = f(x, amount - setupFee, monthlyRepayments);
				if (Math.Abs(f_x) < 1e-6)
					break;
				var f_prime_x = f_prime(x, monthlyRepayments);
				var dx = f_x / f_prime_x;
				x -= dx;
			}
			return Math.Round(x * 100, 2);
		}

		public double CalculateMonthly(decimal amount, List<LoanScheduleItem> monthlyRepayments, int month, decimal setupFee = 0M,
									   DateTime? date = null)
		{
			decimal monthlyRate = monthlyRepayments[month].InterestRate;

			var monthlyRepaimentsCalculatedForMonthlyRate = monthlyRepayments.Select(monthlyRepayment => new LoanScheduleItem
				{
					Interest = monthlyRepayment.BalanceBeforeRepayment*monthlyRate,
					AmountDue = monthlyRepayment.LoanRepayment + (monthlyRepayment.BalanceBeforeRepayment * monthlyRate), 
					Date = monthlyRepayment.Date
				}).ToList();

			return Calculate(amount, monthlyRepaimentsCalculatedForMonthlyRate, setupFee, date);
		}

	}
}
