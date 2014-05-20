namespace EzBob.Backend.Strategies.PricingModel
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text;
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

			Loan loan = CreateLoan(Model.MonthlyInterestRate, Model.SetupFeePounds);
			Model.Apr = GetApr(loan);

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
			Loan loan = CreateLoan(monthlyInterestRate, Model.SetupFeePounds);
			decimal costOfDebtEu = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal netLossFromDefaults = (1 - Model.EuCollectionRate) * Model.LoanAmount * Model.DefaultRate;
			decimal setupFeePounds = Model.ProfitMarkupOutput - interestRevenue + Model.Cogs + Model.OpexAndCapex + netLossFromDefaults + costOfDebtEu;
			decimal setupFee = setupFeePounds / Model.LoanAmount;
			loan = CreateLoan(monthlyInterestRate, setupFeePounds);
			apr = GetApr(loan);
			annualizedInterestRate = Model.TenureMonths != 0 ? (monthlyInterestRate * 12) + (setupFee * 12 / Model.TenureMonths) : 0;
			return setupFee;
		}

		private decimal GetMonthlyInterestRate()
		{
			decimal guessInterval = 0.01m;
			decimal monthlyInterestRate = 0.01m;
			var calculations = new Dictionary<decimal, decimal>();
			decimal minBalanceAbsValue = decimal.MaxValue;
			decimal interestRateForMinBalanceAbs = 0.01m;
			int counter = 0;

			while (minBalanceAbsValue != 0)
			{
				counter++;
				if (counter > 100)
				{
					Log.Warn("Pricing model calculation reached 100 cycles and will stop.");
					
					var inputs = new StringBuilder();
					inputs.Append("LoanAmount:").Append(Model.LoanAmount).Append("\r\n");
					inputs.Append("DefaultRate:").Append(Model.DefaultRate).Append("\r\n");
					inputs.Append("DefaultRateCompanyShare:").Append(Model.DefaultRateCompanyShare).Append("\r\n");
					inputs.Append("DefaultRateCustomerShare:").Append(Model.DefaultRateCustomerShare).Append("\r\n");
					inputs.Append("SetupFeePounds:").Append(Model.SetupFeePounds).Append("\r\n");
					inputs.Append("SetupFeePercents:").Append(Model.SetupFeePercents).Append("\r\n");
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
					var balances = new StringBuilder();
					foreach (decimal interest in calculations.Keys)
					{
						balances.Append("Interest:").Append(interest).Append(" Balance:").Append(calculations[interest]).Append("\r\n");
					}
					Log.Warn("Calculated Balances:\r\n{0}", balances);

					throw new Exception("Pricing model calculation reached 100 cycles and will stop");
				}

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
			Loan loan = CreateLoan(interestRate, Model.SetupFeePounds);
			decimal costOfDebtOutput = GetCostOfDebt(loan.Schedule);
			decimal interestRevenue = GetInterestRevenue(loan.Schedule);
			decimal revenue = Model.SetupFeePounds + interestRevenue;
			decimal grossProfit = revenue - Model.Cogs;
			decimal ebitda = grossProfit - Model.OpexAndCapex;
			decimal netLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
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
