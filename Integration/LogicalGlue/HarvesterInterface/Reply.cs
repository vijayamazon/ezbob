namespace Ezbob.Integration.LogicalGlue.HarvesterInterface {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class Reply {
		[JsonProperty(PropertyName = "score")]
		public decimal Score { get; set; }

		[JsonProperty(PropertyName = "inferenceResultEncoded")]
		public long InferenceResultEncoded { get; set; }

		[JsonProperty(PropertyName = "inferenceResultDecoded")]
		public string InferenceResultDecoded { get; set; }

		[JsonProperty(PropertyName = "warnings")]
		public List<Warning> Warnings { get; set; }

		[JsonProperty(PropertyName = "mapOutputRatios")]
		public Dictionary<string, decimal> MapOutputRatios { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "exception")]
		public string Exception { get; set; }

		[JsonProperty(PropertyName = "errorCode")]
		public string ErrorCode { get; set; }

		[JsonProperty(PropertyName = "missingColumns")]
		public List<string> MissingColumns { get; set; }

		[JsonProperty(PropertyName = "encodingFailures")]
		public List<EncodingFailure> EncodingFailures { get; set; }

		[JsonProperty(PropertyName = "uuid")]
		public Guid? Uuid { get; set; }
	} // class Reply
} // namespace
