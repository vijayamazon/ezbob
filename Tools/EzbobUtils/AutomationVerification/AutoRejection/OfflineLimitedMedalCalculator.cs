namespace AutomationCalculator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class OfflineLImitedMedalCalculator
	{
		public MedalInputModel GetInputParameters(int customerId) {
			//TODO
			return new MedalInputModel();
		}

		public MedalOutputModel CalculateMedal(MedalInputModel model)
		{
			Console.WriteLine(model.ToString());

			if (model.HasMoreThanOneHmrc) {
				var medal = new MedalOutputModel { Error = "More then one hmrc. can't Calc Medal" };
				return medal;
			}

			bool firstRepaymentDatePassed = model.FirstRepaymentDate.HasValue && model.FirstRepaymentDate.Value.Date < DateTime.Today;
			var dict = new Dictionary<Parameter, Weight>
				{
					
					{Parameter.BusinessScore,            GetBusinessScoreWeight(model.BusinessScore, firstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.TangibleEquity,           GetTangibleEquityWeight(model.TangibleEquity)},
					{Parameter.BusinessSeniority,        GetBusinessSeniorityWeight(model.BusinessSeniority, firstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.ConsumerScore,            GetConsumerScoreWeight(model.ConsumerScore, firstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.EzbobSeniority,           GetEzbobSeniorityWeight(model.EzbobSeniority, firstRepaymentDatePassed)},
					{Parameter.MaritalStatus,            GetMaritalStatusWeight(model.MaritalStatus)},
					{Parameter.NumOfOnTimeLoans,         GetNumOfOnTimeLoansWeight(model.NumOfOnTimeLoans, firstRepaymentDatePassed)},
					{Parameter.NumOfLatePayments,        GetNumOfLatePaymentsWeight(model.NumOfLatePayments, firstRepaymentDatePassed)},
					{Parameter.NumOfEarlyPayments,       GetNumOfEarlyPaymentsWeight(model.NumOfEarlyPayments, firstRepaymentDatePassed)},
					{Parameter.AnnualTurnover,           GetAnnualTurnoverWeight(model.AnnualTurnover, model.HasHmrc)},
					{Parameter.FreeCashFlow,             GetFreeCashFlowWeight(model.FreeCashFlow, model.HasHmrc)},
					{Parameter.NetWorth,                 GetNetWorthWeight(model.NetWorth)}
				};

			CalcDelta(model, dict);

			MedalOutputModel scoreMedal = CalcScoreMedalOffer(dict);

			return scoreMedal;
		}

		private MedalOutputModel CalcScoreMedalOffer(Dictionary<Parameter, Weight> dict)
		{
			decimal minScoreSum = 0M;
			decimal maxScoreSum = 0M;
			decimal scoreSum = 0M;
			foreach (var weight in dict.Values)
			{
				weight.Score = weight.Grade*weight.FinalWeight;
				minScoreSum += weight.MinimumScore;
				maxScoreSum += weight.MaximumScore;
				scoreSum += weight.Score;
			}

			decimal score = (scoreSum - minScoreSum)/(maxScoreSum - minScoreSum);
			Medal medal = GetMedal(OfflineLimitedMedalConstants.MedalRanges, score);
			var medalOutput = new MedalOutputModel
				{
					Medal = medal,
					Score = score,
				};

			PrintDict(medalOutput, dict);
			return medalOutput;
		}

		private void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict)
		{

			if (model.BusinessScore <= OfflineLimitedMedalConstants.LowBusinessScore || model.ConsumerScore <= OfflineLimitedMedalConstants.LowConsumerScore) {
				//Sum of all weights
				var sow = dict.Sum(x => x.Value.FinalWeight);
				//Sum of weights of TangibleEquity, NetWorth, MaritalStatus
				var sonf = dict[Parameter.TangibleEquity].FinalWeight + dict[Parameter.NetWorth].FinalWeight + dict[Parameter.MaritalStatus].FinalWeight;
				var sonfDesired = sonf - sow + 1;
				var ratio = sonfDesired/sonf;
				dict[Parameter.TangibleEquity].FinalWeight *= ratio;
				dict[Parameter.NetWorth].FinalWeight *= ratio;
				dict[Parameter.MaritalStatus].FinalWeight *= ratio;
			}

			foreach (var weight in dict.Values)
			{
				weight.MinimumScore = weight.FinalWeight * weight.MinimumGrade;
				weight.MaximumScore = weight.FinalWeight * weight.MaximumGrade;
				weight.Score = weight.FinalWeight * weight.Score;
			}
		}

		

		private Weight GetNumOfEarlyPaymentsWeight(int ezbobNumOfEarlyReayments, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, OfflineLimitedMedalConstants.NumOfEarlyRepaymentsBaseWeight,
						   OfflineLimitedMedalConstants.NumOfEarlyRepaymentsFirstRepaymentWeight, OfflineLimitedMedalConstants.NumOfEarlyRepaymentsRanges, ezbobNumOfEarlyReayments);
		}

		private Weight GetNumOfLatePaymentsWeight(int ezbobNumOfLateRepayments, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, OfflineLimitedMedalConstants.NumOfLateRepaymentsBaseWeight,
						   OfflineLimitedMedalConstants.NumOfLateRepaymentsFirstRepaymentWeight, OfflineLimitedMedalConstants.NumOfLateRepaymentsRanges, ezbobNumOfLateRepayments);
		}

		private Weight GetNumOfOnTimeLoansWeight(int ezbobNumOfLoans, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, OfflineLimitedMedalConstants.NumOfOnTimeLoansBaseWeight,
						   OfflineLimitedMedalConstants.NumOfOnTimeLoansFirstRepaymentWeight, OfflineLimitedMedalConstants.NumOfOnTimeLoansRanges, ezbobNumOfLoans);
		}

		private Weight GetEzbobSeniorityWeight(decimal ezbobSeniority, bool firstRepaymentDatePassed)
		{
			return GetFixedWeight(firstRepaymentDatePassed, OfflineLimitedMedalConstants.EzbobSeniorityBaseWeight,
						   OfflineLimitedMedalConstants.EzbobSeniorityFirstRepaymentWeight, OfflineLimitedMedalConstants.EzbobSeniorityRanges, ezbobSeniority);
		}

		
		private Weight GetAnnualTurnoverWeight(decimal annualTurnover, bool hasHmrc)
		{
			var annualTurnoverWeight = new Weight
				{
					FinalWeight = OfflineLimitedMedalConstants.AnnualTurnoverBaseWeight,
					MinimumGrade = OfflineLimitedMedalConstants.AnnualTurnoverRanges.MinGrade(),
					MaximumGrade = OfflineLimitedMedalConstants.AnnualTurnoverRanges.MaxGrade(),
					Grade = GetGrade(OfflineLimitedMedalConstants.AnnualTurnoverRanges, annualTurnover),
				};

			if (!hasHmrc) {
				annualTurnoverWeight.FinalWeight += OfflineLimitedMedalConstants.AnnualTurnoverNoHmrcWeightChange;
			}

			return annualTurnoverWeight;
		}
		
		private Weight GetMaritalStatusWeight(MaritalStatus maritalStatus)
		{
			var maritalStatusWeight = new Weight
			{
				FinalWeight = OfflineLimitedMedalConstants.MaritalStatusBaseWeight,
				MinimumGrade = OfflineLimitedMedalConstants.MaritalStatusGrade_Single,
				MaximumGrade = OfflineLimitedMedalConstants.MaritalStatusGrade_Married
			};

			switch (maritalStatus)
			{
				case MaritalStatus.Married:
					maritalStatusWeight.Grade = OfflineLimitedMedalConstants.MaritalStatusGrade_Married;
					break;
				case MaritalStatus.Divorced:
					maritalStatusWeight.Grade = OfflineLimitedMedalConstants.MaritalStatusGrade_Divorced;
					break;
				case MaritalStatus.Single:
				case MaritalStatus.Other:
					maritalStatusWeight.Grade = OfflineLimitedMedalConstants.MaritalStatusGrade_Single;
					break;
				case MaritalStatus.Widowed:
					maritalStatusWeight.Grade = OfflineLimitedMedalConstants.MaritalStatusGrade_Widowed;
					break;
				case MaritalStatus.LivingTogether:
					maritalStatusWeight.Grade = OfflineLimitedMedalConstants.MaritalStatusGrade_LivingTogether;
					break;
				case MaritalStatus.Separated:
					maritalStatusWeight.Grade = OfflineLimitedMedalConstants.MaritalStatusGrade_Separated;
					break;
				default:
					maritalStatusWeight.Grade = OfflineLimitedMedalConstants.MaritalStatusGrade_Other;
					break;
			}

			return maritalStatusWeight;
		}

		private Weight GetNetWorthWeight(decimal netWorth)
		{
			var businessScoreWeight = new Weight
			{
				FinalWeight = OfflineLimitedMedalConstants.NetWorthBaseWeight,
				MinimumGrade = OfflineLimitedMedalConstants.NetWorthRanges.MinGrade(),
				MaximumGrade = OfflineLimitedMedalConstants.NetWorthRanges.MaxGrade(),
				Grade = GetGrade(OfflineLimitedMedalConstants.NetWorthRanges, netWorth)
			};

			return businessScoreWeight;
		}

		private Weight GetFreeCashFlowWeight(decimal freeCashFlow, bool hasHmrc)
		{
			var businessScoreWeight = new Weight
			{
				FinalWeight = OfflineLimitedMedalConstants.FreeCashFlowBaseWeight,
				MinimumGrade = OfflineLimitedMedalConstants.FreeCashFlowRanges.MinGrade(),
				MaximumGrade = OfflineLimitedMedalConstants.FreeCashFlowRanges.MaxGrade(),
				Grade = GetGrade(OfflineLimitedMedalConstants.FreeCashFlowRanges, freeCashFlow)
			};

			if (!hasHmrc)
			{
				businessScoreWeight.FinalWeight = OfflineLimitedMedalConstants.FreeCashFlowNoHmrcWeight;
			}
			
			return businessScoreWeight;
		}

		private Weight GetBusinessSeniorityWeight(int businessSeniority, bool firstRepaymentDatePassed, bool hasHmrc)
		{
			var businessScoreWeight = new Weight
			{
				FinalWeight = OfflineLimitedMedalConstants.BusinessSeniorityBaseWeight,
				MinimumGrade = OfflineLimitedMedalConstants.BusinessSeniorityRanges.MinGrade(),
				MaximumGrade = OfflineLimitedMedalConstants.BusinessSeniorityRanges.MaxGrade(),
				Grade = GetGrade(OfflineLimitedMedalConstants.BusinessSeniorityRanges, businessSeniority)
			};

			if (!hasHmrc) {
				businessScoreWeight.FinalWeight += OfflineLimitedMedalConstants.BusinessSeniorityNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed) {
				businessScoreWeight.FinalWeight += OfflineLimitedMedalConstants.BusinessSeniorityFirstRepaymentWeightChange;
			}

			return businessScoreWeight;
		}

		private Weight GetTangibleEquityWeight(decimal tangibleEquity)
		{
			var tangibleEquityWeight = new Weight
			{
				FinalWeight = OfflineLimitedMedalConstants.TangibleEquityBaseWeight,
				MinimumGrade = OfflineLimitedMedalConstants.BusinessScoreRanges.MinGrade(),
				MaximumGrade = OfflineLimitedMedalConstants.BusinessScoreRanges.MaxGrade(),
				Grade = GetGrade(OfflineLimitedMedalConstants.TangibleEquityRanges, tangibleEquity)
			};

			return tangibleEquityWeight;
		}

		private Weight GetBusinessScoreWeight(int businessScore, bool firstRepaymentDatePassed, bool hasHmrc)
		{
			var businessScoreWeight = new Weight
			{
				FinalWeight = OfflineLimitedMedalConstants.BusinessScoreBaseWeight,
				MinimumGrade = OfflineLimitedMedalConstants.BusinessScoreRanges.MinGrade(),
				MaximumGrade = OfflineLimitedMedalConstants.BusinessScoreRanges.MaxGrade(),
				Grade = GetGrade(OfflineLimitedMedalConstants.BusinessScoreRanges, businessScore)
			};

			if (businessScore <= OfflineLimitedMedalConstants.LowBusinessScore)
			{
				businessScoreWeight.FinalWeight = OfflineLimitedMedalConstants.BusinessScoreLowScoreWeight;
			}

			if (!hasHmrc)
			{
				businessScoreWeight.FinalWeight += OfflineLimitedMedalConstants.BusinessScoreNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed)
			{
				businessScoreWeight.FinalWeight += OfflineLimitedMedalConstants.BusinessScoreFirstRepaymentWeightChange;
			}

			return businessScoreWeight;
		}

		private Weight GetConsumerScoreWeight(int consumerScore, bool firstRepaymentDatePassed, bool hasHmrc)
		{
			var consumerScoreWeight = new Weight
				{
					FinalWeight = OfflineLimitedMedalConstants.ConsumerScoreBaseWeight,
					MinimumGrade = OfflineLimitedMedalConstants.ConsumerScoreRanges.MinGrade(),
					MaximumGrade = OfflineLimitedMedalConstants.ConsumerScoreRanges.MaxGrade(),
					Grade = GetGrade(OfflineLimitedMedalConstants.ConsumerScoreRanges, consumerScore)
				};

			if (consumerScore <= OfflineLimitedMedalConstants.LowConsumerScore) {
				consumerScoreWeight.FinalWeight = OfflineLimitedMedalConstants.ConsumerScoreLowScoreWeight;
			}

			if (!hasHmrc) {
				consumerScoreWeight.FinalWeight += OfflineLimitedMedalConstants.ConsumerScoreNoHmrcWeightChange;
			}

			if (firstRepaymentDatePassed) {
				consumerScoreWeight.FinalWeight += OfflineLimitedMedalConstants.ConsumerScoreFirstRepaymentWeightChange;
			}

			return consumerScoreWeight;
		}

		private Weight GetFixedWeight(bool firstRepaymentDatePassed, decimal baseWeight, decimal firstRepaymentWeight, List<RangeGrage> ranges, decimal rangeValue)
		{
			var fixedWeight = new Weight
			{
				FinalWeight = firstRepaymentDatePassed ? firstRepaymentWeight : baseWeight,
				MinimumGrade = ranges.MinGrade(),
				MaximumGrade = ranges.MinGrade(),
				Grade = GetGrade(ranges, rangeValue)
			};

			return fixedWeight;
		}

		private Medal GetMedal(IEnumerable<RangeMedal> rangeMedals, decimal value)
		{
			var range = rangeMedals.GetRange(value);
			if (range != null)
			{
				return range.Medal;
			}
			return Medal.NoMedal;
		}
		
		private int GetGrade(IEnumerable<RangeGrage> rangeGrages, decimal value)
		{
			var range = rangeGrages.GetRange(value);
			if (range != null)
			{
				return range.Grade;
			}
			return 0;
		}



		private void PrintDict(MedalOutputModel medalOutput, Dictionary<Parameter, Weight> dict)
		{
			Console.WriteLine("{0} {1}%", medalOutput.Medal, medalOutput.Score * 100);
			Console.WriteLine();
			decimal s5 = 0M, s6 = 0M, s7 = 0M, s8 = 0M, s9 = 0M, s10 = 0M, s11 = 0M;
			Console.WriteLine("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|", "Parameter".PadRight(25), "Weight".PadRight(10), "MinScore".PadRight(10), "MaxScore".PadRight(10), "MinGrade".PadRight(10), "MaxGrade".PadRight(10), "Grade".PadRight(10), "Score".PadRight(10));
			foreach (var weight in dict) {
				
				Console.WriteLine("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|", 
					weight.Key.ToString().PadRight(25),
					ToPercent(weight.Value.FinalWeight).PadRight(10),
					ToPercent(weight.Value.MinimumScore / 100).PadRight(10),
					ToPercent(weight.Value.MaximumScore / 100).PadRight(10),
					weight.Value.MinimumGrade.ToString().PadRight(10),
					weight.Value.MaximumGrade.ToString().PadRight(10),
					weight.Value.Grade.ToString().PadRight(10),
					ToShort(weight.Value.Score).PadRight(10));
				s5 += weight.Value.FinalWeight;
				s6 += weight.Value.MinimumScore;
				s7 += weight.Value.MaximumScore;
				s8 += weight.Value.MinimumGrade;
				s9 += weight.Value.MaximumGrade;
				s11 += weight.Value.Grade;
				s10 += weight.Value.Score;
			}
			Console.WriteLine("--------------------------------------------------------------------");
			Console.WriteLine("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|", 
				"Sum".PadRight(25), 
				ToPercent(s5).PadRight(10),
				ToPercent(s6 / 100).PadRight(10),
				ToPercent(s7 / 100).PadRight(10),
				s8.ToString().PadRight(10),
				s9.ToString().PadRight(10),
				s11.ToString().PadRight(10),
				ToShort(s10).PadRight(10));

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
