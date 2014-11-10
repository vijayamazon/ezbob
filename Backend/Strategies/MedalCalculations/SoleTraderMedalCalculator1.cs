namespace EzBob.Backend.Strategies.MedalCalculations
{
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SoleTraderMedalCalculator1 : MedalCalculatorBase
	{
		public SoleTraderMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		protected override void SetMedalType()
		{
			Results.MedalType = "SoleTrader";
		}

		public override void SetInitialWeights()
		{
			Results.BusinessScoreWeight = 0;
			Results.FreeCashFlowWeight = 25;
			Results.AnnualTurnoverWeight = 16;
			Results.TangibleEquityWeight = 0;
			Results.BusinessSeniorityWeight = 3;
			Results.ConsumerScoreWeight = 40;
			Results.NetWorthWeight = 10;
			Results.MaritalStatusWeight = 6;
			Results.NumberOfStoresWeight = 0;
			Results.PositiveFeedbacksWeight = 0;
			Results.EzbobSeniorityWeight = 0;
			Results.NumOfLoansWeight = 0;
			Results.NumOfLateRepaymentsWeight = 0;
			Results.NumOfEarlyRepaymentsWeight = 0;
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
