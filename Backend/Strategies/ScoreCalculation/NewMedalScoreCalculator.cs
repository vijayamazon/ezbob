namespace EzBob.Backend.Strategies.ScoreCalculation
{
	using Experian;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils;
	using VatReturn;

	public class NewMedalScoreCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private bool freeCashFlowDataAvailable;
		private bool firstRepaymentDatePassed;
		private bool failedCalculatingFreeCashFlow;
		private bool failedCalculatingTangibleEquity;

		public ScoreResult Results { get; set; }

		public NewMedalScoreCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
		}

		public ScoreResult CalculateMedalScore(ScoreResult inputData, int customerId)
		{
			Results = inputData;
			try
			{
				CalculateWeights();
				CalculateGrades();

				CalculateCustomerScore();
				decimal totalScoreMin = CalculateScoreMin();
				decimal totalScoreMax = CalculateScoreMax();
				Results.TotalScoreNormalized = (Results.TotalScore - totalScoreMin) / (totalScoreMax - totalScoreMin);

				CalculateMedal();
			}
			catch (Exception e)
			{
				log.Error("Failed calculating medal for customer: {0} with error: {1}", customerId, e);
				Results.Error = e.Message;
			}

			return Results;
		}

		public ScoreResult CalculateMedalScore(int customerId)
		{
			ScoreResult scoreResultForError;
			try
			{
				var inputData = GatherData(customerId);
				return CalculateMedalScore(inputData, customerId);
			}
			catch (Exception e)
			{
				log.Error("Failed calculating medal for customer: {0} with error: {1}", customerId, e);
				scoreResultForError = new ScoreResult {Error = e.Message};
			}

			return scoreResultForError;
		}

		private ScoreResult GatherData(int customerId)
		{
			var inputData = new ScoreResult();
			SafeReader sr = db.GetFirst("GetDataForMedalCalculation", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			if (sr.IsEmpty)
				throw new Exception("Couldn't gather required data for the medal calculation");

			inputData.BusinessScore = sr["BusinessScore"];
			decimal tangibleEquity = sr["TangibleEquity"];
			inputData.BusinessSeniority = sr["BusinessSeniority"];
			inputData.ConsumerScore = sr["ConsumerScore"];
			string maritalStatusStr = sr["MaritalStatus"];
			inputData.MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), maritalStatusStr);
			firstRepaymentDatePassed = sr["FirstRepaymentDatePassed"];
			inputData.EzbobSeniority = sr["EzbobSeniority"];
			inputData.NumOfLoans = sr["OnTimeLoans"];
			inputData.NumOfLateRepayments = sr["NumOfLatePayments"];
			inputData.NumOfEarlyRepayments = sr["NumOfEarlyPayments"];
			int hmrcId = sr["HmrcId"];
			decimal yodleeTurnover = sr["YodleeTurnover"];
			int totalZooplaValue = sr["TotalZooplaValue"];
			int numOfHmrcMps = sr["NumOfHmrcMps"];

			if (numOfHmrcMps > 1)
			{
				throw new Exception(string.Format("Medal is meant only for customers with 1 HMRC MP at most. Num of HMRCs: {0}", numOfHmrcMps));
			}

			bool wasAbleToGetSummaryData = false;
			VatReturnSummary[] summaryData = null;
			if (hmrcId != 0)
			{
				var loadVatReturnSummary = new LoadVatReturnSummary(customerId, hmrcId, db, log);
				loadVatReturnSummary.Execute();
				summaryData = loadVatReturnSummary.Summary;

				if (summaryData != null && summaryData.Length != 0)
				{
					wasAbleToGetSummaryData = true;
				}
			}

			failedCalculatingFreeCashFlow = false;

			if (wasAbleToGetSummaryData)
			{
				freeCashFlowDataAvailable = true;

				decimal totalRevenuesInSummary = 0;
				decimal totalFreeCashFlowInSummary = 0;

				foreach (VatReturnSummary singleSummary in summaryData)
				{
					totalRevenuesInSummary += singleSummary.Revenues ?? 0;
					totalFreeCashFlowInSummary += singleSummary.FreeCashFlow.HasValue ? singleSummary.FreeCashFlow.Value : 0;
				}

				inputData.AnnualTurnover = totalRevenuesInSummary;
				if (inputData.AnnualTurnover != 0)
				{
					inputData.FreeCashFlow = totalFreeCashFlowInSummary / inputData.AnnualTurnover;
				}
				else
				{
					failedCalculatingFreeCashFlow = true;
					inputData.FreeCashFlow = 0;
				}
			}
			else
			{
				freeCashFlowDataAvailable = false;
				inputData.AnnualTurnover = yodleeTurnover;
				inputData.FreeCashFlow = 0;
			}

			failedCalculatingTangibleEquity = false;

			if (inputData.AnnualTurnover != 0)
			{
				inputData.TangibleEquity = tangibleEquity / inputData.AnnualTurnover;
			}
			else
			{
				failedCalculatingTangibleEquity = true;
				inputData.TangibleEquity = 0;
			}
			
			decimal mortgageBalance = GetMortgages(customerId);
			if (totalZooplaValue != 0)
			{
				inputData.NetWorth = (totalZooplaValue - mortgageBalance) / totalZooplaValue;
			}
			else
			{
				inputData.NetWorth = 0;
			}

			return inputData;
		}

		private decimal GetMortgages(int customerId)
		{
			var instance = new LoadExperianConsumerMortgageData(customerId, db, log);
			instance.Execute();

			return instance.Result.MortgageBalance;
		}

		private void CalculateMedal()
		{
			if (Results.TotalScoreNormalized <= 0.4m)
			{
				Results.Medal = MedalMultiplier.Silver;
			}
			else if (Results.TotalScoreNormalized <= 0.62m)
			{
				Results.Medal = MedalMultiplier.Gold;
			}
			else if (Results.TotalScoreNormalized <= 0.84m)
			{
				Results.Medal = MedalMultiplier.Platinum;
			}
			else
			{
				Results.Medal = MedalMultiplier.Diamond;
			}
		}

		private decimal CalculateScoreMax()
		{
			int annualTurnoverMaxGrade = 6;
			int businessScoreMaxGrade = 9;
			int freeCashflowMaxGrade = 6;
			int tangibleEquityMaxGrade = 4;
			int businessSeniorityMaxGrade = 4;
			int consumerScoreMaxGrade = 8;
			int netWorthMaxGrade = 3;
			int maritalStatusMaxGrade = 4;
			int ezbobSeniorityMaxGrade = 4;
			int numOfLoansMaxGrade = 4;
			int numOfLateRepaymentsMaxGrade = 5;
			int numOfEarlyRepaymentsMaxGrade = 5;

			decimal annualTurnoverScoreMax = Results.AnnualTurnoverWeight * annualTurnoverMaxGrade;
			decimal businessScoreScoreMax = Results.BusinessScoreWeight * businessScoreMaxGrade;
			decimal freeCashFlowScoreMax = Results.FreeCashFlowWeight * freeCashflowMaxGrade;
			decimal tangibleEquityScoreMax = Results.TangibleEquityWeight * tangibleEquityMaxGrade;
			decimal businessSeniorityScoreMax = Results.BusinessSeniorityWeight * businessSeniorityMaxGrade;
			decimal consumerScoreScoreMax = Results.ConsumerScoreWeight * consumerScoreMaxGrade;
			decimal netWorthScoreMax = Results.NetWorthWeight * netWorthMaxGrade;
			decimal maritalStatusScoreMax = Results.MaritalStatusWeight * maritalStatusMaxGrade;
			decimal ezbobSeniorityScoreMax = Results.EzbobSeniorityWeight * ezbobSeniorityMaxGrade;
			decimal ezbobNumOfLoansScoreMax = Results.NumOfLoansWeight * numOfLoansMaxGrade;
			decimal ezbobNumOfLateRepaymentsScoreMax = Results.NumOfLateRepaymentsWeight * numOfLateRepaymentsMaxGrade;
			decimal ezbobNumOfEarlyRepaymentsScoreMax = Results.NumOfEarlyRepaymentsWeight * numOfEarlyRepaymentsMaxGrade;

			return annualTurnoverScoreMax + businessScoreScoreMax + freeCashFlowScoreMax + tangibleEquityScoreMax +
								 businessSeniorityScoreMax + consumerScoreScoreMax + netWorthScoreMax + maritalStatusScoreMax +
								 ezbobSeniorityScoreMax + ezbobNumOfLoansScoreMax + ezbobNumOfLateRepaymentsScoreMax +
								 ezbobNumOfEarlyRepaymentsScoreMax;
		}

		private decimal CalculateScoreMin()
		{
			int annualTurnoverMinGrade = 0;
			int businessScoreMinGrade = 0;
			int freeCashflowMinGrade = 0;
			int tangibleEquityMinGrade = 0;
			int businessSeniorityMinGrade = 0;
			int consumerScoreMinGrade = 0;
			int netWorthMinGrade = 0;
			int maritalStatusMinGrade = 2;
			int ezbobSeniorityMinGrade = 0;
			int numOfLoansMinGrade = 1;
			int numOfLateRepaymentsMinGrade = 0;
			int numOfEarlyRepaymentsMinGrade = 2;

			decimal annualTurnoverScoreMin = Results.AnnualTurnoverWeight * annualTurnoverMinGrade;
			decimal businessScoreScoreMin = Results.BusinessScoreWeight * businessScoreMinGrade;
			decimal freeCashFlowScoreMin = Results.FreeCashFlowWeight * freeCashflowMinGrade;
			decimal tangibleEquityScoreMin = Results.TangibleEquityWeight * tangibleEquityMinGrade;
			decimal businessSeniorityScoreMin = Results.BusinessSeniorityWeight * businessSeniorityMinGrade;
			decimal consumerScoreScoreMin = Results.ConsumerScoreWeight * consumerScoreMinGrade;
			decimal netWorthScoreMin = Results.NetWorthWeight * netWorthMinGrade;
			decimal maritalStatusScoreMin = Results.MaritalStatusWeight * maritalStatusMinGrade;
			decimal ezbobSeniorityScoreMin = Results.EzbobSeniorityWeight * ezbobSeniorityMinGrade;
			decimal ezbobNumOfLoansScoreMin = Results.NumOfLoansWeight * numOfLoansMinGrade;
			decimal ezbobNumOfLateRepaymentsScoreMin = Results.NumOfLateRepaymentsWeight * numOfLateRepaymentsMinGrade;
			decimal ezbobNumOfEarlyRepaymentsScoreMin = Results.NumOfEarlyRepaymentsWeight * numOfEarlyRepaymentsMinGrade;

			return annualTurnoverScoreMin + businessScoreScoreMin + freeCashFlowScoreMin + tangibleEquityScoreMin +
								 businessSeniorityScoreMin + consumerScoreScoreMin + netWorthScoreMin + maritalStatusScoreMin +
								 ezbobSeniorityScoreMin + ezbobNumOfLoansScoreMin + ezbobNumOfLateRepaymentsScoreMin +
								 ezbobNumOfEarlyRepaymentsScoreMin;
		}

		private void CalculateCustomerScore()
		{
			Results.AnnualTurnoverScore = Results.AnnualTurnoverWeight*Results.AnnualTurnoverGrade;
			Results.BusinessScoreScore = Results.BusinessScoreWeight*Results.BusinessScoreGrade;
			Results.FreeCashFlowScore = Results.FreeCashFlowWeight*Results.FreeCashFlowGrade;
			Results.TangibleEquityScore = Results.TangibleEquityWeight*Results.TangibleEquityGrade;
			Results.BusinessSeniorityScore = Results.BusinessSeniorityWeight*Results.BusinessSeniorityGrade;
			Results.ConsumerScoreScore = Results.ConsumerScoreWeight*Results.ConsumerScoreGrade;
			Results.NetWorthScore = Results.NetWorthWeight*Results.NetWorthGrade;
			Results.MaritalStatusScore = Results.MaritalStatusWeight*Results.MaritalStatusGrade;
			Results.EzbobSeniorityScore = Results.EzbobSeniorityWeight*Results.EzbobSeniorityGrade;
			Results.NumOfLoansScore = Results.NumOfLoansWeight*Results.NumOfLoansGrade;
			Results.NumOfLateRepaymentsScore = Results.NumOfLateRepaymentsWeight*Results.NumOfLateRepaymentsGrade;
			Results.NumOfEarlyRepaymentsScore = Results.NumOfEarlyRepaymentsWeight*Results.NumOfEarlyRepaymentsGrade;

			Results.TotalScore = Results.AnnualTurnoverScore + Results.BusinessScoreScore + Results.FreeCashFlowScore +
			                     Results.TangibleEquityScore +
			                     Results.BusinessSeniorityScore + Results.ConsumerScoreScore + Results.NetWorthScore +
			                     Results.MaritalStatusScore +
			                     Results.EzbobSeniorityScore + Results.NumOfLoansScore + Results.NumOfLateRepaymentsScore +
			                     Results.NumOfEarlyRepaymentsScore;
		}

		private void CalculateGrades()
		{
			CalculateBusinessScoreGrade();
			CalculateFreeCashFlowGrade();
			CalculateAnnualTurnoverGrade();
			CalculateTangibleEquityGrade();
			CalculateBusinessSeniorityGrade();
			CalculateConsumerScoreGrade();
			CalculateNetWorthGrade();
			CalculateMaritalStatusGrade();
			CalculateEzbobSeniorityGrade();
			CalculateNumOfLoansGrade();
			CalculateNumOfLateRepaymentsGrade();
			CalculateNumOfEarlyRepaymentsGrade();
		}

		private void CalculateNumOfLateRepaymentsGrade()
		{
			if (Results.NumOfLateRepayments == 0)
			{
				Results.NumOfLateRepaymentsGrade = 5;
			}
			else if (Results.NumOfLateRepayments == 1)
			{
				Results.NumOfLateRepaymentsGrade = 2;
			}
			else
			{
				Results.NumOfLateRepaymentsGrade = 0;
			}
		}

		private void CalculateNumOfEarlyRepaymentsGrade()
		{
			if (Results.NumOfEarlyRepayments == 0)
			{
				Results.NumOfEarlyRepaymentsGrade = 2;
			}
			else if (Results.NumOfEarlyRepayments < 3)
			{
				Results.NumOfEarlyRepaymentsGrade = 3;
			}
			else
			{
				Results.NumOfEarlyRepaymentsGrade = 5;
			}
		}

		private void CalculateNumOfLoansGrade()
		{
			if (Results.NumOfLoans > 3)
			{
				Results.NumOfLoansGrade = 4;
			}
			else if (Results.NumOfLoans > 1)
			{
				Results.NumOfLoansGrade = 3;
			}
			else
			{
				Results.NumOfLoansGrade = 1;
			}
		}

		private void CalculateEzbobSeniorityGrade()
		{
			int ezbobSeniorityMonthsOnly, ezbobSeniorityYearsOnly;
			MiscUtils.GetFullYearsAndMonths(Results.EzbobSeniority, out ezbobSeniorityMonthsOnly, out ezbobSeniorityYearsOnly);
			decimal ezbobSeniorityMonths = ezbobSeniorityMonthsOnly + 12 * ezbobSeniorityYearsOnly;
			if (ezbobSeniorityMonths > 17)
			{
				Results.EzbobSeniorityGrade = 4;
			}
			else if (ezbobSeniorityMonths > 5)
			{
				Results.EzbobSeniorityGrade = 3;
			}
			else if (ezbobSeniorityMonths > 0)
			{
				Results.EzbobSeniorityGrade = 2;
			}
			else
			{
				Results.EzbobSeniorityGrade = 0;
			}
		}

		private void CalculateMaritalStatusGrade()
		{
			if (Results.MaritalStatus == MaritalStatus.Married || Results.MaritalStatus == MaritalStatus.Widowed)
			{
				Results.MaritalStatusGrade = 4;
			}
			else if (Results.MaritalStatus == MaritalStatus.Divorced || Results.MaritalStatus == MaritalStatus.LivingTogether)
			{
				Results.MaritalStatusGrade = 3;
			}
			else // Single, Separated, Other
			{
				Results.MaritalStatusGrade = 2;
			}
		}

		private void CalculateNetWorthGrade()
		{
			if (Results.NetWorth < 0.15m)
			{
				Results.NetWorthGrade = 0;
			}
			else if (Results.NetWorth < 0.5m)
			{
				Results.NetWorthGrade = 1;
			}
			else if (Results.NetWorth < 1)
			{
				Results.NetWorthGrade = 2;
			}
			else
			{
				// We know that we sometimes miss mortgages the customer has, so instead of grade=3 we give 1
				Results.NetWorthGrade = 1;
			}
		}

		private void CalculateConsumerScoreGrade()
		{
			if (Results.ConsumerScore < 481)
			{
				Results.ConsumerScoreGrade = 0;
			}
			else if (Results.ConsumerScore < 561)
			{
				Results.ConsumerScoreGrade = 1;
			}
			else if (Results.ConsumerScore < 641)
			{
				Results.ConsumerScoreGrade = 2;
			}
			else if (Results.ConsumerScore < 721)
			{
				Results.ConsumerScoreGrade = 3;
			}
			else if (Results.ConsumerScore < 801)
			{
				Results.ConsumerScoreGrade = 4;
			}
			else if (Results.ConsumerScore < 881)
			{
				Results.ConsumerScoreGrade = 5;
			}
			else if (Results.ConsumerScore < 961)
			{
				Results.ConsumerScoreGrade = 6;
			}
			else if (Results.ConsumerScore < 1041)
			{
				Results.ConsumerScoreGrade = 7;
			}
			else
			{
				Results.ConsumerScoreGrade = 8;
			}
		}

		private void CalculateBusinessSeniorityGrade()
		{
			if (!Results.BusinessSeniority.HasValue)
			{
				Results.BusinessSeniorityGrade = 0;
			}
			else if (Results.BusinessSeniority.Value.AddYears(1) > DateTime.UtcNow)
			{
				Results.BusinessSeniorityGrade = 0;
			}
			else if (Results.BusinessSeniority.Value.AddYears(3) > DateTime.UtcNow)
			{
				Results.BusinessSeniorityGrade = 1;
			}
			else if (Results.BusinessSeniority.Value.AddYears(5) > DateTime.UtcNow)
			{
				Results.BusinessSeniorityGrade = 2;
			}
			else if (Results.BusinessSeniority.Value.AddYears(10) > DateTime.UtcNow)
			{
				Results.BusinessSeniorityGrade = 3;
			}
			else
			{
				Results.BusinessSeniorityGrade = 4;
			}
		}

		private void CalculateTangibleEquityGrade()
		{
			if (Results.TangibleEquity < -0.05m || failedCalculatingTangibleEquity) // When turnover is zero we can't calc tangible equity, we want the min grade
			{
				Results.TangibleEquityGrade = 0;
			}
			else if (Results.TangibleEquity < 0)
			{
				Results.TangibleEquityGrade = 1;
			}
			else if (Results.TangibleEquity < 0.1m)
			{
				Results.TangibleEquityGrade = 2;
			}
			else if (Results.TangibleEquity < 0.3m)
			{
				Results.TangibleEquityGrade = 3;
			}
			else
			{
				Results.TangibleEquityGrade = 4;
			}
		}

		private void CalculateAnnualTurnoverGrade()
		{
			if (Results.AnnualTurnover < 30000)
			{
				Results.AnnualTurnoverGrade = 0;
			}
			else if (Results.AnnualTurnover < 100000)
			{
				Results.AnnualTurnoverGrade = 1;
			}
			else if (Results.AnnualTurnover < 200000)
			{
				Results.AnnualTurnoverGrade = 2;
			}
			else if (Results.AnnualTurnover < 400000)
			{
				Results.AnnualTurnoverGrade = 3;
			}
			else if (Results.AnnualTurnover < 800000)
			{
				Results.AnnualTurnoverGrade = 4;
			}
			else if (Results.AnnualTurnover < 2000000)
			{
				Results.AnnualTurnoverGrade = 5;
			}
			else
			{
				Results.AnnualTurnoverGrade = 6;
			}
		}

		private void CalculateFreeCashFlowGrade()
		{
			if (Results.FreeCashFlow < -0.1m || !freeCashFlowDataAvailable || failedCalculatingFreeCashFlow) // When turnover is zero we can't calc FCF, we want the min grade
			{
				Results.FreeCashFlowGrade = 0;
			}
			else if (Results.FreeCashFlow < 0)
			{
				Results.FreeCashFlowGrade = 1;
			}
			else if (Results.FreeCashFlow < 0.1m)
			{
				Results.FreeCashFlowGrade = 2;
			}
			else if (Results.FreeCashFlow < 0.2m)
			{
				Results.FreeCashFlowGrade = 3;
			}
			else if (Results.FreeCashFlow < 0.3m)
			{
				Results.FreeCashFlowGrade = 4;
			}
			else if (Results.FreeCashFlow < 0.4m)
			{
				Results.FreeCashFlowGrade = 5;
			}
			else
			{
				Results.FreeCashFlowGrade = 6;
			}
		}

		private void CalculateBusinessScoreGrade()
		{
			if (Results.BusinessScore < 11)
			{
				Results.BusinessScoreGrade = 0;
			}
			else if (Results.BusinessScore < 21)
			{
				Results.BusinessScoreGrade = 1;
			}
			else if (Results.BusinessScore < 31)
			{
				Results.BusinessScoreGrade = 2;
			}
			else if (Results.BusinessScore < 41)
			{
				Results.BusinessScoreGrade = 3;
			}
			else if (Results.BusinessScore < 51)
			{
				Results.BusinessScoreGrade = 4;
			}
			else if (Results.BusinessScore < 61)
			{
				Results.BusinessScoreGrade = 5;
			}
			else if (Results.BusinessScore < 71)
			{
				Results.BusinessScoreGrade = 6;
			}
			else if (Results.BusinessScore < 81)
			{
				Results.BusinessScoreGrade = 7;
			}
			else if (Results.BusinessScore < 91)
			{
				Results.BusinessScoreGrade = 8;
			}
			else
			{
				Results.BusinessScoreGrade = 9;
			}
		}

		private void CalculateWeights()
		{
			bool sumIs100 = true;

			if (Results.BusinessScore <= 30)
			{
				Results.BusinessScoreWeight = 41.25m;
				sumIs100 = false;
			}

			if (Results.ConsumerScore <= 800)
			{
				Results.ConsumerScoreWeight = 13.75m;
				sumIs100 = false;
			}

			if (!freeCashFlowDataAvailable)
			{
				Results.FreeCashFlowWeight = 0;
				Results.AnnualTurnoverWeight += 7;
				Results.BusinessScoreWeight += 5;
				Results.ConsumerScoreWeight += 3;
				Results.BusinessSeniorityWeight += 4;
			}

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

			if (!sumIs100)
			{
				decimal sumOfWeights = Results.BusinessScoreWeight + Results.FreeCashFlowWeight + Results.AnnualTurnoverWeight +
				                       Results.TangibleEquityWeight +
				                       Results.BusinessSeniorityWeight + Results.ConsumerScoreWeight + Results.NetWorthWeight +
				                       Results.MaritalStatusWeight +
				                       Results.EzbobSeniorityWeight + Results.NumOfLoansWeight + Results.NumOfLateRepaymentsWeight +
				                       Results.NumOfEarlyRepaymentsWeight;

				decimal sumOfNonFixed = Results.TangibleEquityWeight + Results.NetWorthWeight + Results.MaritalStatusWeight;

				decimal sumOfNonFixedDestination = sumOfNonFixed - sumOfWeights + 100;

				decimal ratioForDestionation = sumOfNonFixedDestination/sumOfNonFixed;

				Results.TangibleEquityWeight *= ratioForDestionation;
				Results.NetWorthWeight *= ratioForDestionation;
				Results.MaritalStatusWeight *= ratioForDestionation;
			}
		}

		public int GetOfferedCreditLine(MedalMultiplier medal, decimal annualTurnover, int experianScore) {
			return (int)Math.Round((int)((int)medal * annualTurnover * GetLoanOfferMultiplier(experianScore) / 100) / 100d, 0, MidpointRounding.AwayFromZero) * 100;
		}

		public decimal GetBasicInterestRate(int experianScore) {
			SafeReader sr = db.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "BasicInterestRate"), new QueryParameter("Key", experianScore));
			return sr["Value"];
		}

		private decimal GetLoanOfferMultiplier(int experianScore) {
			SafeReader sr = db.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "LoanOfferMultiplier"), new QueryParameter("Key", experianScore));
			return sr["Value"];
		}
	}
}
