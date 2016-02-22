namespace Ezbob.Backend.CalculateLoan.Tests {
	using System;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Database;
	using NUnit.Framework;

	[TestFixture]
	public class TestLoanCalculator : TestFoundation {
		[Test]
		public void ForRetrospectivePayment() {
			// TODO: revive (test)
			/*
			var lcm = new LoanCalculatorModel {
				LoanAmount = 1000,
				LoanIssueTime = new DateTime(2015, 6, 1, 15, 0, 0, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.Month,
				RepaymentCount = 15,
				MonthlyInterestRate = 0.0225m,
			};

			var lc = new LegacyLoanCalculator(lcm);

			lc.CreateScheduleAndPlan();

			lcm.Repayments.Add(new Repayment(new DateTime(2015, 7, 1, 15, 0, 0, DateTimeKind.Utc), 76m, 22.5m, 0m));
			lcm.Repayments.Add(new Repayment(new DateTime(2015, 7, 31, 8, 0, 0, DateTimeKind.Utc), 69.88m, 20.12m, 0m));
			lcm.Repayments.Add(new Repayment(new DateTime(2015, 8, 1, 15, 0, 0, DateTimeKind.Utc), 66m, 20.79m, 0m));

			lc.CalculateBalance(new DateTime(2015, 8, 2, 15, 0, 0, DateTimeKind.Utc));
			*/
		} // ForRetrospectivePayment

		[Test]
		public void TestBankLikeCalculator() {
			// TestSpecificLoanCalculator(lcm => new BankLikeLoanCalculator(lcm));
		} // TestBankLikeCalculator

		[Test]
		public void TestLegacyCalculator() {
			// TestSpecificLoanCalculator(lcm => new LegacyLoanCalculator(lcm));
		} // TestLegacyCalculator

		[Test]
		public void TestCreateScheduleAndPlan() {
			// TODO: revive (test)
			/*
			var lcm = new LoanCalculatorModel {
				LoanAmount = 1200,
				LoanIssueTime = new DateTime(2015, 1, 31, 14, 15, 16, DateTimeKind.Utc),
				RepaymentIntervalType = RepaymentIntervalTypes.Month,
				RepaymentCount = 15,
				MonthlyInterestRate = 0.03m,
			};

			var lc = new LegacyLoanCalculator(lcm);

			lc.CreateScheduleAndPlan();
			*/
		} // TestCreateScheduleAndPlan

		[Test]
		public void TestSetScheduleCloseDatesFromPayments() {
			// TODO: revive (test)
			/*
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

			lcm.Fees.Add(new Fee(new DateTime(2015, 4, 1), 25m,  FeeTypes.AdminFee));

			lcm.SetScheduleCloseDatesFromPayments();

			Log.Info("Loan model after close dates applied: {0}", lcm);

			CurrentPaymentModel amount = lc.GetAmountToChargeForDashboard(DateTime.UtcNow, true);

			Log.Info("Amount to charge on {0}: {1}.", DateTime.UtcNow.DateStr(), amount);
			*/
		} // TestSetScheduleCloseDatesFromPayments

		private void TestSpecificLoanCalculator(/*Func<LoanCalculatorModel, ALoanCalculator> loanCalculatorFactory*/) {
			// TODO: revive (test)
			/*
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
			*/
		} // TestSpecificLoanCalculator

		[Test]
		public void CreateCalcInstance() {
			NL_Model m = new NL_Model(56);
			try {
				Type myType = Type.GetType(CurrentValues.Instance.DefaultLoanCalculator.Value);
				// "Ezbob.Backend.CalculateLoan.LoanCalculator.LegacyLoanCalculator, Ezbob.Backend.CalculateLoan.LoanCalculator, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null");
				if (myType != null) {
					ALoanCalculator calc = (ALoanCalculator)Activator.CreateInstance(myType, m);
					Console.WriteLine(calc);
					calc.CreateSchedule();
				}
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
		[Test]
		public void Formatting() {
			DateTime d = new DateTime(2015, 12, 28, 13, 25, 59);
			object[] zz = { "LoanScheduleID", "LoanHistoryID", "LoanScheduleStatusID", "Position", "PlannedDate", "Principal", "InterestRate", "Interest", "FeesAmount", "AmountDue", "InterestPaid", "FeesPaid", "Balance" };
			Console.WriteLine("{0,-2} {1,-2} {2,-2} {3,-2} {4,-2} {6,-2} {7,-2} {8,-2} {9,-2} {10,-2} {11,-2} {12,-2} {13,-2} ", zz);
			Console.WriteLine("{0,-16}{1,-15}{2,-22}{3,-10}{4,-13:MM/dd/yy} {6,-11:C2} {7,-14:F4} {8,-10:C2} {9,-12:C2} {10,-11:C2} {11,-14:C2} {12,-10:C2} {13,-9:C2} ", 5, 44, 1, 3, d, 333m, 444, 0.06000000000m, 0m, 0m, 33m, 366m, 0m, 0m);
		}


		[Test]
		public void CreateSchedule() {
			NL_Model model = new NL_Model(56) {
				UserID = 357,
				Loan = new NL_Loans()
			};
			model.Loan.Histories.Add(new NL_LoanHistory() {
				EventTime = new DateTime(2015, 10, 15), //DateTime.UtcNow,
				Amount = 1000,
				RepaymentCount = 4,
				InterestRate = 0.0225m,
			});
			var discounts= DB.Fill<NL_DiscountPlanEntries>("NL_DiscountPlanEntriesGet", CommandSpecies.StoredProcedure, new QueryParameter("@DiscountPlanID", 2));
			foreach (NL_DiscountPlanEntries dpe in discounts) {
				model.Offer.DiscountPlan.Add(Decimal.Parse(dpe.InterestDiscount.ToString(CultureInfo.InvariantCulture)));
			}
			//model.Offer.DiscountPlan.ForEach(d => Log.Info("discount entry: {0}", d));
			model.Offer.OfferFees = DB.Fill<NL_OfferFees>("NL_OfferFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@OfferID", 3));
			model.Offer.OfferFees.ForEach(f => Log.Info("fee: {0}", f));
			
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(model);
				calc.CreateSchedule();
			} catch (NoInitialDataException noInitialDataException) {
				Log.Debug(noInitialDataException);
			} catch (InvalidInitialRepaymentCountException invalidInitialRepaymentCountException) {
				Log.Debug(invalidInitialRepaymentCountException);
			} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
				Log.Debug(invalidInitialInterestRateException);
			} catch (InvalidInitialAmountException invalidInitialAmountException) {
				Log.Debug(invalidInitialAmountException);
			} catch (Exception exception) {
				Log.Error("No calculator instance {0}", exception);
			}

			Log.Info(model.Loan);
			//model.Loan.LastHistory().Schedule.ForEach(s => Log.Info(s.ToString()));
		}


		

	} // class TestLoanCalculator
} // namespace
