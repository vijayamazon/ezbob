namespace EzBob.Backend.Strategies.OfferCalculation
{
	using System;
	using Ezbob.Database;
	using MedalCalculations;

	public class OfferResult
	{
		// Inputs
		public int CustomerId { get; set; }
		public DateTime CalculationTime { get; set; }
		public int Amount { get; set; }
		public MedalClassification MedalClassification { get; set; }

		// Outputs
		public string ScenarioName { get; set; }
		public int Period { get; set; }
		public bool IsEu { get; set; }
		public int LoanTypeId { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
		public string Error { get; set; }

		public bool IsIdentical(OfferResult other)
		{
			if (CustomerId != other.CustomerId ||
			    CalculationTime != other.CalculationTime ||
			    Amount != other.Amount ||
			    MedalClassification != other.MedalClassification ||
			    ScenarioName != other.ScenarioName ||
				Period != other.Period ||
				IsEu != other.IsEu ||
				LoanTypeId != other.LoanTypeId ||
			    InterestRate != other.InterestRate ||
			    SetupFee != other.SetupFee ||
			    Error != other.Error)
			{
				return false;
			}

			return true;
		}

		public void SaveToDb(AConnection db)
		{
			db.ExecuteNonQuery("StoreOffer", CommandSpecies.StoredProcedure,
			                   new QueryParameter("CustomerId", CustomerId),
			                   new QueryParameter("CalculationTime", CalculationTime),
			                   new QueryParameter("Amount", Amount),
			                   new QueryParameter("MedalClassification", MedalClassification.ToString()),
			                   new QueryParameter("ScenarioName", ScenarioName),
							   new QueryParameter("Period", Period),
							   new QueryParameter("IsEu", IsEu),
							   new QueryParameter("LoanTypeId", LoanTypeId),
			                   new QueryParameter("InterestRate", InterestRate),
			                   new QueryParameter("SetupFee", SetupFee),
			                   new QueryParameter("Error", Error));
		}
	}
}
