namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Common;
	using Ezbob.Logger;

	public class MedalTests {
		protected ASafeLog Log;

		public MedalTests(ASafeLog log) {
			Log = log;
		}

		public bool TestMedalCalculation() {
			var passed = true;

			var numChecked = 0;
			var numPassed = 0;
			var numFailed = 0;

			var dbHelper = new DbHelper(Log);
			List<MedalComparisonModel> testMedalData = dbHelper.GetMedalTestData();

			foreach (var medalComparisonModel in testMedalData) {
				IMedalCalulator calulator;
				switch (medalComparisonModel.MedalType) {
					case MedalType.Limited:
						calulator = new OfflineLImitedMedalCalculator(Log);
						break;
					case MedalType.OnlineLimited:
						calulator = new OnlineLImitedMedalCalculator(Log);
						break;
					case MedalType.NonLimited:
						calulator = new NonLimitedMedalCalculator(Log);
						break;
					case MedalType.SoleTrader:
						calulator = new SoleTraderMedalCalculator(Log);
						break;
					default:
						Log.Debug("Skipping medal calc for {0} NoMedal", medalComparisonModel.CustomerId);
						continue;
				}
				numChecked++;
				DateTime companyDate;
				DateTime regDate;
				DateTime today = DateTime.Today;
				decimal businessSeniority;
				decimal ezbobSeniority;
				if (
					!DateTime.TryParseExact(medalComparisonModel.BusinessSeniority.Value,
											new[] { "yyyy-MM-dd HH:mm:ss" }, null, DateTimeStyles.AdjustToUniversal,
					                        out companyDate)) {
					businessSeniority = 0;
				}
				else {
					businessSeniority = (decimal) (today - companyDate).TotalDays/365.0M;
				}

				if (
					!DateTime.TryParseExact(medalComparisonModel.EzbobSeniority.Value,
					                        new[] {"yyyy-MM-dd HH:mm:ss"}, null, DateTimeStyles.AdjustToUniversal,
					                        out regDate)) {
					ezbobSeniority = 0;
				}
				else {
					ezbobSeniority = (decimal) (today - regDate).TotalDays/(365.0M/12.0M);
				}
				var model = new MedalInputModel {
					AnnualTurnover = decimal.Parse(medalComparisonModel.AnnualTurnover.Value),
					TangibleEquity = decimal.Parse(medalComparisonModel.TangibleEquity.Value),
					BusinessScore = int.Parse(medalComparisonModel.BusinessScore.Value),
					FirstRepaymentDatePassed = medalComparisonModel.NumOfLoans.FinalWeight > 0,
					BusinessSeniority = businessSeniority,
					MaritalStatus = (MaritalStatus) Enum.Parse(typeof (MaritalStatus), medalComparisonModel.MaritalStatus.Value),
					EzbobSeniority = ezbobSeniority,
					NetWorth = decimal.Parse(medalComparisonModel.NetWorth.Value),
					NumOfEarlyPayments = int.Parse(medalComparisonModel.NumOfEarlyRepayments.Value),
					NumOfLatePayments = int.Parse(medalComparisonModel.NumOfLateRepayments.Value),
					NumOfOnTimeLoans = int.Parse(medalComparisonModel.NumOfLoans.Value),
					NumOfStores = int.Parse(medalComparisonModel.NumOfStores.Value),
					PositiveFeedbacks = int.Parse(medalComparisonModel.PositiveFeedbacks.Value),
					FreeCashFlow = decimal.Parse(medalComparisonModel.FreeCashFlow.Value),
					
					ConsumerScore = int.Parse(medalComparisonModel.ConsumerScore.Value),
				};

				model.HasHmrc = model.FreeCashFlow != 0;
				var medal = calulator.CalculateMedal(model);



				if (Math.Round(medal.NormalizedScore - medalComparisonModel.TotalScoreNormalized, 2) != 0)
				{
					if (medal.Medal != medalComparisonModel.Medal) {
						passed = false;
						Log.Error("Medal Mismatch for customerid {0} 1st {1} 2nd {2}", medalComparisonModel.CustomerId,
						          medalComparisonModel.Medal, medal.Medal);
					}
					numFailed++;
					passed = false;
					Log.Error("Medal Normalized Score Mismatch for customerid {0} 1st {1} 2nd {2}", medalComparisonModel.CustomerId,
					          medalComparisonModel.TotalScoreNormalized, medal.NormalizedScore);

					PrintComparisonMedal(medalComparisonModel);
				}
				else {
					numPassed++;
				}
			}

			Log.Debug("Test run on {0}, passed: {1}, failed: {2}", numChecked, numPassed, numFailed);
			return passed;
		}

		private void PrintComparisonMedal(MedalComparisonModel medalOutput) {
			var sb = new StringBuilder();
			sb.AppendFormat("Calculation Num 1 .........Medal Type {2} Medal: {0} Score: {1}%\n", medalOutput.Medal,
			                ToPercent(medalOutput.TotalScoreNormalized), medalOutput.MedalType);
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8} \n", "Parameter".PadRight(25), "Weight".PadRight(10),
			                "MinScore".PadRight(10), "MaxScore".PadRight(10), "MinGrade".PadRight(10), "MaxGrade".PadRight(10),
			                "Grade".PadRight(10), "Score".PadRight(10), "Value");

			var weight = medalOutput.BusinessScore;
			var summary = new Weight();
			sb.AddWeight(weight, "BusinessScore", ref summary);
		
			weight = medalOutput.TangibleEquity;
			sb.AddWeight(weight, "TangibleEquity", ref summary);
	
			weight = medalOutput.BusinessSeniority;
			sb.AddWeight(weight, "BusinessSeniority", ref summary);
		
			weight = medalOutput.ConsumerScore;
			sb.AddWeight(weight, "ConsumerScore", ref summary);
		
			weight = medalOutput.EzbobSeniority;
			sb.AddWeight(weight, "EzbobSeniority", ref summary);
			
			weight = medalOutput.MaritalStatus;
			sb.AddWeight(weight, "MaritalStatus", ref summary);
		
			weight = medalOutput.NumOfLoans;
			sb.AddWeight(weight, "NumOfLoans", ref summary);
		
			weight = medalOutput.NumOfLateRepayments;
			sb.AddWeight(weight, "NumOfLateRepayments", ref summary);
		
			weight = medalOutput.NumOfEarlyRepayments;
			sb.AddWeight(weight, "NumOfEarlyRepayments", ref summary);
		
			weight = medalOutput.AnnualTurnover;
			sb.AddWeight(weight, "AnnualTurnover", ref summary);
		
			weight = medalOutput.FreeCashFlow;
			sb.AddWeight(weight, "FreeCashFlow", ref summary);

			weight = medalOutput.NetWorth;
			sb.AddWeight(weight, "NetWorth", ref summary);

			if (medalOutput.MedalType == MedalType.OnlineLimited) {
				weight = medalOutput.NumOfStores;
				sb.AddWeight(weight, "NumOfStores", ref summary);

				weight = medalOutput.PositiveFeedbacks;
				sb.AddWeight(weight, "PositiveFeedbacks", ref summary);
			}

			sb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------");
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
							"Sum".PadRight(25),
							ToShort(summary.FinalWeight).PadRight(10),
							ToPercent(summary.MinimumScore / 100).PadRight(10),
							ToPercent(summary.MaximumScore / 100).PadRight(10),
							summary.MinimumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							summary.MaximumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							summary.Grade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							ToShort(summary.Score).PadRight(10), summary.Value);

			Log.Debug(sb.ToString());
		}

		
		protected string ToPercent(decimal val) {
			return String.Format("{0:F2}", val*100).PadRight(6);
		}

		protected string ToShort(decimal val) {
			return String.Format("{0:F2}", val).PadRight(6);
		}

		public void TestMedalDataGathering() {}

		public void TestFullMedalLogic() {}

	}

	public static class StringBuilderExtention {
		public static void AddWeight(this StringBuilder sb, Weight weight, string name, ref Weight summary)
		{
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
							name.PadRight(25),
							ToShort(weight.FinalWeight).PadRight(10),
							ToPercent(weight.MinimumScore / 100).PadRight(10),
							ToPercent(weight.MaximumScore / 100).PadRight(10),
							weight.MinimumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							weight.MaximumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							weight.Grade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							ToShort(weight.Score).PadRight(10), weight.Value);

			
			
			if (summary == null) summary = weight;
			else {
				summary.FinalWeight += weight.FinalWeight;
				summary.MinimumGrade += weight.MinimumGrade;
				summary.MinimumScore += weight.MinimumScore;
				summary.MaximumGrade += weight.MaximumGrade;
				summary.MaximumScore += weight.MaximumScore;
				summary.Score += weight.Score;
				summary.Grade += weight.Grade;
			}
		}

		public static string ToPercent(decimal val)
		{
			return String.Format("{0:F2}", val * 100).PadRight(6);
		}

		public static string ToShort(decimal val)
		{
			return String.Format("{0:F2}", val).PadRight(6);
		}
	}
}
