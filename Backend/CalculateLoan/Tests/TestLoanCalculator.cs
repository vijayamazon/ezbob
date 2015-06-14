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

		[Test]
		public void TestLoan166() {
			var lcm = new LoanCalculatorModel {
				LoanAmount = 29100,
				LoanIssueTime = new DateTime(2012, 11, 30, 14, 15, 16, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.Month,
				RepaymentCount = 6,
				MonthlyInterestRate = 0.03m,
			};

			var lc = new LegacyLoanCalculator(lcm);

			lc.CreateSchedule();

			lcm.Repayments.Add(new Repayment(new DateTime(2012, 12, 30), 4850, 873, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2013, 1, 30), 4951.16m, 626.34m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2013, 1, 20), 0, 101.16m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2013, 2, 28), 4748.84m, 541.61m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2013, 3, 30), 4846.98m, 467.68m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2013, 4, 30), 4849.91m, 291.09m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2013, 5, 7), 4849.98m, 33.97m, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2013, 10, 15), 3.13m, 0.5m, 0));

			lcm.SetScheduleCloseDatesFromPayments();

			Log.Debug("Model {0}", lcm);

			lc.CalculateBalance(new DateTime(2013, 10, 16));
		}

		[Test]
		public void TestSetScheduleCloseDatesFromPayments() {
			var lcm = new LoanCalculatorModel {
				LoanAmount = 1200,
				LoanIssueTime = new DateTime(2015, 1, 31, 14, 15, 16, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.Month,
				RepaymentCount = 6,
				MonthlyInterestRate = 0.1m,
			};

			var lc = new LegacyLoanCalculator(lcm);

			lc.CreateSchedule();

			lcm.Repayments.Add(new Repayment(new DateTime(2015, 2, 17), 100, 25, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2015, 2, 19),  80, 25, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2015, 2, 23), 250, 25, 0));
			lcm.Repayments.Add(new Repayment(new DateTime(2015, 3, 23), 641, 25, 0));

			lcm.SetScheduleCloseDatesFromPayments();

			Log.Info("Loan model after close dates applied: {0}", lcm);
		} // TestSetScheduleCloseDatesFromPayments

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
