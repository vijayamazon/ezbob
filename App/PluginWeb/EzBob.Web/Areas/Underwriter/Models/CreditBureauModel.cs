using System.Collections.Generic;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class CreditBureauModel
    {
        private readonly List<string> _errorList = new List<string>();
        public int Id { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public bool IsExperianError { get; set; }
        public bool IsError
        {
            get { return _errorList.Count > 0; }
        }
        public List<string> ErrorList
        {
            get { return _errorList; }
        }
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
        public double Score { get; set; } // done     
        public double Odds { get; set; } // done     
        public string ScoreColor { get; set; } // done 
        public string ScorePosition { get; set; } // done
        public string ScoreAlign { get; set; } // done
        public string ScoreValuePosition { get; set; } // done
        public string CII { get; set; } // done
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
        public string NumberOfAccounts { get; set; } // done
        public string NumberOfAccounts3M { get; set; } // done
        public string WorstCurrentStatus { get; set; } // done
        public string WorstCurrentStatus3M { get; set; } // done
        public string NumberOfDefaults { get; set; } // done
        public string NumberOfCCJs { get; set; } // done
        public string AgeOfMostRecentCCJ { get; set; } // done
        public string NumberOfCCOverLimit { get; set; } // done
        public string EnquiriesLast3M { get; set; } // done
        public string EnquiriesLast6M { get; set; } // done
        public string CreditCardUtilization { get; set; } // done
        public string DSRandOwnershipType { get; set; } // !DSR
        public string NOCsOnCCJ { get; set; } // done
        public string NOCsOnCAIS { get; set; } // done
        public string CAISSpecialInstructionFlag { get; set; } // done
        public string SatisfiedJudgements { get; set; } // done 
    }

    public class ConsumerAccountsOverview
    {
        public string OpenAccounts_CC { get; set; } // done
        public string WorstArrears_CC { get; set; } // done
        public string TotalCurLimits_CC { get; set; } // done
        public string Balance_CC { get; set; } // done
        public string OpenAccounts_Mtg { get; set; } // done
        public string WorstArrears_Mtg { get; set; } // done
        public string TotalCurLimits_Mtg { get; set; } // done
        public string Balance_Mtg { get; set; } // done
        public string OpenAccounts_PL { get; set; } // done
        public string WorstArrears_PL { get; set; } // done
        public string TotalCurLimits_PL { get; set; } // done
        public string Balance_PL { get; set; } // done
        public string OpenAccounts_Other { get; set; } // done
        public string WorstArrears_Other { get; set; } // done
        public string TotalCurLimits_Other { get; set; } // done
        public string Balance_Other { get; set; } // done
        public string OpenAccounts_Total { get; set; } // done
        public string WorstArrears_Total { get; set; } // done
        public string TotalCurLimits_Total { get; set; } // done
        public string Balance_Total { get; set; } // done
    }

    public class AccountInfo
    {
        public string OpenDate { get; set; } // done
        public string Account { get; set; } // done
        public string TermAndfreq { get; set; } // done
        public string Limit { get; set; } // done
		public string AccBalance { get; set; } // done
		public string AccountStatus { get; set; } // done
		public string DateType { get; set; } // done
        public string SettlementDate { get; set; } // done
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
        public string BankAccountVerificationResult { get; set; }
        public string AuthenticationText { get; set; }
        public string AccountStatus { get; set; }
        public decimal NameScore { get; set; }
        public decimal AddressScore { get; set; }
    }

    public class ExperianLimitedInfo
    {
        public decimal BureauScore { get; set; }
        public string RiskLevel { get; set; }
        public decimal ExistingBusinessLoans { get; set; }
    }

    public class ExperianNonLimitedInfo
    {
        public decimal BureauScore { get; set; }
        public bool CompanyNotFoundOnBureau { get; set; }
    }

    public class Summary
    {
        public string Score { get; set; }
        public string ConsumerIndebtednessIndex { get; set; }
        public string CheckDate { get; set; }
        public string Validtill { get; set; }
        public string WorstCurrentstatus { get; set; }
        public string WorstHistoricalstatus { get; set; }
        public string Numberofdefaults { get; set; }
        public string Accounts { get; set; }
        public string CCJs { get; set; }
        public string MostrecentCCJ { get; set; }
        public string DSRandownershiptype { get; set; }
        public string Creditcardutilization { get; set; }
        public string Enquiriesinlast6months { get; set; }
        public string Enquiriesinlast3months { get; set; }
        public string Totalbalance { get; set; }
        public string AML { get; set; }
        public string AMLnum { get; set; }
        public string BWA { get; set; }
        public string BWAnum { get; set; }
        public string Businesstype { get; set; }
        public string BusinessScore { get; set; }
        public string RiskLevel { get; set; }
        public string Existingbusinessloans { get; set; }
        public ConsumerAccountsOverview ConsumerAccountsOverview { get; set; }
    }

    // ReSharper disable  InconsistentNaming
    public enum MatchTo
    {
        FinancialAccounts_AliasOfJointApplicant = 6,
        FinancialAccounts_AliasOfMainApplicant = 2,
        FinancialAccounts_AssociationOfJointApplicant = 7,
        FinancialAccounts_AssociationOfMainApplicant = 3,
        FinancialAccounts_JointApplicant = 5,
        FinancialAccounts_MainApplicant = 1,
        FinancialAccounts_No_Match = 9
    }

    public class Helper
    {
        public static string MathToToHumanView(MatchTo input)
        {
            switch (input)
            {
                case MatchTo.FinancialAccounts_AliasOfJointApplicant: return "Alias Of Joint Applicant";
                case MatchTo.FinancialAccounts_AliasOfMainApplicant: return "Alias Of Main Applicant";
                case MatchTo.FinancialAccounts_AssociationOfJointApplicant: return "Association Of Joint Applicant";
                case MatchTo.FinancialAccounts_AssociationOfMainApplicant: return "Association Of Main Applicant";
                case MatchTo.FinancialAccounts_JointApplicant: return "Joint Applicant";
                case MatchTo.FinancialAccounts_MainApplicant: return "Main Applicant";
                case MatchTo.FinancialAccounts_No_Match: return "No Match";
                default: return "-";
            }
        }
    }
}