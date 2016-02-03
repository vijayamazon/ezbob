namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class EncodingFailure : ICanBeEmpty<EncodingFailure> {
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

		public EncodingFailure CloneTo() {
			return new EncodingFailure {
				RowIndex = RowIndex,
				ColumnName = ColumnName,
				UnencodedValue = UnencodedValue,
				Reason = Reason,
				Message = Message,
			};
		} // CloneFrom
	} // class EncodingFailure
} // namespace
