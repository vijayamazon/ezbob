using System;
using System.Collections.Generic;
using System.Linq;

namespace EzBob.Web.Areas.Underwriter.Models
{
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class CreditBureauModel
	{

		public int Id { get; set; } //Customer Id
		public Summary Summary { get; set; }
		public ExperianConsumerModel Consumer { get; set; }
		public List<ExperianConsumerModel> Directors { get; set; }

		public AMLInfo AmlInfo { get; set; }
		public BankAccountVerificationInfo BavInfo { get; set; }
		public IOrderedEnumerable<CheckHistoryModel> CompanyHistory { get; set; }

	}

	public class ExperianConsumerModel
	{
		public IOrderedEnumerable<CheckHistoryModel> ConsumerHistory { get; set; }

		public long? ServiceLogId { get; set; }
		public int Id { get; set; } //CustomerId / DirectorId
		public bool HasExperianError { get; set; }
		public bool HasParsingError { get; set; }

		public bool IsError
		{
			get { return ErrorList != null && ErrorList.Count > 0; }
		}

		public List<string> ErrorList { get; set; }


		public ExperianConsumerDataApplicant Applicant { get; set; }
		public string ApplicantFullNameAge { get; set; }
		public string ModelType { get; set; }
		public string BorrowerType { get; set; } 
		public string CheckDate { get; set; }
		public bool IsDataRelevant { get; set; }
		public string CheckValidity { get; set; } 
		public int? Score { get; set; } 
		public double Odds { get; set; } 
		public string ScoreColor { get; set; }  
		public string ScorePosition { get; set; } 
		public string ScoreAlign { get; set; } 
		public string ScoreValuePosition { get; set; } 
		public int? CII { get; set; } 
		
		public AccountInfo[] AccountsInformation { get; set; }
		public ConsumerAccountsOverview ConsumerAccountsOverview { get; set; }

		public bool HasNOCs
		{
			get { return (NOCs != null) && (NOCs.Length > 0); }
		}

		public NOCInfo[] NOCs { get; set; }

		public int? NumberOfAccounts { get; set; }
		public int? NumberOfAccounts3M { get; set; }
		public string WorstCurrentStatus { get; set; }
		public string WorstCurrentStatus3M { get; set; }
		public int? NumberOfDefaults { get; set; }
		public int? NumberOfCCJs { get; set; }
		public int? AgeOfMostRecentCCJ { get; set; }
		public int? NumberOfCCOverLimit { get; set; }
		public int? EnquiriesLast3M { get; set; }
		public int? EnquiriesLast6M { get; set; }
		public int? CreditCardUtilization { get; set; }
		public bool NOCsOnCCJ { get; set; }
		public bool NOCsOnCAIS { get; set; }
		public bool SatisfiedJudgements { get; set; }
		public string CAISSpecialInstructionFlag { get; set; }
		public int? NumberOfLates { get; set; }
		public string LateStatus { get; set; }
		public int? DefaultAmount { get; set; }
	}

	public class ConsumerAccountsOverview
	{
		public int OpenAccounts_CC { get; set; } 
		public string WorstArrears_CC { get; set; } 
		public int TotalCurLimits_CC { get; set; } 
		public int Balance_CC { get; set; } 
		public int OpenAccounts_Mtg { get; set; } 
		public string WorstArrears_Mtg { get; set; } 
		public int TotalCurLimits_Mtg { get; set; } 
		public int Balance_Mtg { get; set; } 
		public int OpenAccounts_PL { get; set; } 
		public string WorstArrears_PL { get; set; } 
		public int TotalCurLimits_PL { get; set; } 
		public int Balance_PL { get; set; } 
		public int OpenAccounts_Other { get; set; } 
		public string WorstArrears_Other { get; set; } 
		public int TotalCurLimits_Other { get; set; } 
		public int Balance_Other { get; set; } 
		public int OpenAccounts_Total { get; set; } 
		public string WorstArrears_Total { get; set; } 
		public int TotalCurLimits_Total { get; set; } 
		public int Balance_Total { get; set; } 
	}
	

	public class AccountInfo
	{
		public DateTime? OpenDate { get; set; } 
		public string Account { get; set; } 
		public string TermAndfreq { get; set; } 
		public int? Limit { get; set; } 
		public int? AccBalance { get; set; } 
		public string AccountStatus { get; set; } 
		public string DateType { get; set; } 
		public DateTime? SettlementDate { get; set; } 
		public string CashWithdrawals { get; set; } 
		public string MinimumPayment { get; set; } 
		public AccountDisplayedYear[] Years { get; set; } 
		public AccountDisplayedQuarter[] Quarters { get; set; } 
		public string[] MonthsDisplayed { get; set; } 
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
		public int AuthenticationIndex { get; set; }
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
	
	public class Summary
	{
		public int? Score { get; set; }
		public int? ConsumerIndebtednessIndex { get; set; }
		public string CheckDate { get; set; }
		public string Validtill { get; set; }
		public bool IsDataRelevant { get; set; }
		public string WorstCurrentstatus { get; set; }
		public string WorstHistoricalstatus { get; set; }
		public int? Numberofdefaults { get; set; }
		public int? Accounts { get; set; }
		public int? CCJs { get; set; }
		public int? MostrecentCCJ { get; set; }
		public int? Creditcardutilization { get; set; }
		public int? Enquiriesinlast6months { get; set; }
		public int? Enquiriesinlast3months { get; set; }
		public int? Totalbalance { get; set; }
		public string AML { get; set; }
		public string AMLnum { get; set; }
		public string BWA { get; set; }
		public string BWAnum { get; set; }
		public bool ThinFile { get; set; }
	}

	public class CheckHistoryModel
	{
		public DateTime Date { get; set; }
		public int? Score { get; set; }
		public int? CII { get; set; }
		public decimal? Balance { get; set; }
		public long Id { get; set; }
		public long ServiceLogId { get; set; }
	}

	public class DelphiModel
	{
		public string Position { get; set; }
		public string Align { get; set; }
		public string ValPosition { get; set; }
		public string Color { get; set; }
	}
}