namespace EzBob.Backend.Strategies.MedalCalculations
{
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class OnlineNonLimitedWithBusinessScoreMedalCalculator1 : MedalCalculatorBase
	{
		public OnlineNonLimitedWithBusinessScoreMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		protected override void SetMedalType()
		{
			Results.MedalType = "OnlineNonLimitedWithBusinessScore";
		}

		public override void SetInitialWeights()
		{
			Results.BusinessScoreWeight = 25;
			Results.FreeCashFlowWeight = 0;
			Results.AnnualTurnoverWeight = 13;
			Results.TangibleEquityWeight = 0;
			Results.BusinessSeniorityWeight = 5;
			Results.ConsumerScoreWeight = 35;
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
			return 47;
		}

		protected override decimal GetCompanyScoreWeightForLowScore()
		{
			return 33;
		}

		protected override void RedistributeFreeCashFlowWeight()
		{
		}

		protected override void RedistributeWeightsForPayingCustomer()
		{
			Results.BusinessScoreWeight -= 4;
			Results.BusinessSeniorityWeight -= 2;
			Results.ConsumerScoreWeight -= 4;
		}
	}
}
