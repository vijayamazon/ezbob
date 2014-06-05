namespace EzBob.Backend.Strategies.ScoreCalculation
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	
	public class NewMedalScoreCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;

		public NewMedalScoreCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
			Results = new ScoreResult();
		}

		public ScoreResult Results { get; set; }
		private decimal annualTurnover;
		private int businessScore;
		private decimal freeCashFlow;
		private bool freeCashFlowDataAvailable;
		private decimal tangibleEquity;
		private DateTime businessSeniority;
		private int consumerScore;
		private decimal netWorth;
		private MaritalStatus maritalStatus;
		private bool firstRepaymentDatePassed;
		private decimal ezbobSeniorityMonths;
		private int ezbobNumOfLoans;
		private int ezbobNumOfLateRepayments;
		private int ezbobNumOfEarlyReayments;

		private void GatherData(int customerId)
		{
			// TODO: complete logic
			annualTurnover = 1; // use hmrc if have data, otherwise use bank (12M or annualized)
			freeCashFlow = 1;
			freeCashFlowDataAvailable = false;
			netWorth = 1;


			DataTable dt = db.ExecuteReader("GetDataForMedalCalculation", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			if (dt.Rows.Count != 1)
			{
				throw new Exception("Couldn't gather required data for the medal calculation");
			}

			var sr = new SafeReader(dt.Rows[0]);

			businessScore = sr["BusinessScore"];
			tangibleEquity = sr["TangibleEquity"];
			businessSeniority = sr["BusinessSeniority"];
			consumerScore = sr["ConsumerScore"];
			string maritalStatusStr = sr["MaritalStatus"];
			maritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), maritalStatusStr);
			firstRepaymentDatePassed = sr["FirstRepaymentDatePassed"];
			ezbobSeniorityMonths = sr["EzbobSeniority"];
			ezbobNumOfLoans = sr["OnTimeLoans"];
			ezbobNumOfLateRepayments = sr["NumOfLatePayments"];
			ezbobNumOfEarlyReayments = sr["NumOfEarlyPayments"];
		}

		public ScoreResult CalculateMedalScore(int customerId)
		{
			GatherData(customerId);
			CalculateWeights();
			CalculateGrades();

			CalculateCustomerScore();
			decimal totalScoreMin = CalculateScoreMin();
			decimal totalScoreMax = CalculateScoreMax();
			Results.TotalScoreNormalized = (Results.TotalScore - totalScoreMin) / (totalScoreMax - totalScoreMin);
			
			CalculateMedal();

			return Results;
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
			
			// todo: Fill max grades - Vitas should complete tables
			int maritalStatusMaxGrade = 0;
			int ezbobSeniorityMonthsMaxGrade = 0;
			int numOfLoansMaxGrade = 0;
			int numOfLateRepaymentsMaxGrade = 0;
			int numOfEarlyReaymentsMaxGrade = 0;

			decimal annualTurnoverScoreMax = Results.AnnualTurnoverWeight * annualTurnoverMaxGrade;
			decimal businessScoreScoreMax = Results.BusinessScoreWeight * businessScoreMaxGrade;
			decimal freeCashFlowScoreMax = Results.FreeCashFlowWeight * freeCashflowMaxGrade;
			decimal tangibleEquityScoreMax = Results.TangibleEquityWeight * tangibleEquityMaxGrade;
			decimal businessSeniorityScoreMax = Results.BusinessSeniorityWeight * businessSeniorityMaxGrade;
			decimal consumerScoreScoreMax = Results.ConsumerScoreWeight * consumerScoreMaxGrade;
			decimal netWorthScoreMax = Results.NetWorthWeight * netWorthMaxGrade;
			decimal maritalStatusScoreMax = Results.MaritalStatusWeight * maritalStatusMaxGrade;
			decimal ezbobSeniorityMonthsScoreMax = Results.EzbobSeniorityWeight * ezbobSeniorityMonthsMaxGrade;
			decimal ezbobNumOfLoansScoreMax = Results.NumOfLoansWeight * numOfLoansMaxGrade;
			decimal ezbobNumOfLateRepaymentsScoreMax = Results.NumOfLateRepaymentsWeight * numOfLateRepaymentsMaxGrade;
			decimal ezbobNumOfEarlyReaymentsScoreMax = Results.NumOfEarlyReaymentsWeight * numOfEarlyReaymentsMaxGrade;

			return annualTurnoverScoreMax + businessScoreScoreMax + freeCashFlowScoreMax + tangibleEquityScoreMax +
								 businessSeniorityScoreMax + consumerScoreScoreMax + netWorthScoreMax + maritalStatusScoreMax +
								 ezbobSeniorityMonthsScoreMax + ezbobNumOfLoansScoreMax + ezbobNumOfLateRepaymentsScoreMax +
								 ezbobNumOfEarlyReaymentsScoreMax;
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
			int maritalStatusMinGrade = 0;
			int ezbobSeniorityMonthsMinGrade = 0;
			int numOfLoansMinGrade = 0;
			int numOfLateRepaymentsMinGrade = 0;
			int numOfEarlyReaymentsMinGrade = 0;


			decimal annualTurnoverScoreMin = Results.AnnualTurnoverWeight * annualTurnoverMinGrade;
			decimal businessScoreScoreMin = Results.BusinessScoreWeight * businessScoreMinGrade;
			decimal freeCashFlowScoreMin = Results.FreeCashFlowWeight * freeCashflowMinGrade;
			decimal tangibleEquityScoreMin = Results.TangibleEquityWeight * tangibleEquityMinGrade;
			decimal businessSeniorityScoreMin = Results.BusinessSeniorityWeight * businessSeniorityMinGrade;
			decimal consumerScoreScoreMin = Results.ConsumerScoreWeight * consumerScoreMinGrade;
			decimal netWorthScoreMin = Results.NetWorthWeight * netWorthMinGrade;
			decimal maritalStatusScoreMin = Results.MaritalStatusWeight * maritalStatusMinGrade;
			decimal ezbobSeniorityMonthsScoreMin = Results.EzbobSeniorityWeight * ezbobSeniorityMonthsMinGrade;
			decimal ezbobNumOfLoansScoreMin = Results.NumOfLoansWeight * numOfLoansMinGrade;
			decimal ezbobNumOfLateRepaymentsScoreMin = Results.NumOfLateRepaymentsWeight * numOfLateRepaymentsMinGrade;
			decimal ezbobNumOfEarlyReaymentsScoreMin = Results.NumOfEarlyReaymentsWeight * numOfEarlyReaymentsMinGrade;

			return annualTurnoverScoreMin + businessScoreScoreMin + freeCashFlowScoreMin + tangibleEquityScoreMin +
								 businessSeniorityScoreMin + consumerScoreScoreMin + netWorthScoreMin + maritalStatusScoreMin +
								 ezbobSeniorityMonthsScoreMin + ezbobNumOfLoansScoreMin + ezbobNumOfLateRepaymentsScoreMin +
								 ezbobNumOfEarlyReaymentsScoreMin;
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
			Results.NumOfEarlyReaymentsScore = Results.NumOfEarlyReaymentsWeight*Results.NumOfEarlyReaymentsGrade;

			Results.TotalScore = Results.AnnualTurnoverScore + Results.BusinessScoreScore + Results.FreeCashFlowScore +
			                     Results.TangibleEquityScore +
			                     Results.BusinessSeniorityScore + Results.ConsumerScoreScore + Results.NetWorthScore +
			                     Results.MaritalStatusScore +
			                     Results.EzbobSeniorityScore + Results.NumOfLoansScore + Results.NumOfLateRepaymentsScore +
			                     Results.NumOfEarlyReaymentsScore;
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

			// TODO:Complete 5 tables grading - vitas should send
			Results.MaritalStatusGrade = 1;
			Results.EzbobSeniorityGrade = 1;
			Results.NumOfLoansGrade = 1;
			Results.NumOfLateRepaymentsGrade = 1;
			Results.NumOfEarlyReaymentsGrade = 1;
		}

		private void CalculateNetWorthGrade()
		{
			if (netWorth < 15)
			{
				Results.NetWorthGrade = 0;
			}
			else if (netWorth < 50)
			{
				Results.NetWorthGrade = 1;
			}
			else if (netWorth < 100)
			{
				Results.NetWorthGrade = 2;
			}
			else
			{
				Results.NetWorthGrade = 3;
			}
		}

		private void CalculateConsumerScoreGrade()
		{
			if (consumerScore < 481)
			{
				Results.ConsumerScoreGrade = 0;
			}
			else if (consumerScore < 561)
			{
				Results.ConsumerScoreGrade = 1;
			}
			else if (consumerScore < 641)
			{
				Results.ConsumerScoreGrade = 2;
			}
			else if (consumerScore < 721)
			{
				Results.ConsumerScoreGrade = 3;
			}
			else if (consumerScore < 801)
			{
				Results.ConsumerScoreGrade = 4;
			}
			else if (consumerScore < 881)
			{
				Results.ConsumerScoreGrade = 5;
			}
			else if (consumerScore < 961)
			{
				Results.ConsumerScoreGrade = 6;
			}
			else if (consumerScore < 1041)
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
			if (businessSeniority.AddYears(1) > DateTime.UtcNow)
			{
				Results.BusinessSeniorityGrade = 0;
			}
			else if (businessSeniority.AddYears(3) > DateTime.UtcNow)
			{
				Results.BusinessSeniorityGrade = 1;
			}
			else if (businessSeniority.AddYears(5) > DateTime.UtcNow)
			{
				Results.BusinessSeniorityGrade = 2;
			}
			else if (businessSeniority.AddYears(10) > DateTime.UtcNow)
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
			if (tangibleEquity < -5)
			{
				Results.TangibleEquityGrade = 0;
			}
			else if (tangibleEquity < 0)
			{
				Results.TangibleEquityGrade = 1;
			}
			else if (tangibleEquity < 10)
			{
				Results.TangibleEquityGrade = 2;
			}
			else if (tangibleEquity < 30)
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
			if (annualTurnover < 30000)
			{
				Results.AnnualTurnoverGrade = 0;
			}
			else if (annualTurnover < 100000)
			{
				Results.AnnualTurnoverGrade = 1;
			}
			else if (annualTurnover < 200000)
			{
				Results.AnnualTurnoverGrade = 2;
			}
			else if (annualTurnover < 400000)
			{
				Results.AnnualTurnoverGrade = 3;
			}
			else if (annualTurnover < 800000)
			{
				Results.AnnualTurnoverGrade = 4;
			}
			else if (annualTurnover < 2000000)
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
			if (freeCashFlow < -10)
			{
				Results.FreeCashFlowGrade = 0;
			}
			else if (freeCashFlow < 0)
			{
				Results.FreeCashFlowGrade = 1;
			}
			else if (freeCashFlow < 10)
			{
				Results.FreeCashFlowGrade = 2;
			}
			else if (freeCashFlow < 20)
			{
				Results.FreeCashFlowGrade = 3;
			}
			else if (freeCashFlow < 30)
			{
				Results.FreeCashFlowGrade = 4;
			}
			else if (freeCashFlow < 40)
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
			if (businessScore < 11)
			{
				Results.BusinessScoreGrade = 0;
			}
			else if (businessScore < 21)
			{
				Results.BusinessScoreGrade = 1;
			}
			else if (businessScore < 31)
			{
				Results.BusinessScoreGrade = 2;
			}
			else if (businessScore < 41)
			{
				Results.BusinessScoreGrade = 3;
			}
			else if (businessScore < 51)
			{
				Results.BusinessScoreGrade = 4;
			}
			else if (businessScore < 61)
			{
				Results.BusinessScoreGrade = 5;
			}
			else if (businessScore < 71)
			{
				Results.BusinessScoreGrade = 6;
			}
			else if (businessScore < 81)
			{
				Results.BusinessScoreGrade = 7;
			}
			else if (businessScore < 91)
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

			if (businessScore <= 30)
			{
				Results.BusinessScoreWeight = 41;
				sumIs100 = false;
			}

			if (consumerScore <= 800)
			{
				Results.ConsumerScoreWeight = 14;
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
				Results.NumOfEarlyReaymentsWeight = 2;

				Results.BusinessScoreWeight -= 6.3m;
				Results.BusinessSeniorityWeight -= 1.7m;
				Results.ConsumerScoreWeight -= 2.1m;
			}

			if (!sumIs100)
			{
				decimal sumOfWeights = Results.BusinessScoreWeight + Results.FreeCashFlowWeight + Results.AnnualTurnoverWeight +
				                       Results.TangibleEquityWeight +
				                       Results.BusinessSeniorityWeight + Results.ConsumerScoreWeight + Results.NetWorthWeight +
				                       Results.MaritalStatusWeight +
				                       Results.EzbobSeniorityWeight + Results.NumOfLoansWeight + Results.NumOfLateRepaymentsWeight +
				                       Results.NumOfEarlyReaymentsWeight;

				decimal sumOfNonFixed = Results.TangibleEquityWeight + Results.NetWorthWeight + Results.MaritalStatusWeight;

				decimal sumOfNonFixedDestination = sumOfNonFixed - sumOfWeights + 100;

				decimal ratioForDestionation = sumOfNonFixedDestination/sumOfNonFixed;

				Results.TangibleEquityWeight *= ratioForDestionation;
				Results.NetWorthWeight *= ratioForDestionation;
				Results.MaritalStatusWeight *= ratioForDestionation;
			}
		}
	}
}
