namespace AutomationCalculator.OfferCalculation
{
	using System;
	using Common;
	using Ezbob.Logger;

	public class OfferCalculator
	{
		protected decimal SetupFeeStep {get { return 0.05M; }}
		protected decimal InterestRateStep {get { return 0.005M; }}
		protected ASafeLog Log;

		public OfferCalculator(ASafeLog log) {
			Log = log;
		}

		public OfferOutputModel GetOffer(OfferInputModel input) {
			var dbHelper = new DbHelper(Log);
			var setupFeeRange = dbHelper.GetOfferSetupFeeRange(input.Amount);
			var interestRateRange = dbHelper.GetOfferIneterestRateRange(input.Medal);
			var pricingScenario = dbHelper.GetPricingScenario(input.Amount, input.HasLoans);

			var pricingCalculator = new PricingCalculator();

			var outModel = new OfferOutputModel();

			
			if (input.AspireToMinSetupFee) {
				bool wasTooSmallInterest = false;
				var setupFee = setupFeeRange.MinSetupFee;
				do {
					pricingScenario.SetupFee = setupFee;
					var interest = pricingCalculator.GetInterestRate(input.Amount, pricingScenario);
					if (interest >= interestRateRange.MinInterestRate && interest <= interestRateRange.MaxInterestRate) {
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = setupFee;
						return outModel;
					}

					if (interest > interestRateRange.MaxInterestRate && wasTooSmallInterest) {
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = setupFee;
						return outModel;
					}

					if (interest < interestRateRange.MinInterestRate) {
						wasTooSmallInterest = true;
					}
					else {
						wasTooSmallInterest = false;
					}

					setupFee += SetupFeeStep;
				} while (setupFee <= setupFeeRange.MaxSetupFee);
			}


			if (!input.AspireToMinSetupFee) {
				var wasTooBigIneteres = false;
				var setupFee = setupFeeRange.MaxSetupFee;
				do
				{
					pricingScenario.SetupFee = setupFee;
					var interest = pricingCalculator.GetInterestRate(input.Amount, pricingScenario);
					if (interest >= interestRateRange.MinInterestRate && interest <= interestRateRange.MaxInterestRate)
					{
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = setupFee;
						return outModel;
					}

					if (interest < interestRateRange.MinInterestRate && wasTooBigIneteres)
					{
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = setupFee;
						return outModel;
					}

					if (interest > interestRateRange.MaxInterestRate)
					{
						wasTooBigIneteres = true;
					}
					else
					{
						wasTooBigIneteres = false;
					}

					setupFee -= SetupFeeStep;
				} while (setupFee <= setupFeeRange.MaxSetupFee);
			}
			
			return outModel;
		}

		private decimal RoundInterest(decimal interest) {
			return Math.Round(interest*2000, 0, MidpointRounding.AwayFromZero)/20;
		}
	}
}
