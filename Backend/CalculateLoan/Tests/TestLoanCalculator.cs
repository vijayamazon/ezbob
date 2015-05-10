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
		public void TestBankLikeCalculator() {
			TestSpecificLoanCalculator(lcm => new BankLikeLoanCalculator(lcm));
		} // TestBankLikeCalculator

		[Test]
		public void TestLegacyCalculator() {
			TestSpecificLoanCalculator(lcm => new LegacyLoanCalculator(lcm));
		} // TestLegacyCalculator

		private void TestSpecificLoanCalculator(Func<LoanCalculatorModel, ALoanCalculator> loanCalculatorFactory) {
			var lcm = new LoanCalculatorModel {
				LoanAmount = 1200,
				LoanIssueTime = new DateTime(2015, 1, 31, 14, 15, 16, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.Month,
				RepaymentCount = 6,
				MonthlyInterestRate = 0.1m,
				InterestOnlyRepayments = 2,
			};

			lcm.SetDiscountPlan(0, 0, -0.5m, 0.6m);

			ALoanCalculator lc = loanCalculatorFactory(lcm);

			Log.Debug("{1} model before schedule:\n{0}", lc.WorkingModel, lc.Name);

			lc.CreateSchedule();

			Log.Debug("{1} model after schedule:\n{0}", lc.WorkingModel, lc.Name);

			List<Repayment> plan = lc.CalculatePlan();

			Log.Debug("{1} loan plan:\n\t\t{0}", string.Join("\n\t\t", plan), lc.Name);

			lcm.Repayments.Add(new Repayment(
				new DateTime(2015, 2, 17),
				100,
				25,
				0
			));

			decimal balance = lc.CalculateBalance(new DateTime(2015, 2, 19, 0, 0, 0, DateTimeKind.Utc));

			Log.Info("{1} balance on 19/02/2015 is {0}.", balance, lc.Name);

			balance = lc.CalculateBalance(new DateTime(2015, 5, 19, 0, 0, 0, DateTimeKind.Utc));

			Log.Info("{1} balance on 19/05/2015 is {0}.", balance, lc.Name);

			decimal earnedInterest = lc.CalculateEarnedInterest(
				new DateTime(2015, 2, 10, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 3, 19, 0, 0, 0, DateTimeKind.Utc)
			);

			Log.Info("{1} earned interest on 10/02/2015 - 19/03/2015 is {0}.", earnedInterest, lc.Name);
		} // TestSpecificLoanCalculator
	} // class TestLoanCalculator
} // namespace
