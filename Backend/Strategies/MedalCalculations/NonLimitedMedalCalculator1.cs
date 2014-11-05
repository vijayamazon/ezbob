namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class NonLimitedMedalCalculator1 : MedalCalculatorBase
	{
		public NonLimitedMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		protected override ScoreResult CreateResultWithInitialWeights(int customerId, DateTime calculationTime)
		{
			Results = new ScoreResult
				{
					CustomerId = customerId,
					CalculationTime = calculationTime,
					MedalType = "NonLimited",
					BusinessScoreWeight = 21,
					FreeCashFlowWeight = 15,
					AnnualTurnoverWeight = 10,
					TangibleEquityWeight = 0,
					BusinessSeniorityWeight = 8,
					ConsumerScoreWeight = 30,
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

		protected override decimal GetConsumerScoreWeightForLowScore()
		{
			return 41.25m;
		}

		protected override decimal GetCompanyScoreWeightForLowScore()
		{
			return 28.875m;
		}

		protected override void RedistributeFreeCashFlowWeight()
		{
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 5;
			Results.BusinessScoreWeight += 4;
			Results.ConsumerScoreWeight += 4;
			Results.BusinessSeniorityWeight += 2;
		}

		protected override void RedistributeWeightsForPayingCustomer()
		{
			Results.BusinessScoreWeight -= 4;
			Results.BusinessSeniorityWeight -= 2;
			Results.ConsumerScoreWeight -= 4;
		}
	}
}
