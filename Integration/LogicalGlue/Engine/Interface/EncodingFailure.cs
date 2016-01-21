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

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"Row idx: {0}, column name: '{1}', reason: '{2}', message: '{3}', unencoded value: '{4}'",
				RowIndex,
				ColumnName,
				Reason,
				Message,
				UnencodedValue
			);
		} // ToString
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
