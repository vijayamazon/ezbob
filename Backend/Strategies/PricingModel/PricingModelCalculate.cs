﻿namespace EzBob.Backend.Strategies.PricingModel
{
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PricingModelCalculate : AStrategy
	{
		public PricingModelCalculate(PricingModelModel model, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			Model = model;
		}

		public override string Name {
			get { return "Pricing model calculate"; }
		}

		public PricingModelModel Model { get; private set; }
		
		public override void Execute() {
			// TODO: Calculation logic
			Model.InterestRate = 4.03m;
			Model.MonthlyInterestToCharge = 1.23m;
			Model.SetupFeeForEuLoan = 400;
			Model.EuLoanPercentages = 1.75m;
			Model.AverageLoanAmount = 4500;
			Model.AverageRevenuePerLoan = 87;
			Model.CogsOutput = 50;
			Model.GrossProfit = 432;
			Model.OpexAndCapexOutput = 66844;
			Model.Ebitda = 11332;
			Model.NetLossFromDefaults = 8912;
			Model.CostOfDebtOutput = 345;
			Model.TotalCost = 34700;
			Model.ProfitBeforeTaxOutput = 5000;
			Model.Balance = 4567;
		}
	}
}
