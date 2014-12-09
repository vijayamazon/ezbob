namespace AutomationCalculator.Common
{
	using System.Collections.Generic;

	public static class Constants {

		public static readonly bool ManualyRejected = true;
		public static readonly int ManualDecisionDateRangeDays = 30;
		public static readonly bool NoNewDataSources = true;

		//all new client rules plus
		public static readonly int MinNumOfLoans = 1;
		public static readonly decimal MinRepaidPrincipalPercent = 0.5M;

		public static readonly int NewMinReApproveAmount = 1000;
		public static readonly int MissedPaymentCalendarDays = 5; //no need?
		public static readonly int NewReApprovePeriodDays = 30;

		public static readonly int OldReApprovePeriodDays = 28;
		public static readonly int OldMinReApproveAmount = 500;

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

		public static readonly int ApprovalNewMaxDelaysDaysOnCaisAccount = 60;

		public static readonly int ApprovalOldMaxDelaysDaysOnCaisAccount = 90;
		public static readonly decimal ApprovalOldMaxDecreaseOfTurnoverPercent = 0.15M;
		public static readonly int ApprovalOldMaxLatePaymentsDays = 7;
		public static readonly int ApprovalOldMaxNumOfOutstandingLoans = 1;
		public static readonly decimal ApprovalOldMaxOutstandingPrincipalPercent = 0.5M;

	}

	public static class MedalRangesConstats {

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
				new RangeGrage {MinValue = 1,     MaxValue = null,  Grade = 1},
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
				new RangeGrage {MinValue = null, MaxValue = 1,    Grade = 0},
				new RangeGrage {MinValue = 1,    MaxValue = 6,    Grade = 2},
				new RangeGrage {MinValue = 6,    MaxValue = 18,   Grade = 3},
				new RangeGrage {MinValue = 18,   MaxValue = null, Grade = 4},
			};

		public static readonly List<RangeGrage> NumOfOnTimeLoansRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null, MaxValue = 2,    Grade = 1},
				new RangeGrage {MinValue = 2,    MaxValue = 4,    Grade = 3},
				new RangeGrage {MinValue = 4,    MaxValue = null, Grade = 4},
			};

		public static readonly List<RangeGrage> NumOfLateRepaymentsRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null, MaxValue = 1,    Grade = 5},
				new RangeGrage {MinValue = 1,    MaxValue = 2,    Grade = 2},
				new RangeGrage {MinValue = 2,    MaxValue = null, Grade = 0},
			};

		public static readonly List<RangeGrage> NumOfEarlyRepaymentsRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null, MaxValue = 1,    Grade = 2},
				new RangeGrage {MinValue = 1,    MaxValue = 4,    Grade = 3},
				new RangeGrage {MinValue = 4,    MaxValue = null, Grade = 5},
			};

		public static readonly List<RangeGrage> NumOfStoresRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null, MaxValue = 3,    Grade = 1},
				new RangeGrage {MinValue = 3,    MaxValue = 5,    Grade = 3},
				new RangeGrage {MinValue = 5,    MaxValue = null, Grade = 5},
			};

		public static readonly List<RangeGrage> PositiveFeedbacksRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,  MaxValue = 1,     Grade = 0},
				new RangeGrage {MinValue = 1,     MaxValue = 5001,  Grade = 2},
				new RangeGrage {MinValue = 5001,  MaxValue = 50000, Grade = 3},
				new RangeGrage {MinValue = 50000, MaxValue = null,  Grade = 5},
			};

		public static readonly List<RangeMedal> MedalRanges = new List<RangeMedal>
			{
				new RangeMedal {MinValue = null,MaxValue = 0.4M, Medal = Medal.Silver },
				new RangeMedal {MinValue = 0.4M,MaxValue = 0.62M, Medal = Medal.Gold},
				new RangeMedal {MinValue = 0.62M,MaxValue = 0.84M, Medal = Medal.Platinum},
				new RangeMedal {MinValue = 0.84M,MaxValue = null,Medal = Medal.Diamond},
			};

	}
}
