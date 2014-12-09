using System.Collections.Generic;

namespace Ezbob.Backend.Strategies.ScoreCalculation
{
	public static class Constants
	{

		public static readonly decimal ExperianScoreBaseWeight = 0.4M;
		public static readonly decimal ExperianScore650_750Weight = 0.55M;

		public static readonly decimal MpSeniorityBaseWeight = 0.14M;
		public static readonly decimal MpSeniority_LT2_OR_GT4Weight = 0.18M;

		public static readonly decimal MartialStatusBaseWeight = 0.08M;

		public static readonly decimal PositiveFeedbackBaseWeight = 0.18M;
		public static readonly decimal PositiveFeedback_GT50KWeight = 0.19M;

		public static readonly decimal OtherBaseWeight = 0.06M;

		public static readonly decimal AnualTurnoverBaseWeight = 0.1M;
		public static readonly decimal AnualTurnover_FirstRepaymentWeight = 0.07M;

		public static readonly decimal NumOfStoresBaseWeight = 0.04M;

		public static readonly decimal EzbobSeniorityBaseWeight = 0M;
		public static readonly decimal EzbobSeniority_FirstRepaymentWeight = 0.03M;

		public static readonly decimal EzbobNumOfLoansBaseWeight = 0M;
		public static readonly decimal EzbobNumOfLoans_FirstRepaymentWeight = 0.05M;

		public static readonly decimal EzbobNumOfLateRepaymentsBaseWeight = 0M;
		public static readonly decimal EzbobNumOfLateRepayments_FirstRepaymentWeight = 0.04M;

		public static readonly decimal EzbobNumOfEarlyRepaymentsBaseWeight = 0M;
		public static readonly decimal EzbobNumOfEarlyRepayments_FirstRepaymentWeight = 0.03M;

		public static readonly int ExperianScoreWeightMin = 650;
		public static readonly int ExperianScoreWeightMax = 750;

		public static readonly decimal MpSeniorityWeightMin = 4M;
		public static readonly decimal MpSeniorityWeightMax = 2M;

		public static readonly int PositiveFeedback_GT50K = 50000;

		public static readonly decimal ExperianScoreWeightDeduction = 0.08M;
		public static readonly decimal MpSenorityWeightDeduction = 0.04M;
		public static readonly decimal AnnualTurnoverWeightDeduction = 0.03M;

		public static readonly int ExperianScoreGradeMin = 1;
		public static readonly int ExperianScoreGradeMax = 5;
		public static readonly int MpSeniorityGradeMin = 2;
		public static readonly int MpSeniorityGradeMax = 4;
		public static readonly int PositiveFeedbackGradeMin = 2;
		public static readonly int PositiveFeedbackGradeMax = 5;
		public static readonly int OtherGradeMin = 2;
		public static readonly int OtherGradeMax = 2;
		public static readonly int AnnualTurnoverGradeMin = 1;
		public static readonly int AnnualTurnoverGradeMax = 5;
		public static readonly int NumOfStoresGradeMin = 1;
		public static readonly int NumOfStoresGradeMax = 5;
		public static readonly int EzbobSeniorityGradeMin = 2;
		public static readonly int EzbobSeniorityGradeMax = 4;
		public static readonly int EzbobNumOfLoansGradeMin = 1;
		public static readonly int EzbobNumOfLoansGradeMax = 4;
		public static readonly int EzbobNumOfLateRepaymentsGradeMin = 0;
		public static readonly int EzbobNumOfLateRepaymentsGradeMax = 5;
		public static readonly int EzbobNumOfEarlyRepaymentsGradeMin = 2;
		public static readonly int EzbobNumOfEarlyRepaymentsGradeMax = 5;

		public static readonly List<RangeGrage> ExperianRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = null,MaxValue = 680, Grade = 1},
			new RangeGrage {MinValue = 681,MaxValue = 760, Grade = 1},
			new RangeGrage {MinValue = 761,MaxValue = 840, Grade = 2},
			new RangeGrage {MinValue = 841,MaxValue = 920, Grade = 3},
			new RangeGrage {MinValue = 921,MaxValue = 1000, Grade = 4},
			new RangeGrage {MinValue = 1001,MaxValue = null, Grade = 5},
		};

		public static readonly List<RangeGrage> MpSeniorityRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = null,MaxValue = 1, Grade = 2},
			new RangeGrage {MinValue = 1,MaxValue = 4, Grade = 3},
			new RangeGrage {MinValue = 4,MaxValue = null, Grade = 4},
		};

		public static readonly List<RangeGrage> PositiveFeedbackRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = null,MaxValue = 5000, Grade = 2},
			new RangeGrage {MinValue = 5001,MaxValue = 50000, Grade = 3},
			new RangeGrage {MinValue = 50001,MaxValue = null, Grade = 5},
		};

		public static readonly List<RangeGrage> AnnualTurnoverRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = null,MaxValue = 12000, Grade = 1},
			new RangeGrage {MinValue = 12001,MaxValue = 28000, Grade = 4},
			new RangeGrage {MinValue = 28001,MaxValue = 82000, Grade = 5},
			new RangeGrage {MinValue = 82001,MaxValue = 120000, Grade = 3},
			new RangeGrage {MinValue = 120001,MaxValue = null, Grade = 1},
		};

		public static readonly List<RangeGrage> NumOfStoresRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = 1,MaxValue = 2, Grade = 1},
			new RangeGrage {MinValue = 3,MaxValue = 4, Grade = 3},
			new RangeGrage {MinValue = 5,MaxValue = null, Grade = 5},
		};

		public static readonly List<RangeGrage> EzbobSeniorityRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = null,MaxValue = 6, Grade = 2},
			new RangeGrage {MinValue = 6,MaxValue = 18, Grade = 3},
			new RangeGrage {MinValue = 18,MaxValue = null, Grade = 4},
		};

		public static readonly List<RangeGrage> EzbobNumOfLoansRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = 0,MaxValue = 1, Grade = 1},
			new RangeGrage {MinValue = 2,MaxValue = 3, Grade = 3},
			new RangeGrage {MinValue = 4,MaxValue = null, Grade = 4},
		};

		public static readonly List<RangeGrage> EzbobNumOfLateRepaymentsRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = 0,MaxValue = 0, Grade = 5},
			new RangeGrage {MinValue = 1,MaxValue = 1, Grade = 2},
			new RangeGrage {MinValue = 2,MaxValue = null, Grade = 0},
		};

		public static readonly List<RangeGrage> EzbobNumOfEarlyRepaymentsRanges = new List<RangeGrage> {
			new RangeGrage {MinValue = 0,MaxValue = 0, Grade = 2},
			new RangeGrage {MinValue = 1,MaxValue = 2, Grade = 3},
			new RangeGrage {MinValue = 3,MaxValue = null, Grade = 5},
		};

		public static readonly int OtherGrade = 2;

		public static readonly int MaritalStatusGrade_Married = 4;
		public static readonly int MaritalStatusGrade_Divorced = 3;
		public static readonly int MaritalStatusGrade_Single = 2;
		public static readonly int MaritalStatusGrade_Widowed = 4;
		public static readonly int MaritalStatusGrade_LivingTogether = 4;
		public static readonly int MaritalStatusGrade_Separated = 3;

		public static readonly List<RangeMedal> MedalRanges = new List<RangeMedal> {
			new RangeMedal {MinValue = null,MaxValue = 0.4M, Medal = MedalMultiplier.Silver },
			new RangeMedal {MinValue = 0.4M,MaxValue = 0.62M, Medal = MedalMultiplier.Gold},
			new RangeMedal {MinValue = 0.62M,MaxValue = 0.84M, Medal = MedalMultiplier.Platinum},
			new RangeMedal {MinValue = 0.84M,MaxValue = null,Medal = MedalMultiplier.Diamond},
		};

	} // class Constants
} // namespace
