namespace AutomationCalculator
{
	using System.Collections.Generic;

	public static class Constants
	{
		#region AutoRejection

		public static int MinCreditScore = 550;

		public static int MinAnnualTurnover = 10000;

		public static int MinThreeMonthTurnover = 2000;

		public static int DefaultScoreBelow = 800;
		public static int DefaultMinAmount = 300;
		public static int DefaultMinMonths = 12;

		public static int MinMarketPlaceSeniorityDays = 300;
		public static bool AutoRejectIfErrorInAtLeastOneMarketPlace = false;

		public static bool AutoRejectIfWasApprovedOnce = false;

		public static int NoRejectIfTotalAnnualTurnoverAbove = 250000;

		public static int NoRejectIfCreditScoreAbove = 900;

		#endregion AutoRejection

		#region MedalScoreCalculation

		#region Weight

		public static decimal ExperianScoreBaseWeight = 0.4M;
		public static decimal ExperianScore650_750Weight = 0.55M;

		public static decimal MpSeniorityBaseWeight = 0.14M;
		public static decimal MpSeniority_LT2_OR_GT4Weight = 0.18M;

		public static decimal MartialStatusBaseWeight = 0.08M;

		public static decimal PositiveFeedbackBaseWeight = 0.18M;
		public static decimal PositiveFeedback_GT50KWeight = 0.19M;

		public static decimal OtherBaseWeight = 0.06M;

		public static decimal AnualTurnoverBaseWeight = 0.1M;
		public static decimal AnualTurnover_FirstRepaymentWeight = 0.07M;

		public static decimal NumOfStoresBaseWeight = 0.04M;

		public static decimal EzbobSeniorityBaseWeight = 0M;
		public static decimal EzbobSeniority_FirstRepaymentWeight = 0.03M;

		public static decimal EzbobNumOfPreviousOnTimeLoansBaseWeight = 0M;
		public static decimal EzbobNumOfPreviousOnTimeLoans_FirstRepaymentWeight = 0.05M;

		public static decimal EzbobNumOfPreviousLatePaymentsBaseWeight = 0M;
		public static decimal EzbobNumOfPreviousLatePayments_FirstRepaymentWeight = 0.04M;

		public static decimal EzbobNumOfPreviousEarlyPaymentsBaseWeight = 0M;
		public static decimal EzbobNumOfPreviousEarlyPayments_FirstRepaymentWeight = 0.03M;

		#region Weight Min/Max

		public static int ExperianScoreWeightMin = 650;
		public static int ExperianScoreWeightMax = 750;

		public static decimal MpSeniorityWeightMin = 4M;
		public static decimal MpSeniorityWeightMax = 2M;

		#endregion Weight Min/Max

		#region Weight Deduction On First Repayment Reached

		public static decimal ExperianScoreWeightDeduction = 0.08M;
		public static decimal MpSenorityWeightDeduction = 0.04M;
		public static decimal AnnualTurnoverWeightDeduction = 0.03M;

		#endregion Weight Deduction On First Repayment Reached

		#endregion Weight

		#region Grade

		#region Min/Max Grade

		public static int ExperianScoreGradeMin = 1;
		public static int ExperianScoreGradeMax = 5;
		public static int MpSeniorityGradeMin = 2;
		public static int MpSeniorityGradeMax = 4;
		public static int PositiveFeedbackGradeMin = 2;
		public static int PositiveFeedbackGradeMax = 5;
		public static int OtherGradeMin = 2;
		public static int OtherGradeMax = 2;
		public static int AnnualTurnoverGradeMin = 1;
		public static int AnnualTurnoverGradeMax = 5;
		public static int NumOfStoresGradeMin = 1;
		public static int NumOfStoresGradeMax = 5;
		public static int EzbobSeniorityGradeMin = 2;
		public static int EzbobSeniorityGradeMax = 4;
		public static int EzbobNumOfPreviousOnTimeLoansGradeMin = 1;
		public static int EzbobNumOfPreviousOnTimeLoansGradeMax = 4;
		public static int EzbobNumOfPreviousLatePaymentsGradeMin = 0;
		public static int EzbobNumOfPreviousLatePaymentsGradeMax = 5;
		public static int EzbobNumOfPreviousEarlyPaymentsGradeMin = 2;
		public static int EzbobNumOfPreviousEarlyPaymentsGradeMax = 5;

		#endregion Min/Max Grade

		#region Grade Ranges

		public static List<RangeGrage> ExperianRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,MaxValue = 680, Grade = 1},
				new RangeGrage {MinValue = 681,MaxValue = 760, Grade = 1},
				new RangeGrage {MinValue = 761,MaxValue = 840, Grade = 2},
				new RangeGrage {MinValue = 841,MaxValue = 920, Grade = 3},
				new RangeGrage {MinValue = 921,MaxValue = 1000, Grade = 4},
				new RangeGrage {MinValue = 1001,MaxValue = null, Grade = 5},
			};

		public static List<RangeGrage> MpSeniorityRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,MaxValue = 1, Grade = 2},
				new RangeGrage {MinValue = 2,MaxValue = 3, Grade = 3},
				new RangeGrage {MinValue = 4,MaxValue = null, Grade = 4},
			};

		public static List<RangeGrage> PositiveFeedbackRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,MaxValue = 5000, Grade = 2},
				new RangeGrage {MinValue = 5001,MaxValue = 50000, Grade = 3},
				new RangeGrage {MinValue = 50001,MaxValue = null, Grade = 5},
			};
		
		public static List<RangeGrage> AnnualTurnoverRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,MaxValue = 12000, Grade = 1},
				new RangeGrage {MinValue = 12001,MaxValue = 28000, Grade = 4},
				new RangeGrage {MinValue = 28001,MaxValue = 82000, Grade = 5},
				new RangeGrage {MinValue = 82001,MaxValue = 120000, Grade = 3},
				new RangeGrage {MinValue = 120001,MaxValue = null, Grade = 1},
			};

		public static List<RangeGrage> NumOfStoresRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = 1,MaxValue = 2, Grade = 1},
				new RangeGrage {MinValue = 3,MaxValue = 4, Grade = 2},
				new RangeGrage {MinValue = 5,MaxValue = null, Grade = 5},
			};

		public static List<RangeGrage> EzbobSeniorityRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = null,MaxValue = 6, Grade = 2},
				new RangeGrage {MinValue = 6,MaxValue = 18, Grade = 3},
				new RangeGrage {MinValue = 18,MaxValue = null, Grade = 4},
			};

		public static List<RangeGrage> EzbobNumOfLoansRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = 0,MaxValue = 1, Grade = 1},
				new RangeGrage {MinValue = 2,MaxValue = 3, Grade = 3},
				new RangeGrage {MinValue = 4,MaxValue = null, Grade = 4},
			};

		public static List<RangeGrage> EzbobNumOfLateRepaymentsRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = 0,MaxValue = 0, Grade = 5},
				new RangeGrage {MinValue = 1,MaxValue = 1, Grade = 2},
				new RangeGrage {MinValue = 2,MaxValue = null, Grade = 0},
			};

		public static List<RangeGrage> EzbobNumOfEarlyRepaymentsRanges = new List<RangeGrage>
			{
				new RangeGrage {MinValue = 0,MaxValue = 0, Grade = 2},
				new RangeGrage {MinValue = 1,MaxValue = 2, Grade = 3},
				new RangeGrage {MinValue = 3,MaxValue = null, Grade = 5},
			};

		public static int OtherGrade = 2;

		public static int MaritalStatusGrade_Married = 4;
		public static int MaritalStatusGrade_Divorced = 3;
		public static int MaritalStatusGrade_Single = 2;
		public static int MaritalStatusGrade_Widower = 4;

		#endregion Grade Ranges
		
		#endregion Grade
		
		#endregion MedalScoreCalculation
	}
}
