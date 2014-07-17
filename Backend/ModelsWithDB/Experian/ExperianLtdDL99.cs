namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[DL99]
	public class ExperianLtdDL99 : AExperianLtdDataRow {
		public ExperianLtdDL99(ASafeLog oLog = null) : base(oLog) { } // constructor

		[DataMember]
		[DL99("DATEOFACCOUNTS-YYYY")]
		public DateTime? Date { get; set; }
		[DataMember]
		[DL99("CREDDIRLOANS")]
		public decimal? CredDirLoans { get; set; }
		[DataMember]
		[DL99("DEBTORS")]
		public decimal? Debtors { get; set; }
		[DataMember]
		[DL99("DEBTORSDIRLOANS")]
		public decimal? DebtorsDirLoans { get; set; }
		[DataMember]
		[DL99("DEBTORSGROUPLOANS")]
		public decimal? DebtorsGroupLoans { get; set; }
		[DataMember]
		[DL99("INTNGBLASSETS")]
		public decimal? InTngblAssets { get; set; }
		[DataMember]
		[DL99("INVENTORIES")]
		public decimal? Inventories { get; set; }
		[DataMember]
		[DL99("ONCLDIRLOANS")]
		public decimal? OnClDirLoans { get; set; }
		[DataMember]
		[DL99("OTHDEBTORS")]
		public decimal? OtherDebtors { get; set; }
		[DataMember]
		[DL99("PREPAYACCRUALS")]
		public decimal? PrepayAccRuals { get; set; }
		[DataMember]
		[DL99("RETAINEDEARNINGS")]
		public decimal? RetainedEarnings { get; set; }
		[DataMember]
		[DL99("TNGBLASSETS")]
		public decimal? TngblAssets { get; set; }
		[DataMember]
		[DL99("TOTALCASH")]
		public decimal? TotalCash { get; set; }
		[DataMember]
		[DL99("TOTALCURRLBLTS")]
		public decimal? TotalCurrLblts { get; set; }
		[DataMember]
		[DL99("TOTALNONCURR")]
		public decimal? TotalNonCurr { get; set; }
		[DataMember]
		[DL99("TOTALSHAREFUND")]
		public decimal? TotalShareFund { get; set; }
	} // class ExperianLtdDL99
} // namespace
