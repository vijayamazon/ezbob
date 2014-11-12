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

			var getPricingModelModelInstance = new GetPricingModelModel(customerId, result.ScenarioName, db, log);
			getPricingModelModelInstance.Execute();
			PricingModelModel templateModel = getPricingModelModelInstance.Model;

			CalculateInterestRateAndSetupFee(customerId, amount, medalClassification, result, templateModel);
			
			return result;
		}

		private void CalculateInterestRateAndSetupFee(int customerId, int amount, MedalClassification medalClassification, OfferResult result, PricingModelModel templateModel)
		{
			bool aspireToMinSetupFee = CurrentValues.Instance.AspireToMinSetupFee;
			SafeReader sr = db.GetFirst(
				"LoadOfferRanges",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Amount", amount),
				new QueryParameter("@MedalClassification", medalClassification.ToString())
			);

			if (sr.IsEmpty)
			{
				string errorMessage = string.Format("Can't load ranges for amount:{0} and medal classification:{1}", amount, medalClassification);
				log.Alert(errorMessage);
				result.Error = errorMessage;
				return;
			}
			
			decimal minSetupFee = sr["MinSetupFee"];
			decimal maxSetupFee = sr["MaxSetupFee"];
			decimal minInterestRate = sr["MinInterestRate"];
			decimal maxInterestRate = sr["MaxInterestRate"];

			if (aspireToMinSetupFee)
			{
				CalculateHighestSetupFee(customerId, amount, result, templateModel, minSetupFee, maxSetupFee, maxInterestRate, minInterestRate);
			}
			else
			{
				CalculateLowestSetupFee(customerId, amount, result, templateModel, minSetupFee, maxSetupFee, maxInterestRate, minInterestRate);
			}
		}

		private void CalculateLowestSetupFee(int customerId, int amount, OfferResult result, PricingModelModel templateModel,
		                                      decimal minSetupFee, decimal maxSetupFee, decimal maxInterestRate,
		                                      decimal minInterestRate)
		{
			decimal lowerBoundary = minSetupFee;
			decimal upperBoundary = maxSetupFee;

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
			if ((lowerBoundaryResultModel.MonthlyInterestRate * 100 > maxInterestRate &&
				 upperBoundaryResultModel.MonthlyInterestRate * 100 > maxInterestRate) ||
				(lowerBoundaryResultModel.MonthlyInterestRate * 100 < minInterestRate &&
				 upperBoundaryResultModel.MonthlyInterestRate * 100 < minInterestRate))
			{
				result.Error = "Can't calculate interest rate that is in range";
				return;
			}

			// If lower is within range
			if (lowerBoundaryResultModel.MonthlyInterestRate * 100 > minInterestRate &&
				lowerBoundaryResultModel.MonthlyInterestRate * 100 < maxInterestRate)
			{
				RoundSetupFeeAndRecalculateInterestRate(lowerBoundaryResultModel.SetupFeePercents, result, templateModel);
				return;
			}

			// 'Close in' to find best possible value
			decimal epsilon = 0.0001m; // We want to be accurate
			while (maxInterestRate - upperBoundaryResultModel.MonthlyInterestRate * 100 > epsilon)
			{
				decimal midPoint = (lowerBoundary + upperBoundary) / 2;

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

				if (midPointResultModel.MonthlyInterestRate * 100 > maxInterestRate)
				{
					lowerBoundary = midPoint;
				}
				else if (midPointResultModel.MonthlyInterestRate * 100 == maxInterestRate)
				{
					upperBoundaryResultModel = midPointResultModel;
					break;
				}
				else
				{
					upperBoundary = midPoint;
					upperBoundaryResultModel = midPointResultModel;
				}
			}

			RoundSetupFeeAndRecalculateInterestRate(upperBoundaryResultModel.SetupFeePercents, result, templateModel);
		}

		private void CalculateHighestSetupFee(int customerId, int amount, OfferResult result, PricingModelModel templateModel,
		                        decimal minSetupFee, decimal maxSetupFee, decimal maxInterestRate, decimal minInterestRate)
		{
			decimal lowerBoundary = minSetupFee;
			decimal upperBoundary = maxSetupFee;

			PricingModelModel lowerBoundaryModel = templateModel.Clone();
			lowerBoundaryModel.LoanTerm = result.Period;
			lowerBoundaryModel.LoanAmount = amount;
			lowerBoundaryModel.SetupFeePercents = lowerBoundary/100;
			lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents*amount;
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
			upperBoundaryModel.SetupFeePercents = upperBoundary/100;
			upperBoundaryModel.SetupFeePounds = upperBoundaryModel.SetupFeePercents*amount;
			var upperBoundaryPricingModelCalculator = new PricingModelCalculator(customerId, upperBoundaryModel, db, log);
			if (!upperBoundaryPricingModelCalculator.CalculateInterestRate())
			{
				result.Error = upperBoundaryPricingModelCalculator.Error;
				return;
			}
			PricingModelModel upperBoundaryResultModel = upperBoundaryPricingModelCalculator.Model;

			// If both is out of range (same direction)
			if ((lowerBoundaryResultModel.MonthlyInterestRate*100 > maxInterestRate &&
			     upperBoundaryResultModel.MonthlyInterestRate*100 > maxInterestRate) ||
			    (lowerBoundaryResultModel.MonthlyInterestRate*100 < minInterestRate &&
			     upperBoundaryResultModel.MonthlyInterestRate*100 < minInterestRate))
			{
				result.Error = "Can't calculate interest rate that is in range";
				return;
			}

			// If upper is within range
			if (upperBoundaryResultModel.MonthlyInterestRate*100 > minInterestRate &&
			    upperBoundaryResultModel.MonthlyInterestRate*100 < maxInterestRate)
			{
				RoundSetupFeeAndRecalculateInterestRate(upperBoundaryResultModel.SetupFeePercents, result, templateModel);
				return;
			}

			// 'Close in' to find best possible value
			decimal epsilon = 0.0001m; // We want to be accurate
			while (lowerBoundaryResultModel.MonthlyInterestRate*100 - minInterestRate > epsilon)
			{
				decimal midPoint = (lowerBoundary + upperBoundary)/2;

				PricingModelModel midPointModel = templateModel.Clone();
				midPointModel.LoanTerm = result.Period;
				midPointModel.LoanAmount = amount;
				midPointModel.SetupFeePercents = midPoint/100;
				midPointModel.SetupFeePounds = midPointModel.SetupFeePercents*amount;

				var midPointPricingModelCalculator = new PricingModelCalculator(customerId, midPointModel, db, log);
				if (!midPointPricingModelCalculator.CalculateInterestRate())
				{
					result.Error = midPointPricingModelCalculator.Error;
					return;
				}
				PricingModelModel midPointResultModel = midPointPricingModelCalculator.Model;

				if (midPointResultModel.MonthlyInterestRate*100 < minInterestRate)
				{
					upperBoundary = midPoint;
				}
				else if (midPointResultModel.MonthlyInterestRate*100 == minInterestRate)
				{
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

		private void RoundSetupFeeAndRecalculateInterestRate(decimal setupFee, OfferResult result, PricingModelModel templateModel)
		{
			setupFee *= 100;
			decimal roundedSetupFee = Math.Round(setupFee * 2, 0, MidpointRounding.AwayFromZero) /2;
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
	}
}
