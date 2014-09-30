namespace EzBob.Backend.Strategies.PricingModel
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Exceptions;
	using Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.Calculators;
	
	public class PricingModelCalculate : AStrategy
	{
		private readonly int customerId;

		public PricingModelCalculate(int customerId, PricingModelModel model, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			if (model.LoanAmount <= 0)
				throw new StrategyWarning(this, "LoanAmount must be positive");

			if (model.TenureMonths <= 0)
				throw new StrategyWarning(this, "TenureMonths must be positive");

			Model = model;
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Pricing model calculate"; }
		}

		public PricingModelModel Model { get; private set; }

		public override void Execute()
		{
			Model.FeesRevenue = Model.SetupFeePounds;
			Model.MonthlyInterestRate = GetMonthlyInterestRate();

			Loan loan = CreateLoan(Model.MonthlyInterestRate, Model.FeesRevenue + Model.BrokerSetupFeePounds);
			Model.Apr = GetApr(loan);

			Model.CostOfDebtOutput = GetCostOfDebt(loan.Schedule);
			Model.InterestRevenue = GetInterestRevenue(loan.Schedule);

			Model.Revenue = Model.FeesRevenue + Model.InterestRevenue;

			var scoreStrat = new GetExperianConsumerScore(customerId, DB, Log);
			scoreStrat.Execute();

			int consumerScore = scoreStrat.Score;
			
			Model.EuLoanPercentages = GetEuLoanMonthlyInterest(consumerScore);
			Model.CogsOutput = Model.Cogs;
			Model.OpexAndCapexOutput = Model.OpexAndCapex;
			Model.GrossProfit = Model.Revenue - Model.Cogs;
			Model.Ebitda = Model.GrossProfit - Model.OpexAndCapex;
			Model.NetLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			Model.ProfitMarkupOutput = Model.ProfitMarkup * Model.Revenue;
			Model.AnnualizedInterestRate = Model.TenureMonths != 0 ? (Model.MonthlyInterestRate * 12) + (Model.SetupFeePercents * 12 / Model.TenureMonths) : 0;
			Model.TotalCost = Model.CostOfDebtOutput + Model.Cogs + Model.OpexAndCapex + Model.NetLossFromDefaults;

			decimal annualizedInterestRateEu2, aprEu2;
			Model.SetupFeeForEuLoanHigh = GetSetupFeeForEu(0.02m, out annualizedInterestRateEu2, out aprEu2);
			Model.AnnualizedInterestRateEu2 = annualizedInterestRateEu2;
			Model.AprEu2 = aprEu2;

			decimal annualizedInterestRateEu175, aprEu175;
			Model.SetupFeeForEuLoanLow = GetSetupFeeForEu(0.0175m, out annualizedInterestRateEu175, out aprEu175);
			Model.AnnualizedInterestRateEu175 = annualizedInterestRateEu175;
			Model.AprEu175 = aprEu175;
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

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
			calc.GetState();

			return loan;
		}

		private decimal GetSetupFeeForEu(decimal monthlyInterestRate, out decimal annualizedInterestRate, out decimal apr)
		{
			Loan loan = CreateLoan(monthlyInterestRate, Model.FeesRevenue);
			decimal costOfDebtEu = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal netLossFromDefaults = (1 - Model.EuCollectionRate) * Model.LoanAmount * Model.DefaultRate;
			decimal setupFeePounds = Model.ProfitMarkupOutput - interestRevenue + Model.Cogs + Model.OpexAndCapex + netLossFromDefaults + costOfDebtEu;
			decimal setupFee = setupFeePounds / Model.LoanAmount;
			loan = CreateLoan(monthlyInterestRate, setupFeePounds + Model.BrokerSetupFeePounds);
			apr = GetApr(loan);
			annualizedInterestRate = Model.TenureMonths != 0 ? (monthlyInterestRate * 12) + (setupFee * 12 / Model.TenureMonths) : 0;
			return setupFee;
		}

		private decimal FindUpperBoundary(out decimal balance)
		{
			balance = -1;
			decimal monthlyInterestRate = 1;
			while (balance < 0)
			{
				if (monthlyInterestRate > 1000000)
				{
					LogInputs();
					throw new StrategyWarning(this, "Monthly interest rate should be over 1000000, aborting calculation");
				}
				monthlyInterestRate *= 10;
				balance = CalculateBalance(monthlyInterestRate);
			}

			return monthlyInterestRate;
		}

		private decimal GetMonthlyInterestRate()
		{
			decimal lowerBoundary = 0;
			decimal balanceForUpperBoundary;
			decimal upperBoundary = FindUpperBoundary(out balanceForUpperBoundary);
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
			inputs.Append("Cogs:").Append(Model.Cogs).Append("\r\n");
			inputs.Append("DebtPercentOfCapital:").Append(Model.DebtPercentOfCapital).Append("\r\n");
			inputs.Append("CostOfDebt:").Append(Model.CostOfDebt).Append("\r\n");
			inputs.Append("OpexAndCapex:").Append(Model.OpexAndCapex).Append("\r\n");
			inputs.Append("ProfitMarkup:").Append(Model.ProfitMarkup).Append("\r\n");

			Log.Warn("Inputs were:\r\n{0}", inputs);
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
	}
}
