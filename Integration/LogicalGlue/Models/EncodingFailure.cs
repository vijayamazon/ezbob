namespace Ezbob.Integration.LogicalGlue.Models {
	public class EncodingFailure {
		public long ID { get; set; }

		public Response Response { get; set; }

		public string ColumnName { get; set; }
		public string UnencodedValue { get; set; }
		public string Reason { get; set; }
		public string Message { get; set; }
	} // class EncodingFailure
} // namespace
