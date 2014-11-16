namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using System.Globalization;
	using System.Text;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MaritalStatus = EZBob.DatabaseLib.Model.Database.MaritalStatus;

	public class MedalResult
	{
		// Inputs
		public int CustomerId { get; set; }
		public DateTime CalculationTime { get; set; }
		public MedalType MedalType { get; set; }

		// Gathered data
		public int BusinessScore { get; set; }
		public decimal FreeCashFlowValue { get; set; }
		public decimal TangibleEquityValue { get; set; }
		public DateTime? BusinessSeniority { get; set; }
		public int ConsumerScore { get; set; }
		public decimal NetWorth { get; set; }
		public MaritalStatus MaritalStatus { get; set; }
		public int NumberOfStores { get; set; }
		public int PositiveFeedbacks { get; set; }
		public DateTime? EzbobSeniority { get; set; }
		public int NumOfLoans { get; set; }
		public int NumOfLateRepayments { get; set; }
		public int NumOfEarlyRepayments { get; set; }
		public decimal ValueAdded { get; set; }
		public decimal HmrcAnnualTurnover { get; set; }
		public decimal BankAnnualTurnover { get; set; }
		public decimal OnlineAnnualTurnover { get; set; }
		public bool FirstRepaymentDatePassed { get; set; }
		public int NumOfHmrcMps { get; set; }
		public int ZooplaValue { get; set; }
		public DateTime? EarliestHmrcLastUpdateDate { get; set; }
		public DateTime? EarliestYodleeLastUpdateDate { get; set; }
		public int AmazonPositiveFeedbacks { get; set; }
		public int EbayPositiveFeedbacks { get; set; }
		public int NumberOfPaypalPositiveTransactions { get; set; }
		public decimal MortgageBalance { get; set; }

		// Calculated data
		public decimal AnnualTurnover { get; set; }
		public decimal TangibleEquity { get; set; }
		public decimal FreeCashFlow { get; set; }
		public string InnerFlowName { get; set; }
		
		// Weights, grades, scores
		public decimal BusinessScoreWeight { get; set; }
		public decimal BusinessScoreGrade { get; set; }
		public decimal BusinessScoreScore { get; set; }

		public decimal FreeCashFlowWeight { get; set; }
		public decimal FreeCashFlowGrade { get; set; }
		public decimal FreeCashFlowScore { get; set; }

		public decimal AnnualTurnoverWeight { get; set; }
		public decimal AnnualTurnoverGrade { get; set; }
		public decimal AnnualTurnoverScore { get; set; }

		public decimal TangibleEquityWeight { get; set; }
		public decimal TangibleEquityGrade { get; set; }
		public decimal TangibleEquityScore { get; set; }

		public decimal BusinessSeniorityWeight { get; set; }
		public decimal BusinessSeniorityGrade { get; set; }
		public decimal BusinessSeniorityScore { get; set; }

		public decimal ConsumerScoreWeight { get; set; }
		public decimal ConsumerScoreGrade { get; set; }
		public decimal ConsumerScoreScore { get; set; }

		public decimal NetWorthWeight { get; set; }
		public decimal NetWorthGrade { get; set; }
		public decimal NetWorthScore { get; set; }

		public decimal MaritalStatusWeight { get; set; }
		public decimal MaritalStatusGrade { get; set; }
		public decimal MaritalStatusScore { get; set; }

		public decimal NumberOfStoresWeight { get; set; }
		public decimal NumberOfStoresGrade { get; set; }
		public decimal NumberOfStoresScore { get; set; }

		public decimal PositiveFeedbacksWeight { get; set; }
		public decimal PositiveFeedbacksGrade { get; set; }
		public decimal PositiveFeedbacksScore { get; set; }
		
		public decimal EzbobSeniorityWeight { get; set; }
		public decimal EzbobSeniorityGrade { get; set; }
		public decimal EzbobSeniorityScore { get; set; }

		public decimal NumOfLoansWeight { get; set; }
		public decimal NumOfLoansGrade { get; set; }
		public decimal NumOfLoansScore { get; set; }

		public decimal NumOfLateRepaymentsWeight { get; set; }
		public decimal NumOfLateRepaymentsGrade { get; set; }
		public decimal NumOfLateRepaymentsScore { get; set; }

		public decimal NumOfEarlyRepaymentsWeight { get; set; }
		public decimal NumOfEarlyRepaymentsGrade { get; set; }
		public decimal NumOfEarlyRepaymentsScore { get; set; }

		// Output
		public decimal TotalScore { get; set; }
		public decimal TotalScoreNormalized { get; set; }
		public MedalClassification MedalClassification { get; set; }
		public string Error { get; set; }
		public int OfferedLoanAmount { get; set; }

		public bool IsIdentical(MedalOutputModel other)
		{
			//if NoMedal no need to compare any other field
			if (MedalType == MedalType.NoMedal && other.MedalType == AutomationCalculator.Common.MedalType.NoMedal) {
				return true;
			}

			if (MedalType.ToString() != other.MedalType.ToString() ||
				Math.Abs(ValueAdded - other.ValueAdded) > 0.001M ||
				Math.Abs(TotalScore - other.Score*100) > 0.001M ||
				Math.Abs(TotalScoreNormalized - other.NormalizedScore) > 0.001M ||
				MedalClassification.ToString() != other.Medal.ToString() ||
				Math.Abs(OfferedLoanAmount - other.OfferedLoanAmount) > 0.001M)
			{
				return false;
			}

			return true;
		}

		public void SaveToDb(AConnection db)
		{
			db.ExecuteNonQuery("StoreMedal", CommandSpecies.StoredProcedure,
							   new QueryParameter("CustomerId", CustomerId),
							   new QueryParameter("CalculationTime", CalculationTime),
							   new QueryParameter("MedalType", MedalType.ToString()),
							   new QueryParameter("FirstRepaymentDatePassed", FirstRepaymentDatePassed),
							   new QueryParameter("BusinessScore", BusinessScore),
							   new QueryParameter("BusinessScoreWeight", BusinessScoreWeight),
							   new QueryParameter("BusinessScoreGrade", BusinessScoreGrade),
							   new QueryParameter("BusinessScoreScore", BusinessScoreScore),
							   new QueryParameter("FreeCashFlowValue", FreeCashFlowValue),
							   new QueryParameter("FreeCashFlow", FreeCashFlow),
							   new QueryParameter("FreeCashFlowWeight", FreeCashFlowWeight),
							   new QueryParameter("FreeCashFlowGrade", FreeCashFlowGrade),
							   new QueryParameter("FreeCashFlowScore", FreeCashFlowScore),
							   new QueryParameter("HmrcAnnualTurnover", HmrcAnnualTurnover),
							   new QueryParameter("BankAnnualTurnover", BankAnnualTurnover),
							   new QueryParameter("OnlineAnnualTurnover", OnlineAnnualTurnover),
							   new QueryParameter("AnnualTurnover", AnnualTurnover),
							   new QueryParameter("AnnualTurnoverWeight", AnnualTurnoverWeight),
							   new QueryParameter("AnnualTurnoverGrade", AnnualTurnoverGrade),
							   new QueryParameter("AnnualTurnoverScore", AnnualTurnoverScore),
							   new QueryParameter("TangibleEquityValue", TangibleEquityValue),
							   new QueryParameter("TangibleEquity", TangibleEquity),
							   new QueryParameter("TangibleEquityWeight", TangibleEquityWeight),
							   new QueryParameter("TangibleEquityGrade", TangibleEquityGrade),
							   new QueryParameter("TangibleEquityScore", TangibleEquityScore),
							   new QueryParameter("BusinessSeniority", BusinessSeniority.HasValue && BusinessSeniority.Value.Year > 1800 ? BusinessSeniority : null),
							   new QueryParameter("BusinessSeniorityWeight", BusinessSeniorityWeight),
							   new QueryParameter("BusinessSeniorityGrade", BusinessSeniorityGrade),
							   new QueryParameter("BusinessSeniorityScore", BusinessSeniorityScore),
							   new QueryParameter("ConsumerScore", ConsumerScore),
							   new QueryParameter("ConsumerScoreWeight", ConsumerScoreWeight),
							   new QueryParameter("ConsumerScoreGrade", ConsumerScoreGrade),
							   new QueryParameter("ConsumerScoreScore", ConsumerScoreScore),
							   new QueryParameter("NetWorth", NetWorth),
							   new QueryParameter("NetWorthWeight", NetWorthWeight),
							   new QueryParameter("NetWorthGrade", NetWorthGrade),
							   new QueryParameter("NetWorthScore", NetWorthScore),
							   new QueryParameter("MaritalStatus", MaritalStatus.ToString()),
							   new QueryParameter("MaritalStatusWeight", MaritalStatusWeight),
							   new QueryParameter("MaritalStatusGrade", MaritalStatusGrade),
							   new QueryParameter("MaritalStatusScore", MaritalStatusScore),
							   new QueryParameter("NumberOfStores", NumberOfStores),
							   new QueryParameter("NumberOfStoresWeight", NumberOfStoresWeight),
							   new QueryParameter("NumberOfStoresGrade", NumberOfStoresGrade),
							   new QueryParameter("NumberOfStoresScore", NumberOfStoresScore),
							   new QueryParameter("PositiveFeedbacks", PositiveFeedbacks),
							   new QueryParameter("PositiveFeedbacksWeight", PositiveFeedbacksWeight),
							   new QueryParameter("PositiveFeedbacksGrade", PositiveFeedbacksGrade),
							   new QueryParameter("PositiveFeedbacksScore", PositiveFeedbacksScore),
							   new QueryParameter("EzbobSeniority", EzbobSeniority),
							   new QueryParameter("EzbobSeniorityWeight", EzbobSeniorityWeight),
							   new QueryParameter("EzbobSeniorityGrade", EzbobSeniorityGrade),
							   new QueryParameter("EzbobSeniorityScore", EzbobSeniorityScore),
							   new QueryParameter("NumOfLoans", NumOfLoans),
							   new QueryParameter("NumOfLoansWeight", NumOfLoansWeight),
							   new QueryParameter("NumOfLoansGrade", NumOfLoansGrade),
							   new QueryParameter("NumOfLoansScore", NumOfLoansScore),
							   new QueryParameter("NumOfLateRepayments", NumOfLateRepayments),
							   new QueryParameter("NumOfLateRepaymentsWeight", NumOfLateRepaymentsWeight),
							   new QueryParameter("NumOfLateRepaymentsGrade", NumOfLateRepaymentsGrade),
							   new QueryParameter("NumOfLateRepaymentsScore", NumOfLateRepaymentsScore),
							   new QueryParameter("NumOfEarlyRepayments", NumOfEarlyRepayments),
							   new QueryParameter("NumOfEarlyRepaymentsWeight", NumOfEarlyRepaymentsWeight),
							   new QueryParameter("NumOfEarlyRepaymentsGrade", NumOfEarlyRepaymentsGrade),
							   new QueryParameter("NumOfEarlyRepaymentsScore", NumOfEarlyRepaymentsScore),
							   new QueryParameter("ValueAdded", ValueAdded),
							   new QueryParameter("InnerFlowName", InnerFlowName),
							   new QueryParameter("TotalScore", TotalScore),
							   new QueryParameter("TotalScoreNormalized", TotalScoreNormalized),
							   new QueryParameter("Medal", MedalClassification.ToString()),
							   new QueryParameter("Error", Error),
							   new QueryParameter("OfferedLoanAmount", OfferedLoanAmount),
							   new QueryParameter("NumOfHmrcMps", NumOfHmrcMps),
							   new QueryParameter("ZooplaValue", ZooplaValue),
							   new QueryParameter("EarliestHmrcLastUpdateDate", EarliestHmrcLastUpdateDate),
							   new QueryParameter("EarliestYodleeLastUpdateDate", EarliestYodleeLastUpdateDate),
							   new QueryParameter("AmazonPositiveFeedbacks", AmazonPositiveFeedbacks),
							   new QueryParameter("EbayPositiveFeedbacks", EbayPositiveFeedbacks),
							   new QueryParameter("NumberOfPaypalPositiveTransactions", NumberOfPaypalPositiveTransactions),
							   new QueryParameter("MortgageBalance", MortgageBalance));
		}

		public void PrintToLog(ASafeLog log) {

			var sb = new StringBuilder();
			sb.AppendFormat("Calculation Num 1 .........Medal Type {2} Medal: {0} NormalizedScore: {1}% Score: {3}\n", MedalClassification,
							StringBuilderExtention.ToPercent(TotalScoreNormalized), MedalType, TotalScore);
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8} \n", "Parameter".PadRight(25), "Weight".PadRight(10),
							"MinScore".PadRight(10), "MaxScore".PadRight(10), "MinGrade".PadRight(10), "MaxGrade".PadRight(10),
							"Grade".PadRight(10), "Score".PadRight(10), "Value");

			var summary = new Weight();

			Weight weight;

			if (MedalType != MedalType.SoleTrader) {
				weight = new Weight {Value = BusinessScore.ToString(CultureInfo.InvariantCulture), FinalWeight = BusinessScoreWeight,Grade = (int)BusinessScoreGrade,Score = BusinessScoreScore};
				sb.AddWeight(weight, "BusinessScore", ref summary);
			}

			if (MedalType == MedalType.Limited || MedalType == MedalType.OnlineLimited)
			{
				weight = new Weight {Value = TangibleEquity.ToString(CultureInfo.InvariantCulture), FinalWeight = TangibleEquityWeight,Grade = (int)TangibleEquityGrade,Score = TangibleEquityScore};
				sb.AddWeight(weight, "TangibleEquity", ref summary);
			}

			DateTime calcTime = CalculationTime;
			int businessSeniorityYears = 0;
			decimal ezbobSeniorityMonth = 0;
			if (BusinessSeniority.HasValue) {
				businessSeniorityYears = (int)(calcTime - BusinessSeniority.Value).TotalDays / 365;
			}

			if(EzbobSeniority.HasValue){
				ezbobSeniorityMonth = (decimal)(calcTime - EzbobSeniority.Value).TotalDays / (365.0M / 12.0M);
			}

			weight = new Weight { Value = businessSeniorityYears.ToString(CultureInfo.InvariantCulture), FinalWeight = BusinessSeniorityWeight, Grade = (int)BusinessSeniorityGrade, Score = BusinessSeniorityScore };
			sb.AddWeight(weight, "BusinessSeniority", ref summary);

			weight = new Weight {Value = ConsumerScore.ToString(CultureInfo.InvariantCulture), FinalWeight = ConsumerScoreWeight,Grade = (int)ConsumerScoreGrade,Score = ConsumerScoreScore};
			sb.AddWeight(weight, "ConsumerScore", ref summary);

			weight = new Weight { Value = ezbobSeniorityMonth.ToString(CultureInfo.InvariantCulture), FinalWeight = EzbobSeniorityWeight, Grade = (int)EzbobSeniorityGrade, Score = EzbobSeniorityScore };
			sb.AddWeight(weight, "EzbobSeniority", ref summary);

			weight = new Weight {Value = MaritalStatus.ToString(CultureInfo.InvariantCulture), FinalWeight = MaritalStatusWeight,Grade = (int)MaritalStatusGrade,Score = MaritalStatusScore};
			sb.AddWeight(weight, "MaritalStatus", ref summary);

			weight = new Weight {Value = NumOfLoans.ToString(CultureInfo.InvariantCulture), FinalWeight = NumOfLoansWeight,Grade = (int)NumOfLoansGrade,Score = NumOfLoansScore};
			sb.AddWeight(weight, "NumOfLoans", ref summary);

			weight = new Weight {Value = NumOfLateRepayments.ToString(CultureInfo.InvariantCulture), FinalWeight = NumOfLateRepaymentsWeight,Grade = (int)NumOfLateRepaymentsGrade,Score = NumOfLateRepaymentsScore};
			sb.AddWeight(weight, "NumOfLateRepayments", ref summary);

			weight = new Weight {Value = NumOfEarlyRepayments.ToString(CultureInfo.InvariantCulture), FinalWeight = NumOfEarlyRepaymentsWeight,Grade = (int)NumOfEarlyRepaymentsGrade,Score = NumOfEarlyRepaymentsScore};
			sb.AddWeight(weight, "NumOfEarlyRepayments", ref summary);

			weight = new Weight {Value = AnnualTurnover.ToString(CultureInfo.InvariantCulture), FinalWeight = AnnualTurnoverWeight,Grade = (int)AnnualTurnoverGrade,Score = AnnualTurnoverScore};
			sb.AddWeight(weight, "AnnualTurnover", ref summary);

			weight = new Weight {Value = FreeCashFlow.ToString(CultureInfo.InvariantCulture), FinalWeight = FreeCashFlowWeight,Grade = (int)FreeCashFlowGrade,Score = FreeCashFlowScore};
			sb.AddWeight(weight, "FreeCashFlow", ref summary);

			weight = new Weight {Value = NetWorth.ToString(CultureInfo.InvariantCulture), FinalWeight = NetWorthWeight,Grade = (int)NetWorthGrade,Score = NetWorthScore};
			sb.AddWeight(weight, "NetWorth", ref summary);

			if (MedalType == MedalType.OnlineLimited || MedalType == MedalType.OnlineNonLimitedNoBusinessScore || MedalType == MedalType.OnlineNonLimitedWithBusinessScore)
			{
				weight = new Weight {Value = NumberOfStores.ToString(CultureInfo.InvariantCulture), FinalWeight = NumberOfStoresWeight,Grade = (int)NumberOfStoresGrade,Score = NumberOfStoresScore};
				sb.AddWeight(weight, "NumOfStores", ref summary);

				weight = new Weight { Value = PositiveFeedbacks.ToString(CultureInfo.InvariantCulture), FinalWeight = PositiveFeedbacksWeight, Grade = (int)PositiveFeedbacksGrade, Score = PositiveFeedbacksScore };
				sb.AddWeight(weight, "PositiveFeedbacks", ref summary);
			}

			sb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------");
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
							"Sum".PadRight(25),
							StringBuilderExtention.ToShort(summary.FinalWeight).PadRight(10),
							StringBuilderExtention.ToPercent(summary.MinimumScore / 100).PadRight(10),
							StringBuilderExtention.ToPercent(summary.MaximumScore / 100).PadRight(10),
							summary.MinimumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							summary.MaximumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							summary.Grade.ToString(CultureInfo.InvariantCulture).PadRight(10),
							StringBuilderExtention.ToShort(summary.Score).PadRight(10), summary.Value);

			log.Debug(sb.ToString());
			

		}
	}

	public static class StringBuilderExtention
	{
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
			else
			{
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
