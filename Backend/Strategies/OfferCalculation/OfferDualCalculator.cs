namespace EzBob.Backend.Strategies.OfferCalculation
{
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using MedalCalculations;
	using PricingModel;

	public class OfferDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;

		public OfferResult Results { get; set; }

		public OfferDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
		}

		public OfferResult CalculateOffer(int customerId, DateTime calculationTime, int amount, MedalClassification medalClassification)
		{
			try
			{
				OfferResult result1 = null, result2 = null;

				result1 = CalculateOffer1(customerId, calculationTime, amount, medalClassification);
				// TODO: Calculate result2 here

				// TODO: remove these 2 lines
				//result1.SaveToDb(db);
				return result1;

				// TODO: uncomment this code
				//if (result1 != null && result2 != null && result1.IsIdentical(result2))
				//{
				//	result1.SaveToDb(db);
				//	return result1;
				//}

				//log.Error("Mismatch found in the 2 offer calculations of customer: {0}", customerId);
				//return null;
			}
			catch (Exception e)
			{
				log.Warn("Offer calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}

		private OfferResult CalculateOffer1(int customerId, DateTime calculationTime, int amount, MedalClassification medalClassification)
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

			// TODO: vitas to define rules for scenario usage
			result.ScenarioName = "Basic";

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

			CalculateInterestRateAndSetupFee(customerId, amount, result.Period, result.ScenarioName, ranges, result);
			
			return result;
		}

		private void CalculateInterestRateAndSetupFee(int customerId, int amount, int loanTerm, string scenarioName, OfferRanges ranges, OfferResult result)
		{
			bool aspireToMinSetupFee = CurrentValues.Instance.AspireToMinSetupFee;

			if (aspireToMinSetupFee)
			{
				decimal lowerBoundary = ranges.MinSetupFee;
				decimal upperBoundary = ranges.MaxSetupFee;

				var lowerBoundaryModelInstance = new GetPricingModelModel(customerId, scenarioName, db, log);
				lowerBoundaryModelInstance.Execute();

				PricingModelModel lowerBoundaryModel = lowerBoundaryModelInstance.Model;
				lowerBoundaryModel.LoanTerm = loanTerm;
				lowerBoundaryModel.LoanAmount = amount;
				lowerBoundaryModel.SetupFeePercents = lowerBoundary / 100;
				lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents * amount;

				var lowerBoundaryCalculateInstance = new PricingModelCalculate(customerId, lowerBoundaryModel, db, log);
				lowerBoundaryCalculateInstance.Execute();
				PricingModelModel lowerBoundaryResultModel = lowerBoundaryCalculateInstance.Model;


				var upperBoundaryModelInstance = new GetPricingModelModel(customerId, scenarioName, db, log);
				upperBoundaryModelInstance.Execute();

				PricingModelModel upperBoundaryModel = upperBoundaryModelInstance.Model;
				upperBoundaryModel.LoanTerm = loanTerm;
				upperBoundaryModel.LoanAmount = amount;
				upperBoundaryModel.SetupFeePercents = upperBoundary / 100;
				upperBoundaryModel.SetupFeePounds = upperBoundaryModel.SetupFeePercents * amount;

				var upperBoundaryCalculateInstance = new PricingModelCalculate(customerId, upperBoundaryModel, db, log);
				upperBoundaryCalculateInstance.Execute();
				PricingModelModel upperBoundaryResultModel = upperBoundaryCalculateInstance.Model;

				// if both is out of range (same direction)
				if ((lowerBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MaxInterestRate &&
					 upperBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MaxInterestRate) ||
					(lowerBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate &&
					 upperBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate))
				{
					result.Error = "Can't calculate interest rate that is in range";
					return;
				}

				// if both is within range
				if (lowerBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MinInterestRate &&
					lowerBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MinInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate)
				{
					result.SetupFee = upperBoundaryResultModel.SetupFeePercents;
					result.InterestRate = upperBoundaryResultModel.MonthlyInterestRate * 100;
					return;
				}

				// if lower is below range and upper is in range
				if (lowerBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MinInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 > ranges.MinInterestRate &&
					upperBoundaryResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate)
				{
					result.SetupFee = upperBoundaryResultModel.SetupFeePercents;
					result.InterestRate = upperBoundaryResultModel.MonthlyInterestRate * 100;
					return;
				}

				// binary
				// if in range move lower
				// if out of range move upper
				decimal epsilon = 0.01m;
				while (ranges.MaxInterestRate - lowerBoundaryResultModel.MonthlyInterestRate * 100 > epsilon)
				{
					decimal midPoint = (lowerBoundary + upperBoundary)/2;
					var midPointModelInstance = new GetPricingModelModel(customerId, scenarioName, db, log);
					midPointModelInstance.Execute();

					PricingModelModel midPointModel = midPointModelInstance.Model;
					midPointModel.LoanTerm = loanTerm;
					midPointModel.LoanAmount = amount;
					midPointModel.SetupFeePercents = midPoint / 100;
					midPointModel.SetupFeePounds = midPointModel.SetupFeePercents * amount;

					var midPointCalculateInstance = new PricingModelCalculate(customerId, midPointModel, db, log);
					midPointCalculateInstance.Execute();
					PricingModelModel midPointResultModel = lowerBoundaryCalculateInstance.Model;

					if (midPointResultModel.MonthlyInterestRate * 100 < ranges.MaxInterestRate)
					{
						lowerBoundary = midPoint;
					}
					else if (midPointResultModel.MonthlyInterestRate * 100 > ranges.MaxInterestRate)
					{
						upperBoundary = midPoint;
					}
				}

				lowerBoundaryModel = lowerBoundaryModelInstance.Model;
				lowerBoundaryModel.SetupFeePercents = lowerBoundary / 100;
				lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents * amount;

				lowerBoundaryCalculateInstance = new PricingModelCalculate(customerId, lowerBoundaryModel, db, log);
				lowerBoundaryCalculateInstance.Execute();
				lowerBoundaryResultModel = lowerBoundaryCalculateInstance.Model;
				result.SetupFee = lowerBoundaryResultModel.SetupFeePercents * 100;
				result.InterestRate = lowerBoundaryResultModel.MonthlyInterestRate * 100;
			}
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
