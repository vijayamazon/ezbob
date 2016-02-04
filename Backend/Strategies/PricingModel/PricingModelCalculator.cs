namespace Ezbob.Backend.Strategies.PricingModel {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ConfigManager;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using PaymentServices.Calculators;

	public class PricingModelCalculator : AStrategy {
		public PricingModelCalculator(int customerId, PricingModelModel model) {
			Model = model;
			this.customerId = customerId;
			LogInputs();
		} // constructor

		public override string Name { get { return "Pricing Model Calculator"; } }

		public override void Execute() {
			if (CalculateInterestRate())
				CalculateFullModelAfterInterestRate();

			LogInputs();
		} // Execute

		public PricingModelModel Model { get; private set; }
		public string Error { get; private set; }

		private bool CalculateInterestRate() {
			if (Model == null) {
				Error = "Model can't be empty";
				return false;
			} // if

			if (Model.LoanAmount <= 0) {
				Error = "LoanAmount must be positive";
				return false;
			} // if

			if (Model.TenureMonths <= 0) {
				Error = "TenureMonths must be positive";
				return false;
			} // if

			Model.FeesRevenue = Model.SetupFeePounds;

			decimal balanceForUpperBoundary, upperBoundary;

			if (!FindUpperBoundary(out upperBoundary, out balanceForUpperBoundary)) {
				Error = "Monthly interest rate should be over 1000000, aborting calculation";
				return false;
			} // if

			Model.MonthlyInterestRate = GetMonthlyInterestRate(upperBoundary, balanceForUpperBoundary);

			return true;
		} // CalculateInterestRate

		private void CalculateFullModelAfterInterestRate() {
			Loan loan = CreateLoan(Model.MonthlyInterestRate, Model.FeesRevenue + Model.BrokerSetupFeePounds);

			Model.CostOfDebtOutput = GetCostOfDebt(loan.Schedule);
			Model.InterestRevenue = GetInterestRevenue(loan.Schedule);
			Model.Revenue = Model.FeesRevenue + Model.InterestRevenue;
			Model.CogsOutput = Model.Cogs;
			Model.OpexAndCapexOutput = Model.OpexAndCapex;
			Model.GrossProfit = Model.Revenue - Model.Cogs;
			Model.Ebitda = Model.GrossProfit - Model.OpexAndCapex;
			Model.NetLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			Model.ProfitMarkupOutput = Model.ProfitMarkup * Model.Revenue;
			Model.TotalCost = Model.CostOfDebtOutput + Model.Cogs + Model.OpexAndCapex + Model.NetLossFromDefaults;

			Model.PricingSourceModels = new List<PricingSourceModel>();
			Model.PricingSourceModels.Add(new PricingSourceModel {
				Source = "Ezbob loan",
				InterestRate = Model.MonthlyInterestRate,
				SetupFee = Model.SetupFeePercents,
				AIR = Model.TenureMonths != 0
					? (Model.MonthlyInterestRate * 12) + (Model.SetupFeePercents * 12 / Model.TenureMonths)
					: 0,
				APR = GetApr(loan)
			});

			GetPricingModelDefaultRate defaultRate = new GetPricingModelDefaultRate(
				this.customerId,
				Model.DefaultRateCompanyShare
			);
			defaultRate.Execute();

			decimal preferableEUInterest = GetEuLoanMonthlyInterest(defaultRate.ConsumerScore);

			GetPricingSourceForEU("EU Loan (2%)", 0.02M, Model.EuCollectionRate, preferableEUInterest);
			GetPricingSourceForEU("EU Loan (1.75%)", 0.0175M, Model.EuCollectionRate, preferableEUInterest);

			decimal preferableCOSMEInterest = GetCOSMELoanMonthlyInterest(
				defaultRate.ConsumerScore,
				defaultRate.BusinessScore
			);

			Log.Info(
				"Pricing calculator: Customer {0} Consumer score: {1} Company Score: {2} " +
				"preferable COSME interest {3} preferable EU interest {4}",
				this.customerId,
				defaultRate.ConsumerScore,
				defaultRate.BusinessScore,
				preferableCOSMEInterest,
				preferableEUInterest
			);

			GetPricingSourceForEU("COSME Loan (2.25%)", 0.0225M, Model.CosmeCollectionRate, preferableCOSMEInterest);
			GetPricingSourceForEU("COSME Loan (2%)", 0.02M, Model.CosmeCollectionRate, preferableCOSMEInterest);
			GetPricingSourceForEU("COSME Loan (1.75%)", 0.0175M, Model.CosmeCollectionRate, preferableCOSMEInterest);
		} // CalculateFullModelAfterInterestRate

		private void GetPricingSourceForEU(
			string sourceName,
			decimal interestRate,
			decimal collectionRate,
			decimal preferableInterestRate
		) {
			decimal air, apr;

			decimal setupFee = GetSetupFeeForEu(interestRate, collectionRate, out air, out apr);

			bool isPreferable = interestRate == preferableInterestRate;

			var model = new PricingSourceModel {
				Source = sourceName,
				InterestRate = interestRate,
				SetupFee = setupFee,
				AIR = air,
				APR = apr,
				IsPreferable = isPreferable
			};

			Model.PricingSourceModels.Add(model);
		} // GetPricingSourceForEU

		private static decimal GetApr(Loan loan) {
			if (loan.LoanAmount == 0)
				return 0;

			return (decimal)(new APRCalculator().Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date) / 100);
		} // GetApr

		private Loan CreateLoan(decimal interestRate, decimal setupFee) {
			LoanType lt = new StandardLoanType();

			var calculator = new LoanScheduleCalculator { Interest = interestRate, Term = (int)Model.TenureMonths };

			var loan = new Loan {
				LoanAmount = Model.LoanAmount,
				Date = DateTime.UtcNow,
				LoanType = lt,
				CashRequest = null,
				SetupFee = setupFee,
				LoanLegalId = 1
			};
			calculator.Calculate(Model.LoanAmount, loan, loan.Date, Model.InterestOnlyPeriod);

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

			return loan;
		} // CreateLoan

		private decimal GetSetupFeeForEu(
			decimal monthlyInterestRate,
			decimal collectionRate,
			out decimal annualizedInterestRate,
			out decimal apr
		) {
			Loan loan = CreateLoan(monthlyInterestRate, Model.FeesRevenue);

			decimal costOfDebtEu = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal netLossFromDefaults = (1 - collectionRate) * Model.LoanAmount * Model.DefaultRate;
			decimal totalCost = Model.Cogs + Model.OpexAndCapex + netLossFromDefaults + costOfDebtEu;
			decimal profit = totalCost / (1 - Model.ProfitMarkup);
			decimal setupFeePounds = profit - interestRevenue;
			decimal setupFee = setupFeePounds / Model.LoanAmount;

			loan = CreateLoan(monthlyInterestRate, setupFeePounds + Model.BrokerSetupFeePounds);
			apr = GetApr(loan);

			annualizedInterestRate = Model.TenureMonths != 0
				? (monthlyInterestRate * 12) + (setupFee * 12 / Model.TenureMonths)
				: 0;

			return setupFee;
		} // GetSetupFeeForEu

		private bool FindUpperBoundary(out decimal boundary, out decimal balance) {
			balance = -1;
			decimal monthlyInterestRate = 1;

			while (balance < 0) {
				if (monthlyInterestRate > 1000000) {
					LogInputs();
					boundary = -1;
					return false;
				} // if

				monthlyInterestRate *= 10;

				balance = CalculateBalance(monthlyInterestRate);
			} // while

			boundary = monthlyInterestRate;
			return true;
		} // FindUpperBoundary

		private decimal GetMonthlyInterestRate(decimal upperBoundary, decimal balanceForUpperBoundary) {
			decimal lowerBoundary = 0;
			decimal interestRateForMinBalanceAbs;
			decimal minBalanceAbsValue;

			decimal balanceForLowerBoundary = CalculateBalance(lowerBoundary);

			var calculations = new Dictionary<decimal, decimal> {
				{ upperBoundary, balanceForUpperBoundary },
				{ lowerBoundary, balanceForLowerBoundary }
			};

			if (Math.Abs(balanceForUpperBoundary) < Math.Abs(balanceForLowerBoundary)) {
				minBalanceAbsValue = Math.Abs(balanceForUpperBoundary);
				interestRateForMinBalanceAbs = upperBoundary;
			} else {
				minBalanceAbsValue = Math.Abs(balanceForLowerBoundary);
				interestRateForMinBalanceAbs = lowerBoundary;
			} // if

			decimal monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 4, MidpointRounding.AwayFromZero);

			while (minBalanceAbsValue != 0) {
				decimal balance = CalculateBalance(monthlyInterestRate);

				if (Math.Abs(balance) < minBalanceAbsValue) {
					minBalanceAbsValue = Math.Abs(balance);
					interestRateForMinBalanceAbs = monthlyInterestRate;
				} // if

				if (lowerBoundary + 0.0001m == upperBoundary)
					return interestRateForMinBalanceAbs;

				// Can be optimized and avoid calculating balances that were already calculated
				if (!calculations.ContainsKey(monthlyInterestRate))
					calculations.Add(monthlyInterestRate, balance);

				if (balance < 0) {
					lowerBoundary = monthlyInterestRate;

					monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 5, MidpointRounding.AwayFromZero);

					if ((monthlyInterestRate * 100000) % 10 != 0)
						monthlyInterestRate -= 0.00005m;
				} else {
					upperBoundary = monthlyInterestRate;
					monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 4, MidpointRounding.AwayFromZero);
				} // if
			} // while

			return interestRateForMinBalanceAbs;
		} // GetMonthlyInterestRate

		private void LogInputs() {
			var inputs = new StringBuilder();

			inputs.Append("LoanAmount:").Append(Model.LoanAmount).Append("\r\n");
			inputs.Append("DefaultRate:").Append(Model.DefaultRate).Append("\r\n");
			inputs.Append("DefaultRateCompanyShare:").Append(Model.DefaultRateCompanyShare).Append("\r\n");
			inputs.Append("DefaultRateCustomerShare:").Append(Model.DefaultRateCustomerShare).Append("\r\n");
			inputs.Append("SetupFeePounds:").Append(Model.SetupFeePounds).Append("\r\n");
			inputs.Append("SetupFeePercents:").Append(Model.SetupFeePercents).Append("\r\n");
			inputs.Append("BrokerSetupFeePounds:").Append(Model.BrokerSetupFeePounds).Append("\r\n");
			inputs.Append("BrokerSetupFeePercents:").Append(Model.BrokerSetupFeePercents).Append("\r\n");
			inputs.Append("LoanTerm:").Append(Model.LoanTerm).Append("\r\n");
			inputs.Append("InterestOnlyPeriod:").Append(Model.InterestOnlyPeriod).Append("\r\n");
			inputs.Append("TenurePercents:").Append(Model.TenurePercents).Append("\r\n");
			inputs.Append("TenureMonths:").Append(Model.TenureMonths).Append("\r\n");
			inputs.Append("CollectionRate:").Append(Model.CollectionRate).Append("\r\n");
			inputs.Append("EuCollectionRate:").Append(Model.EuCollectionRate).Append("\r\n");
			inputs.Append("CosmeCollectionRate:").Append(Model.CosmeCollectionRate).Append("\r\n");
			inputs.Append("Cogs:").Append(Model.Cogs).Append("\r\n");
			inputs.Append("DebtPercentOfCapital:").Append(Model.DebtPercentOfCapital).Append("\r\n");
			inputs.Append("CostOfDebt:").Append(Model.CostOfDebt).Append("\r\n");
			inputs.Append("OpexAndCapex:").Append(Model.OpexAndCapex).Append("\r\n");
			inputs.Append("ProfitMarkup:").Append(Model.ProfitMarkup).Append("\r\n");

			if (Model.PricingSourceModels != null) {
				foreach (var source in Model.PricingSourceModels) {
					inputs.Append(" Source:").Append(source.Source).Append("\r\n");
					inputs.Append("  InterestRate:").Append(source.InterestRate).Append("\r\n");
					inputs.Append("  SetupFee:").Append(source.SetupFee).Append("\r\n");
					inputs.Append("  AIR:").Append(source.AIR).Append("\r\n");
					inputs.Append("  APR:").Append(source.APR).Append("\r\n");
					inputs.Append("  IsPreferable:").Append(source.IsPreferable).Append("\r\n");
				} // for each
			} else
				inputs.AppendLine("PricingSourceModels NULL!!!!!");

			Log.Info("Pricing Model:\n{0}", inputs);
		} // LogInputs

		private decimal GetCostOfDebt(IEnumerable<LoanScheduleItem> schedule) {
			decimal costOfDebtOutput = 0;
			decimal balanceAtBeginningOfMonth = Model.LoanAmount;

			foreach (LoanScheduleItem scheuldeItem in schedule) {
				costOfDebtOutput += balanceAtBeginningOfMonth * Model.DebtPercentOfCapital * Model.CostOfDebt / 12;
				balanceAtBeginningOfMonth = scheuldeItem.Balance;
			} // for each scheduled installment

			return costOfDebtOutput;
		} // GetCostOfDebt

		private decimal GetInterestRevenue(IEnumerable<LoanScheduleItem> schedule) {
			return schedule.Sum(scheuldeItem => scheuldeItem.Interest) * (1 - Model.DefaultRate);
		} // GetInterestRevenue

		private decimal CalculateBalance(decimal interestRate) {
			Loan loan = CreateLoan(interestRate, Model.FeesRevenue);
			decimal costOfDebtOutput = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal revenue = Model.FeesRevenue + interestRevenue;
			decimal grossProfit = revenue - Model.Cogs;
			decimal ebitda = grossProfit - Model.OpexAndCapex;
			decimal netLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			decimal profitMarkupOutput = Model.ProfitMarkup * revenue;
			return ebitda - netLossFromDefaults - costOfDebtOutput - profitMarkupOutput;
		} // CalculateBalance

		private decimal GetEuLoanMonthlyInterest(int key) {
			SafeReader sr = DB.GetFirst(
				"GetConfigTableValue",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ConfigTableName", "EuLoanMonthlyInterest"),
				new QueryParameter("Key", key)
			);

			return sr["Value"];
		} // GetEuLoanMonthlyInterest

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

			//if companyScore < 50
			return 0.0225M;
		} // GetCOSMELoanMonthlyInterest

		private readonly int customerId;
	} // class PricingModelCalculator
} // namespace
