namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LimitedMedalCalculator1 : MedalCalculatorBase
	{
		public LimitedMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		public override ScoreResult CreateResultWithInitialWeights(int customerId, DateTime calculationTime)
		{
			Results = new ScoreResult
				{
					CustomerId = customerId,
					CalculationTime = calculationTime,
					MedalType = "Limited",
					BusinessScoreWeight = 30,
					FreeCashFlowWeight = 19,
					AnnualTurnoverWeight = 10,
					TangibleEquityWeight = 8,
					BusinessSeniorityWeight = 8,
					ConsumerScoreWeight = 10,
					NetWorthWeight = 10,
					MaritalStatusWeight = 5,
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
			return 13.75m;
		}

		protected override decimal GetCompanyScoreWeightForLowScore()
		{
			return 41.25m;
		}

		protected override void RedistributeFreeCashFlowWeight()
		{
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 7;
			Results.BusinessScoreWeight += 5;
			Results.ConsumerScoreWeight += 3;
			Results.BusinessSeniorityWeight += 4;
		}

		protected override void RedistributeWeightsForPayingCustomer()
		{
			Results.BusinessScoreWeight -= 6.25m;
			Results.BusinessSeniorityWeight -= 1.67m;
			Results.ConsumerScoreWeight -= 2.08m;
		}
	}
}
