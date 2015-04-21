namespace AutomationCalculator.OfferCalculation {
	using System;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;
	// using Ezbob.ValueIntervals;

	public class OfferCalculator {
		public OfferCalculator(AConnection db, ASafeLog log) {
			DB = db;
			Log = log;
			this.dbHelper = new DbHelper(DB, Log);
		} // constructor

		/// <summary>
		/// Get Offer For COSME loan source using hard coded interest selection and calculated setup fee
		/// </summary>
		public OfferOutputModel GetCosmeOffer(OfferInputModel input) {
			PricingScenarioModel pricingScenario = this.dbHelper.GetPricingScenario(input.Amount, input.HasLoans);

			var outModel = new OfferOutputModel {
				ScenarioName = pricingScenario.ScenarioName,
				Amount = input.Amount,
				CustomerId = input.CustomerId,
				Medal = input.Medal,
				CalculationTime = DateTime.UtcNow,
				LoanSourceID = input.LoanSourceId,
				RepaymentPeriod = input.RepaymentPeriod
			};

			var pricingCalculator = new PricingCalculator(
				input.CustomerId,
				pricingScenario,
				input.Amount,
				outModel.RepaymentPeriod,
				DB,
				Log
			);

			// return CalculateLegacyOfferBySeek(
			// input, setupFeeRange, pricingScenario, pricingCalculator, interestRateRange, outModel
			// );

			decimal interestRate = GetCOSMEInterestRate(pricingCalculator.ConsumerScore, pricingCalculator.CompanyScore);
			decimal setupFee = pricingCalculator.GetSetupfee(interestRate, input.LoanSourceId == CosmeLoanSourceId);

			outModel.InterestRate = interestRate * 100;

			setupFee = AdjustMinMaxSetupFee(input.Amount, setupFee);

			outModel.SetupFee = RoundSetupFee(setupFee);
			Log.Info("Verification Rounding setup fee {0} -> {1}", setupFee, outModel.SetupFee);

			return outModel;
		} // GetCosmeOffer

		protected const int CosmeLoanSourceId = 3;
		protected ASafeLog Log { get; set; }
		protected AConnection DB { get; set; }

		private decimal AdjustMinMaxSetupFee(int amount, decimal setupFee) {
			OfferSetupFeeRangeModelDb setupFeeRange = this.dbHelper.GetOfferSetupFeeRange(amount);
			setupFeeRange.MaxSetupFee /= 100;
			setupFeeRange.MinSetupFee /= 100;

			if (setupFee < setupFeeRange.MinSetupFee) {
				Log.Info(
					"Verification Setup fee is {0} less then min {1}, adjusting to the min value.",
					setupFee,
					setupFeeRange.MinSetupFee
				);

				setupFee = setupFeeRange.MinSetupFee;
			} // if

			if (setupFee > setupFeeRange.MaxSetupFee) {
				Log.Info(
					"Verification Setup fee is {0} bigger then max {1}, adjusting to the max value.",
					setupFee,
					setupFeeRange.MaxSetupFee
				);

				setupFee = setupFeeRange.MaxSetupFee;
			} // if

			return setupFee;
		} // AdjustMinMaxSetupFee

		private decimal GetCOSMEInterestRate(int consumerScore, int companyScore) {
			if (companyScore == 0)
				return (consumerScore < 1040) ? 0.0225M : 0.0175M;

			if (companyScore >= 50)
				return (consumerScore < 1040) ? 0.0200M : 0.0175M;
			
			return 0.0225M;
		} // GetCOSMEInterestRate

		private decimal RoundSetupFee(decimal setupFee) {
			return Math.Ceiling(setupFee * 200) / 2;
		} // RoundSetupFee

		private readonly DbHelper dbHelper;

		/*

		/// <summary>
		/// this is old function kept for reference, not in use
		/// calc setup fee for max interest rate (f1) and for min interest rate (f2)
		/// find if any (f1,f2) ∩ (x1, x2) min max interest rate configuration based on medal.
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Offer model : interest rate, setup fee, repayment period, loan type and source</returns>
		public OfferOutputModel GetOfferByBoundariesLegacy(OfferInputModel input) {
			OfferSetupFeeRangeModelDb setupFeeRange = this.dbHelper.GetOfferSetupFeeRange(input.Amount);
			OfferInterestRateRangeModelDb interestRateRange = this.dbHelper.GetOfferIneterestRateRange(input.Medal);

			interestRateRange.MaxInterestRate = interestRateRange.MaxInterestRate / 100;
			interestRateRange.MinInterestRate = interestRateRange.MinInterestRate / 100;

			PricingScenarioModel pricingScenario = this.dbHelper.GetPricingScenario(input.Amount, input.HasLoans);

			var outModel = new OfferOutputModel {
				ScenarioName = pricingScenario.ScenarioName,
				Amount = input.Amount,
				Medal = input.Medal,
				CustomerId = input.CustomerId,
				CalculationTime = DateTime.UtcNow,
				LoanSourceID = input.LoanSourceId,
				RepaymentPeriod = input.RepaymentPeriod
			};

			var pricingCalculator = new PricingCalculator(
				input.CustomerId,
				pricingScenario,
				input.Amount,
				outModel.RepaymentPeriod,
				DB,
				Log
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
		} // GetOfferByBoundariesLegacy

		protected const decimal SetupFeeStep = 0.05M;
		protected const decimal InterestRateStep = 0.005M;

		private decimal RoundInterest(decimal interest) {
			return Math.Ceiling(Math.Round(interest, 4, MidpointRounding.AwayFromZero) * 2000) / 20;
		} // RoundInterest

		*/
	} // class OfferCalculator
} // namespace
