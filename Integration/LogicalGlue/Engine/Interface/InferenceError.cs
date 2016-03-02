namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class InferenceError {
		[DataMember]
		public TimeoutSource TimeoutSource { get; set; }
		[DataMember]
		public string Message { get; set; }
		[DataMember]
		public string ParsingExceptionType { get; set; }
		[DataMember]
		public string ParsingExceptionMessage { get; set; }
	} // class InferenceError

	public static class InferenceErrorExt {
		public static bool HasError(this InferenceError ie) {
			if (ie == null)
				return false;

			return
				(ie.TimeoutSource != null) ||
				!string.IsNullOrWhiteSpace(ie.Message) ||
				!string.IsNullOrWhiteSpace(ie.ParsingExceptionMessage) ||
				!string.IsNullOrWhiteSpace(ie.ParsingExceptionType);
		} // HasError
	} // class InferenceErrorExt
} // namespace
