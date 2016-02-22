namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class OutputRatio : AWithModelOutputID {
		[FieldName("OutputRatioID")]
		[PK(true)]
		public long ID { get; set; }

		[Length(255)]
		public string OutputClass { get; set; }
		public decimal Score { get; set; }
	} // class OutputRatio
} // namespace
