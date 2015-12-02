namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class EncodingFailure : AWithModelOutputID {
		[FieldName("FailureID")]
		[PK(true)]
		public long ID { get; set; }

		public int RowIndex { get; set; }
		public string ColumnName { get; set; }
		public string UnencodedValue { get; set; }
		public string Reason { get; set; }
		public string Message { get; set; }
	} // class EncodingFailure
} // namespace
