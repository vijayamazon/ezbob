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
			Model.MonthlyInterestRate = 0.34m;
			Model.SetupFeeForEuLoan = 0.03m;
			Model.EuLoanPercentages = 0.75m;
			Model.Revenue = 87;
			Model.CogsOutput = 50;
			Model.GrossProfit = 432;
			Model.OpexAndCapexOutput = 66844;
			Model.Ebitda = 11332;
			Model.NetLossFromDefaults = 8912;
			Model.CostOfDebtOutput = 345;
			Model.TotalCost = 34700;
			Model.ProfitMarkupOutput = 5000;
			Model.AnnualizedInterestRate = 0.28m;
			Model.Apr = 1.23m;
		}
	}
}
