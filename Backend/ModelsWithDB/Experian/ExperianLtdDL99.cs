namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DL99]
	public class ExperianLtdDL99 : AExperianLtdDataRow {
		public ExperianLtdDL99(ASafeLog oLog = null) : base(oLog) { } // constructor

		[DataMember]
		[DL99("DATEOFACCOUNTS-YYYY")]
		public DateTime? Date { get; set; }

		[DataMember]
		[DL99("CREDDIRLOANS", Transformation = TransformationType.Money)]
		public decimal? CredDirLoans { get; set; }

		[DataMember]
		[DL99("DEBTORS", Transformation = TransformationType.Money)]
		public decimal? Debtors { get; set; }

		[DataMember]
		[DL99("DEBTORSDIRLOANS", Transformation = TransformationType.Money)]
		public decimal? DebtorsDirLoans { get; set; }

		[DataMember]
		[DL99("DEBTORSGROUPLOANS", Transformation = TransformationType.Money)]
		public decimal? DebtorsGroupLoans { get; set; }

		[DataMember]
		[DL99("INTNGBLASSETS", Transformation = TransformationType.Money)]
		public decimal? InTngblAssets { get; set; }

		[DataMember]
		[DL99("INVENTORIES", Transformation = TransformationType.Money)]
		public decimal? Inventories { get; set; }

		[DataMember]
		[DL99("ONCLDIRLOANS", Transformation = TransformationType.Money)]
		public decimal? OnClDirLoans { get; set; }

		[DataMember]
		[DL99("OTHDEBTORS", Transformation = TransformationType.Money)]
		public decimal? OtherDebtors { get; set; }

		[DataMember]
		[DL99("PREPAYACCRUALS", Transformation = TransformationType.Money)]
		public decimal? PrepayAccRuals { get; set; }

		[DataMember]
		[DL99("RETAINEDEARNINGS", Transformation = TransformationType.Money)]
		public decimal? RetainedEarnings { get; set; }

		[DataMember]
		[DL99("TNGBLASSETS", Transformation = TransformationType.Money)]
		public decimal? TngblAssets { get; set; }

		[DataMember]
		[DL99("TOTALCASH", Transformation = TransformationType.Money)]
		public decimal? TotalCash { get; set; }

		[DataMember]
		[DL99("TOTALCURRLBLTS", Transformation = TransformationType.Money)]
		public decimal? TotalCurrLblts { get; set; }

		[DataMember]
		[DL99("TOTALNONCURR", Transformation = TransformationType.Money)]
		public decimal? TotalNonCurr { get; set; }

		[DataMember]
		[DL99("TOTALSHAREFUND", Transformation = TransformationType.Money)]
		public decimal? TotalShareFund { get; set; }

		[DataMember]
		[DL99("FINDIRLOANS", IsCompanyScoreModel = false)]
		public decimal? FinDirLoans { get; set; }

		[DataMember]
		[DL99("FINLBLTSDIRLOANS", IsCompanyScoreModel = false)]
		public decimal? FinLbltsDirLoans { get; set; }

		[DataMember]
		[DL99("CURRDIRLOANS", IsCompanyScoreModel = false)]
		public decimal? CurrDirLoans { get; set; }

		[DataMember]
		[DL99("TOTALCURRASSETS", IsCompanyScoreModel = false)]
		public decimal? TotalCurrAssets { get; set; }
	} // class ExperianLtdDL99
} // namespace
