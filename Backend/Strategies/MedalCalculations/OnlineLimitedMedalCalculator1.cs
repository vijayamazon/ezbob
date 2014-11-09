namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class OnlineLimitedMedalCalculator1 : MedalCalculatorBase
	{
		public OnlineLimitedMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		public override ScoreResult CreateResultWithInitialWeights(int customerId, DateTime calculationTime)
		{
			Results = new ScoreResult
				{
					CustomerId = customerId,
					CalculationTime = calculationTime,
					MedalType = "OnlineLimited",
					BusinessScoreWeight = 20,
					FreeCashFlowWeight = 13,
					AnnualTurnoverWeight = 10,
					TangibleEquityWeight = 8,
					BusinessSeniorityWeight = 7,
					ConsumerScoreWeight = 20,
					NetWorthWeight = 10,
					MaritalStatusWeight = 5,
					NumberOfStoresWeight = 2,
					PositiveFeedbacksWeight = 5,
					EzbobSeniorityWeight = 0,
					NumOfLoansWeight = 0,
					NumOfLateRepaymentsWeight = 0,
					NumOfEarlyRepaymentsWeight = 0
				};

			return Results;
		}

		protected override void DetermineFlow(decimal hmrcFreeCashFlow, decimal hmrcValueAdded)
		{
			decimal onlineMedalTurnoverCutoff = CurrentValues.Instance.OnlineMedalTurnoverCutoff;
			if (Results.HmrcAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover)
			{
				Results.InnerFlowName = "HMRC";
				Results.AnnualTurnover = Results.HmrcAnnualTurnover;
				Results.FreeCashFlowValue = hmrcFreeCashFlow;
				Results.ValueAdded = hmrcValueAdded;
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
