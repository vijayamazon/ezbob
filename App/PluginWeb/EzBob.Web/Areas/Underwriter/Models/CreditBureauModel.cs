using System;
using System.Collections.Generic;
using System.Linq;

namespace EzBob.Web.Areas.Underwriter.Models
{
	public class CreditBureauModel
	{
		public IOrderedEnumerable<CheckHistoryModel> ConsumerHistory { get; set; }
		public IOrderedEnumerable<CheckHistoryModel> CompanyHistory { get; set; }

		public int Id { get; set; }
		public string Name { get; set; }
		public string MiddleName { get; set; }
		public string Surname { get; set; }
		public string FullName { get; set; }
		public string ApplicantFullName { get; set; }
		public string CompanyName { get; set; }
		public bool HasExperianError { get; set; }
		public bool HasParsingError { get; set; }

		public bool IsError
		{
			get { return ErrorList != null && ErrorList.Count > 0; }
		}

		public List<string> ErrorList { get; set; }

		public string ModelType { get; set; }

		public bool CheckFailed
		{
			get { return CheckStatus == "Error"; }
		}

		public string CheckStatus { get; set; } // done
		public string CheckIcon { get; set; } // done
		public string ButtonStyle { get; set; } // done
		public string BorrowerType { get; set; } // done
		public string CheckDate { get; set; } // done
		public string CheckValidity { get; set; } // done
		public int Score { get; set; } // done
		public double Odds { get; set; } // done
		public string ScoreColor { get; set; } // done 
		public string ScorePosition { get; set; } // done
		public string ScoreAlign { get; set; } // done
		public string ScoreValuePosition { get; set; } // done
		public int CII { get; set; } // done
		public ConsumerSummaryCharacteristics ConsumerSummaryCharacteristics { get; set; }
		public ConsumerAccountsOverview ConsumerAccountsOverview { get; set; }
		public AccountInfo[] AccountsInformation { get; set; }

		public bool HasNOCs
		{
			get { return (NOCs != null) && (NOCs.Length > 0); }
		}

		public NOCInfo[] NOCs { get; set; }
		public AMLInfo AmlInfo { get; set; }
		public BankAccountVerificationInfo BavInfo { get; set; }
		public ExperianLimitedInfo LimitedInfo { get; set; }
		public ExperianNonLimitedInfo NonLimitedInfo { get; set; }
		public CreditBureauModel[] directorsModels { get; set; }
		public Summary Summary { get; set; }
	}

	public class ConsumerSummaryCharacteristics
	{
		public int NumberOfAccounts { get; set; } // done
		public int NumberOfAccounts3M { get; set; } // done
		public string WorstCurrentStatus { get; set; } // done
		public string WorstCurrentStatus3M { get; set; } // done
		public int NumberOfDefaults { get; set; } // done
		public int NumberOfCCJs { get; set; } // done
		public int AgeOfMostRecentCCJ { get; set; } // done
		public int NumberOfCCOverLimit { get; set; } // done
		public int EnquiriesLast3M { get; set; } // done
		public int EnquiriesLast6M { get; set; } // done
		public int CreditCardUtilization { get; set; } // done
		public string DSRandOwnershipType { get; set; } // !DSR
		public bool NOCsOnCCJ { get; set; } // done
		public bool NOCsOnCAIS { get; set; } // done
		public bool SatisfiedJudgements { get; set; }
		public string CAISSpecialInstructionFlag { get; set; } // done


		public int NumberOfLates { get; set; }
		public string LateStatus { get; set; }
		public int DefaultAmount { get; set; }

		// done 
	}

	public class ConsumerAccountsOverview
	{
		public int OpenAccounts_CC { get; set; } // done
		public string WorstArrears_CC { get; set; } // done
		public int TotalCurLimits_CC { get; set; } // done
		public int Balance_CC { get; set; } // done
		public int OpenAccounts_Mtg { get; set; } // done
		public string WorstArrears_Mtg { get; set; } // done
		public int TotalCurLimits_Mtg { get; set; } // done
		public int Balance_Mtg { get; set; } // done
		public int OpenAccounts_PL { get; set; } // done
		public string WorstArrears_PL { get; set; } // done
		public int TotalCurLimits_PL { get; set; } // done
		public int Balance_PL { get; set; } // done
		public int OpenAccounts_Other { get; set; } // done
		public string WorstArrears_Other { get; set; } // done
		public int TotalCurLimits_Other { get; set; } // done
		public int Balance_Other { get; set; } // done
		public int OpenAccounts_Total { get; set; } // done
		public string WorstArrears_Total { get; set; } // done
		public int TotalCurLimits_Total { get; set; } // done
		public int Balance_Total { get; set; } // done
	}

	public class AccountInfo
	{
		public DateTime? OpenDate { get; set; } // done
		public string Account { get; set; } // done
		public string TermAndfreq { get; set; } // done
		public int? Limit { get; set; } // done
		public int? AccBalance { get; set; } // done
		public string AccountStatus { get; set; } // done
		public string DateType { get; set; } // done
		public DateTime SettlementDate { get; set; } // done
		public string CashWithdrawals { get; set; } // done
		public string MinimumPayment { get; set; } // done
		public AccountDisplayedYear[] Years { get; set; } // done
		public AccountDisplayedQuarter[] Quarters { get; set; } // done
		public string[] MonthsDisplayed { get; set; } // done
		public AccountStatus[] LatestStatuses { get; set; } // colors
		public string MatchTo { get; set; }
	}

	public class AccountDisplayedYear
	{
		public int Year { get; set; }
		public int Span { get; set; }
	}

	public class AccountDisplayedQuarter
	{
		public string Quarter { get; set; }
		public int Span { get; set; }
	}

	public class AccountStatus
	{
		public string Status { get; set; }
		public string StatusColor { get; set; }
	}

	public class NOCInfo
	{
		public string NOCReference { get; set; }
		public string NOCLines { get; set; }
	}

	public class AMLInfo
	{
		public bool HasAML { get; set; }
		public string AMLResult { get; set; }
		public decimal AuthenticationIndexType { get; set; }
		public string AuthIndexText { get; set; }
		public decimal NumPrimDataItems { get; set; }
		public decimal NumPrimDataSources { get; set; }
		public decimal NumSecDataItems { get; set; }
		public string StartDateOldestPrim { get; set; }
		public string StartDateOldestSec { get; set; }
		public decimal ReturnedHRPCount { get; set; }
	}

	public class BankAccountVerificationInfo
	{
		public bool HasBWA { get; set; }
		public string BankAccountVerificationResult { get; set; }
		public string AuthenticationText { get; set; }
		public string AccountStatus { get; set; }
		public decimal NameScore { get; set; }
		public decimal AddressScore { get; set; }
	}

	public class ExperianCompanyInfo
	{
		public decimal BureauScore { get; set; }
		public string ScoreColor { get; set; }
		public bool IsDataExpired { get; set; }
		public bool IsError { get; set; }
		public string Error { get; set; }
		public DateTime? LastCheckDate { get; set; }
	}

	public class ExperianLimitedInfo : ExperianCompanyInfo
	{
		public string RiskLevel { get; set; }
		public decimal ExistingBusinessLoans { get; set; }
	}

	public class ExperianNonLimitedInfo : ExperianCompanyInfo
	{
		public bool CompanyNotFoundOnBureau { get; set; }
	}

	public class Summary
	{
		public int Score { get; set; }
		public int ConsumerIndebtednessIndex { get; set; }
		public string CheckDate { get; set; }
		public string Validtill { get; set; }
		public string WorstCurrentstatus { get; set; }
		public string WorstHistoricalstatus { get; set; }
		public int Numberofdefaults { get; set; }
		public int Accounts { get; set; }
		public int CCJs { get; set; }
		public int MostrecentCCJ { get; set; }
		public string DSRandownershiptype { get; set; }
		public int Creditcardutilization { get; set; }
		public int Enquiriesinlast6months { get; set; }
		public int Enquiriesinlast3months { get; set; }
		public int Totalbalance { get; set; }
		public string AML { get; set; }
		public string AMLnum { get; set; }
		public string BWA { get; set; }
		public string BWAnum { get; set; }
		public string Businesstype { get; set; }
		public string BusinessScore { get; set; }
		public string RiskLevel { get; set; }
		public string Existingbusinessloans { get; set; }
		public bool ThinFile { get; set; }
		public ConsumerAccountsOverview ConsumerAccountsOverview { get; set; }
	}

	public class CheckHistoryModel
	{
		public DateTime Date { get; set; }
		public int Score { get; set; }
		public int CII { get; set; }
		public decimal? Balance { get; set; }
		public long Id { get; set; }
	}

	public class DelphiModel
	{
		public string Position { get; set; }
		public string Align { get; set; }
		public string ValPosition { get; set; }
		public string Color { get; set; }
	}
}