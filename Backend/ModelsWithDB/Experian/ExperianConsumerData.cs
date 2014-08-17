namespace Ezbob.Backend.ModelsWithDB.Experian 
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Database;
	using Logger;
	using Newtonsoft.Json;
	using Utils;

	[DataContract]
	public class ExperianConsumerData {
		public ExperianConsumerData() {
			Cais = new List<ExperianConsumerDataCais>();
			Applicants = new List<ExperianConsumerDataApplicant>();
			Nocs = new List<ExperianConsumerDataNoc>();
			Residencies = new List<ExperianConsumerDataResidency>();
			Locations = new List<ExperianConsumerDataLocation>();
		}

		[NonTraversable]
		[DataMember]
		public long Id { get; set; }
		[DataMember]
		public long? ServiceLogId { get; set; }
		[NonTraversable]
		[DataMember]
		public DateTime InsertDate { get; set; }

		[DataMember]
		public int? CustomerId { get; set; }
		[DataMember]
		public int? DirectorId { get; set; }
		[DataMember]
		public string Error { get; set; }
		[DataMember]
		public bool HasParsingError { get; set; }
		[DataMember]
		public bool HasExperianError { get; set; }
		[DataMember]
		public int? BureauScore { get; set; } //E5S051
		[DataMember]
		public int? CII { get; set; } //NDSPCII (SP) Consumer Indebtedness Index
		[DataMember]
		public int? CreditCardBalances { get; set; } //SPA04 SP Balance of active revolving CAIS
		[DataMember]
		public int? ActiveCaisBalanceExcMortgages { get; set; } //SPA02 SP Balance of all active CAIS (excluding mortgages) Used to calculate UnsecuredLoans
		[DataMember]
		public int? NumCreditCards { get; set; } //NOMPMNPRL3M # Credit Card Accounts with Min. Payments made & No Promotional Rate in the Last 3 months
		[DataMember]
		public int? CreditLimitUtilisation { get; set; } //CLUNPRL1M # Credit Card Accounts with Min. Payments made & No Promotional Rate in the Last 3 months
		[DataMember]
		public int? CreditCardOverLimit { get; set; } //SPF131 SP Number of active revolving CAIS with CLU > 100%
		[DataMember]
		public string PersonalLoanStatus { get; set; } //NDHAC05 Current Worst status (fine) on active non revolving CAIS (SP) "Current Worst status (fine) on active non revolving CAIS"
		[DataMember]
		public string WorstStatus { get; set; } //NDHAC09 Worst status (fine) in last 6m on mortgage accounts (SP)
		[DataMember]
		public string WorstCurrentStatus { get; set; } //CAISSummary.WorstCurrent
		[DataMember]
		public string WorstHistoricalStatus { get; set; }//CAISSummary.WorstHistorical
		[DataMember]
		public int? TotalAccountBalances { get; set; } //E1B10 Balance (class 1) (excl. mortgages)
		[DataMember]
		public int? NumAccounts { get; set; } //E1B01 # accounts
		[DataMember]
		public int? NumCCJs { get; set; } //E1A01 # CCJ's
		[DataMember]
		public int? CCJLast2Years { get; set; } //E1A03 Age of most recent CCJ
		[DataMember]
		public int? TotalCCJValue1 { get; set; } //E1A02 Total value (class 1) of CCJ's
		[DataMember]
		public int? TotalCCJValue2 { get; set; } //E2G02 Total value (class 1) of CCJ's
		[DataMember]
		public int? EnquiriesLast6Months { get; set; } //E1E02 # in last 6 months (previous search information)
		[DataMember]
		public int? EnquiriesLast3Months { get; set; } //E1E01 # in last 3 months (previous search information)
		[DataMember]
		public int? MortgageBalance { get; set; } //E1B11 Balance (mortgages)
		[DataMember]
		public DateTime? CaisDOB { get; set; } //EA4S04 CAIS DOB

		//SumOfRepayements = CreditCommitmentsRevolving+CreditCommitmentsNonRevolving+MortgagePayments
		[DataMember]
		public int? CreditCommitmentsRevolving { get; set; } //SPH39 SP Monthly Credit Commitments (revolving)
		[DataMember]
		public int? CreditCommitmentsNonRevolving { get; set; } //SPH40 SP Monthly Credit commitments (non-revolving)
		[DataMember]
		public int? MortgagePayments { get; set; }//SPH41 SP Monthly Mortgage Payments
		[DataMember]
		public bool Bankruptcy { get; set; } //EA1C01 Bankruptcy detected (SP)
		[DataMember]
		public bool OtherBankruptcy { get; set; } //EA2I01 Bankruptcy detected (SPA)
		[DataMember]
		public int? CAISDefaults { get; set; } //E1A05 Tot value (class 1) of CAIS 8/9's
		[DataMember]
		public string BadDebt { get; set; } //E1B08 Worst current status (coarse)
		[DataMember]
		public bool NOCsOnCCJ { get; set; } //EA4Q02 CCJ NOC (ALL)
		[DataMember]
		public bool NOCsOnCAIS { get; set; } //EA4Q04 CAIS NOC (ALL)
		[DataMember]
		public bool NOCAndNOD { get; set; }//EA4Q05 NOC on any item (ALL)
		[DataMember]
		public bool SatisfiedJudgement { get; set; } //EA4Q06 Satisfied CCJ detected (ALL)
		[DataMember]
		public string CAISSpecialInstructionFlag { get; set; } //EA1F04 CAIS special instruction ind (SP)

		[DataMember]
		[NonTraversable]
		public List<ExperianConsumerDataApplicant> Applicants { get; set; }
		[DataMember]
		[NonTraversable]
		public List<ExperianConsumerDataLocation> Locations { get; set; }
		[DataMember]
		[NonTraversable]
		public List<ExperianConsumerDataResidency> Residencies { get; set; }
		[DataMember]
		[NonTraversable]
		public List<ExperianConsumerDataNoc> Nocs { get; set; }
		[DataMember]
		[NonTraversable]
		public List<ExperianConsumerDataCais> Cais { get; set; }

		public override string ToString() {
			return JsonConvert.SerializeObject(this, new JsonSerializerSettings {Formatting = Formatting.Indented});
		}

		/// <summary>
		/// Loads only the consumer data table without all the detailed data
		/// </summary>
		public static ExperianConsumerData Load(long nServiceLogID, AConnection oDB, ASafeLog m_oLog) {
			var result = new ExperianConsumerData();
			var data = oDB.ExecuteEnumerable(
				"LoadFullExperianConsumer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ServiceLogID", nServiceLogID)
				);

			foreach (SafeReader sr in data) {
				string sType = sr["DatumType"];

				switch (sType) {
					case "ExperianConsumerData":
						sr.Fill(result);
						result.Id = sr["Id"];
						break;
				}
			}
			return result;
		}
	}

	[DataContract]
	public class ExperianConsumerDataApplicant
	{
		[DataMember]
		[NonTraversable]
		public long Id { get; set; }
		[DataMember]
		public long? ExperianConsumerDataId { get; set; }

		//Applicant
		[DataMember]
		public int? ApplicantIdentifier { get; set; }
		[DataMember]
		public string Title { get; set; }
		[DataMember]
		public string Forename { get; set; }
		[DataMember]
		public string MiddleName { get; set; }
		[DataMember]
		public string Surname { get; set; }
		[DataMember]
		public string Suffix { get; set; }
		[DataMember]
		public DateTime? DateOfBirth { get; set; }
		[DataMember]
		public string Gender { get; set; }
	}

	[DataContract]
	public class ExperianConsumerDataNoc
	{
		[DataMember]
		[NonTraversable]
		public long Id { get; set; }
		[DataMember]
		public long? ExperianConsumerDataId { get; set; }

		[DataMember]
		public string Reference { get; set; }
		[DataMember]
		public string TextLine { get; set; }
	}

	[DataContract]
	public class ExperianConsumerDataLocation
	{
		[DataMember]
		[NonTraversable]
		public long Id { get; set; }
		[DataMember]
		public long? ExperianConsumerDataId { get; set; }

		[DataMember]
		public int? LocationIdentifier { get; set; }
		[DataMember]
		public string Flat { get; set; }
		[DataMember]
		public string HouseName { get; set; }
		[DataMember]
		public string HouseNumber { get; set; }
		[DataMember]
		public string Street { get; set; }
		[DataMember]
		public string Street2 { get; set; }
		[DataMember]
		public string District { get; set; }
		[DataMember]
		public string District2 { get; set; }
		[DataMember]
		public string PostTown { get; set; }
		[DataMember]
		public string County { get; set; }
		[DataMember]
		public string Postcode { get; set; }
		[DataMember]
		public string POBox { get; set; }
		[DataMember]
		public string Country { get; set; }
		[DataMember]
		public string SharedLetterbox { get; set; }
		[DataMember]
		public string FormattedLocation { get; set; }
		[DataMember]
		public string LocationCode { get; set; }
		[DataMember]
		public string TimeAtYears { get; set; }
		[DataMember]
		public string TimeAtMonths { get; set; }
	}

	[DataContract]
	public class ExperianConsumerDataResidency
	{
		[DataMember]
		[NonTraversable]
		public long Id { get; set; }
		[DataMember]
		public long? ExperianConsumerDataId { get; set; }

		[DataMember]
		public int? ApplicantIdentifier { get; set; }
		[DataMember]
		public int? LocationIdentifier { get; set; }
		[DataMember]
		public string LocationCode { get; set; }
		[DataMember]
		public DateTime? ResidencyDateFrom { get; set; }
		[DataMember]
		public DateTime? ResidencyDateTo { get; set; }
		[DataMember]
		public string TimeAtYears { get; set; }
		[DataMember]
		public string TimeAtMonths { get; set; }
	}

	[DataContract]
	public class ExperianConsumerDataCais
	{
		public ExperianConsumerDataCais()
		{
			AccountBalances = new List<ExperianConsumerDataCaisBalance>();
			CardHistories = new List<ExperianConsumerDataCaisCardHistory>();
		}

		[DataMember]
		[NonTraversable]
		public long Id { get; set; }
		[DataMember]
		public long? ExperianConsumerDataId { get; set; }

		[DataMember]
		public DateTime? CAISAccStartDate { get; set; }
		[DataMember]
		public DateTime? SettlementDate { get; set; } //Default Date is displayed if CAIS status 8/9 is returned
		[DataMember]
		public DateTime? LastUpdatedDate { get; set; }
		[DataMember]
		public int? MatchTo { get; set; }
		[DataMember]
		public int? CreditLimit { get; set; }
		[DataMember]
		public int? Balance { get; set; }
		[DataMember]
		public int? CurrentDefBalance { get; set; }
		[DataMember]
		public int? DelinquentBalance { get; set; }
		[DataMember]
		public string AccountStatusCodes { get; set; }
		[DataMember]
		public string Status1To2 { get; set; }
		[DataMember]
		public string StatusTo3 { get; set; }
		[DataMember]
		public int? NumOfMonthsHistory { get; set; }
		[DataMember]
		public string WorstStatus { get; set; }
		[DataMember]
		public string AccountStatus { get; set; }
		[DataMember]
		public string AccountType { get; set; }
		[DataMember]
		public string CompanyType { get; set; }
		[DataMember]
		public int? RepaymentPeriod { get; set; }
		[DataMember]
		public int? Payment { get; set; }
		[DataMember]
		public int? NumAccountBalances { get; set; }
		[DataMember]
		[NonTraversable]
		public List<ExperianConsumerDataCaisBalance> AccountBalances { get; set; }
		[DataMember]
		public int? NumCardHistories { get; set; }
		[DataMember]
		[NonTraversable]
		public List<ExperianConsumerDataCaisCardHistory> CardHistories { get; set; }

	}

	[DataContract]
	public class ExperianConsumerDataCaisBalance
	{
		[DataMember]
		[NonTraversable]
		public long Id { get; set; }
		[DataMember]
		public long? ExperianConsumerDataCaisId { get; set; }

		[DataMember]
		public int? AccountBalance { get; set; }
		[DataMember]
		public string Status { get; set; }
	}

	[DataContract]
	public class ExperianConsumerDataCaisCardHistory
	{
		[DataMember]
		[NonTraversable]
		public long Id { get; set; }
		[DataMember]
		public long? ExperianConsumerDataCaisId { get; set; }

		[DataMember]
		public int? PrevStatementBal { get; set; }
		[DataMember]
		public string PromotionalRate { get; set; }
		[DataMember]
		public int? PaymentAmount { get; set; }
		[DataMember]
		public int? NumCashAdvances { get; set; }
		[DataMember]
		public int? CashAdvanceAmount { get; set; }
		[DataMember]
		public string PaymentCode { get; set; }
	}

	[DataContract]
	public class ExperianConsumerMortgagesData
	{
		[DataMember]
		public int NumMortgages { get; set; }
		[DataMember]
		public int MortgageBalance { get; set; }
	}
}
