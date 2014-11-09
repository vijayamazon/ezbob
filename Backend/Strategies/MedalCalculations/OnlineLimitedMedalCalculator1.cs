namespace EzBob.Backend.Strategies.MedalCalculations
{
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class OnlineLimitedMedalCalculator1 : MedalCalculatorBase
	{
		public OnlineLimitedMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		public override void SetTypeAndInitialWeights()
		{
			Results.MedalType = "OnlineLimited";
			Results.BusinessScoreWeight = 20;
			Results.FreeCashFlowWeight = 13;
			Results.AnnualTurnoverWeight = 10;
			Results.TangibleEquityWeight = 8;
			Results.BusinessSeniorityWeight = 7;
			Results.ConsumerScoreWeight = 20;
			Results.NetWorthWeight = 10;
			Results.MaritalStatusWeight = 5;
			Results.NumberOfStoresWeight = 2;
			Results.PositiveFeedbacksWeight = 5;
			Results.EzbobSeniorityWeight = 0;
			Results.NumOfLoansWeight = 0;
			Results.NumOfLateRepaymentsWeight = 0;
			Results.NumOfEarlyRepaymentsWeight = 0;
		}

		protected override void DetermineFlow()
		{
			decimal onlineMedalTurnoverCutoff = CurrentValues.Instance.OnlineMedalTurnoverCutoff;
			if (Results.HmrcAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover)
			{
				Results.InnerFlowName = "HMRC";
				Results.AnnualTurnover = Results.HmrcAnnualTurnover;
			}
			else if (Results.BankAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover)
			{
				Results.InnerFlowName = "Bank";
				Results.AnnualTurnover = Results.BankAnnualTurnover;
			}
			else
			{
				Results.InnerFlowName = "Online";
				Results.AnnualTurnover = Results.OnlineAnnualTurnover;
			}
		}

		protected override decimal GetConsumerScoreWeightForLowScore()
		{
			return 27.5m;
		}

		protected override decimal GetCompanyScoreWeightForLowScore()
		{
			return 27.5m;
		}

		protected override void RedistributeFreeCashFlowWeight()
		{
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight += 5;
			Results.BusinessScoreWeight += 3;
			Results.ConsumerScoreWeight += 3;
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
