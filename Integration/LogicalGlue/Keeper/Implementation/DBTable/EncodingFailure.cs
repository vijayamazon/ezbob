namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class EncodingFailure : AWithModelOutputID {
		[FieldName("FailureID")]
		[PK(true)]
		public long ID { get; set; }

		public int RowIndex { get; set; }
		[Length(255)]
		public string ColumnName { get; set; }
		[Length(LengthType.MAX)]
		public string UnencodedValue { get; set; }
		[Length(LengthType.MAX)]
		public string Reason { get; set; }
		[Length(LengthType.MAX)]
		public string Message { get; set; }
	} // class EncodingFailure
} // namespace
