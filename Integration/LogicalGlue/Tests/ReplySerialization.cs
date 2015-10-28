namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue.HarvesterInterface;
	using Newtonsoft.Json;
	using NUnit.Framework;

	[TestFixture]
	class ReplySerialization {
		[Test]
		public void Serialize() {
			string serialized = JsonConvert.SerializeObject(pattern, Formatting.Indented);

			Console.WriteLine("Serialized reply: {0}", serialized);

			Assert.AreEqual(serialized, serializedPattern);
		} // Serialize

		[Test]
		public void Deserialize() {
			InferenceOutput deserialized = JsonConvert.DeserializeObject<InferenceOutput>(serializedPattern);

			string serialized = JsonConvert.SerializeObject(deserialized, Formatting.Indented);

			Console.WriteLine("Serialized after deserialization reply: {0}", serialized);

			Assert.AreEqual(serialized, serializedPattern);
		} // Deserialize

		private static readonly Guid uuid = new Guid("2626f582-bd52-4d92-95fc-c2ec2ee1b73c");

		private static readonly InferenceOutput pattern = new InferenceOutput {
			Score = 0.5927377710432571m,
			InferenceResultDecoded = "BAD",
			InferenceResultEncoded = -2147483446,
			Status = "SUCCESS",
			MapOutputRatios = new Dictionary<string, decimal> {
				{ "BAD", 0.5927377710432571m },
				{ "GOOD", 0.4072622289567429m },
			},
			Exception = "some exception",
			ErrorCode = "no error code",
			EncodingFailures = new List<HarvesterInterface.EncodingFailure> {
				new HarvesterInterface.EncodingFailure {
					ColumnName = "Bad encoded col",
					Message = "it's bad",
					Reason = "good reason",
					RowIndex = 0,
					UnencodedValue = "a value",
				},
				new HarvesterInterface.EncodingFailure {
					ColumnName = null,
					Message = null,
					Reason = null,
					RowIndex = 0,
					UnencodedValue = null,
				},
				new HarvesterInterface.EncodingFailure {
					ColumnName = "Another bad encoded col",
					Message = "it's really bad",
					Reason = "bad reason",
					RowIndex = 1,
					UnencodedValue = "another value",
				},
			},
			MissingColumns = new List<string> { "missing 0", "", "missing 1", null, "  ", "missing 2", },
			Uuid = uuid,
		};

		private const string serializedPattern = @"{
  ""score"": 0.5927377710432571,
  ""inferenceResultEncoded"": -2147483446,
  ""inferenceResultDecoded"": ""BAD"",
  ""warnings"": null,
  ""mapOutputRatios"": {
    ""BAD"": 0.5927377710432571,
    ""GOOD"": 0.4072622289567429
  },
  ""status"": ""SUCCESS"",
  ""exception"": ""some exception"",
  ""errorCode"": ""no error code"",
  ""missingColumns"": [
    ""missing 0"",
    """",
    ""missing 1"",
    null,
    ""  "",
    ""missing 2""
  ],
  ""encodingFailures"": [
    {
      ""rowIndex"": 0,
      ""columnName"": ""Bad encoded col"",
      ""unencodedValue"": ""a value"",
      ""reason"": ""good reason"",
      ""message"": ""it's bad""
    },
    {
      ""rowIndex"": 0,
      ""columnName"": null,
      ""unencodedValue"": null,
      ""reason"": null,
      ""message"": null
    },
    {
      ""rowIndex"": 1,
      ""columnName"": ""Another bad encoded col"",
      ""unencodedValue"": ""another value"",
      ""reason"": ""bad reason"",
      ""message"": ""it's really bad""
    }
  ],
  ""uuid"": ""2626f582-bd52-4d92-95fc-c2ec2ee1b73c""
}";
	} // class ReplySerialization
} // namespace
