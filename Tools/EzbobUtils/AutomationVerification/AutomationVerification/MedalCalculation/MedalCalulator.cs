namespace AutomationCalculator.MedalCalculation
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Common;
	using Ezbob.Logger;

	public interface IMedalCalulator {
		MedalInputModel GetInputParameters(int customerId);
		MedalOutputModel CalculateMedal(MedalInputModel model);
	}

	public abstract class MedalCalculator: IMedalCalulator {
		protected readonly ASafeLog Log;

		protected MedalCalculator(ASafeLog log) {
			Log = log;
		}

		public abstract MedalInputModel GetInputParameters(int customerId);
		public abstract MedalOutputModel CalculateMedal(MedalInputModel model);

		protected Weight GetBaseWeight(decimal value, decimal baseWeight, List<RangeGrage> ranges, bool firstRepaymentDatePassed = false, decimal firstRepaymentWeight = 0)
		{
			var weight = new Weight
			{
				FinalWeight = baseWeight,
				MinimumGrade = ranges.MinGrade(),
				MaximumGrade = ranges.MaxGrade(),
				Grade = GetGrade(ranges, value),
			};

			if (firstRepaymentDatePassed)
			{
				weight.FinalWeight = firstRepaymentWeight;
			}

			return weight;
		}

		protected Medal GetMedal(IEnumerable<RangeMedal> rangeMedals, decimal value)
		{
			var range = rangeMedals.GetRange(value);
			if (range != null)
			{
				return range.Medal;
			}
			return Medal.NoMedal;
		}

		protected int GetGrade(IEnumerable<RangeGrage> rangeGrages, decimal value)
		{
			var range = rangeGrages.GetRange(value);
			if (range != null)
			{
				return range.Grade;
			}
			return 0;
		}

		protected void PrintDict(MedalOutputModel medalOutput, Dictionary<Parameter, Weight> dict)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Medal: {0} Score: {1}%\n", medalOutput.Medal, ToPercent(medalOutput.Score));
			decimal s5 = 0M, s6 = 0M, s7 = 0M, s8 = 0M, s9 = 0M, s10 = 0M, s11 = 0M;
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|\n", "Parameter".PadRight(25), "Weight".PadRight(10), "MinScore".PadRight(10), "MaxScore".PadRight(10), "MinGrade".PadRight(10), "MaxGrade".PadRight(10), "Grade".PadRight(10), "Score".PadRight(10));
			foreach (var weight in dict)
			{

				sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|\n",
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
			sb.AppendLine("--------------------------------------------------------------------");
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|\n",
				"Sum".PadRight(25),
				ToPercent(s5).PadRight(10),
				ToPercent(s6 / 100).PadRight(10),
				ToPercent(s7 / 100).PadRight(10),
				s8.ToString().PadRight(10),
				s9.ToString().PadRight(10),
				s11.ToString().PadRight(10),
				ToShort(s10).PadRight(10));

			Log.Debug(sb.ToString());
		}

		protected string ToPercent(decimal val)
		{
			return String.Format("{0:F2}", val * 100).PadRight(6);
		}

		protected string ToShort(decimal val)
		{
			return String.Format("{0:F2}", val).PadRight(6);
		}
	}
}
