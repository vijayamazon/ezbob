namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using AutomationCalculator.Common;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Matrices;
	using Ezbob.Matrices.Core;
	using Ezbob.Utils;
	using MaritalStatus = EZBob.DatabaseLib.Model.Database.MaritalStatus;
	using Medal = EZBob.DatabaseLib.Model.Database.Medal;

	public enum TurnoverType {
		HMRC,
		Bank,
		Online,
	} // enum TurnoverType

	public class MedalResult {
		public MedalResult(int customerID, ASafeLog log) {
			MedalClassification = Medal.NoClassification;
			MedalType = MedalType.NoMedal;
			OfferedLoanAmount = 0;
			TotalScoreNormalized = 0;
			CustomerId = customerID;
			this.log = log.Safe();
			ExceptionDuringCalculation = null;
			this.wasMismatch = false;
			this.errors = new List<string>();
		} // constructor

		public MedalResult(int customerID, ASafeLog log, Exception ex) : this(customerID, log) {
			ExceptionDuringCalculation = ex;
		} // constructor

		public MedalResult(int customerID, ASafeLog log, string errorMsg) : this(customerID, log) {
			AddError(errorMsg);
		} // constructor

		// Inputs
		public int CustomerId { get; set; }
		public DateTime CalculationTime { get; set; }
		public MedalType MedalType { get; set; }

		// Gathered data
		public int BusinessScore { get; set; }
		public decimal FreeCashFlowValue { get; set; }
		[Traversable]
		public decimal TangibleEquityValue { get; set; }
		[Traversable]
		public DateTime? BusinessSeniority { get; set; }
		public int ConsumerScore { get; set; }
		public decimal NetWorth { get; set; }

		[Traversable]
		[FieldName("MaritalStatus")]
		public string MaritalStatusStr {
			get { return MaritalStatus.ToString(); }
			set {
				MaritalStatus maritalStatus;

				if (Enum.TryParse(value, out maritalStatus))
					MaritalStatus = maritalStatus;
				else {
					this.log.Error(
						"Unable to parse marital status for customer: {0} will use 'Other'. The value was: '{1}'.",
						CustomerId,
						value
					);
					MaritalStatus = MaritalStatus.Other;
				} // if
			} // set
		} // MaritalStatus

		public void CalculateFeedbacks(int defaultValue) {
			PositiveFeedbacks = AmazonPositiveFeedbacks + EbayPositiveFeedbacks;

			if (PositiveFeedbacks == 0)
				PositiveFeedbacks = NumberOfPaypalPositiveTransactions;

			if (PositiveFeedbacks == 0)
				PositiveFeedbacks = defaultValue;

			this.log.Debug(
				"Primary medal - positive feedbacks:\n" +
				"\tAmazon: {0}\n\teBay: {1}\n\tPay Pal: {2}\n\tDefault: {3}\n\tFinal: {4}",
				AmazonPositiveFeedbacks,
				EbayPositiveFeedbacks,
				NumberOfPaypalPositiveTransactions,
				defaultValue,
				PositiveFeedbacks
			);
		} // CalculateFeedbacks

		public MaritalStatus MaritalStatus { get; private set; }

		public int NumberOfStores { get; set; }
		public int PositiveFeedbacks { get; private set; }
		[Traversable]
		public DateTime? EzbobSeniority { get; set; }
		[Traversable]
		public int NumOfLoans { get; set; }
		[Traversable]
		public int NumOfLateRepayments { get; set; }
		[Traversable]
		public int NumOfEarlyRepayments { get; set; }
		public decimal ValueAdded { get; set; }
		public decimal HmrcAnnualTurnover { get; set; }
		public decimal BankAnnualTurnover { get; set; }
		public decimal OnlineAnnualTurnover { get; set; }
		[Traversable]
		public bool FirstRepaymentDatePassed { get; set; }
		public int NumOfHmrcMps { get; set; }
		public int NumOfBanks { get; set; }
		[Traversable]
		public int ZooplaValue { get; set; }
		public DateTime? EarliestHmrcLastUpdateDate { get; set; }
		public DateTime? EarliestYodleeLastUpdateDate { get; set; }
		[Traversable]
		public int AmazonPositiveFeedbacks { get; set; }
		[Traversable]
		public int EbayPositiveFeedbacks { get; set; }
		[Traversable]
		[FieldName("NumOfPaypalPositiveTransactions")]
		public int NumberOfPaypalPositiveTransactions { get; set; }
		public decimal MortgageBalance { get; set; }

		public DBMatrix CapOfferByCustomerScoresTable { get; set; }

		// Calculated data
		public decimal AnnualTurnover { get; set; }
		public decimal TangibleEquity { get; set; }
		public decimal FreeCashFlow { get; set; }
		public TurnoverType? TurnoverType { get; set; }

		public decimal CapOfferByCustomerScoresValue {
			get {
				if (CapOfferByCustomerScoresTable == null)
					return 0;

				return CapOfferByCustomerScoresTable.IsInitialized
					? (CapOfferByCustomerScoresTable[BusinessScore, ConsumerScore] ?? 0)
					: 0;
			} // get
		} // CapOfferByCustomerScoresValue

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
		public EZBob.DatabaseLib.Model.Database.Medal MedalClassification { get; set; }
		public int OfferedLoanAmount { get; set; }
		public int MaxOfferedLoanAmount { get; set; }
		public Exception ExceptionDuringCalculation { get; set; }

		public string Error { get { return string.Join(" ", FullErrorList); } }

		public bool HasError {
			get { return (ExceptionDuringCalculation != null) || (this.errors.Count > 0) || this.wasMismatch; }
		} // HasError

		public static int RoundOfferedAmount(decimal amount) {
			decimal roundTo = CurrentValues.Instance.GetCashSliderStep;

			if (roundTo < 0.0000001m)
				roundTo = 1;

			return (int)(Math.Truncate(amount / roundTo) * roundTo);
		} // RoundOfferedAmount

		public int RoundOfferedAmount() {
			return RoundOfferedAmount(OfferedLoanAmount);
		} // RoundOfferedAmount

		public int RoundMaxOfferedAmount() {
			return RoundOfferedAmount(MaxOfferedLoanAmount);
		} // RoundMaxOfferedAmount

		public bool OfferedAmountsDiffer() {
			return RoundOfferedAmount() != RoundMaxOfferedAmount();
		} // OfferedAmountsDiffer

		public bool UseHmrc() {
			return (NumOfHmrcMps > 0) && (TurnoverType == Ezbob.Backend.Strategies.MedalCalculations.TurnoverType.HMRC);
		} // UseHmrc

		public void CheckForMatch(MedalOutputModel other) {
			// Other is null, i.e. it was not calculated, so it is a match.
			if (other == null) {
				this.wasMismatch = false;
				return;
			} // if

			if (!string.IsNullOrWhiteSpace(other.Error)) {
				AddError("Error in verification calculation: " + other.Error);
				return;
			} // if

			// At this point both are not null and no error reported.

			// if NoMedal in both, no need to compare any other field.
			if ((MedalType == MedalType.NoMedal) && (other.MedalType == AutomationCalculator.Common.MedalType.NoMedal)) {
				this.wasMismatch = false;
				return;
			} // if

			this.wasMismatch =
				MedalType.ToString() != other.MedalType.ToString() ||
				Math.Abs(ValueAdded - other.ValueAdded) > 0.001M ||
				Math.Abs(TotalScore - other.Score * 100) > 0.001M ||
				Math.Abs(TotalScoreNormalized - other.NormalizedScore) > 0.001M ||
				MedalClassification.ToString() != other.Medal.ToString() ||
				Math.Abs(OfferedLoanAmount - other.OfferedLoanAmount) > 100; // TODO understand the difference in yodlee calculation and change the allowed diff to 0.01M
		} // CheckForMatch

		public void SaveToDb(long? cashRequestID, long? nlCashRequestID, string tag, AConnection db) {
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
				new QueryParameter(
					"BusinessSeniority",
					BusinessSeniority.HasValue && BusinessSeniority.Value.Year > 1800 ? BusinessSeniority : null
				),
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
				new QueryParameter("InnerFlowName", TurnoverType == null ? null : TurnoverType.ToString()),
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
				new QueryParameter("MortgageBalance", MortgageBalance),
				new QueryParameter("CapOfferByCustomerScoresValue", CapOfferByCustomerScoresValue),
				new QueryParameter(
					"CapOfferByCustomerScoresTable",
					CapOfferByCustomerScoresTable.SafeToFormattedString()
				),
				new QueryParameter("Tag", tag),
				new QueryParameter("MaxOfferedLoanAmount", MaxOfferedLoanAmount),
				new QueryParameter("CashRequestID", cashRequestID),
				new QueryParameter("NLCashRequestID", nlCashRequestID)
			);
		} // SaveToDb

		public override string ToString() {
			var sb = new StringBuilder();

			sb.AppendFormat(
				"Calculation Num 1 .........Medal Type {2} Medal: {0} " +
				"NormalizedScore: {1}% Score: {3} Offered amount: {4}\n",
				MedalClassification,
				StringBuilderExtention.ToPercent(TotalScoreNormalized),
				MedalType,
				TotalScore,
				OfferedLoanAmount
			);

			sb.AppendFormat(
				"{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8} \n",
				"Parameter".PadRight(25),
				"Weight".PadRight(10),
				"MinScore".PadRight(10),
				"MaxScore".PadRight(10),
				"MinGrade".PadRight(10),
				"MaxGrade".PadRight(10),
				"Grade".PadRight(10),
				"Score".PadRight(10),
				"Value"
			);

			var summary = new Weight();

			Weight weight;

			if (MedalType != MedalType.SoleTrader) {
				weight = new Weight {
					Value = BusinessScore.ToString(CultureInfo.InvariantCulture),
					FinalWeight = BusinessScoreWeight,
					Grade = (int)BusinessScoreGrade,
					Score = BusinessScoreScore
				};
				sb.AddWeight(weight, "BusinessScore", ref summary);
			} // if

			if (MedalType == MedalType.Limited || MedalType == MedalType.OnlineLimited) {
				weight = new Weight {
					Value = TangibleEquity.ToString(CultureInfo.InvariantCulture),
					FinalWeight = TangibleEquityWeight,
					Grade = (int)TangibleEquityGrade,
					Score = TangibleEquityScore
				};
				sb.AddWeight(weight, "TangibleEquity", ref summary);
			} // if

			DateTime calcTime = CalculationTime;
			int businessSeniorityYears = 0;
			decimal ezbobSeniorityMonth = 0;
			if (BusinessSeniority.HasValue)
				businessSeniorityYears = (int)(calcTime - BusinessSeniority.Value).TotalDays / 365;

			if (EzbobSeniority.HasValue)
				ezbobSeniorityMonth = (decimal)(calcTime - EzbobSeniority.Value).TotalDays / (365.0M / 12.0M);

			weight = new Weight {
				Value = businessSeniorityYears.ToString(CultureInfo.InvariantCulture),
				FinalWeight = BusinessSeniorityWeight,
				Grade = (int)BusinessSeniorityGrade,
				Score = BusinessSeniorityScore
			};
			sb.AddWeight(weight, "BusinessSeniority", ref summary);

			weight = new Weight {
				Value = ConsumerScore.ToString(CultureInfo.InvariantCulture),
				FinalWeight = ConsumerScoreWeight,
				Grade = (int)ConsumerScoreGrade,
				Score = ConsumerScoreScore
			};
			sb.AddWeight(weight, "ConsumerScore", ref summary);

			weight = new Weight {
				Value = ezbobSeniorityMonth.ToString(CultureInfo.InvariantCulture),
				FinalWeight = EzbobSeniorityWeight,
				Grade = (int)EzbobSeniorityGrade,
				Score = EzbobSeniorityScore
			};
			sb.AddWeight(weight, "EzbobSeniority", ref summary);

			weight = new Weight {
				Value = MaritalStatus.ToString(),
				FinalWeight = MaritalStatusWeight,
				Grade = (int)MaritalStatusGrade,
				Score = MaritalStatusScore
			};
			sb.AddWeight(weight, "MaritalStatus", ref summary);

			weight = new Weight {
				Value = NumOfLoans.ToString(CultureInfo.InvariantCulture),
				FinalWeight = NumOfLoansWeight,
				Grade = (int)NumOfLoansGrade,
				Score = NumOfLoansScore
			};
			sb.AddWeight(weight, "NumOfLoans", ref summary);

			weight = new Weight {
				Value = NumOfLateRepayments.ToString(CultureInfo.InvariantCulture),
				FinalWeight = NumOfLateRepaymentsWeight,
				Grade = (int)NumOfLateRepaymentsGrade,
				Score = NumOfLateRepaymentsScore
			};
			sb.AddWeight(weight, "NumOfLateRepayments", ref summary);

			weight = new Weight {
				Value = NumOfEarlyRepayments.ToString(CultureInfo.InvariantCulture),
				FinalWeight = NumOfEarlyRepaymentsWeight,
				Grade = (int)NumOfEarlyRepaymentsGrade,
				Score = NumOfEarlyRepaymentsScore
			};
			sb.AddWeight(weight, "NumOfEarlyRepayments", ref summary);

			weight = new Weight {
				Value = AnnualTurnover.ToString(CultureInfo.InvariantCulture),
				FinalWeight = AnnualTurnoverWeight,
				Grade = (int)AnnualTurnoverGrade,
				Score = AnnualTurnoverScore
			};
			sb.AddWeight(weight, "AnnualTurnover", ref summary);

			weight = new Weight {
				Value = FreeCashFlow.ToString(CultureInfo.InvariantCulture),
				FinalWeight = FreeCashFlowWeight,
				Grade = (int)FreeCashFlowGrade,
				Score = FreeCashFlowScore
			};
			sb.AddWeight(weight, "FreeCashFlow", ref summary);

			weight = new Weight {
				Value = NetWorth.ToString(CultureInfo.InvariantCulture),
				FinalWeight = NetWorthWeight,
				Grade = (int)NetWorthGrade,
				Score = NetWorthScore
			};
			sb.AddWeight(weight, "NetWorth", ref summary);

			bool isOnline =
				MedalType == MedalType.OnlineLimited ||
				MedalType == MedalType.OnlineNonLimitedNoBusinessScore ||
				MedalType == MedalType.OnlineNonLimitedWithBusinessScore;

			if (isOnline) {
				weight = new Weight {
					Value = NumberOfStores.ToString(CultureInfo.InvariantCulture),
					FinalWeight = NumberOfStoresWeight,
					Grade = (int)NumberOfStoresGrade,
					Score = NumberOfStoresScore
				};
				sb.AddWeight(weight, "NumOfStores", ref summary);

				weight = new Weight {
					Value = PositiveFeedbacks.ToString(CultureInfo.InvariantCulture),
					FinalWeight = PositiveFeedbacksWeight,
					Grade = (int)PositiveFeedbacksGrade,
					Score = PositiveFeedbacksScore
				};
				sb.AddWeight(weight, "PositiveFeedbacks", ref summary);
			} // if

			sb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------");

			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
				"Sum".PadRight(25),
				StringBuilderExtention.ToShort(summary.FinalWeight)
					.PadRight(10),
				"-".PadRight(10),
				"-".PadRight(10),
				"-".PadRight(10),
				"-".PadRight(10),
				summary.Grade.ToString(CultureInfo.InvariantCulture)
					.PadRight(10),
				StringBuilderExtention.ToShort(summary.Score)
					.PadRight(10), summary.Value);

			return sb.ToString();
		} // ToString

		private void AddError(string errorMsg) {
			if (!string.IsNullOrWhiteSpace(errorMsg))
				this.errors.Add(errorMsg.Trim());
		} // AddError

		private IEnumerable<string> FullErrorList {
			get {
				if (ExceptionDuringCalculation != null)
					yield return "Exception during calculation: " + ExceptionDuringCalculation.Message;

				if (this.wasMismatch)
					yield return "Mismatch detected.";

				foreach (string s in this.errors)
					yield return s;
			} // get
		} // FullErrorList

		private readonly ASafeLog log;
		private readonly List<string> errors;
		private bool wasMismatch;
	} // class MedalResult

	internal static class StringBuilderExtention {
		public static void AddWeight(this StringBuilder sb, Weight weight, string name, ref Weight summary) {
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
				name.PadRight(25),
				ToShort(weight.FinalWeight)
					.PadRight(10),
				"-".PadRight(10),
				"-".PadRight(10),
				"-".PadRight(10),
				"-".PadRight(10),
				weight.Grade.ToString(CultureInfo.InvariantCulture)
					.PadRight(10),
				ToShort(weight.Score)
					.PadRight(10), weight.Value);

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
			} // if
		} // AddWeight

		public static string ToPercent(decimal val) {
			return String.Format("{0:F2}", val * 100).PadRight(6);
		} // ToPercent

		public static string ToShort(decimal val) {
			return String.Format("{0:F2}", val).PadRight(6);
		} // ToShort
	} // class StringBuilderExtention
} // namespace
