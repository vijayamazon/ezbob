namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SoleTraderMedalCalculator1 : MedalCalculatorBase
	{
		public SoleTraderMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		public override ScoreResult CreateResultWithInitialWeights(int customerId, DateTime calculationTime)
		{
			Results = new ScoreResult
				{
					CustomerId = customerId,
					CalculationTime = calculationTime,
					MedalType = "SoleTrader",
					BusinessScoreWeight = 0,
					FreeCashFlowWeight = 25,
					AnnualTurnoverWeight = 16,
					TangibleEquityWeight = 0,
					BusinessSeniorityWeight = 3,
					ConsumerScoreWeight = 40,
					NetWorthWeight = 10,
					MaritalStatusWeight = 6,
					NumberOfStoresWeight = 0,
					PositiveFeedbacksWeight = 0,
					EzbobSeniorityWeight = 0,
					NumOfLoansWeight = 0,
					NumOfLateRepaymentsWeight = 0,
					NumOfEarlyRepaymentsWeight = 0
				};

			return Results;
		}

		protected override void AdditionalLegalInputValidations()
		{
			if (!earliestHmrcLastUpdateDate.HasValue && !earliestYodleeLastUpdateDate.HasValue)
			{
				throw new Exception("Medal is meant only for customers with HMRC or bank");
			}
		}

		protected override decimal GetConsumerScoreWeightForLowScore()
		{
			return 55;
		}

		protected override decimal GetCompanyScoreWeightForLowScore()
		{
			return 0;
		}

		protected override void RedistributeFreeCashFlowWeight()
		{
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 10;
			Results.ConsumerScoreWeight += 13;
			Results.BusinessSeniorityWeight += 2;
		}

		protected override void RedistributeWeightsForPayingCustomer()
		{
			Results.BusinessSeniorityWeight -= 1;
			Results.ConsumerScoreWeight -= 9;
		}
	}
}
