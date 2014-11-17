﻿namespace AutomationCalculator.Common
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Ezbob.Logger;

	public class MedalOutputModel
	{
		public Medal Medal { get; set; }
		public MedalType MedalType { get; set; }
		public decimal Score { get; set; }
		public decimal NormalizedScore { get; set; }
		public string Error { get; set; }
		public int CustomerId { get; set; }
		public DateTime CalculationDate { get; set; }

		public MedalComparisonModel MedalComparisonModel { get; set; } //todo remove
		public Dictionary<Parameter, Weight> Dict { get; set; }

		public bool FirstRepaymentDatePassed { get; set; }
		public int OfferedLoanAmount { get; set; }
		public int NumOfHmrcMps { get; set; }
		public DateTime? EarliestHmrcLastUpdateDate { get; set; }
		public DateTime? EarliestYodleeLastUpdateDate { get; set; }
		public int AmazonPositiveFeedbacks { get; set; }
		public int EbayPositiveFeedbacks { get; set; }
		public int NumberOfPaypalPositiveTransactions { get; set; }
		public decimal ValueAdded { get; set; }
		public decimal FreeCashflow { get; set; }
		public decimal AnnualTurnover { get; set; }
		public bool UseHmrc { get; set; }

		public void SaveToDb(ASafeLog log)
		{
			PrintDict(log);
			var dbHelper = new DbHelper(log);
			dbHelper.StoreMedalVerification(this);
		}

		public void PrintDict(ASafeLog log)
		{
			Dictionary<Parameter, Weight> dict = Dict ?? new Dictionary<Parameter, Weight>();
			var sb = new StringBuilder();
			sb.AppendFormat("Medal Type {2} Medal: {0} NormalizedScore: {1}% Score: {3}\n", Medal, ToPercent(NormalizedScore), MedalType, Score);
			decimal s5 = 0M, s6 = 0M, s7 = 0M, s8 = 0M, s9 = 0M, s10 = 0M, s11 = 0M;
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8} \n", "Parameter".PadRight(25), "Weight".PadRight(10), "MinScore".PadRight(10), "MaxScore".PadRight(10), "MinGrade".PadRight(10), "MaxGrade".PadRight(10), "Grade".PadRight(10), "Score".PadRight(10), "Value");
			foreach (var weight in dict)
			{

				sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}| {8}\n",
					weight.Key.ToString().PadRight(25),
					ToPercent(weight.Value.FinalWeight).PadRight(10),
					ToPercent(weight.Value.MinimumScore / 100).PadRight(10),
					ToPercent(weight.Value.MaximumScore / 100).PadRight(10),
					weight.Value.MinimumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
					weight.Value.MaximumGrade.ToString(CultureInfo.InvariantCulture).PadRight(10),
					weight.Value.Grade.ToString(CultureInfo.InvariantCulture).PadRight(10),
					ToShort(weight.Value.Score * 100).PadRight(10), weight.Value.Value);
				s5 += weight.Value.FinalWeight;
				s6 += weight.Value.MinimumScore;
				s7 += weight.Value.MaximumScore;
				s8 += weight.Value.MinimumGrade;
				s9 += weight.Value.MaximumGrade;
				s11 += weight.Value.Grade;
				s10 += weight.Value.Score;
			}
			sb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------");
			sb.AppendFormat("{0}| {1}| {2}| {3}| {4}| {5}| {6}| {7}|\n",
				"Sum".PadRight(25),
				ToPercent(s5).PadRight(10),
				ToPercent(s6 / 100).PadRight(10),
				ToPercent(s7 / 100).PadRight(10),
				s8.ToString(CultureInfo.InvariantCulture).PadRight(10),
				s9.ToString(CultureInfo.InvariantCulture).PadRight(10),
				s11.ToString(CultureInfo.InvariantCulture).PadRight(10),
				ToShort(s10 * 100).PadRight(10));

			log.Debug(sb.ToString());
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

	public class MedalComparisonModel {
		public int CustomerId { get; set; }
		public MedalType MedalType { get; set; }
		public Weight BusinessScore { get; set; }
		public Weight FreeCashFlow { get; set; }
		public Weight AnnualTurnover { get; set; }
		public Weight TangibleEquity { get; set; }
		public Weight BusinessSeniority { get; set; }
		public Weight ConsumerScore { get; set; }
		public Weight NetWorth { get; set; }
		public Weight MaritalStatus { get; set; }
		public Weight NumOfStores { get; set; }
		public Weight PositiveFeedbacks { get; set; }
		public Weight EzbobSeniority { get; set; }
		public Weight NumOfLoans { get; set; }
		public Weight NumOfLateRepayments { get; set; }
		public Weight NumOfEarlyRepayments { get; set; }

		public decimal? ValueAdded { get; set; }
		public string InnerFlow { get; set; }
		public string Error { get; set; }
		public decimal? HmrcTurnover { get; set; }
		public decimal? BankTurnover { get; set; }
		public decimal? OnlineTurnover { get; set; }

		public Medal Medal { get; set; }
		public decimal TotalScore { get; set; }
		public decimal TotalScoreNormalized { get; set; }

		public DateTime CalculationTime { get; set; }
		public bool FirstRepaymentDatePassed { get; set; }
		public int OfferedLoanAmount { get; set; }
		public int NumOfHmrcMps { get; set; }
		public DateTime? EarliestHmrcLastUpdateDate { get; set; }
		public DateTime? EarliestYodleeLastUpdateDate { get; set; }
		public int AmazonPositiveFeedbacks { get; set; }
		public int EbayPositiveFeedbacks { get; set; }
		public int NumberOfPaypalPositiveTransactions { get; set; }
		public decimal MortageBalance { get; set; }
	}

	public class Weight
	{
		public string Value { get; set; }
		public decimal FinalWeight { get; set; }
		public decimal MinimumScore { get; set; }
		public decimal MaximumScore { get; set; }
		public int MinimumGrade { get; set; }
		public int MaximumGrade { get; set; }
		public int Grade { get; set; }
		public decimal Score { get; set; }
	}

	public class AutoDecision
	{
		public int CashRequestId { get; set; }
		public int CustomerId { get; set; }
		public Decision SystemDecision { get; set; }
		public DateTime SystemDecisionDate { get; set; }
		public int SystemCalculatedSum { get; set; }
		public int SystemApprovedSum { get; set; }
		public Medal MedalType { get; set; }
		public int RepaymentPeriod { get; set; }
		public double ScorePoints { get; set; }
		public int ExpirianRating { get; set; }
		public int AnualTurnover { get; set; }
		public double InterestRate { get; set; }
		public bool HasLoans { get; set; }
		public string Comment { get; set; }
	}

	public class VerificationReport
	{
		public int CashRequestId { get; set; }
		public int CustomerId { get; set; }
		public Decision SystemDecision { get; set; }
		public Decision VerificationDecision { get; set; }
		public string SystemComment { get; set; }
		public string VerificationComment { get; set; }
		public int SystemCalculatedSum { get; set; }
		public int SystemApprovedSum { get; set; }
		public int VerificationApprovedSum { get; set; }
		public bool IsMatch { get; set; }
	}

	public class MarketPlace
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public DateTime? OriginationDate { get; set; }
	}

	public class AnalysisFunction
	{
		public DateTime Updated { get; set; }
		public string Function { get; set; }
		public TimePeriodEnum TimePeriod { get; set; }
		public double Value { get; set; }
		public string MarketPlaceName { get; set; }
	}

	public class OnlineRevenues {
		public decimal AnnualizedRevenue1M { get; set; }
		public decimal AnnualizedRevenue3M { get; set; }
		public decimal AnnualizedRevenue6M { get; set; }
		public decimal AnnualizedRevenue1Y { get; set; }
		public decimal Revenue1Y{ get; set; }
	}

	public static class AnalysisFunctionIncome
	{
		public static string[] IncomeFunctions =
			{
				"TotalIncome",
				"TotalNetInPayments",
				"TotalSumOfOrders"
			};
	}

	public class ReRejectionData
	{
		public DateTime? ManualRejectDate { get; set; }
		public DateTime AutomaticDecisionDate { get; set; }
		public bool IsNewClient { get; set; }
		public bool NewDataSourceAdded { get; set; }
		public decimal RepaidAmount { get; set; }
		public int LoanAmount { get; set; }
	}

	public class RejectionConstants
	{
		public int MinCreditScore { get; set; }
		public int MinCompanyCreditScore { get; set; }
		public int MinAnnualTurnover { get; set; }
		public int MinThreeMonthTurnover { get; set; }

		public int LowOfflineAnnualRevenue { get; set; }
		public int LowOfflineQuarterRevenue { get; set; }

		public int DefaultScoreBelow { get; set; }
		public int DefaultMinAmount { get; set; }
		public int DefaultMinAccountsNum { get; set; }

		public int DefaultCompanyScoreBelow { get; set; }
		public int DefaultCompanyMinAmount { get; set; }
		public int DefaultCompanyMinAccountsNum { get; set; }

		public int LateAccountMinDays { get; set; }
		public int LateAccountMinNumber { get; set; }
		public int LateAccountLastMonth { get; set; }

		public int MinMarketPlaceSeniorityDays { get; set; }

		public int NoRejectIfTotalAnnualTurnoverAbove { get; set; }
		public int NoRejectIfCreditScoreAbove { get; set; }
		public int NoRejectIfCompanyCreditScoreAbove { get; set; }

		public int AutoRejectIfErrorInAtLeastOneMPMinScore { get; set; }
		public int AutoRejectIfErrorInAtLeastOneMPMinCompanyScore { get; set; }
	}

	public class RejectionData
	{
		//from sp
		public string CustomerStatus { get; set; }

		public int ExperianScore { get; set; }
		public int CompanyScore { get; set; }
		
		public bool WasApproved { get; set; }
		
		public int DefaultAccountsNum { get; set; }
		public int DefaultAccountAmount { get; set; }

		public int DefaultCompanyAccountsNum { get; set; }
		public int DefaultCompanyAccountAmount { get; set; }

		public int NumLateAccounts { get; set; } // todo personal/company?

		public bool HasErrorMp { get; set; }
		public bool HasCompanyFiles { get; set; }

		//Calculated
		public double AnualTurnover { get; set; }
		public double ThreeMonthTurnover { get; set; }
		public int MpsSeniority { get; set; }

		public bool IsBrokerLead { get; set; }
	}

	public class ReApprovalData
	{
		public DateTime? ManualApproveDate { get; set; }
		public bool IsNewClient { get; set; }
		public bool NewDataSourceAdded { get; set; }
		public int OfferedAmount { get; set; }
		public int TookAmountLastRequest { get; set; }
		public bool TookLoanLastRequest { get; set; }
		public decimal PrincipalRepaymentsSinceOffer { get; set; }
		public bool WasLate { get; set; }
	}

	public class PositiveFeedbacksModelDb {
		public int AmazonFeedbacks { get; set; }
		public int EbayFeedbacks { get; set; }
		public int PaypalFeedbacks { get; set; }
		public int DefaultFeedbacks { get; set; }
	}

	public class MedalChooserInputModelDb {
		public bool IsLimited { get; set; }
		public bool HasOnline { get; set; } //ebay/amazon/paypal mp
		public bool HasHmrc { get; set; }
		public bool HasCompanyScore { get; set; }
		public bool HasPersonalScore { get; set; }
		public bool HasBank { get; set; }
		public int NumOfHmrc { get; set; }
		public DateTime? LastBankHmrcUpdateDate { get; set; }
		public int MedalDaysOfMpRelevancy { get; set; }
		public int MinApprovalAmount { get; set; }
	}

	public class MedalInputModelDb {
		public int BusinessScore { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public decimal TangibleEquity { get; set; }
		public int ConsumerScore { get; set; }
		public DateTime RegistrationDate { get; set; }
		public string MaritalStatus { get; set; }
		public string TypeOfBusiness { get; set; }
		public int NumOfOnTimeLoans { get; set; }
		public int NumOfLatePayments { get; set; }
		public int NumOfEarlyPayments { get; set; }
		public bool HasHmrc { get; set; }
		public bool HasMoreThanOneHmrc { get; set; }
		public decimal HmrcRevenues { get; set; }
		public decimal HmrcFreeCashFlow { get; set; }
		public decimal HmrcValueAdded { get; set; }
		public decimal FCFFactor { get; set; }
		public decimal CurrentBalanceSum { get; set; }
		public int ZooplaValue { get; set; }
		public int Mortages { get; set; }
		public DateTime? FirstRepaymentDate { get; set; }
		public int NumOfStores { get; set; }
		public decimal OnlineMedalTurnoverCutoff { get; set; }
	}

	public class MedalInputModel {
		public int CustomerId { get; set; }
		public DateTime CalculationDate { get; set; }
		public int BusinessScore { get; set; }
		public decimal TangibleEquity { get; set; }
		public int BusinessSeniority { get; set; }
		public int ConsumerScore { get; set; }
		public decimal EzbobSeniority { get; set; }
		public MaritalStatus MaritalStatus { get; set; }
		public int NumOfOnTimeLoans { get; set; }
		public int NumOfLatePayments { get; set; }
		public int NumOfEarlyPayments { get; set; }
		public decimal AnnualTurnover { get; set; }
		public decimal FreeCashFlow { get; set; }
		public decimal FreeCashFlowValue { get; set; }
		public decimal ValueAdded { get; set; }
		public decimal NetWorth { get; set; }
		public bool HasHmrc { get; set; }
		public bool UseHmrc { get; set; }
		public bool FirstRepaymentDatePassed { get; set; }
		
		/*online only*/
		public int NumOfStores { get; set; }
		public int PositiveFeedbacks { get; set; }

		public MedalInputModelDb MedalInputModelDb { get; set; }
		public override string ToString()
		{
			return string.Format(
				@"Medal input params:
					BusinessScore: {0}, 
					TangibleEquity: {1}, 
					BusinessSeniority: {2}, 
					ConsumerScore: {3}, 
					EzbobSeniority: {4}, 
					MaritalStatus: {5}, 
					NumOfOnTimeLoans: {6}, 
					NumOfLatePayments: {7}, 
					NumOfEarlyPayments: {8}, 
					AnnualTurnover: {9}, 
					FreeCashFlow: {10}, 
					NetWorth: {11},
					ValueAdded: {12},
					FirstRepaymentDatePassed: {13},

					NumOfStores: {14}
					PositiveFeedbacks: {15}"
				,
				BusinessScore, TangibleEquity, BusinessSeniority, ConsumerScore, EzbobSeniority,
				MaritalStatus, NumOfOnTimeLoans, NumOfLatePayments, NumOfEarlyPayments, AnnualTurnover,
				FreeCashFlow, NetWorth, ValueAdded, FirstRepaymentDatePassed, NumOfStores, PositiveFeedbacks); 
		}
	}

	public class YodleeRevenuesModelDb {
		public decimal YodleeRevenues { get; set; }
		public DateTime? MinDate { get; set; }
		public DateTime? MaxDate { get; set; }
	}

	public class MedalCoefficientsModelDb {
		public Medal Medal { get; set; }
		public decimal AnnualTurnover { get; set; }
		public decimal ValueAdded { get; set; }
		public decimal FreeCashFlow { get; set; }
	}

	public class PricingScenarioModel
	{
		public string ScenarioName { get; set; }
		public decimal TenurePercents { get; set; }
		public decimal SetupFee { get; set; }
		public decimal ProfitMarkupPercentsOfRevenue { get; set; }
		public decimal OpexAndCapex { get; set; }
		public decimal InterestOnlyPeriod { get; set; }
		public decimal EuCollectionRate { get; set; }
		public decimal DefaultRateCompanyShare { get; set; }
		public decimal DebtPercentOfCapital { get; set; }
		public decimal CostOfDebtPA { get; set; }
		public decimal CollectionRate { get; set; }
		public decimal Cogs { get; set; }
		public decimal BrokerSetupFee { get; set; }
	}
	public class OfferInputModel {
		public int Amount { get; set; }
		public bool HasLoans { get; set; }
		public Medal Medal { get; set; }
		public bool AspireToMinSetupFee { get; set; }
	}

	public class OfferOutputModel
	{
		public int RepaymentPeriod { get { return 12; } } //Currently Hard coded
		public LoanSource LoanSource { get { return LoanSource.Standard; } } //Currently Hard coded
		public LoanType LoanType { get { return LoanType.StandardLoanType; } } //Currently Hard coded
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
	}

	public class OfferInterestRateRangeModelDb {
		public decimal MinInterestRate { get; set; }
		public decimal MaxInterestRate { get; set; }
	}

	public class OfferSetupFeeRangeModelDb
	{
		public string LoanSizeName { get; set; }
		public decimal MinSetupFee { get; set; }
		public decimal MaxSetupFee { get; set; }
	}
}
