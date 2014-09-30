namespace EzBob.Backend.Strategies.ScoreCalculationVerification
{
	using System.Collections.Generic;
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

		public ScoreResult CalculateMedalScore(int inputCustomerId)
		{
			customerId = inputCustomerId;

			var scoreResult = new ScoreResult();
			try
			{
				GatherData();

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

				scoreResult.EzbobSeniority = ezbobSeniorityMedalParameter.EzbobSeniority.HasValue ? ezbobSeniorityMedalParameter.EzbobSeniority.Value : DateTime.UtcNow;
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

		private void GatherData()
		{
			SafeReader sr = db.GetFirst("GetDataForMedalCalculation2", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			if (sr.IsEmpty)
				throw new Exception("Failed gathering data from DB");
			
			int businessScore = sr["BusinessScore"];
			decimal rawTangibleEquity = sr["TangibleEquity"];
			DateTime? businessSeniority = sr["BusinessSeniority"];
			int consumerScore = sr["ConsumerScore"];
			decimal ebida = sr["Ebida"];
			decimal hmrcAnnualTurnover = sr["HmrcAnnualTurnover"];
			int totalZooplaValue = sr["TotalZooplaValue"];
			string maritalStatusString = sr["MaritalStatus"];
			DateTime? ezbobSeniority = sr["EzbobSeniority"];
			int numOfOnTimeLoans = sr["OnTimeLoans"];
			int numOfLateRepayments = sr["NumOfLatePayments"];
			int numOfEarlyRepayments = sr["NumOfEarlyPayments"];
			DateTime? firstRepaymentDate = sr["FirstRepaymentDate"];
			string typeOfBusiness = sr["TypeOfBusiness"];
			int numOfHmrcMps = sr["NumOfHmrcMps"];
			decimal yodleeAnnualTurnover = sr["YodleeTurnover"];
			decimal mortgageBalance = sr["BalanceOfMortgages"];
			decimal actualLoanRepayments = sr["ActualLoanRepayments"];
			decimal fcfFactor = sr["FcfFactor"];
			bool foundSummary = sr["FoundSummary"];

			if (typeOfBusiness != "Limited" && typeOfBusiness != "LLP")
			{
				throw new Exception(string.Format("Medal is meant only for customers with limited company. Company type: {0}", typeOfBusiness));
			}

			if (numOfHmrcMps > 1)
			{
				throw new Exception(string.Format("Medal is meant only for customers with 1 HMRC MP at most. Num of HMRCs: {0}", numOfHmrcMps));
			}
			
			decimal annualTurnover = !foundSummary ? yodleeAnnualTurnover : hmrcAnnualTurnover;

			decimal freeCashFlow = 0;
			decimal tangibleEquity = 0;
			decimal factoredLoanRepayments = actualLoanRepayments;
			if (fcfFactor != 0)
			{
				factoredLoanRepayments /= fcfFactor;
			}
			if (factoredLoanRepayments < 0)
			{
				factoredLoanRepayments = 0;
			}

			if (annualTurnover != 0)
			{
				if (foundSummary)
				{
					freeCashFlow = (ebida - factoredLoanRepayments)/annualTurnover;
				}
				tangibleEquity = rawTangibleEquity / annualTurnover;
			}

			var maritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), maritalStatusString);
			bool firstRepaymentDatePassed = firstRepaymentDate != null && firstRepaymentDate < DateTime.UtcNow;
			
			decimal netWorth;
			if (totalZooplaValue != 0)
			{
				netWorth = (totalZooplaValue - mortgageBalance) / totalZooplaValue;
			}
			else
			{
				netWorth = 0;
			}
			
			businessScoreMedalParameter = new BusinessScoreMedalParameter(businessScore, foundSummary, firstRepaymentDatePassed);
			tangibleEquityMedalParameter = new TangibleEquityMedalParameter(tangibleEquity, annualTurnover != 0);
			businessSeniorityMedalParameter = new BusinessSeniorityMedalParameter(businessSeniority, foundSummary, firstRepaymentDatePassed);
			consumerScoreMedalParameter = new ConsumerScoreMedalParameter(consumerScore, foundSummary, firstRepaymentDatePassed);
			maritalStatusMedalParameter = new MaritalStatusMedalParameter(maritalStatus);
			freeCashFlowMedalParameter = new FreeCashFlowMedalParameter(freeCashFlow, foundSummary, annualTurnover != 0);
			annualTurnoverMedalParameter = new AnnualTurnoverMedalParameter(annualTurnover, foundSummary);
			netWorthMedalParameter = new NetWorthMedalParameter(netWorth);
			ezbobSeniorityMedalParameter = new EzbobSeniorityMedalParameter(ezbobSeniority, firstRepaymentDatePassed);
			onTimeLoansMedalParameter = new OnTimeLoansMedalParameter(numOfOnTimeLoans, firstRepaymentDatePassed);
			lateRepaymentsMedalParameter = new LateRepaymentsMedalParameter(numOfLateRepayments, firstRepaymentDatePassed);
			earlyRepaymentsMedalParameter = new EarlyRepaymentsMedalParameter(numOfEarlyRepayments, firstRepaymentDatePassed);
		}
	}
}
