namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class InferenceError {
		[DataMember]
		public TimeoutSources? TimeoutSource { get; set; }
		[DataMember]
		public string Message { get; set; }
		[DataMember]
		public string ParsingExceptionType { get; set; }
		[DataMember]
		public string ParsingExceptionMessage { get; set; }

		public bool HasError() {
			return
				(TimeoutSource != null) ||
				!string.IsNullOrWhiteSpace(Message) ||
				!string.IsNullOrWhiteSpace(ParsingExceptionMessage) ||
				!string.IsNullOrWhiteSpace(ParsingExceptionType);
		} // HasError
	} // class InferenceError
} // namespace
