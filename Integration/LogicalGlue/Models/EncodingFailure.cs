namespace Ezbob.Integration.LogicalGlue.Models {
	public class EncodingFailure {
		public int RowIndex { get; set; }
		public string ColumnName { get; set; }
		public string UnencodedValue { get; set; }
		public string Reason { get; set; }
		public string Message { get; set; }
	} // class EncodingFailure
} // namespace
