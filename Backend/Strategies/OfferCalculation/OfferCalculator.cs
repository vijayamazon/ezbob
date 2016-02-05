namespace Ezbob.Backend.Strategies.OfferCalculation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.PricingModel;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using PaymentServices.Calculators;

	public class OfferCalculator {
		public OfferCalculator(
			int customerId,
			DateTime calculationTime,
			int amount,
			bool hasLoans,
			Medal medalClassification,
			int period
		) {
			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;

			this.result = new OfferResult {
				CustomerId = customerId,
				CalculationTime = calculationTime,
				Amount = amount,
				MedalClassification = medalClassification,
				Period = period,
			};

			this.hasLoans = hasLoans;
		} // constructor

		public OfferResult CalculateOffer() {
			// We always use standard loan type
			SafeReader sr = this.db.GetFirst("GetStandardLoanTypeId", CommandSpecies.StoredProcedure);

			if (sr.IsEmpty) {
				this.result.IsError = true;
				this.result.Message = "Can't load standard loan type";
				return this.result;
			} // if

			this.result.LoanTypeId = sr["Id"];

			// Choose scenario
			if (this.result.Amount <= CurrentValues.Instance.SmallLoanScenarioLimit)
				this.result.ScenarioName = "Small Loan";
			else if (!this.hasLoans)
				this.result.ScenarioName = "Basic New";
			else
				this.result.ScenarioName = "Basic Repeating";

			var getPricingModelModelInstance = new GetPricingModelModel(this.result.CustomerId, this.result.ScenarioName);
			getPricingModelModelInstance.Execute();

			PricingModelModel templateModel = getPricingModelModelInstance.Model;
			templateModel.LoanAmount = this.result.Amount;
			templateModel.LoanTerm = this.result.Period;
			templateModel.TenureMonths = this.result.Period * templateModel.TenurePercents;

			templateModel.MonthlyInterestRate = GetCOSMELoanMonthlyInterest(
				templateModel.ConsumerScore,
				templateModel.CompanyScore
			);

			decimal calculatedSetupFee = GetSetupFeeForCOSME(templateModel);

			decimal adjustedSetupFee = AdjustToMinMaxSetupFee(templateModel.LoanAmount, calculatedSetupFee);

			SetRounded(templateModel.MonthlyInterestRate, adjustedSetupFee);

			if (adjustedSetupFee != calculatedSetupFee) {
				this.result.Message = string.Format(
					"Calculated setup fee of {0:P2} was adjusted to {1:P2}.",
					calculatedSetupFee,
					adjustedSetupFee
				);
			} // if

			return this.result;
		} // CalculateOffer

		/// <summary>
		/// Checks if the calculated setup fee is in range; adjusts to the closest edge if needed.
		/// </summary>
		/// <returns>true if setup fee was adjusted</returns>
		private decimal AdjustToMinMaxSetupFee(decimal amount, decimal originalSetupFee) {
			SafeReader sr = this.db.GetFirst(
				"LoadOfferRanges",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Amount", amount),
				new QueryParameter("@IsNewLoan", !this.hasLoans)
			);

			decimal minSetupFee = sr["MinSetupFee"];
			decimal maxSetupFee = sr["MaxSetupFee"];

			this.log.Debug(
				"Primary set up fee range({0:C2}, {1} loan) = {2} [{3:P2}, {4:P2}].",
				amount,
				this.hasLoans ? "repeating" : "new",
				(string)sr["LoanSizeName"],
				minSetupFee,
				maxSetupFee
			);

			if (originalSetupFee < minSetupFee) {
				this.log.Info("Primary setup fee is {0:P2} less then min {1:P2}, adjusted.", originalSetupFee, minSetupFee);
				return minSetupFee;
			} // if

			if (originalSetupFee > maxSetupFee) {
				this.log.Info(
					"Primary setup fee is {0:P2} bigger then max {1:P2}, adjusted.",
					originalSetupFee,
					maxSetupFee
				);
				return maxSetupFee;
			} // if

			this.log.Info(
				"Primary setup fee is {0:P2} is in range [{1:P2}, {2:P2}], not adjusted.",
				originalSetupFee,
				minSetupFee,
				maxSetupFee
			);

			return originalSetupFee;
		} // AdjustToMinMaxSetupFee

		/// <summary>
		/// Retrieve the preferable COSME interest rate based on customers personal and business score
		/// TODO make configurable in DB
		/// </summary>
		/// <returns>preferable interest rate</returns>
		private static decimal GetCOSMELoanMonthlyInterest(int consumerScore, int companyScore) {
			if (consumerScore < 1040 && companyScore == 0)
				return 0.0225M;

			if (consumerScore >= 1040 && companyScore == 0)
				return 0.0175M;

			if (consumerScore < 1040 && companyScore >= 50)
				return 0.02M;

			if (consumerScore >= 1040 && companyScore >= 50)
				return 0.0175M;

			// if companyScore < 50
			return 0.0225M;
		} // GetCOSMELoanMonthlyInterest

		private static decimal GetSetupFeeForCOSME(PricingModelModel model) {
			Loan loan = CreateLoan(model);

			decimal costOfDebtEu = GetCostOfDebt(
				model.LoanAmount,
				model.DebtPercentOfCapital,
				model.CostOfDebt,
				loan.Schedule
			);

			decimal interestRevenue = loan.Schedule.Sum(scheuldeItem => scheuldeItem.Interest) * (1 - model.DefaultRate);
			decimal netLossFromDefaults = (1 - model.CosmeCollectionRate) * model.LoanAmount * model.DefaultRate;
			decimal totalCost = model.Cogs + model.OpexAndCapex + netLossFromDefaults + costOfDebtEu;
			decimal profit = totalCost / (1 - model.ProfitMarkup);
			decimal setupFeePounds = profit - interestRevenue;
			return setupFeePounds / model.LoanAmount;
		} // GetSetupFeeForCOSME

		private static Loan CreateLoan(PricingModelModel model) {
			var calculator = new LoanScheduleCalculator {
				Interest = model.MonthlyInterestRate,
				Term = (int)model.TenureMonths,
			};

			LoanType lt = new StandardLoanType();

			var loan = new Loan {
				LoanAmount = model.LoanAmount,
				Date = DateTime.UtcNow,
				LoanType = lt,
				CashRequest = null,
				SetupFee = model.FeesRevenue,
				LoanLegalId = 1
			};

			calculator.Calculate(model.LoanAmount, loan, loan.Date);

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

			return loan;
		} // CreateLoan

		private static decimal GetCostOfDebt(
			decimal loanAmount,
			decimal debtPercentOfCapital,
			decimal costOfDebt,
			IEnumerable<LoanScheduleItem> schedule
		) {
			decimal costOfDebtOutput = 0;
			decimal balanceAtBeginningOfMonth = loanAmount;

			foreach (LoanScheduleItem scheuldeItem in schedule) {
				costOfDebtOutput += balanceAtBeginningOfMonth * debtPercentOfCapital * costOfDebt / 12;
				balanceAtBeginningOfMonth = scheuldeItem.Balance;
			} // for each

			return costOfDebtOutput;
		} // GetCostOfDebt

		private void SetRounded(decimal interestRate, decimal setupFee) {
			this.result.InterestRate = Math.Ceiling(interestRate * 2000) / 20;
			this.result.SetupFee = Math.Ceiling(setupFee * 200) / 2;

			this.log.Info(
				"Rounding setup fee {0:P2} -> {1:N2}%, interest rate {2:P2} - > {3:N2}%.",
				setupFee,
				this.result.SetupFee,
				interestRate,
				this.result.InterestRate
			);
		} // SetRounded

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly OfferResult result;
		private readonly bool hasLoans;
	} // class OfferCalculator
} // namespace
