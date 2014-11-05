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

		protected override ScoreResult CreateResultWithInitialWeights(int customerId, DateTime calculationTime)
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

		protected override void DetermineFlow(decimal hmrcFreeCashFlow, decimal hmrcValueAdded)
		{
			if (freeCashFlowDataAvailable)
			{
				Results.InnerFlowName = "HMRC";
				Results.AnnualTurnover = Results.HmrcAnnualTurnover;
				Results.FreeCashFlowValue = hmrcFreeCashFlow;
				Results.ValueAdded = hmrcValueAdded;
			}
			else
			{
				Results.InnerFlowName = "Bank";
				Results.AnnualTurnover = Results.BankAnnualTurnover;
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
			if (Results.InnerFlowName != "HMRC")
			{
				Results.FreeCashFlowWeight = 0;
				Results.AnnualTurnoverWeight += 10;
				Results.ConsumerScoreWeight += 13;
				Results.BusinessSeniorityWeight += 2;
			}
		}

		protected override void RedistributeWightsForPayingCustomer()
		{
			if (firstRepaymentDatePassed)
			{
				Results.EzbobSeniorityWeight = 2;
				Results.NumOfLoansWeight = 3.33m;
				Results.NumOfLateRepaymentsWeight = 2.67m;
				Results.NumOfEarlyRepaymentsWeight = 2;

				Results.BusinessSeniorityWeight -= 1;
				Results.ConsumerScoreWeight -= 9;
			}
		}
	}
}
