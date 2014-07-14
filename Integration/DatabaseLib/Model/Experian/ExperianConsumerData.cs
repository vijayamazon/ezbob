namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using System.Collections.Generic;

	public class ExperianConsumerData
	{
		public virtual long Id { get; set; }
		public virtual long ServiceLogId { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public virtual int? CustomerId { get; set; }
		public virtual int? DirectorId { get; set; }

		public virtual string Error { get; set; }
		public virtual bool HasParsingError { get; set; }
		public virtual bool HasExperianError { get; set; }

		public virtual int BureauScore { get; set; } //E5S051
		public virtual int CreditCardBalances { get; set; } //SPA04 SP Balance of active revolving CAIS
		public virtual int ActiveCaisBalanceExcMortgages { get; set; } //SPA02 SP Balance of all active CAIS (excluding mortgages) Used to calculate UnsecuredLoans
		public virtual int NumCreditCards { get; set; } //NOMPMNPRL3M # Credit Card Accounts with Min. Payments made & No Promotional Rate in the Last 3 months
		public virtual int CreditLimitUtilisation { get; set; } //CLUNPRL1M # Credit Card Accounts with Min. Payments made & No Promotional Rate in the Last 3 months
		public virtual int CreditCardOverLimit { get; set; } //SPF131 SP Number of active revolving CAIS with CLU > 100%
		public virtual string PersonalLoanStatus { get; set; } //NDHAC05 Current Worst status (fine) on active non revolving CAIS (SP) "Current Worst status (fine) on active non revolving CAIS"
		public virtual string WorstStatus { get; set; } //NDHAC09 Worst status (fine) in last 6m on mortgage accounts (SP)
		public virtual string WorstCurrentStatus { get; set; } //CAISSummary.WorstCurrent
		public virtual string WorstHistoricalStatus { get; set; }//CAISSummary.WorstHistorical
		public virtual int TotalAccountBalances { get; set; } //E1B10 Balance (class 1) (excl. mortgages)
		public virtual int NumAccounts { get; set; } //E1B01 # accounts
		public virtual int NumCCJs { get; set; } //E1A01 # CCJ's
		public virtual int CCJLast2Years { get; set; } //E1A03 Age of most recent CCJ
		public virtual int TotalCCJValue1 { get; set; } //E1A02 Total value (class 1) of CCJ's
		public virtual int TotalCCJValue2 { get; set; } //E2G02 Total value (class 1) of CCJ's
		public virtual int EnquiriesLast6Months { get; set; } //E1E02 # in last 6 months (previous search information)
		public virtual int EnquiriesLast3Months { get; set; } //E1E01 # in last 3 months (previous search information)
		public virtual int MortgageBalance { get; set; } //E1B11 Balance  (mortgages)
		public virtual DateTime? CaisDOB { get; set; } //EA4S04 CAIS DOB

		//SumOfRepayements = CreditCommitmentsRevolving+CreditCommitmentsNonRevolving+MortgagePayments
		public virtual int CreditCommitmentsRevolving { get; set; } //SPH39 SP Monthly Credit Commitments (revolving)
		public virtual int CreditCommitmentsNonRevolving { get; set; } //SPH40 SP Monthly Credit commitments (non-revolving)
		public virtual int MortgagePayments { get; set; }//SPH41 SP Monthly Mortgage Payments

		public virtual bool Bankruptcy { get; set; } //EA1C01 Bankruptcy detected (SP)
		public virtual bool OtherBankruptcy { get; set; } //EA2I01 Bankruptcy detected (SPA)

		public virtual int CAISDefaults { get; set; } //E1A05 Tot value (class 1) of CAIS 8/9's
		public virtual string BadDebt { get; set; } //E1B08 Worst current status (coarse)
		
		public virtual bool NOCsOnCCJ { get; set; } //EA4Q02 CCJ NOC (ALL)
		public virtual bool NOCsOnCAIS { get; set; } //EA4Q04 CAIS NOC  (ALL)
		public virtual bool NOCAndNOD { get; set; }//EA4Q05 NOC on any item (ALL)
		public virtual bool SatisfiedJudgement { get; set; } //EA4Q06 Satisfied CCJ detected (ALL)

		public virtual int CII { get; set; } //NDSPCII (SP) Consumer Indebtedness Index
		public virtual string CAISSpecialInstructionFlag { get; set; } //EA1F04 CAIS special instruction ind (SP)

		public virtual List<ExperianConsumerDataApplicant> Applicants { get; set; }
		public virtual List<ExperianConsumerDataLocation> ConsumerDataLocations { get; set; }
		public virtual List<ExperianConsumerDataCais> Cais { get; set; }
		
	}

	public class ExperianConsumerDataApplicant
	{
		public virtual int Id { get; set; }
		public virtual ExperianConsumerData ExperianConsumerData { get; set; }

		//Applicant
		public virtual string ApplicantIdentifier { get; set; }
		public virtual string Title { get; set; }
		public virtual string Forename { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string Surname { get; set; }
		public virtual string Suffix { get; set; }
		public virtual DateTime? DateOfBirth { get; set; }
		public virtual string Gender { get; set; }
	}

	public class ExperianConsumerDataLocation
	{
		public virtual int Id { get; set; }
		public virtual ExperianConsumerData ExperianConsumerData { get; set; }

		public virtual int LocationIdentifier { get; set; }
		public virtual string Flat { get; set; }
		public virtual string HouseName { get; set; }
		public virtual string HouseNumber { get; set; }
		public virtual string Street { get; set; }
		public virtual string Street2 { get; set; }
		public virtual string District { get; set; }
		public virtual string District2 { get; set; }
		public virtual string PostTown { get; set; }
		public virtual string County { get; set; }
		public virtual string Postcode { get; set; }
		public virtual string POBox { get; set; }
		public virtual string Country { get; set; }
		public virtual string SharedLetterbox { get; set; }
		public virtual string FormattedLocation { get; set; }
		public virtual string LocationCode { get; set; }
		public virtual string TimeAtYears { get; set; }
		public virtual string TimeAtMonths { get; set; }
	}
	
	public class ExperianConsumerDataCais
	{
		public virtual int Id { get; set; }
		public virtual ExperianConsumerData ExperianConsumerData { get; set; }

		public DateTime? CAISAccStartDate { get; set; }
		public DateTime? SettlementDate { get; set; }  //Default Date is displayed if CAIS status 8/9 is returned
		public DateTime? LastUpdatedDate { get; set; }

		public int MatchTo { get; set; }

		public int? CreditLimit { get; set; }
		public int? Balance { get; set; }
		public int? CurrentDefBalance { get; set; }
		public int? DelinquentBalance { get; set; }
		public string AccountStatusCodes { get; set; }

		public string Status1To2 { get; set; }
		public string StatusTo3 { get; set; }

		public int NumOfMonthsHistory { get; set; }
		public string WorstStatus { get; set; }
		public string AccountStatus { get; set; }
		public string AccountType { get; set; }
		public string CompanyType { get; set; }
		public int RepaymentPeriod { get; set; }
		public int? Payment { get; set; }

		public int NumAccountBalances { get; set; }
		public List<ExperianConsumerDataCaisBalance> AccountBalances { get; set; }

		public int NumCardHistories { get; set; }
		public List<ExperianConsumerDataCaisCardHistory> CardHistories { get; set; }

	}

	public class ExperianConsumerDataCaisBalance
	{
		public int AccountBalance { get; set; }
		public string Status { get; set; }
	}

	public class ExperianConsumerDataCaisCardHistory
	{
		public int PrevStatementBal { get; set; }
		public string PromotionalRate { get; set; }
		public int PaymentAmount { get; set; }
		public int NumCashAdvances { get; set; }
		public int CashAdvanceAmount { get; set; }
		public string PaymentCode { get; set; }
	}
}
