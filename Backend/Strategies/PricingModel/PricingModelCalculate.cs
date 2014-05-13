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
			Model.Apr = (decimal)(loan.LoanAmount == 0 ? 0 : aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date));

			Model.CostOfDebtOutput = 0;
			Model.InterestRevenue = 0;
			foreach (LoanScheduleItem scheuldeItem in loan.Schedule)
			{
				Model.InterestRevenue += scheuldeItem.Interest;
				Model.CostOfDebtOutput += scheuldeItem.AmountDue * Model.DebtPercentOfCapital * Model.CostOfDebt / 12;
			}
			Model.InterestRevenue *= (100 - Model.DefaultRate) / 100;

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

			Model.SetupFeeForEuLoanHigh = GetSetupFeeForEu(2);
			Model.SetupFeeForEuLoanLow = GetSetupFeeForEu(1.75m);
		}

		private Loan CreateLoan(decimal interestRate)
		{
			var sfc = new SetupFeeCalculator(false, false, 0, 0);
			var setupFee = sfc.Calculate(Model.LoanAmount);

			var calculator = new LoanScheduleCalculator { Interest = interestRate, Term = Model.LoanTerm };
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

		private decimal GetSetupFeeForEu(decimal monthlyInterestRate)
		{
			Loan loan = CreateLoan(monthlyInterestRate);
			decimal interestRevenue = loan.Schedule.Sum(scheuldeItem => scheuldeItem.Interest);
			interestRevenue *= (100 - Model.DefaultRate) / 100;
			return (Model.ProfitMarkupOutput/Model.ProfitMarkup - interestRevenue)/Model.LoanAmount;
		}

		private decimal GetMonthlyInterestRate()
		{
			decimal guessInterval = 1;
			decimal monthlyInterestRate = 1;
			var calculations = new Dictionary<decimal, decimal>();
			decimal minBalanceAbsValue = decimal.MaxValue;
			decimal interestRateForMinBalanceAbs = 1;

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
					if (guessInterval == 1)
					{
						guessInterval = 0.1m;
						nextMonthlyInterestRate = balance < 0 ? monthlyInterestRate + guessInterval : monthlyInterestRate - guessInterval;
					}
					else if (guessInterval == 0.1m)
					{
						guessInterval = 0.01m;
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

		private decimal CalculateBalance(decimal interestRate)
		{
			Loan loan = CreateLoan(interestRate);

			decimal costOfDebtOutput = 0;
			decimal interestRevenue = 0;
			foreach (LoanScheduleItem scheuldeItem in loan.Schedule)
			{
				interestRevenue += scheuldeItem.Interest;
				costOfDebtOutput += scheuldeItem.AmountDue * Model.DebtPercentOfCapital * Model.CostOfDebt / 12;
			}
			interestRevenue *= (100 - Model.DefaultRate) / 100;

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
