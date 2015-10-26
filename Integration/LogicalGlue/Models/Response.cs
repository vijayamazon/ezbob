namespace Ezbob.Integration.LogicalGlue.Models {
	using System;
	using System.Collections.Generic;

	public class Response {
		public DateTime ReceivingTime { get; set; }

		public string Status { get; set; }
		public string ErrorCode { get; set; }
		public string Exception { get; set; }
		public Guid? Uuid { get; set; }

		public string Content { get; set; }

		public List<Warning> Warnings { get; set; }
		public List<EncodingFailure> EncodingFailures { get; set; }
	} // class Response
} // namespace
