namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DL97]
	public class ExperianLtdDL97 : AExperianLtdDataRow {
		public ExperianLtdDL97(ASafeLog oLog = null) : base(oLog) { } // constructor

		[DataMember]
		[DL97("ACCTSTATE", "Account State", @"{
			""D"": ""Defaulted"",
			""F"": ""Delinquent"",
			""A"": ""Active"",
			""S"": ""Settled""
		}")]
		public string AccountState { get; set; } // State

		[DataMember]
		[DL97("COMPANYTYPE", "Company Type", @"{
			""1"": ""Bank"",
			""2"": ""Finance House"",
			""3"": ""Retailer"",
			""4"": ""Mail Order"",
			""5"": ""Collection Agency"",
			""6"": ""TV Rental"",
			""7"": ""Insurance"",
			""8"": ""Building Society"",
			""9"": ""Credit Card""
		}")]
		public int? CompanyType { get; set; } // CompanyType

		[DataMember]
		[DL97("ACCTTYPE", "Account Type", @"{
			""00"": ""Bank"",
			""01"": ""Hire purchase"",
			""02"": ""Unsecured loan"",
			""03"": ""Mortgage"",
			""04"": ""Budget"",
			""05"": ""Credit card"",
			""06"": ""Charge card"",
			""07"": ""Rental"",
			""08"": ""Mail Order Agency"",
			""09"": ""Mail Order Direct"",
			""10"": ""Mail Order Cash"",
			""11"": ""Overdraft"",
			""12"": ""CML member"",
			""13"": ""CML member"",
			""14"": ""CML member"",
			""15"": ""Current accounts"",
			""16"": ""Secured loan or Second mortgage"",
			""17"": ""Credit sale fixed term"",
			""18"": ""Communications"",
			""19"": ""Fixed term deferred payment"",
			""20"": ""Variable subscription"",
			""21"": ""Utility"",
			""22"": ""Finance Lease"",
			""23"": ""Operating Lease"",
			""24"": ""Unpresentable cheques"",
			""25"": ""Flexible Mortgages"",
			""26"": ""Consolidated Debt"",
			""27"": ""Primary Lease"",
			""28"": ""Secondary Lease"",
			""29"": ""Balloon Rental"",
			""30"": ""Dealer buy-back"",
			""31"": ""Fixed Term Account"",
			""32"": ""Variable Term Account"",
			""34"": ""Flexi Rate Credit Card"",
			""35"": ""Merchant Account"",
			""61"": ""Home Credit"",
			""71"": ""Contract Hire""
		}")]
		public int? AccountType { get; set; } // Type

		[DataMember]
		[DL97("DEFAULTDATE-YYYY", "Default Date")]
		public DateTime? DefaultDate { get; set; } // --

		[DataMember]
		[DL97("SETTLEMTDATE-YYYY", "Settlement Date")]
		public DateTime? SettlementDate { get; set; } // --

		[DataMember]
		[DL97("CURRBALANCE", "Current Balance", Transformation = TransformationType.Money)]
		public decimal? CurrentBalance { get; set; } // CurrentBalance

		[DataMember]
		[DL97("STATUS1TO2", "Status 1-2")]
		public decimal? Status12 { get; set; } // Status1to2

		[DataMember]
		[DL97("STATUS3TO9", "Status 3-9")]
		public decimal? Status39 { get; set; } // Status3to9

		[DataMember]
		[DL97("CAISLASTUPDATED-YYYY", "CAIS Last Updated Date")]
		public DateTime? CAISLastUpdatedDate { get; set; } // LastUpdated

		[DataMember]
		[DL97("ACCTSTATUS12", "Account status (Last 12 Account Statuses")]
		public string AccountStatusLast12AccountStatuses { get; set; } // Status12Months

		[DataMember]
		[DL97("AGREEMTNUM", "Agreement Number")]
		public string AgreementNumber { get; set; } // --

		[DataMember]
		[DL97("MONTHSDATA", "Months Data")]
		public int? MonthsData { get; set; } // MonthsData

		[DataMember]
		[DL97("DEFAULTBALANCE", "Default Balance", Transformation = TransformationType.Money)]
		public decimal? DefaultBalance { get; set; } // --
	} // class ExperianLtdDL97
} // namespace
