namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using ScoreCalculation;

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
			MaritalStatusWeight = 5;
			EzbobSeniorityWeight = 0;
			NumOfLoansWeight = 0;
			NumOfLateRepaymentsWeight = 0;
			NumOfEarlyRepaymentsWeight = 0;
			MedalType = "Limited";
		}

		/// <summary>
		/// Constructor for broker instant offer
		/// </summary>
		public ScoreResult(
			int businessScore, 
			decimal freeCashFlow,
			decimal annualTurnover,
			decimal tangibleEquity, 
			DateTime? businessSeniority,
			int consumerScore,
			decimal netWorth ): this() {

			BusinessScore = businessScore;
			FreeCashFlow = freeCashFlow;
			AnnualTurnover = annualTurnover;
			TangibleEquity = tangibleEquity;
			BusinessSeniority = businessSeniority;
			ConsumerScore = consumerScore;
			NetWorth = netWorth;
			
			MaritalStatus = MaritalStatus.Married; //todo?
			EzbobSeniority = DateTime.Today;
			NumOfEarlyRepayments = 0;
			NumOfLateRepayments = 0;
			NumOfLoans = 0;
		}

		#region input 

		public int BusinessScore { get; set; }
		public decimal FreeCashFlow { get; set; }
		public decimal AnnualTurnover { get; set; }
		public decimal TangibleEquity { get; set; }
		public DateTime? BusinessSeniority { get; set; }
		public int ConsumerScore { get; set; }
		public decimal NetWorth { get; set; }
		public MaritalStatus MaritalStatus { get; set; }
		public int NumberOfStores { get; set; }
		public int PositiveFeedbacks { get; set; }
		public DateTime? EzbobSeniority { get; set; }
		public int NumOfLoans { get; set; }
		public int NumOfLateRepayments { get; set; }
		public int NumOfEarlyRepayments { get; set; }

		#endregion input

		public decimal FreeCashFlowValue { get; set; }
		public decimal TangibleEquityValue { get; set; }
		public decimal ValueAdded { get; set; }
		public string InnerFlowName { get; set; }
		public string MedalType { get; set; }

		#region constants / internal

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

		public decimal NumberOfStoresWeight { get; set; }
		public decimal NumberOfStoresGrade { get; set; }
		public decimal NumberOfStoresScore { get; set; }

		public decimal PositiveFeedbacksWeight { get; set; }
		public decimal PositiveFeedbacksGrade { get; set; }
		public decimal PositiveFeedbacksScore { get; set; }
		
		public decimal EzbobSeniorityWeight { get; set; }
		public decimal EzbobSeniorityGrade { get; set; }
		public decimal EzbobSeniorityScore { get; set; }

		public decimal NumOfLoansWeight { get; set; }
		public decimal NumOfLoansGrade { get; set; }
		public decimal NumOfLoansScore { get; set; }

		public decimal NumOfLateRepaymentsWeight { get; set; }
		public decimal NumOfLateRepaymentsGrade { get; set; }
		public decimal NumOfLateRepaymentsScore { get; set; }

		public decimal NumOfEarlyRepaymentsWeight { get; set; }
		public decimal NumOfEarlyRepaymentsGrade { get; set; }
		public decimal NumOfEarlyRepaymentsScore { get; set; }

		#endregion constants / internal

		#region output

		public decimal TotalScore { get; set; }
		public decimal TotalScoreNormalized { get; set; }
		public MedalMultiplier Medal { get; set; }
		public string Error { get; set; }

		#endregion output
	}
}
