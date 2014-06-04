namespace EzBob.Backend.Strategies.ScoreCalculation
{
	using Ezbob.Database;
	using Ezbob.Logger;
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
			NumOfEarlyReaymentsWeight = 0;
		}

		public decimal BusinessScoreWeight { get; set; }
		public decimal BusinessScoreGrade { get; set; }
		public decimal BusinessScoreScore { get; set; }

		public decimal FreeCashFlowWeight { get; set; }
		public decimal FreeCashFlowGrade { get; set; }
		public decimal FreeCashFlowScore { get; set; }

		public decimal AnnualTurnoverWeight { get; set; }
		public decimal AnnualTurnoverGrade { get; set; }
		public decimal AnnualTurnoverScore { get; set; }

		public decimal TangibleEquityWeight { get; set; }
		public decimal TangibleEquityGrade { get; set; }
		public decimal TangibleEquityScore { get; set; }

		public decimal BusinessSeniorityWeight { get; set; }
		public decimal BusinessSeniorityGrade { get; set; }
		public decimal BusinessSeniorityScore { get; set; }
		
		public decimal ConsumerScoreWeight { get; set; }
		public decimal ConsumerScoreGrade { get; set; }
		public decimal ConsumerScoreScore { get; set; }

		public decimal NetWorthWeight { get; set; }
		public decimal NetWorthGrade { get; set; }
		public decimal NetWorthScore { get; set; }

		public decimal MaritalStatusWeight { get; set; }
		public decimal MaritalStatusGrade { get; set; }
		public decimal MaritalStatusScore { get; set; }

		public decimal EzbobSeniorityWeight { get; set; }
		public decimal EzbobSeniorityGrade { get; set; }
		public decimal EzbobSeniorityScore { get; set; }

		public decimal NumOfLoansWeight { get; set; }
		public decimal NumOfLoansGrade { get; set; }
		public decimal NumOfLoansScore { get; set; }

		public decimal NumOfLateRepaymentsWeight { get; set; }
		public decimal NumOfLateRepaymentsGrade { get; set; }
		public decimal NumOfLateRepaymentsScore { get; set; }

		public decimal NumOfEarlyReaymentsWeight { get; set; }
		public decimal NumOfEarlyReaymentsGrade { get; set; }
		public decimal NumOfEarlyReaymentsScore { get; set; }

		public decimal TotalScore { get; set; }
		public decimal TotalScoreNormalized { get; set; }
		public MedalMultiplier Medal { get; set; }
	}
}
