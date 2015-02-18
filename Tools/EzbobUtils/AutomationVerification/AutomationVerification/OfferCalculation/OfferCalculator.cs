namespace AutomationCalculator.OfferCalculation {
	using System;
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;

	public class OfferCalculator {
		public OfferCalculator(AConnection db, ASafeLog log) {
			DB = db;
			Log = log;
		} // calculator

		/// <summary>
		/// Get Offer Using seek (change setup fee to find interest rate in range
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public OfferOutputModel GetOfferBySeek(OfferInputModel input) {
			var dbHelper = new DbHelper(DB, Log);

			OfferSetupFeeRangeModelDb setupFeeRange = dbHelper.GetOfferSetupFeeRange(input.Amount);
			OfferInterestRateRangeModelDb interestRateRange = dbHelper.GetOfferIneterestRateRange(input.Medal);

			interestRateRange.MaxInterestRate = interestRateRange.MaxInterestRate / 100;
			interestRateRange.MinInterestRate = interestRateRange.MinInterestRate / 100;

			var pricingScenario = dbHelper.GetPricingScenario(input.Amount, input.HasLoans);

			var outModel = new OfferOutputModel {
				ScenarioName = pricingScenario.ScenarioName,
				Amount = input.Amount,
				CustomerId = input.CustomerId,
				Medal = input.Medal,
				CalculationTime = DateTime.UtcNow
			};

			var pricingCalculator = new PricingCalculator(
				input.CustomerId,
				pricingScenario,
				input.Amount,
				outModel.RepaymentPeriod,
				DB
			);

			if (input.AspireToMinSetupFee) {
				bool wasTooSmallInterest = false;

				decimal setupFee = setupFeeRange.MinSetupFee;

				do {
					pricingScenario.SetupFee = setupFee;

					decimal interest = pricingCalculator.GetInterestRate();

					if (interest >= interestRateRange.MinInterestRate && interest <= interestRateRange.MaxInterestRate) {
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = RoundSetupFee(setupFee / 100);
						outModel.HasDecision = true;
						return outModel;
					} // if

					if (interest > interestRateRange.MaxInterestRate && wasTooSmallInterest) {
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = RoundSetupFee(setupFee / 100);
						outModel.HasDecision = true;
						return outModel;
					} // if

					wasTooSmallInterest = interest < interestRateRange.MinInterestRate;

					setupFee += SetupFeeStep;
				} while (setupFee <= setupFeeRange.MaxSetupFee);
			} else {
				bool wasTooBigIneterest = false;

				decimal setupFee = setupFeeRange.MaxSetupFee;

				do {
					pricingScenario.SetupFee = setupFee;

					decimal interest = pricingCalculator.GetInterestRate();

					if (interest >= interestRateRange.MinInterestRate && interest <= interestRateRange.MaxInterestRate) {
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = RoundSetupFee(setupFee / 100);
						outModel.HasDecision = true;
						return outModel;
					} // if

					if (interest < interestRateRange.MinInterestRate && wasTooBigIneterest) {
						outModel.InterestRate = RoundInterest(interest);
						outModel.SetupFee = RoundSetupFee(setupFee / 100);
						outModel.HasDecision = true;
						return outModel;
					} // if

					wasTooBigIneterest = interest > interestRateRange.MaxInterestRate;

					setupFee -= SetupFeeStep;
				} while (setupFee <= setupFeeRange.MaxSetupFee);
			} // if

			outModel.Message = "No interest rate found in range.";

			if (input.AspireToMinSetupFee) {
				outModel.InterestRate = RoundInterest(interestRateRange.MaxInterestRate);
				outModel.SetupFee = RoundSetupFee(setupFeeRange.MinSetupFee / 100);
			} else {
				outModel.InterestRate = RoundInterest(interestRateRange.MinInterestRate);
				outModel.SetupFee = RoundSetupFee(setupFeeRange.MaxSetupFee / 100);
			} // if

			return outModel;
		} // GetOfferBySeek

		/// <summary>
		/// calc setup fee for max interest rate (f1) and for min interest rate (f2)
		/// find if any (f1,f2) ∩ (x1, x2) min max interest rate configuration based on medal.
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Offer model : interest rate, setup fee, repayment period, loan type and source</returns>
		public OfferOutputModel GetOfferByBoundaries(OfferInputModel input) {
			var dbHelper = new DbHelper(DB, Log);

			OfferSetupFeeRangeModelDb setupFeeRange = dbHelper.GetOfferSetupFeeRange(input.Amount);
			OfferInterestRateRangeModelDb interestRateRange = dbHelper.GetOfferIneterestRateRange(input.Medal);

			interestRateRange.MaxInterestRate = interestRateRange.MaxInterestRate / 100;
			interestRateRange.MinInterestRate = interestRateRange.MinInterestRate / 100;

			PricingScenarioModel pricingScenario = dbHelper.GetPricingScenario(input.Amount, input.HasLoans);

			var outModel = new OfferOutputModel {
				ScenarioName = pricingScenario.ScenarioName,
				Amount = input.Amount,
				Medal = input.Medal,
				CustomerId = input.CustomerId,
				CalculationTime = DateTime.UtcNow
			};

			var pricingCalculator = new PricingCalculator(
				input.CustomerId,
				pricingScenario,
				input.Amount,
				outModel.RepaymentPeriod,
				DB
			);

			decimal setupfeeRight = pricingCalculator.GetSetupfee(interestRateRange.MinInterestRate);

			decimal setupfeeLeft = pricingCalculator.GetSetupfee(interestRateRange.MaxInterestRate);

			TInterval<decimal> calcSetupfees = new TInterval<decimal>(
				new DecimalIntervalEdge(setupfeeLeft),
				new DecimalIntervalEdge(setupfeeRight)
			);

			TInterval<decimal> configSetupfees = new TInterval<decimal>(
				new DecimalIntervalEdge(setupFeeRange.MinSetupFee / 100),
				new DecimalIntervalEdge(setupFeeRange.MaxSetupFee / 100)
			);

			TInterval<decimal> intersect = calcSetupfees * configSetupfees;

			if (intersect == null) {
				outModel.Message = string.Format(
					"No setup fee intersection found between (min max interest rate) {0} and {1} (configuration range)",
					calcSetupfees,
					configSetupfees
				);

				Log.Warn("No setup fee intersect found between {0} and {1}", calcSetupfees, configSetupfees);

				if (input.AspireToMinSetupFee) {
					outModel.SetupFee = RoundSetupFee(setupFeeRange.MinSetupFee / 100);
					outModel.InterestRate = RoundInterest(interestRateRange.MaxInterestRate);
				} else {
					outModel.SetupFee = RoundSetupFee(setupFeeRange.MaxSetupFee / 100);
					outModel.InterestRate = RoundInterest(interestRateRange.MinInterestRate);
				} // if
			} else {
				outModel.HasDecision = true;

				decimal setupFee = input.AspireToMinSetupFee ? intersect.Left.Value : intersect.Right.Value;

				pricingScenario.SetupFee = setupFee * 100;
				decimal interestRate = pricingCalculator.GetInterestRate();

				outModel.SetupFee = RoundSetupFee(setupFee);
				outModel.InterestRate = RoundInterest(interestRate);
			} // if

			return outModel;
		} // GetOfferByBoundaries

		protected decimal SetupFeeStep { get { return 0.05M; } }
		protected decimal InterestRateStep { get { return 0.005M; } }
		protected ASafeLog Log;
		protected AConnection DB;

		private decimal RoundInterest(decimal interest) {
			return Math.Ceiling(Math.Round(interest, 4, MidpointRounding.AwayFromZero) * 2000) / 20;
		} // RoundInterest

		private decimal RoundSetupFee(decimal setupFee) {
			return Math.Ceiling(setupFee * 200) / 2;
		} // RoundSetupFee
	} // class OfferCalculator
} // namespace
