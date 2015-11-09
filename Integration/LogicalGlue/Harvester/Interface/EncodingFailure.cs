namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Newtonsoft.Json;

	public class EncodingFailure {
		[JsonProperty(PropertyName = "rowIndex")]
		public int RowIndex { get; set; }

		[JsonProperty(PropertyName = "columnName")]
		public string ColumnName { get; set; }

		[JsonProperty(PropertyName = "unencodedValue")]
		public string UnencodedValue { get; set; }

		[JsonProperty(PropertyName = "reason")]
		public string Reason { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }
	} // class EncodingFailure
} // namespace
