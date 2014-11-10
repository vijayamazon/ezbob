namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using ScoreCalculation;

	public enum MedalType
	{
		NoMedal,
		Limited,
		NonLimited,
		OnlineLimited,
		OnlineNonLimitedNoBusinessScore,
		OnlineNonLimitedWithBusinessScore,
		SoleTrader
	}

	public class ScoreResult
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
		public MedalMultiplier Medal { get; set; }
		public string Error { get; set; }
		public int OfferedLoanAmount { get; set; }

		public bool IsIdentical(ScoreResult other)
		{
			if (CustomerId != other.CustomerId ||
				CalculationTime != other.CalculationTime ||
				MedalType != other.MedalType ||
				FirstRepaymentDatePassed != other.FirstRepaymentDatePassed ||
				BusinessScore != other.BusinessScore ||
				BusinessScoreWeight != other.BusinessScoreWeight ||
				BusinessScoreGrade != other.BusinessScoreGrade ||
				BusinessScoreScore != other.BusinessScoreScore ||
				FreeCashFlowValue != other.FreeCashFlowValue ||
				FreeCashFlow != other.FreeCashFlow ||
				FreeCashFlowWeight != other.FreeCashFlowWeight ||
				FreeCashFlowGrade != other.FreeCashFlowGrade ||
				FreeCashFlowScore != other.FreeCashFlowScore ||
				HmrcAnnualTurnover != other.HmrcAnnualTurnover ||
				BankAnnualTurnover != other.BankAnnualTurnover ||
				OnlineAnnualTurnover != other.OnlineAnnualTurnover ||
				AnnualTurnover != other.AnnualTurnover ||
				AnnualTurnoverWeight != other.AnnualTurnoverWeight ||
				AnnualTurnoverGrade != other.AnnualTurnoverGrade ||
				AnnualTurnoverScore != other.AnnualTurnoverScore ||
				TangibleEquityValue != other.TangibleEquityValue ||
				TangibleEquity != other.TangibleEquity ||
				TangibleEquityWeight != other.TangibleEquityWeight ||
				TangibleEquityGrade != other.TangibleEquityGrade ||
				TangibleEquityScore != other.TangibleEquityScore ||
				BusinessSeniority != other.BusinessSeniority ||
				BusinessSeniorityWeight != other.BusinessSeniorityWeight ||
				BusinessSeniorityGrade != other.BusinessSeniorityGrade ||
				BusinessSeniorityScore != other.BusinessSeniorityScore ||
				ConsumerScore != other.ConsumerScore ||
				ConsumerScoreWeight != other.ConsumerScoreWeight ||
				ConsumerScoreGrade != other.ConsumerScoreGrade ||
				ConsumerScoreScore != other.ConsumerScoreScore ||
				NetWorth != other.NetWorth ||
				NetWorthWeight != other.NetWorthWeight ||
				NetWorthGrade != other.NetWorthGrade ||
				NetWorthScore != other.NetWorthScore ||
				MaritalStatus != other.MaritalStatus ||
				MaritalStatusWeight != other.MaritalStatusWeight ||
				MaritalStatusGrade != other.MaritalStatusGrade ||
				MaritalStatusScore != other.MaritalStatusScore ||
				NumberOfStores != other.NumberOfStores ||
				NumberOfStoresWeight != other.NumberOfStoresWeight ||
				NumberOfStoresGrade != other.NumberOfStoresGrade ||
				NumberOfStoresScore != other.NumberOfStoresScore ||
				PositiveFeedbacks != other.PositiveFeedbacks ||
				PositiveFeedbacksWeight != other.PositiveFeedbacksWeight ||
				PositiveFeedbacksGrade != other.PositiveFeedbacksGrade ||
				PositiveFeedbacksScore != other.PositiveFeedbacksScore ||
				EzbobSeniority != other.EzbobSeniority ||
				EzbobSeniorityWeight != other.EzbobSeniorityWeight ||
				EzbobSeniorityGrade != other.EzbobSeniorityGrade ||
				EzbobSeniorityScore != other.EzbobSeniorityScore ||
				NumOfLoans != other.NumOfLoans ||
				NumOfLoansWeight != other.NumOfLoansWeight ||
				NumOfLoansGrade != other.NumOfLoansGrade ||
				NumOfLoansScore != other.NumOfLoansScore ||
				NumOfLateRepayments != other.NumOfLateRepayments ||
				NumOfLateRepaymentsWeight != other.NumOfLateRepaymentsWeight ||
				NumOfLateRepaymentsGrade != other.NumOfLateRepaymentsGrade ||
				NumOfLateRepaymentsScore != other.NumOfLateRepaymentsScore ||
				NumOfEarlyRepayments != other.NumOfEarlyRepayments ||
				NumOfEarlyRepaymentsWeight != other.NumOfEarlyRepaymentsWeight ||
				NumOfEarlyRepaymentsGrade != other.NumOfEarlyRepaymentsGrade ||
				NumOfEarlyRepaymentsScore != other.NumOfEarlyRepaymentsScore ||
				ValueAdded != other.ValueAdded ||
				InnerFlowName != other.InnerFlowName ||
				TotalScore != other.TotalScore ||
				TotalScoreNormalized != other.TotalScoreNormalized ||
				Medal != other.Medal ||
				Error != other.Error ||
				OfferedLoanAmount != other.OfferedLoanAmount ||
				NumOfHmrcMps != other.NumOfHmrcMps ||
				ZooplaValue != other.ZooplaValue ||
				EarliestHmrcLastUpdateDate != other.EarliestHmrcLastUpdateDate ||
				EarliestYodleeLastUpdateDate != other.EarliestYodleeLastUpdateDate ||
				AmazonPositiveFeedbacks != other.AmazonPositiveFeedbacks ||
				EbayPositiveFeedbacks != other.EbayPositiveFeedbacks ||
				NumberOfPaypalPositiveTransactions != other.NumberOfPaypalPositiveTransactions ||
				MortgageBalance != other.MortgageBalance)
			{
				return false;
			}

			return true;
		}

		public void SaveToDb(AConnection db)
		{
			db.ExecuteNonQuery("StoreNewMedal", CommandSpecies.StoredProcedure,
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
							   new QueryParameter("Medal", Medal.ToString()),
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
	}
}
