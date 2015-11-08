namespace Ezbob.Integration.LogicalGlue.Keeper.DBTable {
	class EncodingFailure {
		public long FailureID { get; set; }
		public long ResponseID { get; set; }
		public int RowIndex { get; set; }
		public string ColumnName { get; set; }
		public string UnencodedValue { get; set; }
		public string Reason { get; set; }
		public string Message { get; set; }
	} // class EncodingFailure
} // namespace
