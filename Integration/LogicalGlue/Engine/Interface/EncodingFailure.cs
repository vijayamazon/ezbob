namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class EncodingFailure : ICanBeEmpty {
		[DataMember]
		public int RowIndex { get; set; }

		[DataMember]
		public string ColumnName { get; set; }

		[DataMember]
		public string UnencodedValue { get; set; }

		[DataMember]
		public string Reason { get; set; }

		[DataMember]
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
