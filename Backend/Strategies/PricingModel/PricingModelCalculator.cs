namespace Ezbob.Backend.Strategies.PricingModel
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Database;
	using PaymentServices.Calculators;
	
	public class PricingModelCalculator: AStrategy
	{
		private readonly int customerId;

		public PricingModelCalculator(int customerId, PricingModelModel model)
		{
			Model = model;
			this.customerId = customerId;
			LogInputs();
		}

		public PricingModelModel Model { get; private set; }
		public string Error { get; private set; }

		public bool CalculateInterestRate()
		{
			if (Model == null)
			{
				Error = "Model can't be empty";
				return false;
			}

			if (Model.LoanAmount <= 0)
			{
				Error = "LoanAmount must be positive";
				return false;
			}

			if (Model.TenureMonths <= 0)
			{
				Error = "TenureMonths must be positive";
				return false;
			}

			Model.FeesRevenue = Model.SetupFeePounds;

			decimal balanceForUpperBoundary, upperBoundary;
			if (!FindUpperBoundary(out upperBoundary, out balanceForUpperBoundary))
			{
				Error = "Monthly interest rate should be over 1000000, aborting calculation";
				return false;
			}

			Model.MonthlyInterestRate = GetMonthlyInterestRate(upperBoundary, balanceForUpperBoundary);

			return true;
		}

		public void Calculate()
		{
			if (!CalculateInterestRate())
			{
				return;
			}

			CalculateFullModelAfterInterestRate();
		}

		private void CalculateFullModelAfterInterestRate()
		{
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
				AIR = Model.TenureMonths != 0 ? (Model.MonthlyInterestRate * 12) + (Model.SetupFeePercents * 12 / Model.TenureMonths) : 0,
				APR = GetApr(loan)
			});

		    GetPricingModelDefaultRate defaultRate = new GetPricingModelDefaultRate(this.customerId, Model.DefaultRateCompanyShare);
		    defaultRate.Execute();


            decimal preferableEUInterest = GetEuLoanMonthlyInterest(defaultRate.ConsumerScore);
			Model.PricingSourceModels.Add(GetPricingSourceForEU("EU Loan (2%)", 0.02M, Model.EuCollectionRate, preferableEUInterest));
			Model.PricingSourceModels.Add(GetPricingSourceForEU("EU Loan (1.75%)", 0.0175M, Model.EuCollectionRate, preferableEUInterest));

            decimal preferableCOSMEInterest = GetCOSMELoanMonthlyInterest(defaultRate.ConsumerScore, defaultRate.BusinessScore);
            Log.Info("Pricing calculator: Customer {0} Consumer score: {1} Company Score: {2} preferable COSME interest {3} preferable EU interest {4}", this.customerId, defaultRate.ConsumerScore, defaultRate.BusinessScore, preferableCOSMEInterest, preferableEUInterest);
			Model.PricingSourceModels.Add(GetPricingSourceForEU("COSME Loan (2.25%)", 0.0225M, Model.CosmeCollectionRate, preferableCOSMEInterest));
			Model.PricingSourceModels.Add(GetPricingSourceForEU("COSME Loan (2%)", 0.02M, Model.CosmeCollectionRate, preferableCOSMEInterest));
			Model.PricingSourceModels.Add(GetPricingSourceForEU("COSME Loan (1.75%)", 0.0175M, Model.CosmeCollectionRate, preferableCOSMEInterest));
		}

		private PricingSourceModel GetPricingSourceForEU(string sourceName, decimal interestRate, decimal collectionRate, decimal preferableInterestRate) {
			decimal air, apr;
			decimal setupFee = GetSetupFeeForEu(interestRate, collectionRate, out air, out apr);
			bool isPreferable = interestRate == preferableInterestRate;
			return new PricingSourceModel {
				Source = sourceName,
				InterestRate = interestRate,
				SetupFee = setupFee,
				AIR = air,
				APR = apr,
				IsPreferable = isPreferable
			};
		}
		private decimal GetApr(Loan loan)
		{
			var aprCalc = new APRCalculator();
			return (decimal)(loan.LoanAmount == 0 ? 0 : aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date) / 100);
		}

		private Loan CreateLoan(decimal interestRate, decimal setupFee)
		{
			var calculator = new LoanScheduleCalculator { Interest = interestRate, Term = (int)Model.TenureMonths };
			LoanType lt = new StandardLoanType();
			var loan = new Loan
			{
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
		}

		private decimal GetSetupFeeForEu(decimal monthlyInterestRate, decimal collectionRate, out decimal annualizedInterestRate, out decimal apr)
		{
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
			annualizedInterestRate = Model.TenureMonths != 0 ? (monthlyInterestRate * 12) + (setupFee * 12 / Model.TenureMonths) : 0;
			return setupFee;
		}

		private bool FindUpperBoundary(out decimal boundary, out decimal balance)
		{
			balance = -1;
			decimal monthlyInterestRate = 1;
			while (balance < 0)
			{
				if (monthlyInterestRate > 1000000)
				{
					LogInputs();
					boundary = -1;
					return false;
				}
				monthlyInterestRate *= 10;
				balance = CalculateBalance(monthlyInterestRate);
			}

			boundary = monthlyInterestRate;
			return true;
		}

		private decimal GetMonthlyInterestRate(decimal upperBoundary, decimal balanceForUpperBoundary)
		{
			decimal lowerBoundary = 0;
			decimal interestRateForMinBalanceAbs;
			decimal minBalanceAbsValue;
			var calculations = new Dictionary<decimal, decimal>();
			decimal balanceForLowerBoundary = CalculateBalance(lowerBoundary);
			calculations.Add(upperBoundary, balanceForUpperBoundary);
			calculations.Add(lowerBoundary, balanceForLowerBoundary);

			if (Math.Abs(balanceForUpperBoundary) < Math.Abs(balanceForLowerBoundary))
			{
				minBalanceAbsValue = Math.Abs(balanceForUpperBoundary);
				interestRateForMinBalanceAbs = upperBoundary;
			}
			else
			{
				minBalanceAbsValue = Math.Abs(balanceForLowerBoundary);
				interestRateForMinBalanceAbs = lowerBoundary;
			}

			decimal monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 4, MidpointRounding.AwayFromZero);

			while (minBalanceAbsValue != 0)
			{
				decimal balance = CalculateBalance(monthlyInterestRate);
				if (Math.Abs(balance) < minBalanceAbsValue)
				{
					minBalanceAbsValue = Math.Abs(balance);
					interestRateForMinBalanceAbs = monthlyInterestRate;
				}

				if (lowerBoundary + 0.0001m == upperBoundary)
				{
					return interestRateForMinBalanceAbs;
				}

				if (!calculations.ContainsKey(monthlyInterestRate)) // Can be optimized and avoid calculating balances that were already calculated
				{
					calculations.Add(monthlyInterestRate, balance);
				}

				if (balance < 0)
				{
					lowerBoundary = monthlyInterestRate;
					monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 5, MidpointRounding.AwayFromZero);
					if ((monthlyInterestRate*100000)%10 != 0)
					{
						monthlyInterestRate -= 0.00005m;
					}
				}
				else
				{
					upperBoundary = monthlyInterestRate;
					monthlyInterestRate = Math.Round((lowerBoundary + upperBoundary) / 2, 4, MidpointRounding.AwayFromZero);
				}
			}

			return interestRateForMinBalanceAbs;
		}

		private void LogInputs()
		{
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
				}
			} else {
				inputs.AppendLine("PricingSourceModels NULL!!!!!");
			}

			Log.Info("Pricing Model:\n{0}", inputs);
		}

		private decimal GetCostOfDebt(IEnumerable<LoanScheduleItem> schedule)
		{
			decimal costOfDebtOutput = 0;
			decimal balanceAtBeginningOfMonth = Model.LoanAmount;
			foreach (LoanScheduleItem scheuldeItem in schedule)
			{
				costOfDebtOutput += balanceAtBeginningOfMonth * Model.DebtPercentOfCapital * Model.CostOfDebt / 12;
				balanceAtBeginningOfMonth = scheuldeItem.Balance;
			}

			return costOfDebtOutput;
		}

		private decimal GetInterestRevenue(IEnumerable<LoanScheduleItem> schedule)
		{
			decimal interestRevenue = schedule.Sum(scheuldeItem => scheuldeItem.Interest);
			interestRevenue *= 1 - Model.DefaultRate;
			return interestRevenue;
		}

		private decimal CalculateBalance(decimal interestRate)
		{
			Loan loan = CreateLoan(interestRate, Model.FeesRevenue);
			decimal costOfDebtOutput = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal revenue = Model.FeesRevenue + interestRevenue;
			decimal grossProfit = revenue - Model.Cogs;
			decimal ebitda = grossProfit - Model.OpexAndCapex;
			decimal netLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			decimal profitMarkupOutput = Model.ProfitMarkup * revenue;
			return ebitda - netLossFromDefaults - costOfDebtOutput - profitMarkupOutput;
		}

		private decimal GetEuLoanMonthlyInterest(int key)
		{
			SafeReader sr = DB.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "EuLoanMonthlyInterest"), new QueryParameter("Key", key));
			return sr["Value"];
		}

		/// <summary>
		/// Retrieve the preferable COSME interest rate based on customers personal and business score
		/// TODO make configurable in DB
		/// </summary>
		/// <returns>preferable interest rate</returns>
		private decimal GetCOSMELoanMonthlyInterest(int consumerScore, int companyScore) {
			if (consumerScore < 1040 && companyScore == 0) {
				return 0.0225M;
			} 
			if (consumerScore >= 1040 && companyScore == 0) {
				return 0.0175M;
			}
			if (consumerScore < 1040 && companyScore >= 50) {
				return 0.02M;
			}
			if (consumerScore >= 1040 && companyScore >= 50) {
				return 0.0175M;
			}
			//if companyScore < 50
			return 0.0225M;
		}
		private int GetCompanyScore() {
			var outputScore = new QueryParameter("CompanyScore") {
				Type = DbType.Int32,
				Direction = ParameterDirection.Output,
			};

			var outputDate = new QueryParameter("IncorporationDate") {
				Type = DbType.DateTime2,
				Direction = ParameterDirection.Output,
			};

			DB.ExecuteNonQuery(
				"GetCompanyScoreAndIncorporationDate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("TakeMinScore", false),
				outputScore,
				outputDate
				);

			int score;
			if (int.TryParse(outputScore.SafeReturnedValue, out score)) {
				Log.Info("Business score for customer {0} is {1}", customerId, score);
				return score;
			}
			return 0;
		}

		public override string Name {
			get {return "Pricing Model Calculator";}
		}

		public override void Execute() {
			Calculate();
			LogInputs();
		}
	}
}
