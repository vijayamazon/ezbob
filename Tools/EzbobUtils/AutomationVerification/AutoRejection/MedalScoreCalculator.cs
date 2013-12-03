namespace AutomationCalculator
{
	using System.Collections.Generic;

	

	public class MedalScoreCalculator
	{
		public ScoreMedal CalculateMedalScore(
			decimal annualTurnover,
			int experianScore,
			decimal mpSeniorityYears,
			decimal positiveFeedbackCount,
			MaritalStatus maritalStatus,
			Gender gender,
			int numberOfStores,
			bool firstRepaymentDatePassed,
			decimal ezbobSeniority,
			int ezbobNumOfLoans,
			int ezbobNumOfLateRepayments,
			int ezbobNumOfEarlyReayments,
			int customerId)
		{

			var dict = new Dictionary<Parameter, Weight>
				{
					{Parameter.ExperianScore,            GetExperianScoreWeight(experianScore, firstRepaymentDatePassed)},
					{Parameter.MpSeniority,              GetMpSeniorityWeight(mpSeniorityYears, firstRepaymentDatePassed)},
					{Parameter.MaritalStatus,            GetMaritalStatusWeight(maritalStatus, firstRepaymentDatePassed)},
					{Parameter.PositiveFeedback,         GetPositiveFeedbackWeight(positiveFeedbackCount, firstRepaymentDatePassed)},
					{Parameter.Other,                    GetOtherWeight(gender, firstRepaymentDatePassed)},
					{Parameter.AnnualTurnover,           GetAnnualTurnoverWeight(experianScore, firstRepaymentDatePassed)},
					{Parameter.NumOfStores,              GetNumOfStoresWeight(numberOfStores, firstRepaymentDatePassed)},
					{Parameter.EzbobSeniority,           GetEzbobSeniorityWeight(ezbobSeniority, firstRepaymentDatePassed)},
					{Parameter.EzbobNumOfLoans,          GetEzbobNumOfLoansWeight(ezbobNumOfLoans, firstRepaymentDatePassed)},
					{Parameter.EzbobNumOfLateRepayments, GetEzbobNumOfLateRepaymentsWeight(ezbobNumOfLateRepayments, firstRepaymentDatePassed)},
					{Parameter.EzbobNumOfEarlyRepayments,GetEzbobNumOfEarlyRepaymentsWeight(ezbobNumOfEarlyReayments, firstRepaymentDatePassed)}
				};


			return new ScoreMedal();
		}

		private Weight GetEzbobNumOfEarlyRepaymentsWeight(int ezbobNumOfEarlyReayments, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetEzbobNumOfLateRepaymentsWeight(int ezbobNumOfLateRepayments, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetEzbobNumOfLoansWeight(int ezbobNumOfLoans, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetEzbobSeniorityWeight(decimal ezbobSeniority, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetNumOfStoresWeight(int numberOfStores, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetAnnualTurnoverWeight(int experianScore, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetOtherWeight(Gender gender, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetPositiveFeedbackWeight(decimal positiveFeedbackCount, bool firstRepaymentDatePassed)
		{
			throw new System.NotImplementedException();
		}

		private Weight GetMaritalStatusWeight(MaritalStatus maritalStatus, bool firstRepaymentDatePassed)
		{
			var maritalStatusWeight = new Weight
			{
				FinalWeightFixedWeightParameter = 0,
				StandardWeightFixedWeightParameter = 0,
				StandardWeightAdjustableWeightParameter = Constants.MartialStatusBaseWeight,
				DeltaForAdjustableWeightParameter = 0, //todo calc
				MinimumGrade = Constants.MpSeniorityGradeMin,
				MaximumGrade = Constants.MpSeniorityGradeMax
			};

			maritalStatusWeight.FinalWeight = maritalStatusWeight.StandardWeightAdjustableWeightParameter - maritalStatusWeight.DeltaForAdjustableWeightParameter;//todo
			maritalStatusWeight.MinimumScore = maritalStatusWeight.FinalWeight * maritalStatusWeight.MinimumGrade;//todo
			maritalStatusWeight.MaximumScore = maritalStatusWeight.FinalWeight * maritalStatusWeight.MaximumGrade;//todo

			switch (maritalStatus)
			{
				case MaritalStatus.Married:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Married;
					break;
				case MaritalStatus.Divorced:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Divorced;
					break;
				case MaritalStatus.Single:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Single;
					break;
				case MaritalStatus.Widower:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Widower;
					break;
				//todo Other???
			}

			return maritalStatusWeight;
		}

		private Weight GetMpSeniorityWeight(decimal mpSeniority, bool firstRepaymentDatePassed)
		{
			var mpSeniorityWeight = new Weight
				{
					FinalWeightFixedWeightParameter = Constants.MpSeniorityBaseWeight,
					StandardWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.MpSeniorityBaseWeight - Constants.MpSenorityWeightDeduction : Constants.MpSeniorityBaseWeight,
					StandardWeightAdjustableWeightParameter = 0,
					DeltaForAdjustableWeightParameter = 0,
					MinimumGrade = Constants.MpSeniorityGradeMin,
					MaximumGrade = Constants.MpSeniorityGradeMax
				};

			if (mpSeniority < Constants.MpSeniorityWeightMax || mpSeniority > Constants.MpSeniorityWeightMin)
			{
				mpSeniorityWeight.FinalWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.MpSeniority_LT2_OR_GT4Weight - Constants.MpSenorityWeightDeduction : Constants.MpSeniority_LT2_OR_GT4Weight;
			}

			mpSeniorityWeight.FinalWeight = mpSeniorityWeight.FinalWeightFixedWeightParameter;

			mpSeniorityWeight.MinimumScore = mpSeniorityWeight.FinalWeight * mpSeniorityWeight.MinimumGrade;
			mpSeniorityWeight.MaximumScore = mpSeniorityWeight.FinalWeight * mpSeniorityWeight.MaximumGrade;

			mpSeniorityWeight.Grade = GetGrade(Constants.MpSeniorityRanges, mpSeniority);

			return mpSeniorityWeight;
		}

		private Weight GetExperianScoreWeight(int experianScore, bool firstRepaymentDatePassed)
		{
			var experianWeight = new Weight
				{
					FinalWeightFixedWeightParameter = Constants.ExperianScoreBaseWeight,
					StandardWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.ExperianScoreBaseWeight = Constants.ExperianScoreWeightDeduction : Constants.ExperianScoreBaseWeight,
					StandardWeightAdjustableWeightParameter = 0,
					DeltaForAdjustableWeightParameter = 0,
					MinimumGrade = Constants.ExperianScoreGradeMin,
					MaximumGrade = Constants.ExperianScoreGradeMax,
				};
			if (experianScore >= Constants.ExperianScoreWeightMin && experianScore <= Constants.ExperianScoreWeightMax)
			{
				experianWeight.FinalWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.ExperianScore650_750Weight - Constants.ExperianScoreWeightDeduction : Constants.ExperianScore650_750Weight;
			}

			experianWeight.FinalWeight = experianWeight.FinalWeightFixedWeightParameter;
			experianWeight.MinimumScore = experianWeight.FinalWeight * experianWeight.MinimumGrade;
			experianWeight.MaximumScore = experianWeight.FinalWeight * experianWeight.MaximumGrade;

			experianWeight.Grade = GetGrade(Constants.ExperianRanges, experianScore);

			return experianWeight;
		}

		int GetGrade(IEnumerable<RangeGrage> rangeGrages, decimal value)
		{
			foreach (var rangeGrage in rangeGrages)
			{
				if (rangeGrage.IsInRange(value))
				{
					return rangeGrage.Grade;
				}
			}
			return 0;
		}
	}

}
