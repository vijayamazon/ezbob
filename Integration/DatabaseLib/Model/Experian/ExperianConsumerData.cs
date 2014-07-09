namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using Iesi.Collections.Generic;

	public class ExperianConsumerData
	{
		public virtual int Id { get; set; }
		public virtual int ServiceLogId { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public virtual int? CustomerId { get; set; }
		public virtual int? Director { get; set; }

		public virtual string Error { get; set; }
		public virtual bool HasError { get; set; }

		//Applicant
		public virtual string Title { get; set; }
		public virtual string Forename { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string Surname { get; set; }
		public virtual string Suffix { get; set; }
		public virtual DateTime? DateOfBirth { get; set; }
		public virtual string Gender { get; set; }
		
		public virtual int BureauScore { get; set; } //E5S051
		public virtual int CreditCardBalances { get; set; } //SPA04 SP Balance of active revolving CAIS
		public virtual int ActiveCaisBalanceExcMortgages { get; set; } //SPA02 SP Balance of all active CAIS (excluding mortgages) Used to calculate UnsecuredLoans
		public virtual int NumCreditCards { get; set; } //NOMPMNPRL3M # Credit Card Accounts with Min. Payments made & No Promotional Rate in the Last 3 months
		public virtual int CreditLimitUtilisation { get; set; } //CLUNPRL1M # Credit Card Accounts with Min. Payments made & No Promotional Rate in the Last 3 months
		public virtual int CreditCardOverLimit { get; set; } //SPF131 SP Number of active revolving CAIS with CLU > 100%
		public virtual string PersonalLoanStatus { get; set; } //NDHAC05 Current Worst status (fine) on active non revolving CAIS (SP) "Current Worst status (fine) on active non revolving CAIS"
		public virtual string WorstStatus { get; set; } //NDHAC09 Worst status (fine) in last 6m on mortgage accounts (SP)
		public virtual int TotalAccountBalances { get; set; } //E1B10 Balance (class 1) (excl. mortgages)
		public virtual int NumAccounts { get; set; } //E1B01 # accounts
		public virtual int NumCCJs { get; set; } //E1A01 # CCJ's
		public virtual int CCJLast2Years { get; set; } //E1A03 Age of most recent CCJ
		public virtual int TotalCCJValue1 { get; set; } //E1A02 Total value (class 1) of CCJ's
		public virtual int TotalCCJValue2 { get; set; } //E2G02 Total value (class 1) of CCJ's
		public virtual int EnquiriesLast6Months { get; set; } //E1E02 # in last 6 months (previous search information)
		public virtual int EnquiriesLast3Months { get; set; } //E1E01 # in last 3 months (previous search information)
		public virtual int MortgageBalance { get; set; } //E1B11 Balance  (mortgages)
		public virtual int CaisDOB { get; set; } //EA4S04 CAIS DOB

		//SumOfRepayements = CreditCommitmentsRevolving+CreditCommitmentsNonRevolving+MortgagePayments
		public virtual int CreditCommitmentsRevolving { get; set; } //SPH39 SP Monthly Credit Commitments (revolving)
		public virtual int CreditCommitmentsNonRevolving { get; set; } //SPH40 SP Monthly Credit commitments (non-revolving)
		public virtual int MortgagePayments { get; set; }//SPH41 SP Monthly Mortgage Payments

		public virtual bool Bankruptcy { get; set; } //EA1C01 Bankruptcy detected (SP)
		public virtual bool OtherBankruptcy { get; set; } //EA2I01 Bankruptcy detected (SPA)

		public virtual int CAISDefaults { get; set; } //E1A05 Tot value (class 1) of CAIS 8/9's
		public virtual string BadDebt { get; set; } //E1B08 Worst current status (coarse)
		
		public virtual int NOCsOnCCJ { get; set; } //EA4Q02 CCJ NOC (ALL)
		public virtual int NOCsOnCAIS { get; set; } //EA4Q04 CAIS NOC  (ALL)
		public virtual int NOCAndNOD { get; set; }//EA4Q05 NOC on any item (ALL)
		public virtual int SatisfiedJudgement { get; set; } //EA4Q06 Satisfied CCJ detected (ALL)

		public virtual int CII { get; set; } //NDSPCII (SP) Consumer Indebtedness Index
		public virtual int CAISSpecialInstructionFlag { get; set; } //EA1F04 CAIS special instruction ind (SP)
		/*public virtual int MortgageBalance { get; set; }
		public virtual int MortgageBalance { get; set; }
		public virtual int MortgageBalance { get; set; }
		public virtual int MortgageBalance { get; set; }
		public virtual int MortgageBalance { get; set; }
		public virtual int MortgageBalance { get; set; }*/

		public virtual ISet<ExperianConsumerDataAlias> ConsumerDataAlias { get; set; }
		public virtual ISet<ExperianConsumerDataLocation> ConsumerDataLocations { get; set; }
		
	}

	public class ExperianConsumerDataAlias
	{
		public virtual int Id { get; set; }
		public virtual ExperianConsumerData ExperianConsumerData { get; set; }

		public virtual string Title { get; set; }
		public virtual string Forename { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string Surname { get; set; }
		public virtual string Suffix { get; set; }
		public virtual string Source { get; set; }
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

	//TODO if necessary 
	public class ExperianConsumerDataAssociation
	{
		public virtual int Id { get; set; }
		public virtual ExperianConsumerData ExperianConsumerData { get; set; }

		/*
		LocationIndicator
		ApplicantIndicator
		<Title>
		<Forename>
		<MiddleName>
		<Surname>
		Suffix
		DoBAssociateOrAlias
		CCYY
		MM
		DD
		Gender
		Location
		Flat
		HouseName
		HouseNumber
		Street
		Street2
		District
		District2
		PostTown
		County
		Postcode
		Country
		Source
		StreetMatchLevel
		HouseMatchLevel
		BureauRefCategory
		MatchTo
		InformationType
		CompanyType
		InformationSource
		 */
	}

	public class ExperianConsumerDataFinancialAccounts
	{
		public DateTime? SettlementDate { get; set; }  //Default Date is displayed if CAIS status 8/9 is returned
		public DateTime? LastUpdatedDate { get; set; }
	}


}
