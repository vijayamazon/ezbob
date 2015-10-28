namespace Ezbob.Integration.LogicalGlue.Interface {
	public interface IEncodingFailure : ICanBeEmpty {
		int RowIndex { get; set; }
		string ColumnName { get; set; }
		string UnencodedValue { get; set; }
		string Reason { get; set; }
		string Message { get; set; }
	} // interface IEncodingFailure
} // namespace
