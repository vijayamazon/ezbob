namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[DL97]
	public class ExperianLtdDL97 : AExperianLtdDataRow {
		public ExperianLtdDL97(ASafeLog oLog = null) : base(oLog) { } // constructor

		[DataMember]
		[DL97("ACCTSTATE")]
		public string AccountState { get; set; } // State

		[DataMember]
		[DL97("COMPANYTYPE")]
		public int? CompanyType { get; set; } // CompanyType

		[DataMember]
		[DL97("ACCTTYPE")]
		public int? AccountType { get; set; } // Type

		[DataMember]
		[DL97("DEFAULTDATE-YYYY")]
		public DateTime? DefaultDate { get; set; } // --

		[DataMember]
		[DL97("SETTLEMTDATE-YYYY")]
		public DateTime? SettlementDate { get; set; } // --

		[DataMember]
		[DL97("CURRBALANCE")]
		public decimal? CurrentBalance { get; set; } // CurrentBalance

		[DataMember]
		[DL97("STATUS1TO2")]
		public decimal? Status12 { get; set; } // Status1to2

		[DataMember]
		[DL97("STATUS3TO9")]
		public decimal? Status39 { get; set; } // Status3to9

		[DataMember]
		[DL97("CAISLASTUPDATED-YYYY")]
		public DateTime? CAISLastUpdatedDate { get; set; } // LastUpdated

		[DataMember]
		[DL97("ACCTSTATUS12")]
		public string AccountStatusLast12AccountStatuses { get; set; } // Status12Months

		[DataMember]
		[DL97("AGREEMTNUM")]
		public string AgreementNumber { get; set; } // --

		[DataMember]
		[DL97("MONTHSDATA")]
		public int? MonthsData { get; set; } // MonthsData

		[DataMember]
		[DL97("DEFAULTBALANCE")]
		public decimal? DefaultBalance { get; set; } // --
	} // class ExperianLtdDL97
} // namespace
