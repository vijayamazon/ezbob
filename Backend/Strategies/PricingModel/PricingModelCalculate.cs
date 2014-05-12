namespace EzBob.Backend.Strategies.PricingModel
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.Calculators;

	public class PricingModelCalculate : AStrategy
	{
		public PricingModelCalculate(int customerId, PricingModelModel model, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			Model = model;
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Pricing model calculate"; }
		}

		private readonly int customerId;
		public PricingModelModel Model { get; private set; }

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
			return loan;
		}
		public override void Execute()
		{
			Model.FeesRevenue = Model.SetupFeePounds;
			Model.MonthlyInterestRate = GetMonthlyInterestRate();

			// Algorithm logic:
			// 1. Find MonthlyInterestRate that will result in balance = 0 (balance = ebitda - net loss from default - cost of debt - profit before tax)
			// 2. Remember the profit before tax that was calculated.
			// 3. FeesRevenue = Guessed setup fee for eu (use eu collection rate as collection rate and use eu loan percentages as monthly interest)
			// 4. Find a setup fee that yeilds the same profit as the one remembered on #3

			Model.SetupFeeForEuLoanHigh = 0.03m;
			Model.SetupFeeForEuLoanLow = 0.03m;

			Loan loan = CreateLoan(Model.MonthlyInterestRate);
			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
			calc.GetState();
			var aprCalc = new APRCalculator();
			Model.Apr = (decimal)(loan.LoanAmount == 0 ? 0 : aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date));

			Model.CostOfDebtOutput = 0;
			Model.InterestRevenue = 0;
			foreach (LoanScheduleItem scheuldeItem in loan.Schedule)
			{
				Model.InterestRevenue += scheuldeItem.Interest;
				Model.CostOfDebtOutput += scheuldeItem.AmountDue * Model.DebtPercentOfCapital * Model.CostOfDebt / 12;
			}
			Model.InterestRevenue *= 1 - Model.DefaultRate;

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
			Model.NetLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			Model.ProfitMarkupOutput = Model.ProfitMarkup * Model.Revenue;
			Model.AnnualizedInterestRate = Model.TenureMonths != 0 ? (Model.MonthlyInterestRate * 12) + (Model.SetupFeePercents * 12 / Model.TenureMonths) : 0;

			Model.TotalCost = Model.CostOfDebtOutput + Model.Cogs + Model.OpexAndCapex + Model.NetLossFromDefaults;
		}

		private decimal GetMonthlyInterestRate()
		{
			decimal guessInterval = 1;
			decimal monthlyInterestRate = 1;
			decimal balance = 1;
			var calculations = new Dictionary<decimal, decimal>();
			decimal minBalanceAbsValue = decimal.MaxValue;
			decimal interestRateForMinBalanceAbs = 1;

			while (balance != 0)
			{
				balance = CalculateBalance(monthlyInterestRate);
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
			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
			calc.GetState();

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
