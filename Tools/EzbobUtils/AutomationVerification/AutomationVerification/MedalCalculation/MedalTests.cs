namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MedalTests {
		protected ASafeLog Log;
		protected AConnection DB;

		public MedalTests(AConnection db, ASafeLog log) {
			DB = db;
			Log = log;
		}

		public bool TestMedalCalculation() {
			var passed = true;

			var numChecked = 0;
			var numPassed = 0;
			var numFailed = 0;

			var dbHelper = new DbHelper(new SqlConnection(Log), Log);
			List<MedalComparisonModel> testMedalData = dbHelper.GetMedalTestData();

			foreach (var medalComparisonModel in testMedalData) {
				IMedalCalulator calulator;
				switch (medalComparisonModel.MedalType) {
				case MedalType.Limited:
					calulator = new OfflineLImitedMedalCalculator(DB, Log);
					break;
				case MedalType.OnlineLimited:
					calulator = new OnlineLImitedMedalCalculator(DB, Log);
					break;
				case MedalType.NonLimited:
					calulator = new NonLimitedMedalCalculator(DB, Log);
					break;
				case MedalType.SoleTrader:
					calulator = new SoleTraderMedalCalculator(DB, Log);
					break;
				default:
					Log.Debug("Skipping medal calc for {0} NoMedal", medalComparisonModel.CustomerId);
					continue;
				}
				numChecked++;
				DateTime companyDate;
				DateTime regDate;
				DateTime calcTime = medalComparisonModel.CalculationTime;
				int businessSeniority;
				decimal ezbobSeniority;
				if (
					!DateTime.TryParseExact(medalComparisonModel.BusinessSeniority.Value,
											new[] { "yyyy-MM-dd HH:mm:ss" }, null, DateTimeStyles.AdjustToUniversal,
											out companyDate)) {
					businessSeniority = 0;
				} else {
					businessSeniority = (int)(calcTime - companyDate).TotalDays / 365;
				}

				if (
					!DateTime.TryParseExact(medalComparisonModel.EzbobSeniority.Value,
											new[] { "yyyy-MM-dd HH:mm:ss" }, null, DateTimeStyles.AdjustToUniversal,
											out regDate)) {
					ezbobSeniority = 0;
				} else {
					ezbobSeniority = (decimal)(calcTime - regDate).TotalDays / (365.0M / 12.0M);
				}
				var model = new MedalInputModel {
					AnnualTurnover = decimal.Parse(medalComparisonModel.AnnualTurnover.Value),
					TangibleEquity = decimal.Parse(medalComparisonModel.TangibleEquity.Value),
					BusinessScore = int.Parse(medalComparisonModel.BusinessScore.Value),
					FirstRepaymentDatePassed = medalComparisonModel.FirstRepaymentDatePassed,
					BusinessSeniority = businessSeniority,
					MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), medalComparisonModel.MaritalStatus.Value),
					EzbobSeniority = ezbobSeniority,
					NetWorth = decimal.Parse(medalComparisonModel.NetWorth.Value),
					NumOfEarlyPayments = int.Parse(medalComparisonModel.NumOfEarlyRepayments.Value),
					NumOfLatePayments = int.Parse(medalComparisonModel.NumOfLateRepayments.Value),
					NumOfOnTimeLoans = int.Parse(medalComparisonModel.NumOfLoans.Value),
					NumOfStores = int.Parse(medalComparisonModel.NumOfStores.Value),
					PositiveFeedbacks = int.Parse(medalComparisonModel.PositiveFeedbacks.Value),
					FreeCashFlow = decimal.Parse(medalComparisonModel.FreeCashFlow.Value),

					ConsumerScore = int.Parse(medalComparisonModel.ConsumerScore.Value),
					HasHmrc = medalComparisonModel.NumOfHmrcMps > 0
				};

				var medal = calulator.CalculateMedal(model);

				if (Math.Abs(medal.NormalizedScore - medalComparisonModel.TotalScoreNormalized) > 0.009M) {
					Log.Debug("{0}", medal);
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
				} else {
					numPassed++;
				}
			}

			Log.Debug("Test run on {0}, passed: {1}, failed: {2}", numChecked, numPassed, numFailed);
			return passed;
		}

		private void PrintComparisonMedal(MedalComparisonModel medalOutput) {
			var sb = new StringBuilder();
			sb.AppendFormat("Calculation Num 1 .........Medal Type {2} Medal: {0} NormalizedScore: {1}% Score: {3}\n", medalOutput.Medal,
							ToPercent(medalOutput.TotalScoreNormalized), medalOutput.MedalType, medalOutput.TotalScore);
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8} \n", "Parameter".PadRight(25), "Weight".PadRight(10),
							"MinScore".PadRight(10), "MaxScore".PadRight(10), "MinGrade".PadRight(10), "MaxGrade".PadRight(10),
							"Grade".PadRight(10), "Score".PadRight(10), "Value");

			var summary = new Weight();

			Weight weight;

			if (medalOutput.MedalType != MedalType.SoleTrader) {
				weight = medalOutput.BusinessScore;
				sb.AddWeight(weight, "BusinessScore", ref summary);
			}

			if (medalOutput.MedalType == MedalType.Limited || medalOutput.MedalType == MedalType.OnlineLimited) {
				weight = medalOutput.TangibleEquity;
				sb.AddWeight(weight, "TangibleEquity", ref summary);
			}

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
			return String.Format("{0:F2}", val * 100).PadRight(6);
		}

		protected string ToShort(decimal val) {
			return String.Format("{0:F2}", val).PadRight(6);
		}

		public void TestMedalDataGathering() {
			var db = new SqlConnection(Log);
			var dbHelper = new DbHelper(db, Log);
			var customers = dbHelper.GetCustomersForMedalsCompare();
			//new int[] { 25, 26, 28, 29, 30, 31, 37, 38, 41, 42, 43, 44, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 57, 58, 60, 61, 64, 65, 66, 69, 70, 71, 72, 74, 75, 76, 77, 78, 83, 84, 85, 105, 108, 2106, 4108, 4116, 4117, 4118, 4119, 5116, 5117, 5118, 5119, 5120, 5121, 5122, 5125, 5126, 5127, 5128, 5129, 5133, 5134, 5135, 7133, 10139, 10140, 10141, 10144, 10145, 10146, 11154, 11159, 11164, 14164, 14165, 14166, 14173, 14175, 14177, 14178, 14183, 14197, 14198, 14209, 14214, 14216, 14217, 14218, 14220, 14221, 14222, 14223, 14226, 15227, 15228, 15230, 15232, 16232, 16235, 16236, 16237, 16238, 16240, 16241, 16242, 16243, 16244, 16246, 16248, 16249, 17251, 17252, 17254, 17258, 17259, 17260, 17261, 17265, 17267, 18268, 18269, 18271, 18275, 18277, 18281, 18285, 18286, 18287, 18289, 18290, 20290, 20291, 20292, 20293, 20294, 20295, 20296, 20297, 20298, 20299, 20300, 20302, 20303, 20304, 20305, 20306, 20307, 20308, 20309, 20310, 20311, 20312, 20313, 20319, 20321, 21322, 21323, 21327, 21333, 21335, 21336, 21337, 21340, 21341, 21342, 21343, 21344, 21345, 21364, 21367, 21370, 21375, 21377, 21378, 21387, 21388, 21394, 21399, 21400, 21402, 21403 };
			var medalChooser = new MedalChooser(db, Log);

			foreach (var customer in customers) {
				var medal = medalChooser.GetMedal(customer.Key, customer.Value);
				dbHelper.StoreMedalVerification(medal, null, null, null);
			}
		}

		public void TestFullMedalLogic() { }

	}

	public static class StringBuilderExtention {
		public static void AddWeight(this StringBuilder sb, Weight weight, string name, ref Weight summary) {
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
							name.PadRight(25),
							ToShort(weight.FinalWeight).PadRight(10),
							ToPercent(weight.MinimumScore / 100).PadRight(10),
							ToPercent(weight.MaximumScore / 100).PadRight(10),
							weight.MinimumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							weight.MaximumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							weight.Grade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							ToShort(weight.Score).PadRight(10), weight.Value);

			if (summary == null)
				summary = weight;
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

		public static string ToPercent(decimal val) {
			return String.Format("{0:F2}", val * 100).PadRight(6);
		}

		public static string ToShort(decimal val) {
			return String.Format("{0:F2}", val).PadRight(6);
		}
	}
}
