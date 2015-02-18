namespace Ezbob.Backend.Strategies.OfferCalculation {
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using PricingModel;

	public class OfferCalculator1 {
		public OfferCalculator1() {
			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;
		} // constructor

		public OfferResult CalculateOffer(
			int customerId,
			DateTime calculationTime,
			int amount,
			bool hasLoans,
			Medal medalClassification
		) {
			var result = new OfferResult {
				CustomerId = customerId,
				CalculationTime = calculationTime,
				Amount = amount,
				MedalClassification = medalClassification,
				Period = 12, // Period is always 12
				IsEu = false,
			};

			// We always use standard loan type
			// We use standard loan type
			SafeReader sr = db.GetFirst("GetStandardLoanTypeId", CommandSpecies.StoredProcedure);

			if (sr.IsEmpty) {
				result.IsError = true;
				result.Message = "Can't load standard loan type";
				return result;
			} // if

			result.LoanTypeId = sr["Id"];

			// Choose scenario
			if (amount <= CurrentValues.Instance.SmallLoanScenarioLimit)
				result.ScenarioName = "Small Loan";
			else if (!hasLoans)
				result.ScenarioName = "Basic New";
			else
				result.ScenarioName = "Basic Repeating";

			var getPricingModelModelInstance = new GetPricingModelModel(customerId, result.ScenarioName);
			getPricingModelModelInstance.Execute();

			PricingModelModel templateModel = getPricingModelModelInstance.Model;
			templateModel.LoanAmount = amount;
			templateModel.LoanTerm = result.Period;
			templateModel.TenureMonths = result.Period * templateModel.TenurePercents;

			CalculateInterestRateAndSetupFee(customerId, amount, medalClassification, result, templateModel);

			return result;
		} // CalculateOffer

		private void CalculateInterestRateAndSetupFee(
			int customerId,
			int amount,
			Medal medalClassification,
			OfferResult result,
			PricingModelModel templateModel
		) {
			bool aspireToMinSetupFee = CurrentValues.Instance.AspireToMinSetupFee;

			SafeReader sr = db.GetFirst(
				"LoadOfferRanges",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Amount", amount),
				new QueryParameter("@MedalClassification", medalClassification.ToString())
			);

			if (sr.IsEmpty) {
				string errorMessage = string.Format(
					"Can't load ranges for amount:{0} and medal classification:{1}",
					amount,
					medalClassification
				);

				log.Alert(errorMessage);
				result.IsError = true;
				result.Message = errorMessage;

				return;
			} // if

			decimal minSetupFee = sr["MinSetupFee"];
			decimal maxSetupFee = sr["MaxSetupFee"];
			decimal minInterestRate = sr["MinInterestRate"];
			decimal maxInterestRate = sr["MaxInterestRate"];

			bool checkBoundaries = CheckBounderies(
				customerId,
				amount,
				result,
				templateModel,
				minSetupFee,
				maxSetupFee,
				maxInterestRate,
				minInterestRate
			);

			if (checkBoundaries) {
				if (aspireToMinSetupFee) {
					CalculateLowestSetupFee(
						customerId,
						amount,
						result,
						templateModel,
						minSetupFee,
						maxSetupFee,
						maxInterestRate,
						minInterestRate
					);
				} else {
					CalculateHighestSetupFee(
						customerId,
						amount,
						result,
						templateModel,
						minSetupFee,
						maxSetupFee,
						maxInterestRate,
						minInterestRate
					);
				} // if
			} // if

			if (!result.HasDecision) {
				if (aspireToMinSetupFee)
					SetRounded(result, maxInterestRate / 100.0m, minSetupFee / 100.0m);
				else
					SetRounded(result, minInterestRate / 100.0m, maxSetupFee / 100.0m);
			} // if
		} // CalculateInterestRateAndSetupFee

		private bool CheckBounderies(
			int customerId,
			int amount,
			OfferResult result,
			PricingModelModel templateModel,
			decimal minSetupFee,
			decimal maxSetupFee,
			decimal maxInterestRate,
			decimal minInterestRate
		) {
			decimal lowerBoundary = minSetupFee;
			decimal upperBoundary = maxSetupFee;

			PricingModelModel lowerBoundaryModel = templateModel.Clone();

			lowerBoundaryModel.SetupFeePercents = lowerBoundary / 100;
			lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents * amount;

			var lowerBoundaryPricingModelCalculator = new PricingModelCalculator(customerId, lowerBoundaryModel);

			if (!lowerBoundaryPricingModelCalculator.CalculateInterestRate()) {
				result.IsError = true;
				result.Message = lowerBoundaryPricingModelCalculator.Error;
				return false;
			} // if

			lowerBoundaryModel = lowerBoundaryPricingModelCalculator.Model;

			PricingModelModel upperBoundaryModel = templateModel.Clone();

			upperBoundaryModel.SetupFeePercents = upperBoundary / 100;
			upperBoundaryModel.SetupFeePounds = upperBoundaryModel.SetupFeePercents * amount;

			var upperBoundaryPricingModelCalculator = new PricingModelCalculator(customerId, upperBoundaryModel);

			if (!upperBoundaryPricingModelCalculator.CalculateInterestRate()) {
				result.IsError = true;
				result.Message = lowerBoundaryPricingModelCalculator.Error;
				return false;
			} // if

			upperBoundaryModel = upperBoundaryPricingModelCalculator.Model;

			bool isOutOfRange = (
				(upperBoundaryModel.MonthlyInterestRate * 100 < minInterestRate) ||
				(upperBoundaryModel.MonthlyInterestRate * 100 > maxInterestRate)
			) && (
				(lowerBoundaryModel.MonthlyInterestRate * 100 < minInterestRate) ||
				(lowerBoundaryModel.MonthlyInterestRate * 100 > maxInterestRate)
			);

			if (isOutOfRange) {
				result.Message = string.Format(
					"No interest rate found in range (min max interest rate) " +
					"[{0:N2} - {1:N2}] and [{2:N2} - {3:N2}] (config)",
					upperBoundaryModel.MonthlyInterestRate * 100,
					lowerBoundaryModel.MonthlyInterestRate * 100,
					minInterestRate, maxInterestRate
				);

				return false;
			} // if

			result.HasDecision = true;
			return true;
		} // CheckBounderies

		private void CalculateLowestSetupFee(
			int customerId,
			int amount,
			OfferResult result,
			PricingModelModel templateModel,
			decimal minSetupFee,
			decimal maxSetupFee,
			decimal maxInterestRate,
			decimal minInterestRate
		) {
			PricingModelModel lowerBoundaryModel = templateModel.Clone();

			lowerBoundaryModel.SetupFeePercents = minSetupFee / 100;
			lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents * amount;

			do {
				var lowerBoundaryPricingModelCalculator = new PricingModelCalculator(customerId, lowerBoundaryModel);

				if (!lowerBoundaryPricingModelCalculator.CalculateInterestRate()) {
					result.IsError = true;
					result.Message = lowerBoundaryPricingModelCalculator.Error;
					return;
				} // if

				lowerBoundaryModel = lowerBoundaryPricingModelCalculator.Model;

				if (lowerBoundaryModel.SetupFeePercents * 100 > maxSetupFee) {
					result.IsError = true;
					result.Message = "No interest rate found";
					return;
				} // if

				if (lowerBoundaryModel.MonthlyInterestRate * 100 <= maxInterestRate &&
					lowerBoundaryModel.MonthlyInterestRate * 100 >= minInterestRate) {
					break;
				} // if

				lowerBoundaryModel.SetupFeePercents += SetupFeeStep;
				lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents * amount;
			} while ((lowerBoundaryModel.MonthlyInterestRate * 100 > maxInterestRate));

			RoundSetupFeeAndRecalculateInterestRate(lowerBoundaryModel.SetupFeePercents, result, templateModel);
		} // CalculateLowestSetupFee

		private void CalculateHighestSetupFee(
			int customerId,
			int amount,
			OfferResult result,
			PricingModelModel templateModel,
			decimal minSetupFee,
			decimal maxSetupFee,
			decimal maxInterestRate,
			decimal minInterestRate
		) {
			PricingModelModel upperBoundaryModel = templateModel.Clone();
			upperBoundaryModel.SetupFeePercents = maxSetupFee / 100;
			upperBoundaryModel.SetupFeePounds = upperBoundaryModel.SetupFeePercents * amount;

			do {
				var upperBoundaryPricingModelCalculator = new PricingModelCalculator(customerId, upperBoundaryModel);
				if (!upperBoundaryPricingModelCalculator.CalculateInterestRate()) {
					result.IsError = true;
					result.Message = upperBoundaryPricingModelCalculator.Error;
					return;
				}
				upperBoundaryModel = upperBoundaryPricingModelCalculator.Model;

				if (upperBoundaryModel.SetupFeePercents * 100 < minSetupFee) {
					result.IsError = true;
					result.Message = "No interest rate found";
					return;
				}

				if (upperBoundaryModel.MonthlyInterestRate * 100 <= maxInterestRate &&
					upperBoundaryModel.MonthlyInterestRate * 100 >= minInterestRate) {
					break;
				}

				upperBoundaryModel.SetupFeePercents -= SetupFeeStep;
				upperBoundaryModel.SetupFeePounds = upperBoundaryModel.SetupFeePercents * amount;
			} while (upperBoundaryModel.MonthlyInterestRate * 100 < minInterestRate);

			RoundSetupFeeAndRecalculateInterestRate(upperBoundaryModel.SetupFeePercents, result, templateModel);
		} // CalculateHighestSetupFee

		private void RoundSetupFeeAndRecalculateInterestRate(
			decimal setupFee,
			OfferResult result,
			PricingModelModel templateModel
		) {
			PricingModelModel model = templateModel.Clone();
			model.SetupFeePercents = setupFee;
			model.SetupFeePounds = setupFee * model.LoanAmount;

			var pricingModelCalculator = new PricingModelCalculator(result.CustomerId, model);
			pricingModelCalculator.Calculate();

			SetRounded(result, pricingModelCalculator.Model.MonthlyInterestRate, setupFee);
		} // RoundSetupFeeAndRecalculateInterestRate

		private void SetRounded(OfferResult result, decimal interestRate, decimal setupFee) {
			result.InterestRate = Math.Ceiling(interestRate * 2000) / 20;
			result.SetupFee = Math.Ceiling(setupFee * 200) / 2;
		} // SetRounded

		private readonly ASafeLog log;
		private readonly AConnection db;

		private const decimal SetupFeeStep = 0.0005M;
	} // class OfferCalculator1
} // namespace
