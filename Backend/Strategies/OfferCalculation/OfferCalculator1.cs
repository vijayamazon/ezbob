namespace Ezbob.Backend.Strategies.OfferCalculation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Strategies.PricingModel;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using PaymentServices.Calculators;

	public class OfferCalculator1 {
		public OfferCalculator1() {
			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;
		} // constructor

		public OfferResult CalculateOffer(
			int customerId,
			DateTime calculationTime,
			int amount,
			bool hasLoans,
			Medal medalClassification,
			int period
		) {
			var result = new OfferResult {
				CustomerId = customerId,
				CalculationTime = calculationTime,
				Amount = amount,
				MedalClassification = medalClassification,
				Period = period,
			};

			// We always use standard loan type
			SafeReader sr = this.db.GetFirst("GetStandardLoanTypeId", CommandSpecies.StoredProcedure);

			if (sr.IsEmpty) {
				result.IsError = true;
				result.Message = "Can't load standard loan type";
				return result;
			} // if

			result.LoanTypeId = sr["Id"];

			// Choose scenario
			if (amount <= CurrentValues.Instance.SmallLoanScenarioLimit)
				result.ScenarioName = "Small Loan";
			else if (!hasLoans)
				result.ScenarioName = "Basic New";
			else
				result.ScenarioName = "Basic Repeating";

			var getPricingModelModelInstance = new GetPricingModelModel(customerId, result.ScenarioName);
			getPricingModelModelInstance.Execute();

			PricingModelModel templateModel = getPricingModelModelInstance.Model;
			templateModel.SetLoanAmount(amount);
			templateModel.LoanTerm = result.Period;
			templateModel.TenureMonths = result.Period * templateModel.TenurePercents;

			templateModel.MonthlyInterestRate = GetCOSMELoanMonthlyInterest(
				templateModel.ConsumerScore,
				templateModel.CompanyScore
			);

			decimal calculatedSetupFee = GetSetupFeeForCOSME(templateModel);

			decimal adjustedSetupFee = AdjustToMinMaxSetupFee(templateModel.LoanAmount, !hasLoans, calculatedSetupFee);

			SetRounded(result, templateModel.MonthlyInterestRate, adjustedSetupFee);

			if (adjustedSetupFee != calculatedSetupFee) {
				result.Message = string.Format(
					"Calculated setup fee of {0:P2} was adjusted to {1:P2}.",
					calculatedSetupFee,
					adjustedSetupFee
				);
			} // if

			return result;
		} // CalculateOffer

		/// <summary>
		/// Checks if the calculated setup fee is in range; adjusts to the closest edge if needed.
		/// </summary>
		/// <returns>true if setup fee was adjusted</returns>
		private decimal AdjustToMinMaxSetupFee(decimal amount, bool isNewLoan, decimal originalSetupFee) {
			SafeReader sr = this.db.GetFirst(
				"LoadOfferRanges",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Amount", amount),
				new QueryParameter("@IsNewLoan", isNewLoan)
			);

			decimal minSetupFee = sr["MinSetupFee"];
			decimal maxSetupFee = sr["MaxSetupFee"];

			this.log.Debug(
				"Primary set up fee range({0:C2}, {1} loan) = {2} [{3:P2}, {4:P2}].",
				amount,
				isNewLoan ? "new" : "repeating",
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
		private decimal GetCOSMELoanMonthlyInterest(int consumerScore, int companyScore) {
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

		private decimal GetSetupFeeForCOSME(PricingModelModel model) {
			Loan loan = CreateLoan(model.LoanAmount, model.MonthlyInterestRate, model.FeesRevenue, (int)model.TenureMonths);

			decimal costOfDebtEu = GetCostOfDebt(
				model.LoanAmount,
				model.DebtPercentOfCapital,
				model.CostOfDebt,
				loan.Schedule
			);

			decimal interestRevenue = loan.Schedule.Sum(scheuldeItem => scheuldeItem.Interest);
			interestRevenue *= 1 - model.DefaultRate;
			decimal netLossFromDefaults = (1 - model.CosmeCollectionRate) * model.LoanAmount * model.DefaultRate;
			decimal totalCost = model.Cogs + model.OpexAndCapex + netLossFromDefaults + costOfDebtEu;
			decimal profit = totalCost / (1 - model.ProfitMarkup);
			decimal setupFeePounds = profit - interestRevenue;
			decimal setupFee = setupFeePounds / model.LoanAmount;

			return setupFee;
		} // GetSetupFeeForCOSME

		private Loan CreateLoan(decimal loanAmount, decimal interestRate, decimal setupFee, int tenureMonths) {
			var calculator = new LoanScheduleCalculator { Interest = interestRate, Term = tenureMonths };

			LoanType lt = new StandardLoanType();

			var loan = new Loan {
				LoanAmount = loanAmount,
				Date = DateTime.UtcNow,
				LoanType = lt,
				CashRequest = null,
				SetupFee = setupFee,
				LoanLegalId = 1
			};

			calculator.Calculate(loanAmount, loan, loan.Date);

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

			return loan;
		} // CreateLoan

		private decimal GetCostOfDebt(
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

		private void SetRounded(OfferResult result, decimal interestRate, decimal setupFee) {
			result.InterestRate = Math.Ceiling(interestRate * 2000) / 20;
			result.SetupFee = Math.Ceiling(setupFee * 200) / 2;

			this.log.Info(
				"Rounding setup fee {0:P2} -> {1:N2}%, interest rate {2:P2} - > {3:N2}%.",
				setupFee,
				result.SetupFee,
				interestRate,
				result.InterestRate
			);
		} // SetRounded

		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class OfferCalculator1
} // namespace
