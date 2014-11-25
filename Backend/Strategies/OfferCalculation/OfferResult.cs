namespace EzBob.Backend.Strategies.OfferCalculation
{
	using System;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using MedalCalculations;

	public class OfferResult : IEquatable<OfferOutputModel>
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

		public bool Equals(OfferOutputModel other)
		{
			if (Amount != other.Amount || ScenarioName != other.ScenarioName || InterestRate != other.InterestRate || SetupFee != other.SetupFee)
			{
				Error = Error == null ? "Mismatch in two offer calculations" : Error + ", Mismatch in two offer calculations";
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

		public override string ToString()
		{
			return string.Format("InterestRate {0},SetupFee: {1},RepaymentPeriod: {2},LoanType: {3},IsEu: {4}{5}",
				InterestRate, SetupFee, Period, LoanTypeId, IsEu, Error == null ? "" : "Error: " + Error);
		}

		
	}
}
