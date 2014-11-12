namespace EzBob.Backend.Strategies.OfferCalculation
{
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using MedalCalculations;
	using PricingModel;

	public class OfferCalculator1
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		
		public OfferCalculator1(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
		}

		public OfferResult CalculateOffer(int customerId, DateTime calculationTime, int amount, bool hasLoans, MedalClassification medalClassification)
		{
			var result = new OfferResult
				{
					CustomerId = customerId,
					CalculationTime = calculationTime,
					Amount = amount,
					MedalClassification = medalClassification
				};

			// Period is always 12
			result.Period = 12;

			// We always use standard loan type
			result.IsEu = false;

			// Choose scenario
			if (amount <= CurrentValues.Instance.SmallLoanScenarioLimit)
			{
				result.ScenarioName = "Small Loan";
			}
			else if (hasLoans)
			{
				result.ScenarioName = "Basic New";
			}
			else
			{
				result.ScenarioName = "Basic Repeating";
			}

			OfferRanges ranges = db.FillFirst<OfferRanges>(
				"LoadOfferRanges",
				new QueryParameter("@Amount", amount),
				new QueryParameter("@MedalClassification", medalClassification.ToString())
			);

			if (ranges == null)
			{
				string errorMessage = string.Format("Can't load ranges for amount:{0} and medal classification:{1}", amount, medalClassification);
				log.Alert(errorMessage);
				result.Error = errorMessage;
				return result;
			}

			var getPricingModelModelInstance = new GetPricingModelModel(customerId, result.ScenarioName, db, log);
			getPricingModelModelInstance.Execute();
			PricingModelModel templateModel = getPricingModelModelInstance.Model;

			CalculateInterestRateAndSetupFee(customerId, amount, ranges, result, templateModel);
			
			return result;
		}

		private void CalculateInterestRateAndSetupFee(int customerId, int amount, OfferRanges ranges, OfferResult result, PricingModelModel templateModel)
		{
			bool aspireToMinSetupFee = CurrentValues.Instance.AspireToMinSetupFee;

			if (aspireToMinSetupFee)
			{
				decimal lowerBoundary = ranges.MinSetupFee;
				decimal upperBoundary = ranges.MaxSetupFee;

				PricingModelModel lowerBoundaryModel = templateModel.Clone();
				lowerBoundaryModel.LoanTerm = result.Period;
				lowerBoundaryModel.LoanAmount = amount;
				lowerBoundaryModel.SetupFeePercents = lowerBoundary / 100;
				lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents * amount;
				var lowerBoundaryPricingModelCalculator = new PricingModelCalculator(customerId, lowerBoundaryModel, db, log);
				if (!lowerBoundaryPricingModelCalculator.CalculateInterestRate())
				{
					result.Error = lowerBoundaryPricingModelCalculator.Error;
					return;
				}
				PricingModelModel lowerBoundaryResultModel = lowerBoundaryPricingModelCalculator.Model;
				
				PricingModelModel upperBoundaryModel = templateModel.Clone();
				upperBoundaryModel.LoanTerm = result.Period;
				upperBoundaryModel.LoanAmount = amount;
				upperBoundaryModel.SetupFeePercents = upperBoundary / 100;
				upperBoundaryModel.SetupFeePounds = upperBoundaryModel.SetupFeePercents * amount;
				var upperBoundaryPricingModelCalculator = new PricingModelCalculator(customerId, upperBoundaryModel, db, log);
				if (!upperBoundaryPricingModelCalculator.CalculateInterestRate())
				{
					result.Error = upperBoundaryPricingModelCalculator.Error;
					return;
				}
				PricingModelModel upperBoundaryResultModel = upperBoundaryPricingModelCalculator.Model;
				
				// If both is out of range (same direction)
				if ((lowerBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MaxInterestRate &&
					 upperBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MaxInterestRate) ||
					(lowerBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MinInterestRate &&
					 upperBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MinInterestRate))
				{
					result.Error = "Can't calculate interest rate that is in range";
					return;
				}

				// If both is within range
				if (lowerBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MinInterestRate &&
					lowerBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MinInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate)
				{
					RoundSetupFeeAndRecalculateInterestRate(upperBoundaryResultModel.SetupFeePercents, result, templateModel);
					return;
				}

				// If lower is below range and upper is in range
				if (lowerBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MinInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MinInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate)
				{
					RoundSetupFeeAndRecalculateInterestRate(upperBoundaryResultModel.SetupFeePercents, result, templateModel);
					return;
				}

				// 'Close in' to find best possible value
				decimal epsilon = 0.0001m; // We want to be accurate
				while (lowerBoundaryResultModel.MonthlyInterestRate * 100 - ranges.MinInterestRate > epsilon)
				{
					decimal midPoint = (lowerBoundary + upperBoundary)/2;

					PricingModelModel midPointModel = templateModel.Clone();
					midPointModel.LoanTerm = result.Period;
					midPointModel.LoanAmount = amount;
					midPointModel.SetupFeePercents = midPoint / 100;
					midPointModel.SetupFeePounds = midPointModel.SetupFeePercents * amount;

					var midPointPricingModelCalculator = new PricingModelCalculator(customerId, midPointModel, db, log);
					if (!midPointPricingModelCalculator.CalculateInterestRate())
					{
						result.Error = midPointPricingModelCalculator.Error;
						return;
					}
					PricingModelModel midPointResultModel = midPointPricingModelCalculator.Model;

					if (midPointResultModel.MonthlyInterestRate * 100 < ranges.MinInterestRate)
					{
						upperBoundary = midPoint;
						//upperBoundaryResultModel = midPointResultModel;
					}
					else if (midPointResultModel.MonthlyInterestRate*100 == ranges.MinInterestRate)
					{
						//lowerBoundary = midPoint;
						lowerBoundaryResultModel = midPointResultModel;
						break;
					}
					else
					{
						lowerBoundary = midPoint;
						lowerBoundaryResultModel = midPointResultModel;
					}
				}

				RoundSetupFeeAndRecalculateInterestRate(lowerBoundaryResultModel.SetupFeePercents, result, templateModel);
			}
		}

		private void RoundSetupFeeAndRecalculateInterestRate(decimal setupFee, OfferResult result, PricingModelModel templateModel)
		{
			setupFee *= 100;
			decimal roundedSetupFee = Math.Round(setupFee * 2, 0, MidpointRounding.AwayFromZero) /2 ;
			result.SetupFee = roundedSetupFee;

			PricingModelModel model = templateModel.Clone();
			model.LoanTerm = result.Period;
			model.LoanAmount = result.Amount;
			model.SetupFeePercents = roundedSetupFee / 100 * model.LoanAmount;
			model.SetupFeePounds = roundedSetupFee;

			var lowerBoundaryPricingModelCalculator = new PricingModelCalculator(result.CustomerId, model, db, log);
			if (!lowerBoundaryPricingModelCalculator.CalculateInterestRate())
			{
				result.Error = lowerBoundaryPricingModelCalculator.Error;
				return;
			}

			result.InterestRate = lowerBoundaryPricingModelCalculator.Model.MonthlyInterestRate * 100;
		}

		public class OfferRanges
		{
			public decimal MinInterestRate { get; set; }
			public decimal MaxInterestRate { get; set; }
			public decimal MinSetupFee { get; set; }
			public decimal MaxSetupFee { get; set; }
		}
	}
}
