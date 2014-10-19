namespace MailApi.Model {
	using System;

	public class MandrillException : Exception {
		public ErrorResponseModel Error { get; private set; }

		public MandrillException() {}

		public MandrillException(string message) : base(message) {}

		public MandrillException(string message, Exception innerException) : base(message, innerException) {}

		public MandrillException(ErrorResponseModel error, string message) : base(message) {
			Error = error;
		} // constructor
	} // class MandrillException
} // namespace
