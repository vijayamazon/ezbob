namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

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
		
		public override void Execute()
		{
			Model.FeesRevenue = Model.SetupFeePounds;


			// These values should be calculated accoring to loan calculation variables
			Model.InterestRevenue = 0; // Sum the middle column in the approve loan window
			// Get Loan calculation variables... follow loan approval process to get APR and outstandingprincipal
			Model.CostOfDebtOutput = 0;
			for (int i = 0; i < Model.LoanTerm; i++)
			{
				//Model.CostOfDebtOutput +=
			}
			Model.Apr = 1.23m;


			Model.Revenue = Model.FeesRevenue + Model.InterestRevenue;


			Model.MonthlyInterestRate = 0.34m;
			Model.SetupFeeForEuLoan = 0.03m;



			int consumerScore = DB.ExecuteScalar<int>(
				"GetExperianScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			Model.EuLoanPercentages = GetEuLoanMonthlyInterest(consumerScore);
			
			Model.CogsOutput = Model.Cogs;
			Model.OpexAndCapexOutput = Model.OpexAndCapex;
			Model.Ebitda = Model.GrossProfit - Model.OpexAndCapex;
			Model.GrossProfit = Model.Revenue - Model.Cogs;
			Model.NetLossFromDefaults = (1 - Model.CollectionRate) * Model.LoanAmount * Model.DefaultRate;
			Model.ProfitMarkupOutput = Model.ProfitMarkup * Model.Revenue;
			Model.AnnualizedInterestRate = Model.TenureMonths != 0 ? (Model.MonthlyInterestRate * 12) + (Model.SetupFeePercents * 12 / Model.TenureMonths) : 0;


			


			Model.TotalCost = Model.CostOfDebtOutput + Model.Cogs + Model.OpexAndCapex + Model.NetLossFromDefaults;
		}

		private decimal GetEuLoanMonthlyInterest(int key)
		{
			DataTable dt = DB.ExecuteReader("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "EuLoanMonthlyInterest"), new QueryParameter("Key", key));
			var sr = new SafeReader(dt.Rows[0]);
			return sr["Value"];
		}
	}
}
