namespace Ezbob.Backend.CalculateLoan.Tests {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using NUnit.Framework;

	[TestFixture]
	public class TestLoanCalculator : TestFoundation {
		[Test]
		public void MiscTests() {
			/*
			var lcm = new LoanCalculatorModel {
				LoanAmount = 1200,
				LoanIssueTime = new DateTime(2015, 1, 31, 14, 15, 16, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.TenDays,
				RepaymentCount = 6,
				MonthlyInterestRate = 0.1m,
				InterestOnlyMonths = 2,
			};

			lcm.SetDiscountPlan(0, 0, -0.5m, 0.6m);

			var lc = new BankLikeLoanCalculator(lcm);

			// Log.Debug("Loan calculator model before schedule:\n{0}", lc.WorkingModel);

			lc.CreateSchedule();

			// Log.Debug("Loan calculator model after schedule:\n{0}", lc.WorkingModel);

			List<Repayment> plan = lc.CalculatePlan();

			// Log.Debug("Loan plan:\n\t\t{0}", string.Join("\n\t\t", plan));

			lcm.Repayments.Add(new Repayment(
				new DateTime(2015, 2, 17),
				100,
				25,
				0
			));

			decimal balance = lc.CalculateBalance(new DateTime(2015, 2, 19, 0, 0, 0, DateTimeKind.Utc));

			// Assert.Less(Math.Abs(balance - 1149.96m), 0.01m);

			balance = lc.CalculateBalance(new DateTime(2015, 5, 19, 0, 0, 0, DateTimeKind.Utc));

			balance = lc.CalculateEarnedInterest(
				new DateTime(2015, 2, 10, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 3, 19, 0, 0, 0, DateTimeKind.Utc)
			);
			*/

			var lcm = new LoanCalculatorModel {
				LoanAmount = 53000,
				LoanIssueTime = new DateTime(2014, 11, 21, 11, 40, 15, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.Month,
				RepaymentCount = 12,
				MonthlyInterestRate = 0.033m,
			};

			var lc = new LegacyLoanCalculator(lcm);

			lc.CreateSchedule();

			lc.WorkingModel.Schedule.RemoveAt(1);

			Log.Debug("Loan calculator model after schedule:\n{0}", lc.WorkingModel);

			lcm.Repayments.Add(new Repayment(new DateTime(2014, 11, 21, 11, 40, 15, DateTimeKind.Utc), 0, 5, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2014, 12, 19, 11, 40, 15, DateTimeKind.Utc), 4540.75m, 1632.25m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2015, 1, 22, 11, 40, 15, DateTimeKind.Utc), 4261.84m, 1757.17m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2015, 2, 18, 11, 40, 15, DateTimeKind.Utc), 4603.11m, 1270.17m, 0));

			lc.CalculateBalance(new DateTime(2015, 1, 22, 12, 0, 0, DateTimeKind.Utc));
		} // MiscTests
	} // class TestLoanCalculator
} // namespace
