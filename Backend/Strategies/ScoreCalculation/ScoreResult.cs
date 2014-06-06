﻿namespace EzBob.Backend.Strategies.ScoreCalculation
{
	using System;
	using EZBob.DatabaseLib.Model.Database;

	public class ScoreResult
	{
		public ScoreResult()
		{
			BusinessScoreWeight = 30;
			FreeCashFlowWeight = 19;
			AnnualTurnoverWeight = 10;
			TangibleEquityWeight = 8;
			BusinessSeniorityWeight = 8;
			ConsumerScoreWeight = 10;
			NetWorthWeight = 10;
			EzbobSeniorityWeight = 5;
			NumOfLoansWeight = 0;
			NumOfLateRepaymentsWeight = 0;
			NumOfEarlyRepaymentsWeight = 0;
		}

		public int BusinessScore { get; set; }
		public decimal BusinessScoreWeight { get; set; }
		public decimal BusinessScoreGrade { get; set; }
		public decimal BusinessScoreScore { get; set; }

		public decimal FreeCashFlow { get; set; }
		public decimal FreeCashFlowWeight { get; set; }
		public decimal FreeCashFlowGrade { get; set; }
		public decimal FreeCashFlowScore { get; set; }

		public decimal AnnualTurnover { get; set; }
		public decimal AnnualTurnoverWeight { get; set; }
		public decimal AnnualTurnoverGrade { get; set; }
		public decimal AnnualTurnoverScore { get; set; }

		public decimal TangibleEquity { get; set; }
		public decimal TangibleEquityWeight { get; set; }
		public decimal TangibleEquityGrade { get; set; }
		public decimal TangibleEquityScore { get; set; }

		public DateTime BusinessSeniority { get; set; }
		public decimal BusinessSeniorityWeight { get; set; }
		public decimal BusinessSeniorityGrade { get; set; }
		public decimal BusinessSeniorityScore { get; set; }

		public int ConsumerScore { get; set; }
		public decimal ConsumerScoreWeight { get; set; }
		public decimal ConsumerScoreGrade { get; set; }
		public decimal ConsumerScoreScore { get; set; }

		public decimal NetWorth { get; set; }
		public decimal NetWorthWeight { get; set; }
		public decimal NetWorthGrade { get; set; }
		public decimal NetWorthScore { get; set; }

		public MaritalStatus MaritalStatus { get; set; }
		public decimal MaritalStatusWeight { get; set; }
		public decimal MaritalStatusGrade { get; set; }
		public decimal MaritalStatusScore { get; set; }

		public DateTime EzbobSeniority { get; set; }
		public decimal EzbobSeniorityWeight { get; set; }
		public decimal EzbobSeniorityGrade { get; set; }
		public decimal EzbobSeniorityScore { get; set; }

		public int NumOfLoans { get; set; }
		public decimal NumOfLoansWeight { get; set; }
		public decimal NumOfLoansGrade { get; set; }
		public decimal NumOfLoansScore { get; set; }

		public int NumOfLateRepayments { get; set; }
		public decimal NumOfLateRepaymentsWeight { get; set; }
		public decimal NumOfLateRepaymentsGrade { get; set; }
		public decimal NumOfLateRepaymentsScore { get; set; }

		public int NumOfEarlyRepayments { get; set; }
		public decimal NumOfEarlyRepaymentsWeight { get; set; }
		public decimal NumOfEarlyRepaymentsGrade { get; set; }
		public decimal NumOfEarlyRepaymentsScore { get; set; }
		
		public decimal TotalScore { get; set; }
		public decimal TotalScoreNormalized { get; set; }
		public MedalMultiplier Medal { get; set; }
	}
}
