namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Experian;
	using EzBob.Models.Marketplaces.Builders;
	using EzBob.Models.Marketplaces.Yodlee;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using VatReturn;

	public class LimitedMedalCalculator1 : MedalCalculatorBase
	{
		public LimitedMedalCalculator1(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

		protected override ScoreResult CreateResultWithInitialWeights(int customerId, DateTime calculationTime)
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

		protected override void GatherInputData()
		{
			SafeReader sr = db.GetFirst("GetDataForMedalCalculation1", CommandSpecies.StoredProcedure,
			                            new QueryParameter("CustomerId", Results.CustomerId),
			                            new QueryParameter("CalculationTime", Results.CalculationTime));

			if (sr.IsEmpty)
			{
				throw new Exception("Couldn't gather required data for the medal calculation");
			}

			Results.BusinessScore = sr["BusinessScore"];
			Results.TangibleEquityValue = sr["TangibleEquity"];
			Results.BusinessSeniority = sr["BusinessSeniority"];
			Results.ConsumerScore = sr["ConsumerScore"];
			string maritalStatusStr = sr["MaritalStatus"];
			Results.MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), maritalStatusStr);
			firstRepaymentDatePassed = sr["FirstRepaymentDatePassed"];
			Results.EzbobSeniority = sr["EzbobSeniority"];
			Results.NumOfLoans = sr["OnTimeLoans"];
			Results.NumOfLateRepayments = sr["NumOfLatePayments"];
			Results.NumOfEarlyRepayments = sr["NumOfEarlyPayments"];
			int hmrcId = sr["HmrcId"];
			int totalZooplaValue = sr["TotalZooplaValue"];
			int numOfHmrcMps = sr["NumOfHmrcMps"];
			DateTime? earliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"];
			DateTime? earliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"];

			if (numOfHmrcMps > 1)
			{
				throw new Exception(string.Format("Medal is meant only for customers with 1 HMRC MP at most. Num of HMRCs: {0}", numOfHmrcMps));
			}
			if (earliestHmrcLastUpdateDate.HasValue &&
					earliestHmrcLastUpdateDate.Value.AddDays(CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy) < Results.CalculationTime)
			{
				throw new Exception(string.Format("HMRC data of customer {0} is too old: {1}. Threshold is: {2} days ", Results.CustomerId, earliestHmrcLastUpdateDate.Value, CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy.Value));
			}
			if (earliestYodleeLastUpdateDate.HasValue &&
				earliestYodleeLastUpdateDate.Value.AddDays(CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy) < Results.CalculationTime)
			{
				throw new Exception(string.Format("Yodlee data of customer {0} is too old: {1}. Threshold is: {2} days ", Results.CustomerId, earliestYodleeLastUpdateDate.Value, CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy.Value));
			}

			bool wasAbleToGetSummaryData = false;
			VatReturnSummary[] summaryData = null;
			if (hmrcId != 0)
			{
				var loadVatReturnSummary = new LoadVatReturnSummary(Results.CustomerId, hmrcId, db, log);
				loadVatReturnSummary.Execute();
				summaryData = loadVatReturnSummary.Summary;

				if (summaryData != null && summaryData.Length != 0)
				{
					wasAbleToGetSummaryData = true;
				}
			}

			failedCalculatingFreeCashFlow = false;
			freeCashFlowDataAvailable = false;
			CalculateBankAnnualTurnover();

			if (wasAbleToGetSummaryData)
			{
				Results.InnerFlowName = "HMRC";
				freeCashFlowDataAvailable = true;

				foreach (VatReturnSummary singleSummary in summaryData)
				{
					Results.HmrcAnnualTurnover += singleSummary.AnnualizedTurnover.HasValue ? singleSummary.AnnualizedTurnover.Value : 0;
					Results.FreeCashFlowValue += singleSummary.AnnualizedFreeCashFlow.HasValue ? singleSummary.AnnualizedFreeCashFlow.Value : 0;
					Results.ValueAdded += singleSummary.AnnualizedValueAdded.HasValue ? singleSummary.AnnualizedValueAdded.Value : 0;
				}

				Results.AnnualTurnover = Results.HmrcAnnualTurnover;
			}
			else
			{
				Results.InnerFlowName = "Bank";
				Results.AnnualTurnover = Results.BankAnnualTurnover;
			}

			failedCalculatingTangibleEquity = false;

			if (Results.AnnualTurnover > 0)
			{
				Results.TangibleEquity = Results.TangibleEquityValue / Results.AnnualTurnover;
				Results.FreeCashFlow = Results.FreeCashFlowValue / Results.AnnualTurnover;
			}
			else
			{
				failedCalculatingFreeCashFlow = true;
				failedCalculatingTangibleEquity = true;
				Results.TangibleEquity = 0;
				Results.AnnualTurnover = 0;
				Results.FreeCashFlow = 0;
			}

			decimal mortgageBalance = GetMortgages(Results.CustomerId);
			if (totalZooplaValue != 0)
			{
				Results.NetWorth = (totalZooplaValue - mortgageBalance) / totalZooplaValue;
			}
			else
			{
				Results.NetWorth = 0;
			}
		}

		private void CalculateBankAnnualTurnover()
		{
			var yodleeMps = new List<int>();

				db.ForEachRowSafe((yodleeSafeReader, bRowsetStart) =>
				{
					int mpId = yodleeSafeReader["Id"];
					yodleeMps.Add(mpId);
					return ActionResult.Continue;
				}, "GetYodleeMps", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", Results.CustomerId));

				foreach (int mpId in yodleeMps)
				{
					var yodleeModelBuilder = new YodleeMarketplaceModelBuilder();
					YodleeModel yodleeModel = yodleeModelBuilder.BuildYodlee(mpId);

					Results.BankAnnualTurnover += (decimal)yodleeModel.BankStatementAnnualizedModel.Revenues;
				}
		}

		private decimal GetMortgages(int customerId)
		{
			var instance = new LoadExperianConsumerMortgageData(customerId, db, log);
			instance.Execute();

			return instance.Result.MortgageBalance;
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
			if (!freeCashFlowDataAvailable)
			{
				Results.FreeCashFlowWeight = 0;
				Results.AnnualTurnoverWeight += 7;
				Results.BusinessScoreWeight += 5;
				Results.ConsumerScoreWeight += 3;
				Results.BusinessSeniorityWeight += 4;
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

				Results.BusinessScoreWeight -= 6.25m;
				Results.BusinessSeniorityWeight -= 1.67m;
				Results.ConsumerScoreWeight -= 2.08m;
			}
		}
	}
}
