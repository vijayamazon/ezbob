namespace Ezbob.Backend.CalculateLoan.Tests {
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using NUnit.Framework;

	[TestFixture]
	class TestAprCalculator : TestFoundation {
		[Test]
		public void TestApr() {
			// TODO: revive (test)

			/*
			var lc = CreateCalculator();
			lc.CalculateApr(lc.WorkingModel.LoanIssueTime);
			*/
		} // TestApr

		[Test]
		public void CompareApr() {
			// TODO: revive (test)
			/*
			var lc = CreateCalculator();
			decimal newApr = lc.CalculateApr(lc.WorkingModel.LoanIssueTime, 1e-6);

			List<Repayment> plan = lc.CalculatePlan(false);

			var lst = new List<LoanScheduleItem>();

			foreach (Repayment r in plan) {
				var item = new LoanScheduleItem {
					Date = r.Date,
					AmountDue = r.Amount,
				};

				lst.Add(item);
			} // for each repayment

			var setupFee = lc.WorkingModel.Fees.Where(fee => fee.FeeType == FeeTypes.SetupFee).Sum(fee => fee.Amount);

			var calc = new APRCalculator();

			decimal oldApr = (decimal)calc.Calculate(
				lc.WorkingModel.LoanAmount,
				lst,
				setupFee,
				lc.WorkingModel.LoanIssueTime
			);

			Log.Debug("Old APR = {0}", oldApr);
			Log.Debug("New APR = {0}", newApr);
			Log.Debug("Delta   = {0}", Math.Abs(oldApr - newApr));

			Assert.LessOrEqual(Math.Abs(oldApr - newApr), 1);
			*/
		} // CompareApr

		private ALoanCalculator CreateCalculator() {
			return null;
			// TODO: revive (test)
			/*
			var lcm = new LoanCalculatorModel {
				LoanAmount = 1200,
				LoanIssueTime = new DateTime(2015, 1, 31, 14, 15, 16, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.Month,
				RepaymentCount = 15,
				MonthlyInterestRate = 0.03m,
			};

			lcm.Fees.Add(new Fee(lcm.LoanIssueTime, 100, FeeTypes.SetupFee));

			for (int i = 1; i <= lcm.RepaymentCount + 1; i++) {
				lcm.Fees.Add(new Fee(lcm.LoanIssueTime.AddMonths(i), 5, FeeTypes.ServicingFee));
				lcm.Fees.Add(new Fee(lcm.LoanIssueTime.AddMonths(i), 10, FeeTypes.AdminFee));
			} // for

			var lc = new LegacyLoanCalculator(lcm);

			lc.CreateSchedule();

			return lc;
			*/
		} // CreateCalculator
	} // class TestAprCalculator
} // namespace
