namespace EzBob.Backend.Strategies.MedalCalculations
{
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LimitedMedalCalculator1 : MedalCalculatorBase
	{
		public LimitedMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		public override void SetTypeAndInitialWeights()
		{
			Results.MedalType = "Limited";
			Results.BusinessScoreWeight = 30;
			Results.FreeCashFlowWeight = 19;
			Results.AnnualTurnoverWeight = 10;
			Results.TangibleEquityWeight = 8;
			Results.BusinessSeniorityWeight = 8;
			Results.ConsumerScoreWeight = 10;
			Results.NetWorthWeight = 10;
			Results.MaritalStatusWeight = 5;
			Results.NumberOfStoresWeight = 0;
			Results.PositiveFeedbacksWeight = 0;
			Results.EzbobSeniorityWeight = 0;
			Results.NumOfLoansWeight = 0;
			Results.NumOfLateRepaymentsWeight = 0;
			Results.NumOfEarlyRepaymentsWeight = 0;
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
