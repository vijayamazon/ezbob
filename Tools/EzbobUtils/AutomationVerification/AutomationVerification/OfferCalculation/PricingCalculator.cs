
namespace AutomationCalculator.OfferCalculation
{
	using System;
	using Common;

	public class PricingCalculator
	{
		public decimal GetInterestRate(int loanAmount, decimal setupFee, PricingScenarioModel pricingScenario) {
			//TODO Implement
			var rand = new Random(loanAmount);
			return rand.Next(0,10) / 100.0M;
		}
	}
}
