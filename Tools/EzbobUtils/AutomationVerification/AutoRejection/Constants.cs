namespace AutomationCalculator
{
	using System.Collections.Generic;

	public static class Constants {
		#region AutoReRejection

		#region New Client Rules

		public static readonly bool ManualyRejected = true;
		public static readonly int ManualDecisionDateRangeDays = 30;
		public static readonly bool NoNewDataSources = true;

		#endregion

		#region Old Client Rules

		//all new client rules plus
		public static readonly int MinNumOfLoans = 1;
		public static readonly decimal MinRepaidPrincipalPercent = 0.5M;

		#endregion

		#endregion

		#region AutoReApproval

		#region New Client

		public static readonly int NewMinReApproveAmount = 1000;
		public static readonly int MissedPaymentCalendarDays = 5; //no need?
		public static readonly int NewReApprovePeriodDays = 30;

		#endregion

		#region Old Client

		public static readonly int OldReApprovePeriodDays = 28;
		public static readonly int OldMinReApproveAmount = 500;

		#endregion

		#endregion

		#region AutoApproval

		public static readonly int ApprovalMinCreditScore = 900;
		public static readonly int ApprovalMinAnnualTurnover = 10000;
		public static readonly int ApprovalMinQuarterTurnover = 2000;
		public static readonly int ApprovalMinLastTurnover = 1000;
		public static readonly int ApprovalMinSeniorityMonths = 12;
		public static readonly int ApprovalMinAge = 22;
		public static readonly int ApprovalMaxAge = 60;

		public static readonly int ApprovalMinAmount = 1000;
		public static readonly int ApprovalMaxAmount = 10000;
		public static readonly int ApprovalMaxOutstandingOffersAmountPerDay = 200000;
		public static readonly int ApprovalMaxIssuedAmountPerDay = 150000;

		#region New Client

		public static readonly int ApprovalNewMaxDelaysDaysOnCaisAccount = 60;

		#endregion

		#region Old Client

		public static readonly int ApprovalOldMaxDelaysDaysOnCaisAccount = 90;
		public static readonly decimal ApprovalOldMaxDecreaseOfTurnoverPercent = 0.15M;
		public static readonly int ApprovalOldMaxLatePaymentsDays = 7;
		public static readonly int ApprovalOldMaxNumOfOutstandingLoans = 1;
		public static readonly decimal ApprovalOldMaxOutstandingPrincipalPercent = 0.5M;

		#endregion

		#endregion
	}

	public static class OfflineLimitedMedalConstants {

		#region MedalScoreCalculation

		#region Weight

		#region Base Weight
		public static readonly decimal BusinessScoreBaseWeight = 0.30M;
		public static readonly decimal FreeCashFlowBaseWeight = 0.19M;
		public static readonly decimal AnnualTurnoverBaseWeight = 0.10M;
		public static readonly decimal TangibleEquityBaseWeight = 0.08M;
		public static readonly decimal BusinessSeniorityBaseWeight = 0.08M;
		public static readonly decimal ConsumerScoreBaseWeight = 0.10M;
		public static readonly decimal NetWorthBaseWeight = 0.10M;
		public static readonly decimal MaritalStatusBaseWeight = 0.05M;
		public static readonly decimal EzbobSeniorityBaseWeight = 0;
		public static readonly decimal NumOfOnTimeLoansBaseWeight = 0;
		public static readonly decimal NumOfLateRepaymentsBaseWeight = 0;
		public static readonly decimal NumOfEarlyRepaymentsBaseWeight = 0;
		#endregion

		#region No HMRC Weight
		public static readonly decimal FreeCashFlowNoHmrcWeight = 0;
		public static readonly decimal AnnualTurnoverNoHmrcWeightChange = 0.07M;
		public static readonly decimal BusinessScoreNoHmrcWeightChange = 0.05M;
		public static readonly decimal ConsumerScoreNoHmrcWeightChange = 0.03M;
		public static readonly decimal BusinessSeniorityNoHmrcWeightChange = 0.04M;
		#endregion

		#region Low Score Weight

		public static readonly int LowBusinessScore = 30;
		public static readonly int LowConsumerScore = 800;

		public static readonly decimal BusinessScoreLowScoreWeight = 0.4125M;
		public static readonly decimal ConsumerScoreLowScoreWeight = 0.1375M;
		
		#endregion

		#region First Repayment Passed Weight
		public static readonly decimal EzbobSeniorityFirstRepaymentWeight = 0.02M;
		public static readonly decimal NumOfOnTimeLoansFirstRepaymentWeight = 0.0333M;
		public static readonly decimal NumOfLateRepaymentsFirstRepaymentWeight = 0.0267M;
		public static readonly decimal NumOfEarlyRepaymentsFirstRepaymentWeight = 0.02M;
		
		public static readonly decimal ConsumerScoreFirstRepaymentWeightChange = -0.0208M;
		public static readonly decimal BusinessScoreFirstRepaymentWeightChange = -0.0625M;
		public static readonly decimal BusinessSeniorityFirstRepaymentWeightChange = -0.0167M;

		#endregion

		#endregion Weight

		#region Grade Ranges

		public static readonly List<RangeGrage> BusinessScoreRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,MaxValue = 11, Grade = 0},
				new RangeGrage {MinValue = 11,MaxValue = 21,   Grade = 1},
				new RangeGrage {MinValue = 21,MaxValue = 31,   Grade = 2},
				new RangeGrage {MinValue = 31,MaxValue = 41,   Grade = 3},
				new RangeGrage {MinValue = 41,MaxValue = 51,   Grade = 4},
				new RangeGrage {MinValue = 51,MaxValue = 61,   Grade = 5},
				new RangeGrage {MinValue = 61,MaxValue = 71,   Grade = 6},
				new RangeGrage {MinValue = 71,MaxValue = 81,   Grade = 7},
				new RangeGrage {MinValue = 81,MaxValue = 91,   Grade = 8},
				new RangeGrage {MinValue = 91,MaxValue = null, Grade = 9},
			};

		public static readonly List<RangeGrage> FreeCashFlowRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,  MaxValue = -0.1M, Grade = 0},
				new RangeGrage {MinValue = -0.1M, MaxValue = 0,     Grade = 1},
				new RangeGrage {MinValue = 0,     MaxValue = 0.1M,  Grade = 2},
				new RangeGrage {MinValue = 0.1M,  MaxValue = 0.2M,  Grade = 3},
				new RangeGrage {MinValue = 0.2M,  MaxValue = 0.3M,  Grade = 4},
				new RangeGrage {MinValue = 0.3M,  MaxValue = 0.4M,  Grade = 5},
				new RangeGrage {MinValue = 0.4M,  MaxValue = null,  Grade = 6},
			};

		public static readonly List<RangeGrage> AnnualTurnoverRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,    MaxValue = 30000,   Grade = 0},
				new RangeGrage {MinValue = 30000,   MaxValue = 100000,  Grade = 1},
				new RangeGrage {MinValue = 100000,  MaxValue = 200000,  Grade = 2},
				new RangeGrage {MinValue = 200000,  MaxValue = 400000,  Grade = 3},
				new RangeGrage {MinValue = 400000,  MaxValue = 800000,  Grade = 4},
				new RangeGrage {MinValue = 800000,  MaxValue = 2000000, Grade = 5},
				new RangeGrage {MinValue = 2000000, MaxValue = null,    Grade = 6},
			};

		public static readonly List<RangeGrage> TangibleEquityRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,   MaxValue = -0.05M, Grade = 0},
				new RangeGrage {MinValue = -0.05M, MaxValue = 0,      Grade = 1},
				new RangeGrage {MinValue = 0,      MaxValue = 0.1M,   Grade = 2},
				new RangeGrage {MinValue = 0.1M,   MaxValue = 0.3M,   Grade = 3},
				new RangeGrage {MinValue = 0.3M,   MaxValue = null,   Grade = 4},
			};

		public static readonly List<RangeGrage> BusinessSeniorityRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null, MaxValue = 1,    Grade = 0},
				new RangeGrage {MinValue = 1,    MaxValue = 3,    Grade = 1},
				new RangeGrage {MinValue = 3,    MaxValue = 5,    Grade = 2},
				new RangeGrage {MinValue = 5,    MaxValue = 10,   Grade = 3},
				new RangeGrage {MinValue = 10,   MaxValue = null, Grade = 4},
			};

		public static readonly List<RangeGrage> ConsumerScoreRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null, MaxValue = 481,  Grade = 0},
				new RangeGrage {MinValue = 481,  MaxValue = 561,  Grade = 1},
				new RangeGrage {MinValue = 561,  MaxValue = 641,  Grade = 2},
				new RangeGrage {MinValue = 641,  MaxValue = 721,  Grade = 3},
				new RangeGrage {MinValue = 721,  MaxValue = 801,  Grade = 4},
				new RangeGrage {MinValue = 801,  MaxValue = 881,  Grade = 5},
				new RangeGrage {MinValue = 881,  MaxValue = 961,  Grade = 6},
				new RangeGrage {MinValue = 961,  MaxValue = 1041, Grade = 7},
				new RangeGrage {MinValue = 1041, MaxValue = null, Grade = 8},
			};

		public static readonly List<RangeGrage> NetWorthRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,  MaxValue = 0.15M, Grade = 0},
				new RangeGrage {MinValue = 0.15M, MaxValue = 0.5M,  Grade = 1},
				new RangeGrage {MinValue = 0.5M,  MaxValue = 1,     Grade = 2},
				new RangeGrage {MinValue = 1,     MaxValue = null,  Grade = 3},
			};

		public static readonly int MaritalStatusGrade_Married = 4;
		public static readonly int MaritalStatusGrade_Widowed = 4;
		public static readonly int MaritalStatusGrade_Divorced = 3;
		public static readonly int MaritalStatusGrade_Single = 2;
		public static readonly int MaritalStatusGrade_LivingTogether = 3;
		public static readonly int MaritalStatusGrade_Separated = 2;
		public static readonly int MaritalStatusGrade_Other = 2;

		public static readonly List<RangeGrage> EzbobSeniorityRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,MaxValue = 1, Grade = 0},
				new RangeGrage {MinValue = 1,MaxValue = 6, Grade = 2},
				new RangeGrage {MinValue = 6,MaxValue = 18, Grade = 3},
				new RangeGrage {MinValue = 18,MaxValue = null, Grade = 4},
			};

		public static readonly List<RangeGrage> NumOfOnTimeLoansRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = 0,MaxValue = 2, Grade = 1},
				new RangeGrage {MinValue = 2,MaxValue = 4, Grade = 3},
				new RangeGrage {MinValue = 4,MaxValue = null, Grade = 4},
			};

		public static readonly List<RangeGrage> NumOfLateRepaymentsRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = 0,MaxValue = 1, Grade = 5},
				new RangeGrage {MinValue = 1,MaxValue = 2, Grade = 2},
				new RangeGrage {MinValue = 2,MaxValue = null, Grade = 0},
			};

		public static readonly List<RangeGrage> NumOfEarlyRepaymentsRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = 0,MaxValue = 1, Grade = 2},
				new RangeGrage {MinValue = 1,MaxValue = 4, Grade = 3},
				new RangeGrage {MinValue = 4,MaxValue = null, Grade = 5},
			};
		
		#endregion Grade Ranges

		#region Medal Range

		public static readonly List<RangeMedal> MedalRanges = new List<RangeMedal>
			{
				new RangeMedal {MinValue = null,MaxValue = 0.4M, Medal = Medal.Silver },
				new RangeMedal {MinValue = 0.4M,MaxValue = 0.62M, Medal = Medal.Gold},
				new RangeMedal {MinValue = 0.62M,MaxValue = 0.84M, Medal = Medal.Platinum},
				new RangeMedal {MinValue = 0.84M,MaxValue = null,Medal = Medal.Diamond},
			};

		#endregion Medal Range

		#endregion MedalScoreCalculation
	}
}
