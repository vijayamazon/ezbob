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

			CalculateInterestRateAndSetupFee(customerId, amount, ranges, result);
			
			return result;
		}

		private void CalculateInterestRateAndSetupFee(int customerId, int amount, OfferRanges ranges, OfferResult result)
		{
			bool aspireToMinSetupFee = CurrentValues.Instance.AspireToMinSetupFee;

			if (aspireToMinSetupFee)
			{
				decimal lowerBoundary = ranges.MinSetupFee;
				decimal upperBoundary = ranges.MaxSetupFee;

				var lowerBoundaryModelInstance = new GetPricingModelModel(customerId, result.ScenarioName, db, log);
				lowerBoundaryModelInstance.Execute();

				PricingModelModel lowerBoundaryModel = lowerBoundaryModelInstance.Model;
				lowerBoundaryModel.LoanTerm = result.Period;
				lowerBoundaryModel.LoanAmount = amount;
				lowerBoundaryModel.SetupFeePercents = lowerBoundary / 100;
				lowerBoundaryModel.SetupFeePounds = lowerBoundaryModel.SetupFeePercents * amount;

				var pricingModelCalculator = new PricingModelCalculator(customerId, lowerBoundaryModel, db, log);
				if (!pricingModelCalculator.CalculateInterestRate())
				{
					result.Error = pricingModelCalculator.Error;
					return;
				}

				var lowerBoundaryCalculateInstance = new PricingModelCalculate(customerId, lowerBoundaryModel, db, log);
				lowerBoundaryCalculateInstance.Execute();
				PricingModelModel lowerBoundaryResultModel = lowerBoundaryCalculateInstance.Model;
				
				var upperBoundaryModelInstance = new GetPricingModelModel(customerId, result.ScenarioName, db, log);
				upperBoundaryModelInstance.Execute();

				PricingModelModel upperBoundaryModel = upperBoundaryModelInstance.Model;
				upperBoundaryModel.LoanTerm = result.Period;
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

				// 'Close in' to find best possible value
				decimal epsilon = 0.01m; // TODO: define config with vitas
				while (ranges.MaxInterestRate - lowerBoundaryResultModel.MonthlyInterestRate * 100 > epsilon)
				{
					decimal midPoint = (lowerBoundary + upperBoundary)/2;
					var midPointModelInstance = new GetPricingModelModel(customerId, result.ScenarioName, db, log);
					midPointModelInstance.Execute();

					PricingModelModel midPointModel = midPointModelInstance.Model;
					midPointModel.LoanTerm = result.Period;
					midPointModel.LoanAmount = amount;
					midPointModel.SetupFeePercents = midPoint / 100;
					midPointModel.SetupFeePounds = midPointModel.SetupFeePercents * amount;

					var midPointCalculateInstance = new PricingModelCalculate(customerId, midPointModel, db, log);
					midPointCalculateInstance.Execute();
					PricingModelModel midPointResultModel = lowerBoundaryCalculateInstance.Model;

					if (midPointResultModel.MonthlyInterestRate * 100 <= ranges.MaxInterestRate)
					{
						lowerBoundary = midPoint;
						lowerBoundaryResultModel = midPointResultModel;
					}
					else
					{
						upperBoundary = midPoint;
					}
				}

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
