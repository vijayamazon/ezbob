namespace EzBob.Backend.Strategies.LimitedMedalCalculation
{
	using System.Collections.Generic;
	using ConfigManager;
	using EzBob.Models.Marketplaces.Builders;
	using EzBob.Models.Marketplaces.Yodlee;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using ScoreCalculation;

	public class NewMedalScoreCalculator2
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private int customerId;
		private decimal freeCashFlowValue;
		private decimal valueAdded;
		private decimal tangibleEquityValue;
		private bool basedOnHmrc;

		private BusinessScoreMedalParameter businessScoreMedalParameter;
		private TangibleEquityMedalParameter tangibleEquityMedalParameter;
		private BusinessSeniorityMedalParameter businessSeniorityMedalParameter;
		private ConsumerScoreMedalParameter consumerScoreMedalParameter;
		private MaritalStatusMedalParameter maritalStatusMedalParameter;
		private FreeCashFlowMedalParameter freeCashFlowMedalParameter;
		private AnnualTurnoverMedalParameter annualTurnoverMedalParameter;
		private NetWorthMedalParameter netWorthMedalParameter;
		private EzbobSeniorityMedalParameter ezbobSeniorityMedalParameter;
		private OnTimeLoansMedalParameter onTimeLoansMedalParameter;
		private LateRepaymentsMedalParameter lateRepaymentsMedalParameter;
		private EarlyRepaymentsMedalParameter earlyRepaymentsMedalParameter;

		public NewMedalScoreCalculator2(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
		}

		public ScoreResult CalculateMedalScore(int inputCustomerId, DateTime calculationTime)
		{
			customerId = inputCustomerId;

			var scoreResult = new ScoreResult();
			try
			{
				GatherData(calculationTime);

				var list = new List<MedalParameter>
					{
						businessScoreMedalParameter,
						tangibleEquityMedalParameter,
						businessSeniorityMedalParameter,
						consumerScoreMedalParameter,
						maritalStatusMedalParameter,
						freeCashFlowMedalParameter,
						annualTurnoverMedalParameter,
						netWorthMedalParameter,
						ezbobSeniorityMedalParameter,
						onTimeLoansMedalParameter,
						lateRepaymentsMedalParameter,
						earlyRepaymentsMedalParameter
					};

				foreach (MedalParameter medalParameter in list)
				{
					medalParameter.CalculateWeight();
				}

				AdjustSumOfWeights(list);

				decimal totalScore = 0;
				decimal totalMinScore = 0;
				decimal totalMaxScore = 0;
				foreach (MedalParameter medalParameter in list)
				{
					medalParameter.CalculateGrade();
					medalParameter.CalculateScore();
					totalScore += medalParameter.Score;
					totalMinScore += medalParameter.MinGrade * medalParameter.Weight;
					totalMaxScore += medalParameter.MaxGrade * medalParameter.Weight;
				}

				decimal normalizedTotalScore = (totalScore - totalMinScore) / (totalMaxScore - totalMinScore);
			
				scoreResult.AnnualTurnover = annualTurnoverMedalParameter.AnnualTurnover;
				scoreResult.AnnualTurnoverWeight = annualTurnoverMedalParameter.Weight;
				scoreResult.AnnualTurnoverGrade = annualTurnoverMedalParameter.Grade;
				scoreResult.AnnualTurnoverScore = annualTurnoverMedalParameter.Score;

				scoreResult.BusinessScore = businessScoreMedalParameter.BusinessScore;
				scoreResult.BusinessScoreWeight = businessScoreMedalParameter.Weight;
				scoreResult.BusinessScoreGrade = businessScoreMedalParameter.Grade;
				scoreResult.BusinessScoreScore = businessScoreMedalParameter.Score;

				scoreResult.BusinessSeniority = businessSeniorityMedalParameter.IncorporationDate;
				scoreResult.BusinessSeniorityWeight = businessSeniorityMedalParameter.Weight;
				scoreResult.BusinessSeniorityGrade = businessSeniorityMedalParameter.Grade;
				scoreResult.BusinessSeniorityScore = businessSeniorityMedalParameter.Score;

				scoreResult.ConsumerScore = consumerScoreMedalParameter.ConsumerScore;
				scoreResult.ConsumerScoreWeight = consumerScoreMedalParameter.Weight;
				scoreResult.ConsumerScoreGrade = consumerScoreMedalParameter.Grade;
				scoreResult.ConsumerScoreScore = consumerScoreMedalParameter.Score;

				scoreResult.EzbobSeniority = ezbobSeniorityMedalParameter.EzbobSeniority;
				scoreResult.EzbobSeniorityWeight = ezbobSeniorityMedalParameter.Weight;
				scoreResult.EzbobSeniorityGrade = ezbobSeniorityMedalParameter.Grade;
				scoreResult.EzbobSeniorityScore = ezbobSeniorityMedalParameter.Score;

				scoreResult.FreeCashFlow = freeCashFlowMedalParameter.FreeCashFlow;
				scoreResult.FreeCashFlowWeight = freeCashFlowMedalParameter.Weight;
				scoreResult.FreeCashFlowGrade = freeCashFlowMedalParameter.Grade;
				scoreResult.FreeCashFlowScore = freeCashFlowMedalParameter.Score;

				scoreResult.MaritalStatus = maritalStatusMedalParameter.MaritalStatus;
				scoreResult.MaritalStatusWeight = maritalStatusMedalParameter.Weight;
				scoreResult.MaritalStatusGrade = maritalStatusMedalParameter.Grade;
				scoreResult.MaritalStatusScore = maritalStatusMedalParameter.Score;

				scoreResult.NetWorth = netWorthMedalParameter.NetWorth;
				scoreResult.NetWorthWeight = netWorthMedalParameter.Weight;
				scoreResult.NetWorthGrade = netWorthMedalParameter.Grade;
				scoreResult.NetWorthScore = netWorthMedalParameter.Score;

				scoreResult.NumOfEarlyRepayments = earlyRepaymentsMedalParameter.NumOfEarlyRepayments;
				scoreResult.NumOfEarlyRepaymentsWeight = earlyRepaymentsMedalParameter.Weight;
				scoreResult.NumOfEarlyRepaymentsGrade = earlyRepaymentsMedalParameter.Grade;
				scoreResult.NumOfEarlyRepaymentsScore = earlyRepaymentsMedalParameter.Score;

				scoreResult.NumOfLateRepayments = lateRepaymentsMedalParameter.NumOfLateRepayments;
				scoreResult.NumOfLateRepaymentsWeight = lateRepaymentsMedalParameter.Weight;
				scoreResult.NumOfLateRepaymentsGrade = lateRepaymentsMedalParameter.Grade;
				scoreResult.NumOfLateRepaymentsScore = lateRepaymentsMedalParameter.Score;

				scoreResult.NumOfLoans = onTimeLoansMedalParameter.NumOfOnTimeLoans;
				scoreResult.NumOfLoansWeight = onTimeLoansMedalParameter.Weight;
				scoreResult.NumOfLoansGrade = onTimeLoansMedalParameter.Grade;
				scoreResult.NumOfLoansScore = onTimeLoansMedalParameter.Score;

				scoreResult.TangibleEquity = tangibleEquityMedalParameter.TangibleEquity;
				scoreResult.TangibleEquityWeight = tangibleEquityMedalParameter.Weight;
				scoreResult.TangibleEquityGrade = tangibleEquityMedalParameter.Grade;
				scoreResult.TangibleEquityScore = tangibleEquityMedalParameter.Score;

				scoreResult.TotalScore = totalScore;
				scoreResult.TotalScoreNormalized = normalizedTotalScore;

				scoreResult.FreeCashFlowValue = freeCashFlowValue;
				scoreResult.TangibleEquityValue = tangibleEquityValue;
				scoreResult.BasedOnHmrcValues = basedOnHmrc;
				scoreResult.ValueAdded = valueAdded;

				scoreResult.Medal = CalculateMedal(normalizedTotalScore);
			}
			catch (Exception e)
			{
				log.Error("Failed calculating medal for customer: {0} with error: {1}", customerId, e);
				scoreResult.Error = e.Message;
			}

			return scoreResult;
		}

		private MedalMultiplier CalculateMedal(decimal normalizedTotalScore)
		{
			if (normalizedTotalScore <= 0.4m)
			{
				return MedalMultiplier.Silver;
			}
			
			if (normalizedTotalScore <= 0.62m)
			{
				return MedalMultiplier.Gold;
			}
			
			if (normalizedTotalScore <= 0.84m)
			{
				return MedalMultiplier.Platinum;
			}
			
			return MedalMultiplier.Diamond;
		}

		private void AdjustSumOfWeights(List<MedalParameter> list)
		{
			decimal sumOfWeights = 0;
			decimal sumOfNonFixedWeights = 0;

			foreach (MedalParameter medalParameter in list)
			{
				sumOfWeights += medalParameter.Weight;
				if (!medalParameter.IsWeightFixed)
				{
					sumOfNonFixedWeights += medalParameter.Weight;
				}
			}

			decimal targetSumOfNonFixedWeights = sumOfNonFixedWeights + 100 - sumOfWeights;
			decimal ratioToGetToTarget = targetSumOfNonFixedWeights / sumOfNonFixedWeights;

			foreach (MedalParameter medalParameter in list)
			{
				if (!medalParameter.IsWeightFixed)
				{
					medalParameter.Weight *= ratioToGetToTarget;
				}
			}
		}

		private void GatherData(DateTime calculationTime)
		{
			SafeReader sr = db.GetFirst("GetDataForMedalCalculation2", CommandSpecies.StoredProcedure,
			                            new QueryParameter("CustomerId", customerId),
			                            new QueryParameter("CalculationTime", calculationTime));
			if (sr.IsEmpty)
			{
				throw new Exception("Failed gathering data from DB");
			}

			int businessScore = sr["BusinessScore"];
			decimal rawTangibleEquity = sr["TangibleEquity"];
			DateTime? businessSeniority = sr["BusinessSeniority"];
			int consumerScore = sr["ConsumerScore"];
			decimal ebida = sr["Ebida"];
			decimal hmrcAnnualTurnover = sr["HmrcAnnualTurnover"];
			decimal hmrcValueAdded = sr["HmrcValueAdded"];
			int totalZooplaValue = sr["TotalZooplaValue"];
			string maritalStatusString = sr["MaritalStatus"];
			DateTime? ezbobSeniority = sr["EzbobSeniority"];
			int numOfOnTimeLoans = sr["OnTimeLoans"];
			int numOfLateRepayments = sr["NumOfLatePayments"];
			int numOfEarlyRepayments = sr["NumOfEarlyPayments"];
			DateTime? firstRepaymentDate = sr["FirstRepaymentDate"];
			string typeOfBusiness = sr["TypeOfBusiness"];
			int numOfHmrcMps = sr["NumOfHmrcMps"];
			decimal mortgageBalance = sr["BalanceOfMortgages"];
			decimal actualLoanRepayments = sr["ActualLoanRepayments"];
			decimal fcfFactor = sr["FcfFactor"];
			bool foundSummary = sr["FoundSummary"];
			DateTime? earliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"];
			DateTime? earliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"];

			if (typeOfBusiness != "Limited" && typeOfBusiness != "LLP")
			{
				throw new Exception(string.Format("Medal is meant only for customers with limited company. Company type: {0}", typeOfBusiness));
			}

			if (numOfHmrcMps > 1)
			{
				throw new Exception(string.Format("Medal is meant only for customers with 1 HMRC MP at most. Num of HMRCs: {0}", numOfHmrcMps));
			}

			decimal annualTurnover;
			decimal freeCashFlow = 0;
			decimal tangibleEquity = 0;
			bool hasFreeCashFlowData = false;
			if (foundSummary)
			{
				if (earliestHmrcLastUpdateDate.HasValue &&
					earliestHmrcLastUpdateDate.Value.AddDays(CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy) < calculationTime)
				{
					throw new Exception(string.Format("HMRC data of customer {0} is too old: {1}. Threshold is: {2} days ", customerId, earliestHmrcLastUpdateDate.Value, CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy));
				}

				annualTurnover = hmrcAnnualTurnover;
				tangibleEquityValue = rawTangibleEquity;
				basedOnHmrc = true;

				decimal factoredLoanRepayments = actualLoanRepayments;
				if (fcfFactor != 0)
				{
					factoredLoanRepayments /= fcfFactor;
				}
				if (factoredLoanRepayments < 0)
				{
					factoredLoanRepayments = 0;
				}

				freeCashFlowValue = ebida - factoredLoanRepayments;
				hasFreeCashFlowData = true;

				if (annualTurnover < 0)
				{
					annualTurnover = 0;
				}
				else if (annualTurnover != 0)
				{
					freeCashFlow = freeCashFlowValue / annualTurnover;
					tangibleEquity = rawTangibleEquity / annualTurnover;
				}

				valueAdded = hmrcValueAdded;
			}
			else
			{
				if (earliestYodleeLastUpdateDate.HasValue &&
					earliestYodleeLastUpdateDate.Value.AddDays(CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy) < calculationTime)
				{
					throw new Exception(string.Format("Yodlee data of customer {0} is too old: {1}. Threshold is: {2} days ", customerId, earliestYodleeLastUpdateDate.Value, CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy));
				}

				var yodleeMps = new List<int>();
				tangibleEquityValue = 0;
				basedOnHmrc = false;

				db.ForEachRowSafe((yodleeSafeReader, bRowsetStart) =>
					{
						int mpId = yodleeSafeReader["Id"];
						yodleeMps.Add(mpId);
						return ActionResult.Continue;
					}, "GetYodleeMps", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

				if (yodleeMps.Count == 0)
				{
					freeCashFlowValue = 0;
					valueAdded = 0;
					annualTurnover = 0;
				}
				else
				{
					decimal totalFreeCashFlowValue = 0;
					decimal totalValueAdded = 0;
					decimal totalAnnualTurnover = 0;

					foreach (int mpId in yodleeMps)
					{
						var yodleeModelBuilder = new YodleeMarketplaceModelBuilder();
						YodleeModel yodleeModel = yodleeModelBuilder.BuildYodlee(mpId);

						totalFreeCashFlowValue += (decimal)yodleeModel.BankStatementAnnualizedModel.FreeCashFlow;
						totalValueAdded += (decimal)yodleeModel.BankStatementAnnualizedModel.TotalValueAdded;
						totalAnnualTurnover += (decimal)yodleeModel.BankStatementAnnualizedModel.Revenues;
					}

					freeCashFlowValue = totalFreeCashFlowValue;
					hasFreeCashFlowData = true;
					valueAdded = totalValueAdded;
					annualTurnover = totalAnnualTurnover;
					if (annualTurnover < 0)
					{
						annualTurnover = 0;
					}
					else if (annualTurnover != 0)
					{
						freeCashFlow = freeCashFlowValue/annualTurnover;
					}
				}
			}

			var maritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), maritalStatusString);
			bool firstRepaymentDatePassed = firstRepaymentDate != null && firstRepaymentDate < calculationTime;
			
			decimal netWorth;
			if (totalZooplaValue != 0)
			{
				netWorth = (totalZooplaValue - mortgageBalance) / totalZooplaValue;
			}
			else
			{
				netWorth = 0;
			}

			businessScoreMedalParameter = new BusinessScoreMedalParameter(businessScore, hasFreeCashFlowData, firstRepaymentDatePassed);
			tangibleEquityMedalParameter = new TangibleEquityMedalParameter(tangibleEquity, annualTurnover != 0);
			businessSeniorityMedalParameter = new BusinessSeniorityMedalParameter(businessSeniority, hasFreeCashFlowData, firstRepaymentDatePassed, calculationTime);
			consumerScoreMedalParameter = new ConsumerScoreMedalParameter(consumerScore, hasFreeCashFlowData, firstRepaymentDatePassed);
			maritalStatusMedalParameter = new MaritalStatusMedalParameter(maritalStatus);
			freeCashFlowMedalParameter = new FreeCashFlowMedalParameter(freeCashFlow, hasFreeCashFlowData, annualTurnover != 0);
			annualTurnoverMedalParameter = new AnnualTurnoverMedalParameter(annualTurnover, hasFreeCashFlowData);
			netWorthMedalParameter = new NetWorthMedalParameter(netWorth);
			ezbobSeniorityMedalParameter = new EzbobSeniorityMedalParameter(ezbobSeniority, firstRepaymentDatePassed);
			onTimeLoansMedalParameter = new OnTimeLoansMedalParameter(numOfOnTimeLoans, firstRepaymentDatePassed);
			lateRepaymentsMedalParameter = new LateRepaymentsMedalParameter(numOfLateRepayments, firstRepaymentDatePassed);
			earlyRepaymentsMedalParameter = new EarlyRepaymentsMedalParameter(numOfEarlyRepayments, firstRepaymentDatePassed);
		}
	}
}
