namespace AutomationCalculator.OfferCalculation {
	using System;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

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

			decimal interestRate = GetCOSMEInterestRate(pricingCalculator.ConsumerScore, pricingCalculator.CompanyScore);
			decimal setupFee = pricingCalculator.GetSetupfee(interestRate, input.LoanSourceId == CosmeLoanSourceId);

			outModel.InterestRate = interestRate * 100;

			setupFee = AdjustMinMaxSetupFee(input.Amount, !input.HasLoans, setupFee);

			outModel.SetupFee = RoundSetupFee(setupFee);
			Log.Info("Verification Rounding setup fee {0:P2} -> {1:N2}%.", setupFee, outModel.SetupFee);

			return outModel;
		} // GetCosmeOffer

		protected const int CosmeLoanSourceId = 3;
		protected ASafeLog Log { get; set; }
		protected AConnection DB { get; set; }

		private decimal AdjustMinMaxSetupFee(int amount, bool isNewLoan, decimal setupFee) {
			OfferSetupFeeRangeModelDb setupFeeRange = DB.FillFirst<OfferSetupFeeRangeModelDb>(
				"AV_GetOfferSetupFeeRange",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Amount", amount),
				new QueryParameter("@IsNewLoan", isNewLoan)
			);

			Log.Debug(
				"Verification set up fee range({0:C2}, {1} loan) = {2} [{3:P2}, {4:P2}].",
				amount,
				isNewLoan ? "new" : "repeating",
				setupFeeRange.LoanSizeName,
				setupFeeRange.MinSetupFee,
				setupFeeRange.MaxSetupFee
			);

			if (setupFee < setupFeeRange.MinSetupFee) {
				Log.Info(
					"Verification setup fee {0:P2} is less then min {1:P2}, adjusted to the min value.",
					setupFee,
					setupFeeRange.MinSetupFee
				);

				setupFee = setupFeeRange.MinSetupFee;
			} else if (setupFee > setupFeeRange.MaxSetupFee) {
				Log.Info(
					"Verification setup fee {0:P2} is bigger then max {1:P2}, adjusted to the max value.",
					setupFee,
					setupFeeRange.MaxSetupFee
				);

				setupFee = setupFeeRange.MaxSetupFee;
			} else {
				Log.Info(
					"Verification setup fee {0:P2} is in range [{1:P2}, {2:P2}], not adjusted.",
					setupFee,
					setupFeeRange.MinSetupFee,
					setupFeeRange.MaxSetupFee
				);
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

		private static decimal RoundSetupFee(decimal setupFee) {
			return Math.Ceiling(setupFee * 200) / 2;
		} // RoundSetupFee

		private readonly DbHelper dbHelper;
	} // class OfferCalculator
} // namespace
