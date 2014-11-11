namespace EzBob.Backend.Strategies.OfferCalculation
{
	using System;
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
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
	}
}
