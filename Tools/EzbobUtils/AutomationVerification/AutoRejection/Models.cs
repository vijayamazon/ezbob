namespace AutomationCalculator
{
	using System;
	using System.Collections.Generic;

	public class ScoreMedalOffer
	{
		public Medal Medal { get; set; }
		public decimal Score { get; set; }
		public int MaxOffer { get; set; }
		public decimal MaxOfferPercent { get; set; }
	}

	public class Weight
	{
		public decimal FinalWeightFixedWeightParameter { get; set; }
		public decimal StandardWeightFixedWeightParameter { get; set; }
		public decimal StandardWeightAdjustableWeightParameter { get; set; }
		public decimal DeltaForAdjustableWeightParameter { get; set; }
		public decimal FinalWeight { get; set; }
		public decimal MinimumScore { get; set; }
		public decimal MaximumScore { get; set; }
		public int MinimumGrade { get; set; }
		public int MaximumGrade { get; set; }

		public int Grade { get; set; }
		public decimal Score { get; set; }
	}

	public class Range
	{
		public decimal? MinValue { get; set; }
		public decimal? MaxValue { get; set; }

		public bool IsInRange(decimal value)
		{
			if (MinValue.HasValue && MaxValue.HasValue && value >= MinValue.Value && value <= MaxValue.Value) return true;
			if (MaxValue.HasValue && !MinValue.HasValue && value <= MaxValue.Value) return true;
			if (MinValue.HasValue && !MaxValue.HasValue && value >= MinValue.Value) return true;
			return false;
		}
	}

	public class RangeGrage : Range
	{
		public int Grade { get; set; }
	}

	public class RangeMedal : Range
	{
		public Medal Medal { get; set; }
	}

	public class RangeOfferPercent : Range
	{
		public decimal OfferPercent { get; set; }
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
		public string CustomerStatus { get; set; }

		public int ExperianScore { get; set; }
		public int CompanyScore { get; set; }
		
		public double AnualTurnover { get; set; }
		public double ThreeMonthTurnover { get; set; }
		public int MpsSeniority { get; set; }

		public bool WasApproved { get; set; }
		
		public int DefaultAccountsNum { get; set; }
		public int DefaultAccountAmount { get; set; }

		public int DefaultCompanyAccountsNum { get; set; }
		public int DefaultCompanyAccountAmount { get; set; }

		public int NumLateAccounts { get; set; } // todo personal/company?

		public bool HasErrorMp { get; set; }
		public bool HasCompanyFiles { get; set; }
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
}
