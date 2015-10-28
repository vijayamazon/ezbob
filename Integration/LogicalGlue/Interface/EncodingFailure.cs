namespace Ezbob.Integration.LogicalGlue.Interface {
	public class EncodingFailure : ICanBeEmpty {
		public int RowIndex { get; set; }
		public string ColumnName { get; set; }
		public string UnencodedValue { get; set; }
		public string Reason { get; set; }
		public string Message { get; set; }

		public bool IsEmpty {
			get { return string.IsNullOrWhiteSpace(ColumnName); }
		} // IsEmpty
	} // class EncodingFailure

	public static class EncodingFailureExt {
		public static EncodingFailure CloneFrom(this EncodingFailure target, EncodingFailure source) {
			if (source == null)
				return new EncodingFailure();

			target = target ?? new EncodingFailure();

			target.RowIndex = source.RowIndex;
			target.ColumnName = source.ColumnName;
			target.UnencodedValue = source.UnencodedValue;
			target.Reason = source.Reason;
			target.Message = source.Message;

			return target;
		} // CloneFrom
	} // class EncodingFailureExt
} // namespace
