namespace Ezbob.Backend.Strategies.ScoreCalculation
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using EZBob.DatabaseLib.Model.Database;

	public class MedalScoreCalculator
	{
		private readonly ASafeLog m_oLog;
		private readonly AConnection db;

		public MedalScoreCalculator(AConnection db, ASafeLog oLog)
		{
			m_oLog = oLog;
			this.db = db;
		}

		public ScoreMedalOffer CalculateMedalScore(
			decimal annualTurnover,
			int experianScore,
			decimal mpSeniorityYears,
			decimal positiveFeedbackCount,
			MaritalStatus maritalStatus,
			Gender gender,
			int numberOfStores,
			bool firstRepaymentDatePassed,
			decimal ezbobSeniorityMonths,
			int ezbobNumOfLoans,
			int ezbobNumOfLateRepayments,
			int ezbobNumOfEarlyReayments)
		{
			m_oLog.Debug(
				"CalculateMedalScore input params: annualTurnover: {0}, experianScore: {1}, mpSeniorityYears: {2}, positiveFeedbackCount: {3}, maritalStatus: {4}, gender: {5}, numberOfStores(eBay\\Amazon\\PayPal): {6}, firstRepaymentDatePassed: {7}, ezbobSeniorityMonths: {8}, ezbobNumOfLoans: {9}, ezbobNumOfLateRepayments: {10}, ezbobNumOfEarlyReayments: {11}",
				annualTurnover, experianScore, mpSeniorityYears, positiveFeedbackCount, maritalStatus, gender, numberOfStores,
				firstRepaymentDatePassed, ezbobSeniorityMonths, ezbobNumOfLoans, ezbobNumOfLateRepayments, ezbobNumOfEarlyReayments);

			var dict = new Dictionary<Parameter, Weight> {
				{Parameter.ExperianScore,            GetExperianScoreWeight(experianScore, firstRepaymentDatePassed)},
				{Parameter.MpSeniority,              GetMpSeniorityWeight(mpSeniorityYears, firstRepaymentDatePassed)},
				{Parameter.MaritalStatus,            GetMaritalStatusWeight(maritalStatus, firstRepaymentDatePassed)},
				{Parameter.PositiveFeedback,         GetPositiveFeedbackWeight(positiveFeedbackCount, firstRepaymentDatePassed)},
				{Parameter.Other,                    GetOtherWeight(gender, firstRepaymentDatePassed)},
				{Parameter.AnnualTurnover,           GetAnnualTurnoverWeight(annualTurnover, firstRepaymentDatePassed)},
				{Parameter.NumOfStores,              GetNumOfStoresWeight(numberOfStores, firstRepaymentDatePassed)},
				{Parameter.EzbobSeniority,           GetEzbobSeniorityWeight(ezbobSeniorityMonths, firstRepaymentDatePassed)},
				{Parameter.EzbobNumOfLoans,          GetEzbobNumOfLoansWeight(ezbobNumOfLoans, firstRepaymentDatePassed)},
				{Parameter.EzbobNumOfLateRepayments, GetEzbobNumOfLateRepaymentsWeight(ezbobNumOfLateRepayments, firstRepaymentDatePassed)},
				{Parameter.EzbobNumOfEarlyRepayments,GetEzbobNumOfEarlyRepaymentsWeight(ezbobNumOfEarlyReayments, firstRepaymentDatePassed)}
			};

			CalcDelta(dict);
			ScoreMedalOffer scoreMedal = CalcScoreMedalOffer(dict, annualTurnover, experianScore);

			return scoreMedal;
		} // constructor

		private ScoreMedalOffer CalcScoreMedalOffer(Dictionary<Parameter, Weight> dict, decimal annualTurnover, int experianScore)
		{
			decimal minScoreSum = 0M;
			decimal maxScoreSum = 0M;
			decimal scoreSum = 0M;
			foreach (var weight in dict.Values)
			{
				weight.Score = weight.Grade * weight.FinalWeight;
				minScoreSum += weight.MinimumScore;
				maxScoreSum += weight.MaximumScore;
				scoreSum += weight.Score;
			}

			decimal score = (scoreSum - minScoreSum) / (maxScoreSum - minScoreSum);
			MedalMultiplier medal = GetMedal(Constants.MedalRanges, score);
			var smo = new ScoreMedalOffer
				{
					Medal = medal,
					ScorePoints = scoreSum * 100,
					ScoreResult = score,
					MaxOffer = (int)Math.Round((int)((int)medal * annualTurnover * GetLoanOfferMultiplier(experianScore) / 100) / 100d, 0, MidpointRounding.AwayFromZero) * 100,
					MaxOfferPercent = GetBasicInterestRate(experianScore),
					AcDescriptors = "Experian score;Marketplace seniority;Marital status;Positive feedback count;Other;Annual turnover;Number of stores;EZBOB seniority;EZBOB number of loans;EZBOB previous late payments;EZBOB previous early payments",
					AcParameters = BuildParameters(dict),
					ResultWeigts = BuildWeights(dict),
					ResultMaxPoints = BuildMaxPoints(dict)
				};

			PrintDict(smo, dict);
			return smo;
		}

		public decimal GetBasicInterestRate(int experianScore)
		{
			SafeReader sr = db.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "BasicInterestRate"), new QueryParameter("Key", experianScore));
			return sr["Value"];
		}

		public decimal GetLoanOfferMultiplier(int experianScore)
		{
			SafeReader sr = db.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "LoanOfferMultiplier"), new QueryParameter("Key", experianScore));
			return sr["Value"];
		}

		private string BuildMaxPoints(Dictionary<Parameter, Weight> dict)
		{
			var sb = new StringBuilder();
			foreach (var pair in dict)
			{
				sb.Append(pair.Value.MaximumScore * 100);
				sb.Append("; ");
			}
			return sb.ToString();
		}

		private string BuildWeights(Dictionary<Parameter, Weight> dict)
		{
			var sb = new StringBuilder();
			foreach (var pair in dict)
			{
				sb.Append(pair.Value.FinalWeight * 100);
				sb.Append("; ");
			}
			return sb.ToString();
		}

		private string BuildParameters(Dictionary<Parameter, Weight> dict)
		{
			return
				string.Format(
					"ExperianScore ({0});MarketplaceSeniority ({1});MaritalStatus ({2});PositiveFeedbackCount ({3});Gender ({4});AnnualTurnover ({5});NumberOfStores ({6});EZBOBSeniority ({7});EZBOBNumberOfLoans ({8});EZBOBPrevLatePayments ({9});EZBOBPrevEarlyPayments ({10})",
					dict[Parameter.ExperianScore].Score * 100,
					dict[Parameter.MpSeniority].Score * 100,
					dict[Parameter.MaritalStatus].Score * 100,
					dict[Parameter.PositiveFeedback].Score * 100,
					dict[Parameter.Other].Score * 100,
					dict[Parameter.AnnualTurnover].Score * 100,
					dict[Parameter.NumOfStores].Score * 100,
					dict[Parameter.EzbobSeniority].Score * 100,
					dict[Parameter.EzbobNumOfLoans].Score * 100,
					dict[Parameter.EzbobNumOfLateRepayments].Score * 100,
					dict[Parameter.EzbobNumOfEarlyRepayments].Score * 100);
		}

		private void CalcDelta(Dictionary<Parameter, Weight> dict)
		{
			decimal finalWeightsFixedWeightParameterSum = 0M;
			decimal standardWeightsFixedWeightParameterSum = 0M;
			decimal standardWeightsAdjustableWeightParameterSum = 0M;

			foreach (var weight in dict.Values)
			{
				finalWeightsFixedWeightParameterSum += weight.FinalWeightFixedWeightParameter;
				standardWeightsFixedWeightParameterSum += weight.StandardWeightFixedWeightParameter;
				standardWeightsAdjustableWeightParameterSum += weight.StandardWeightAdjustableWeightParameter;
			}

			CalcDeltaPerParameter(dict[Parameter.MaritalStatus], finalWeightsFixedWeightParameterSum,
								  standardWeightsFixedWeightParameterSum, standardWeightsAdjustableWeightParameterSum);
			CalcDeltaPerParameter(dict[Parameter.Other], finalWeightsFixedWeightParameterSum,
								  standardWeightsFixedWeightParameterSum, standardWeightsAdjustableWeightParameterSum);
			CalcDeltaPerParameter(dict[Parameter.AnnualTurnover], finalWeightsFixedWeightParameterSum,
								  standardWeightsFixedWeightParameterSum, standardWeightsAdjustableWeightParameterSum);
			CalcDeltaPerParameter(dict[Parameter.NumOfStores], finalWeightsFixedWeightParameterSum,
								  standardWeightsFixedWeightParameterSum, standardWeightsAdjustableWeightParameterSum);

		}

		private void CalcDeltaPerParameter(Weight weight,
			decimal finalWeightsFixedWeightParameterSum,
			decimal standardWeightsFixedWeightParameterSum,
			decimal standardWeightsAdjustableWeightParameterSum)
		{
			weight.DeltaForAdjustableWeightParameter = (weight.StandardWeightAdjustableWeightParameter / standardWeightsAdjustableWeightParameterSum) *
													   (finalWeightsFixedWeightParameterSum - standardWeightsFixedWeightParameterSum);

			weight.FinalWeight = weight.StandardWeightAdjustableWeightParameter - weight.DeltaForAdjustableWeightParameter;
			weight.MinimumScore = weight.FinalWeight * weight.MinimumGrade;
			weight.MaximumScore = weight.FinalWeight * weight.MaximumGrade;
		}

		private Weight GetEzbobNumOfEarlyRepaymentsWeight(int ezbobNumOfEarlyReayments, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, Constants.EzbobNumOfEarlyRepaymentsBaseWeight,
						   Constants.EzbobNumOfEarlyRepayments_FirstRepaymentWeight, Constants.EzbobNumOfEarlyRepaymentsGradeMin,
						   Constants.EzbobNumOfEarlyRepaymentsGradeMax, Constants.EzbobNumOfEarlyRepaymentsRanges, ezbobNumOfEarlyReayments);
		}

		private Weight GetEzbobNumOfLateRepaymentsWeight(int ezbobNumOfLateRepayments, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, Constants.EzbobNumOfLateRepaymentsBaseWeight,
						   Constants.EzbobNumOfLateRepayments_FirstRepaymentWeight, Constants.EzbobNumOfLateRepaymentsGradeMin,
						   Constants.EzbobNumOfLateRepaymentsGradeMax, Constants.EzbobNumOfLateRepaymentsRanges, ezbobNumOfLateRepayments);
		}

		private Weight GetEzbobNumOfLoansWeight(int ezbobNumOfLoans, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, Constants.EzbobNumOfLoansBaseWeight,
						   Constants.EzbobNumOfLoans_FirstRepaymentWeight, Constants.EzbobNumOfLoansGradeMin,
						   Constants.EzbobNumOfLoansGradeMax, Constants.EzbobNumOfLoansRanges, ezbobNumOfLoans);
		}

		private Weight GetEzbobSeniorityWeight(decimal ezbobSeniority, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, Constants.EzbobSeniorityBaseWeight,
						   Constants.EzbobSeniority_FirstRepaymentWeight, Constants.EzbobSeniorityGradeMin,
						   Constants.EzbobSeniorityGradeMax, Constants.EzbobSeniorityRanges, ezbobSeniority);
		}

		private Weight GetNumOfStoresWeight(int numberOfStores, bool firstRepaymentDatePassed)
		{
			var numOfStoresWeight = new Weight
			{
				FinalWeightFixedWeightParameter = 0,
				StandardWeightFixedWeightParameter = 0,
				StandardWeightAdjustableWeightParameter = Constants.NumOfStoresBaseWeight,
				MinimumGrade = Constants.NumOfStoresGradeMin,
				MaximumGrade = Constants.NumOfStoresGradeMax,
				Grade = GetGrade(Constants.NumOfStoresRanges, numberOfStores),
			};

			return numOfStoresWeight;
		}

		private Weight GetAnnualTurnoverWeight(decimal annualTurnover, bool firstRepaymentDatePassed)
		{
			var annualTurnoverWeight = new Weight
				{
					FinalWeightFixedWeightParameter = 0,
					StandardWeightFixedWeightParameter = 0,
					StandardWeightAdjustableWeightParameter =
						firstRepaymentDatePassed
							? Constants.AnualTurnoverBaseWeight - Constants.AnnualTurnoverWeightDeduction
							: Constants.AnualTurnoverBaseWeight,
					MinimumGrade = Constants.AnnualTurnoverGradeMin,
					MaximumGrade = Constants.AnnualTurnoverGradeMax,
					Grade = GetGrade(Constants.AnnualTurnoverRanges, annualTurnover),
				};

			return annualTurnoverWeight;
		}

		private Weight GetOtherWeight(Gender gender, bool firstRepaymentDatePassed)
		{
			var otherWeight = new Weight
			{
				FinalWeightFixedWeightParameter = 0,
				StandardWeightFixedWeightParameter = 0,
				StandardWeightAdjustableWeightParameter = Constants.OtherBaseWeight,

				MinimumGrade = Constants.OtherGradeMin,
				MaximumGrade = Constants.OtherGradeMax,
				Grade = Constants.OtherGrade
			};

			return otherWeight;
		}

		private Weight GetPositiveFeedbackWeight(decimal positiveFeedbackCount, bool firstRepaymentDatePassed)
		{
			var postitiveFeedbackWeight = new Weight
			{
				FinalWeightFixedWeightParameter = positiveFeedbackCount > Constants.PositiveFeedback_GT50K ? Constants.PositiveFeedback_GT50KWeight : Constants.PositiveFeedbackBaseWeight,
				StandardWeightFixedWeightParameter = Constants.PositiveFeedbackBaseWeight,
				StandardWeightAdjustableWeightParameter = 0,
				DeltaForAdjustableWeightParameter = 0,
				MinimumGrade = Constants.PositiveFeedbackGradeMin,
				MaximumGrade = Constants.PositiveFeedbackGradeMax,
				Grade = GetGrade(Constants.PositiveFeedbackRanges, positiveFeedbackCount)
			};

			postitiveFeedbackWeight.FinalWeight = postitiveFeedbackWeight.FinalWeightFixedWeightParameter;

			postitiveFeedbackWeight.MinimumScore = postitiveFeedbackWeight.FinalWeight * postitiveFeedbackWeight.MinimumGrade;
			postitiveFeedbackWeight.MaximumScore = postitiveFeedbackWeight.FinalWeight * postitiveFeedbackWeight.MaximumGrade;

			return postitiveFeedbackWeight;
		}

		private Weight GetMaritalStatusWeight(MaritalStatus maritalStatus, bool firstRepaymentDatePassed)
		{
			var maritalStatusWeight = new Weight
			{
				FinalWeightFixedWeightParameter = 0,
				StandardWeightFixedWeightParameter = 0,
				StandardWeightAdjustableWeightParameter = Constants.MartialStatusBaseWeight,
				MinimumGrade = Constants.MpSeniorityGradeMin,
				MaximumGrade = Constants.MpSeniorityGradeMax
			};

			switch (maritalStatus)
			{
				case MaritalStatus.Married:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Married;
					break;
				case MaritalStatus.Divorced:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Divorced;
					break;
				case MaritalStatus.Single:
				case MaritalStatus.Other:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Single;
					break;
				case MaritalStatus.Widowed:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Widowed;
					break;
				case MaritalStatus.LivingTogether:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_LivingTogether;
					break;
				case MaritalStatus.Separated:
					maritalStatusWeight.Grade = Constants.MaritalStatusGrade_Separated;
					break;
			}

			return maritalStatusWeight;
		}

		private Weight GetMpSeniorityWeight(decimal mpSeniority, bool firstRepaymentDatePassed)
		{
			var mpSeniorityWeight = new Weight
				{
					FinalWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.MpSeniorityBaseWeight - Constants.MpSenorityWeightDeduction : Constants.MpSeniorityBaseWeight,
					StandardWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.MpSeniorityBaseWeight - Constants.MpSenorityWeightDeduction : Constants.MpSeniorityBaseWeight,
					StandardWeightAdjustableWeightParameter = 0,
					DeltaForAdjustableWeightParameter = 0,
					MinimumGrade = Constants.MpSeniorityGradeMin,
					MaximumGrade = Constants.MpSeniorityGradeMax,
					Grade = GetGrade(Constants.MpSeniorityRanges, mpSeniority)
				};

			if (mpSeniority < Constants.MpSeniorityWeightMax || mpSeniority > Constants.MpSeniorityWeightMin)
			{
				mpSeniorityWeight.FinalWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.MpSeniority_LT2_OR_GT4Weight - Constants.MpSenorityWeightDeduction : Constants.MpSeniority_LT2_OR_GT4Weight;
			}

			mpSeniorityWeight.FinalWeight = mpSeniorityWeight.FinalWeightFixedWeightParameter;

			mpSeniorityWeight.MinimumScore = mpSeniorityWeight.FinalWeight * mpSeniorityWeight.MinimumGrade;
			mpSeniorityWeight.MaximumScore = mpSeniorityWeight.FinalWeight * mpSeniorityWeight.MaximumGrade;

			return mpSeniorityWeight;
		}

		private Weight GetExperianScoreWeight(int experianScore, bool firstRepaymentDatePassed)
		{
			var experianWeight = new Weight
				{
					FinalWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.ExperianScoreBaseWeight - Constants.ExperianScoreWeightDeduction : Constants.ExperianScoreBaseWeight,
					StandardWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.ExperianScoreBaseWeight - Constants.ExperianScoreWeightDeduction : Constants.ExperianScoreBaseWeight,
					StandardWeightAdjustableWeightParameter = 0,
					DeltaForAdjustableWeightParameter = 0,
					MinimumGrade = Constants.ExperianScoreGradeMin,
					MaximumGrade = Constants.ExperianScoreGradeMax,
					Grade = GetGrade(Constants.ExperianRanges, experianScore)
				};
			if (experianScore >= Constants.ExperianScoreWeightMin && experianScore <= Constants.ExperianScoreWeightMax)
			{
				experianWeight.FinalWeightFixedWeightParameter = firstRepaymentDatePassed ? Constants.ExperianScore650_750Weight - Constants.ExperianScoreWeightDeduction : Constants.ExperianScore650_750Weight;
			}

			experianWeight.FinalWeight = experianWeight.FinalWeightFixedWeightParameter;
			experianWeight.MinimumScore = experianWeight.FinalWeight * experianWeight.MinimumGrade;
			experianWeight.MaximumScore = experianWeight.FinalWeight * experianWeight.MaximumGrade;

			return experianWeight;
		}

		private Weight GetFixedWeight(bool firstRepaymentDatePassed, decimal baseWeight, decimal firstRepaymentWeight, int minGrade, int maxGrade, IEnumerable<RangeGrage> ranges, decimal rangeValue)
		{
			var fixedWeight = new Weight
			{
				FinalWeightFixedWeightParameter = firstRepaymentDatePassed ? firstRepaymentWeight : baseWeight,
				StandardWeightFixedWeightParameter = firstRepaymentDatePassed ? firstRepaymentWeight : baseWeight,
				StandardWeightAdjustableWeightParameter = 0,
				DeltaForAdjustableWeightParameter = 0,
				FinalWeight = firstRepaymentDatePassed ? firstRepaymentWeight : baseWeight,
				MinimumGrade = minGrade,
				MaximumGrade = maxGrade,
				Grade = GetGrade(ranges, rangeValue)
			};

			fixedWeight.MinimumScore = fixedWeight.FinalWeight * fixedWeight.MinimumGrade;
			fixedWeight.MaximumScore = fixedWeight.FinalWeight * fixedWeight.MaximumGrade;

			return fixedWeight;
		}

		private MedalMultiplier GetMedal(IEnumerable<RangeMedal> rangeMedals, decimal value)
		{
			var range = GetRange(rangeMedals, value);
			if (range != null)
			{
				return range.Medal;
			}
			return MedalMultiplier.NoMedal;
		}

		private int GetGrade(IEnumerable<RangeGrage> rangeGrages, decimal value)
		{
			var range = GetRange(rangeGrages, value);
			if (range != null)
			{
				return range.Grade;
			}
			return 0;
		}

		public T GetRange<T>(IEnumerable<T> ranges, decimal value) where T : Range
		{
			foreach (var rangeGrage in ranges)
			{
				if (rangeGrage.IsInRange(value))
				{
					return rangeGrage;
				}
			}
			return null;
		}

		private void PrintDict(ScoreMedalOffer scoreMedal, Dictionary<Parameter, Weight> dict)
		{
			m_oLog.Info("Medal Results: \n\n");
			m_oLog.Info(
				"medal: {0} points: {4} result: {1}%, offer: {2} £ at  {3}% \n ResultMaxPoints: {5} \n ResultWeights: {6} \n ACDescriptors: {7} \n AcParameters: {8} \n\n",
				scoreMedal.Medal,
				ToShort(scoreMedal.ScoreResult * 100),
				scoreMedal.MaxOffer,
				scoreMedal.MaxOfferPercent * 100,
				ToShort(scoreMedal.ScorePoints),
				scoreMedal.ResultMaxPoints,
				scoreMedal.ResultWeigts,
				scoreMedal.AcDescriptors,
				scoreMedal.AcParameters);

			decimal s1 = 0M, s2 = 0M, s3 = 0M, s4 = 0M, s5 = 0M, s6 = 0M, s7 = 0M, s8 = 0M, s9 = 0M, s10 = 0M, s11 = 0M;
			foreach (var weight in dict)
			{
				m_oLog.Info("{0}| {10}| {11}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}| {9}", weight.Key.ToString().PadRight(25),
							ToPercent(weight.Value.FinalWeightFixedWeightParameter),
							ToPercent(weight.Value.StandardWeightFixedWeightParameter),
							ToPercent(weight.Value.StandardWeightAdjustableWeightParameter),
							ToPercent(weight.Value.DeltaForAdjustableWeightParameter),
							ToPercent(weight.Value.FinalWeight),
							ToPercent(weight.Value.MinimumScore / 100),
							ToPercent(weight.Value.MaximumScore / 100),
							ToShort(weight.Value.MinimumGrade),
							ToShort(weight.Value.MaximumGrade),
							ToShort(weight.Value.Grade),
							ToShort(weight.Value.Score).PadRight(50));
				s1 += weight.Value.FinalWeightFixedWeightParameter;
				s2 += weight.Value.StandardWeightFixedWeightParameter;
				s3 += weight.Value.StandardWeightAdjustableWeightParameter;
				s4 += weight.Value.DeltaForAdjustableWeightParameter;
				s5 += weight.Value.FinalWeight;
				s6 += weight.Value.MinimumScore;
				s7 += weight.Value.MaximumScore;
				s8 += weight.Value.MinimumGrade;
				s9 += weight.Value.MaximumGrade;
				s10 += weight.Value.Score;
				s11 += weight.Value.Grade;
			}
			m_oLog.Info("---------------------------------------------------------------------------------------------------------------------------------------------------------------");
			m_oLog.Info("{0}| {10}| {11}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}| {9}",
			            "Sum".PadRight(25), ToPercent(s1), ToPercent(s2), ToPercent(s3), ToPercent(s4), ToPercent(s5),
			            ToPercent(s6/100), ToPercent(s7/100), ToShort(s8), ToShort(s9), ToShort(s11), ToShort(s10).PadRight(50));

		}

		private string ToPercent(decimal val)
		{
			return String.Format("{0:F2}", val * 100).PadRight(6);
		}

		private string ToShort(decimal val)
		{
			return String.Format("{0:F2}", val).PadRight(6);
		}
	}
}
