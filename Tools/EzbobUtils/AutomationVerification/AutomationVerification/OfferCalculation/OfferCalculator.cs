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
            this.dbHelper = new DbHelper(DB, Log);
		} // constructor

		/// <summary>
		/// Get Offer For COSME loan source using hard coded interest selection and calculated setup fee
		/// </summary>
		public OfferOutputModel GetOfferBySeek(OfferInputModel input) {
			
			var pricingScenario = this.dbHelper.GetPricingScenario(input.Amount, input.HasLoans);

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

			//return CalculateLegacyOfferBySeek(input, setupFeeRange, pricingScenario, pricingCalculator, interestRateRange, outModel);

		    decimal interestRate = GetCOSMEInterestRate(pricingCalculator.ConsumerScore, pricingCalculator.CompanyScore);
            decimal setupFee = pricingCalculator.GetSetupfee(interestRate, input.LoanSourceId == CosmeLoanSourceId);

		    outModel.InterestRate = interestRate*100;

            setupFee = AdjustMinMaxSetupFee(input.Amount, setupFee);

		    outModel.SetupFee = RoundSetupFee(setupFee);
            Log.Info("Verification Rounding setup fee {0} -> {1}", setupFee, outModel.SetupFee);
            
		    return outModel;
		}// GetOfferBySeek

	    private decimal AdjustMinMaxSetupFee(int amount, decimal setupFee) {
	        OfferSetupFeeRangeModelDb setupFeeRange = this.dbHelper.GetOfferSetupFeeRange(amount);
	        setupFeeRange.MaxSetupFee /= 100;
	        setupFeeRange.MinSetupFee /= 100;

	        if (setupFee < setupFeeRange.MinSetupFee) {
	            Log.Info("Verification Setup fee is {0} less then min {1}, adjusting", setupFee, setupFeeRange.MinSetupFee);
	            setupFee = setupFeeRange.MinSetupFee;
	        }

	        if (setupFee > setupFeeRange.MaxSetupFee) {
	            Log.Info("Verification Setup fee is {0} bigger then max {1}, adjusting", setupFee, setupFeeRange.MaxSetupFee);
	            setupFee = setupFeeRange.MaxSetupFee;
	        }
	        return setupFee;
	    }//AdjustMinMaxSetupFee



	    private decimal GetCOSMEInterestRate(int consumerScore, int companyScore) {
            if (consumerScore < 1040 && companyScore == 0)   { return 0.0225M; }
            if (consumerScore >= 1040 && companyScore == 0)  { return 0.0175M; }
            if (consumerScore < 1040 && companyScore >= 50)  { return 0.0200M; }
            if (consumerScore >= 1040 && companyScore >= 50) { return 0.0175M; }
            //if companyScore < 50
            return 0.0225M;
	    }//GetCOSMEInterestRate

        /// <summary>
        /// This is the old function kept for reference, not in use
        /// </summary>
        internal OfferOutputModel CalculateLegacyOfferBySeek(
            OfferInputModel input,
            PricingScenarioModel pricingScenario, 
            PricingCalculator pricingCalculator, 
            OfferOutputModel outModel) {
            OfferSetupFeeRangeModelDb setupFeeRange = dbHelper.GetOfferSetupFeeRange(input.Amount);
            OfferInterestRateRangeModelDb interestRateRange = dbHelper.GetOfferIneterestRateRange(input.Medal);

            interestRateRange.MaxInterestRate = interestRateRange.MaxInterestRate / 100;
            interestRateRange.MinInterestRate = interestRateRange.MinInterestRate / 100;

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
	    }//CalculateLegacyOfferBySeek

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

		private decimal RoundInterest(decimal interest) {
			return Math.Ceiling(Math.Round(interest, 4, MidpointRounding.AwayFromZero) * 2000) / 20;
		} // RoundInterest

		private decimal RoundSetupFee(decimal setupFee) {
			return Math.Ceiling(setupFee * 200) / 2;
		} // RoundSetupFee

        protected const decimal SetupFeeStep = 0.05M; 
        protected const decimal InterestRateStep = 0.005M;
	    protected const int CosmeLoanSourceId = 3;
        protected ASafeLog Log { get; set; }
        protected AConnection DB { get; set; }
        private readonly DbHelper dbHelper;

	} // class OfferCalculator
} // namespace
