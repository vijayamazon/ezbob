namespace EzBob.Backend.Strategies.PricingModel
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
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
			{
				throw new Exception("LoanAmount must be positive");
			}
			if (model.TenureMonths <= 0)
			{
				throw new Exception("TenureMonths must be positive");
			}
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
			
			Loan loan = CreateLoan(Model.MonthlyInterestRate);
			var aprCalc = new APRCalculator();
			Model.Apr = (decimal)(loan.LoanAmount == 0 ? 0 : aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date) / 100);

			Model.CostOfDebtOutput = GetCostOfDebt(loan.Schedule);
			Model.InterestRevenue = GetInterestRevenue(loan.Schedule);

			Model.Revenue = Model.FeesRevenue + Model.InterestRevenue;

			int consumerScore = DB.ExecuteScalar<int>(
				"GetExperianScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			Model.EuLoanPercentages = GetEuLoanMonthlyInterest(consumerScore);
			Model.CogsOutput = Model.Cogs;
			Model.OpexAndCapexOutput = Model.OpexAndCapex;
			Model.GrossProfit = Model.Revenue - Model.Cogs;
			Model.Ebitda = Model.GrossProfit - Model.OpexAndCapex;
			Model.NetLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate / 100;
			Model.ProfitMarkupOutput = Model.ProfitMarkup * Model.Revenue;
			Model.AnnualizedInterestRate = Model.TenureMonths != 0 ? (Model.MonthlyInterestRate * 12) + (Model.SetupFeePercents * 12 / Model.TenureMonths) : 0;
			Model.TotalCost = Model.CostOfDebtOutput + Model.Cogs + Model.OpexAndCapex + Model.NetLossFromDefaults;

			Model.SetupFeeForEuLoanHigh = GetSetupFeeForEu(0.02m);
			Model.SetupFeeForEuLoanLow = GetSetupFeeForEu(0.0175m);
		}

		private Loan CreateLoan(decimal interestRate)
		{
			var calculator = new LoanScheduleCalculator { Interest = interestRate, Term = (int)Model.TenureMonths };
			LoanType lt = new StandardLoanType();
			var loan = new Loan
			{
				LoanAmount = Model.LoanAmount,
				Date = DateTime.UtcNow,
				LoanType = lt,
				CashRequest = null,
				SetupFee = Model.SetupFeePounds,
				LoanLegalId = 1
			};
			calculator.Calculate(Model.LoanAmount, loan, loan.Date, Model.InterestOnlyPeriod);

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
			calc.GetState();

			return loan;
		}

		private decimal GetSetupFeeForEu(decimal monthlyInterestRate)
		{
			Loan loan = CreateLoan(monthlyInterestRate);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			return (Model.ProfitMarkupOutput/Model.ProfitMarkup - interestRevenue)/Model.LoanAmount;
		}

		private decimal GetMonthlyInterestRate()
		{
			decimal guessInterval = 0.01m;
			decimal monthlyInterestRate = 0.01m;
			var calculations = new Dictionary<decimal, decimal>();
			decimal minBalanceAbsValue = decimal.MaxValue;
			decimal interestRateForMinBalanceAbs = 0.01m;

			while (minBalanceAbsValue != 0)
			{
				decimal balance = CalculateBalance(monthlyInterestRate);
				if (Math.Abs(balance) < minBalanceAbsValue)
				{
					minBalanceAbsValue = Math.Abs(balance);
					interestRateForMinBalanceAbs = monthlyInterestRate;
				}

				calculations.Add(monthlyInterestRate, balance);

				decimal nextMonthlyInterestRate = balance < 0 ? monthlyInterestRate + guessInterval : monthlyInterestRate - guessInterval;
				if (calculations.ContainsKey(nextMonthlyInterestRate))
				{
					if (guessInterval == 0.01m)
					{
						guessInterval = 0.001m;
						nextMonthlyInterestRate = balance < 0 ? monthlyInterestRate + guessInterval : monthlyInterestRate - guessInterval;
					}
					else if (guessInterval == 0.001m)
					{
						guessInterval = 0.0001m;
						nextMonthlyInterestRate = balance < 0 ? monthlyInterestRate + guessInterval : monthlyInterestRate - guessInterval;
					}
					else
					{
						break;
					}
				}
				monthlyInterestRate = nextMonthlyInterestRate;
			}

			return interestRateForMinBalanceAbs;
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
			interestRevenue *= (100 - Model.DefaultRate) / 100;
			return interestRevenue;
		}

		private decimal CalculateBalance(decimal interestRate)
		{
			Loan loan = CreateLoan(interestRate);
			decimal costOfDebtOutput = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal revenue = Model.SetupFeePounds + interestRevenue;
			decimal grossProfit = revenue - Model.Cogs;
			decimal ebitda = grossProfit - Model.OpexAndCapex;
			decimal netLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate / 100;
			decimal profitMarkupOutput = Model.ProfitMarkup * revenue;
			return ebitda - netLossFromDefaults - costOfDebtOutput - profitMarkupOutput;
		}

		private decimal GetEuLoanMonthlyInterest(int key)
		{
			DataTable dt = DB.ExecuteReader("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "EuLoanMonthlyInterest"), new QueryParameter("Key", key));
			var sr = new SafeReader(dt.Rows[0]);
			return sr["Value"];
		}
	}
}
