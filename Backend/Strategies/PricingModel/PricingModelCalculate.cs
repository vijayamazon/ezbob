namespace EzBob.Backend.Strategies.PricingModel
{
	using System;
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

		private Loan CreateLoan()
		{
			var sfc = new SetupFeeCalculator(false, false, 0, 0);
			var setupFee = sfc.Calculate(Model.LoanAmount);

			var calculator = new LoanScheduleCalculator { Interest = Model.MonthlyInterestRate, Term = Model.LoanTerm };
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
			//GetMonthlyInterestRate();
			Model.FeesRevenue = Model.SetupFeePounds;

			// Algorithm logic:
			// 1. Find MonthlyInterestRate that will result in balance = 0 (balance = ebitda - net loss from default - cost of debt - profit before tax)
			// 2. Remember the profit before tax that was calculated.
			// 3. FeesRevenue = Guessed setup fee for eu (use eu collection rate as collection rate and use eu loan percentages as monthly interest)
			// 4. Find a setup fee that yeilds the same profit as the one remembered on #3

			Model.MonthlyInterestRate = 0.34m; // TODO: implement calc
			Model.SetupFeeForEuLoanHigh = 0.03m;
			Model.SetupFeeForEuLoanLow = 0.03m;

			Loan loan = CreateLoan();
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

		private void GetMonthlyInterestRate()
		{
			decimal guessInterval = 1;
			Model.MonthlyInterestRate = 1;
			decimal balance = CalculateBalance();
			decimal prevBalance = -1000000;
			decimal prevPrevBalance = -1000000;
			while (balance < -1 || balance > 1)
			{
				if (prevPrevBalance == balance)
				{
					if (guessInterval == 1)
					{
						guessInterval = 0.1m;
					}
					else if (guessInterval == 0.1m)
					{
						guessInterval = 0.01m;
					}
					else
					{
						return;
					}
				}
				if (balance < 0)
				{
					Model.MonthlyInterestRate += guessInterval;
				}
				else
				{
					Model.MonthlyInterestRate -= guessInterval;
				}

				prevPrevBalance = prevBalance;
				prevBalance = balance;
				balance = CalculateBalance();
			}
		}

		private decimal CalculateBalance()
		{
			Model.FeesRevenue = Model.SetupFeePounds; // Can be done outside...
			Loan loan = CreateLoan();
			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
			calc.GetState();

			Model.CostOfDebtOutput = 0;
			Model.InterestRevenue = 0;
			foreach (LoanScheduleItem scheuldeItem in loan.Schedule)
			{
				Model.InterestRevenue += scheuldeItem.Interest;
				Model.CostOfDebtOutput += scheuldeItem.AmountDue * Model.DebtPercentOfCapital * Model.CostOfDebt / 12;
			}
			Model.InterestRevenue *= 1 - Model.DefaultRate;

			Model.Revenue = Model.FeesRevenue + Model.InterestRevenue;
			Model.GrossProfit = Model.Revenue - Model.Cogs;
			Model.Ebitda = Model.GrossProfit - Model.OpexAndCapex;
			Model.NetLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			Model.ProfitMarkupOutput = Model.ProfitMarkup * Model.Revenue;
			return Model.Ebitda - Model.NetLossFromDefaults - Model.CostOfDebtOutput - Model.ProfitMarkupOutput;
		}

		private decimal GetEuLoanMonthlyInterest(int key)
		{
			DataTable dt = DB.ExecuteReader("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "EuLoanMonthlyInterest"), new QueryParameter("Key", key));
			var sr = new SafeReader(dt.Rows[0]);
			return sr["Value"];
		}
	}
}
