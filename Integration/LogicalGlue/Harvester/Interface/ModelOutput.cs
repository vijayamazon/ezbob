namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class ModelOutput {
		[JsonProperty(PropertyName = "score", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Score { get; set; }

		[JsonProperty(PropertyName = "inferenceResultEncoded", NullValueHandling = NullValueHandling.Ignore)]
		public long? EncodedResult { get; set; }

		[JsonProperty(PropertyName = "inferenceResultDecoded", NullValueHandling = NullValueHandling.Ignore)]
		public string DecodedResult { get; set; }

		[JsonProperty(PropertyName = "warnings", NullValueHandling = NullValueHandling.Ignore)]
		public List<Warning> Warnings { get; set; }

		[JsonProperty(PropertyName = "mapOutputRatios", NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, decimal> OutputRatios { get; set; }

		[JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "exception", NullValueHandling = NullValueHandling.Ignore)]
		public string Exception { get; set; }

		[JsonProperty(PropertyName = "errorCode", NullValueHandling = NullValueHandling.Ignore)]
		public string ErrorCode { get; set; }

		[JsonProperty(PropertyName = "missingColumns", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> MissingColumns { get; set; }

		[JsonProperty(PropertyName = "encodingFailures", NullValueHandling = NullValueHandling.Ignore)]
		public List<EncodingFailure> EncodingFailures { get; set; }

		[JsonProperty(PropertyName = "uuid", NullValueHandling = NullValueHandling.Ignore)]
		public Guid? Uuid { get; set; }
	} // class ModelOutput
} // namespace
