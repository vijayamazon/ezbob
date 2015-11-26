namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Newtonsoft.Json;
	using NUnit.Framework;

	[TestFixture]
	class HarvesterReplySerialization : ABaseTest {
		[Test]
		public void TestFullReply() {
			Engine.Interface.ModelOutput mo = CreateInternalModelOutput();

			TestOneFullReply(new Reply {
				Status = HttpStatusCode.OK,
				Error = "no error",
				Inference = new InferenceOutput {
					NeuralNetwork = new Harvester.Interface.ModelOutput {
						DecodedResult = mo.Grade.DecodedResult,
						EncodedResult = mo.Grade.EncodedResult,
						EncodingFailures = new List<Harvester.Interface.EncodingFailure>(
							mo.Error.EncodingFailures.Select(ef => new Harvester.Interface.EncodingFailure {
								ColumnName = ef.ColumnName,
								Message = ef.Message,
								Reason = ef.Reason,
								RowIndex = ef.RowIndex,
								UnencodedValue = ef.UnencodedValue,
							})
						),
						ErrorCode = mo.Error.ErrorCode,
						Exception = mo.Error.Exception,
						MissingColumns = new List<string>(mo.Error.MissingColumns),
						OutputRatios = new Dictionary<string, decimal>(mo.Grade.OutputRatios),
						Score = mo.Grade.Score,
						Status = mo.Status,
						Uuid = mo.Error.Uuid,
					},
					FuzzyLogic = new Harvester.Interface.ModelOutput {
						DecodedResult = mo.Grade.DecodedResult,
						EncodedResult = mo.Grade.EncodedResult,
						EncodingFailures = new List<Harvester.Interface.EncodingFailure>(
							mo.Error.EncodingFailures.Select(ef => new Harvester.Interface.EncodingFailure {
								ColumnName = ef.ColumnName,
								Message = ef.Message,
								Reason = ef.Reason,
								RowIndex = ef.RowIndex,
								UnencodedValue = ef.UnencodedValue,
							})
						),
						ErrorCode = mo.Error.ErrorCode,
						Exception = mo.Error.Exception,
						MissingColumns = new List<string>(mo.Error.MissingColumns),
						OutputRatios = new Dictionary<string, decimal>(mo.Grade.OutputRatios),
						Score = mo.Grade.Score,
						Status = mo.Status,
						Uuid = mo.Error.Uuid,
					},
					Bucket = Bucket.B,
				},
			});

			TestOneFullReply(new Reply {
				Status = HttpStatusCode.OK,
				Timeout = TimeoutReasons.Equifax,
			});
		} // TestFullReply

		private void TestOneFullReply(Reply reply) {
			Console.WriteLine("************************************************************************");
			Console.WriteLine("*");
			Console.WriteLine("* Test one full reply - start");
			Console.WriteLine("*");
			Console.WriteLine("************************************************************************");

			Console.WriteLine("Source object: {0}", reply);

			string serialized = JsonConvert.SerializeObject(reply, Formatting.Indented);

			Console.WriteLine("\n************************************************************************\n");

			Console.WriteLine("Serialized reply: {0}", serialized);

			Console.WriteLine("\n************************************************************************\n");

			Reply deserialized = JsonConvert.DeserializeObject<Reply>(serialized);

			Console.WriteLine("Target object: {0}", deserialized);

			Console.WriteLine("************************************************************************");
			Console.WriteLine("*");
			Console.WriteLine("* Test one full reply - end");
			Console.WriteLine("*");
			Console.WriteLine("************************************************************************");

			Assert.AreEqual(reply, deserialized);
		} // TestOneFullReply

		[Test]
		public void SerializeModelOutput() {
			string serialized = JsonConvert.SerializeObject(pattern, Formatting.Indented);

			Console.WriteLine("Serialized reply: {0}", serialized);

			Assert.AreEqual(serialized, serializedPattern);
		} // SerializeModelOutput

		[Test]
		public void DeserializeModelOutput() {
			Harvester.Interface.ModelOutput deserialized = JsonConvert.DeserializeObject<Harvester.Interface.ModelOutput>(serializedPattern);

			string serialized = JsonConvert.SerializeObject(deserialized, Formatting.Indented);

			Console.WriteLine("Serialized after deserialization reply: {0}", serialized);

			Assert.AreEqual(serialized, serializedPattern);
		} // DeserializeModelOutput

		private static readonly Guid uuid = new Guid("2626f582-bd52-4d92-95fc-c2ec2ee1b73c");

		private static readonly Harvester.Interface.ModelOutput pattern = new Harvester.Interface.ModelOutput {
			Score = 0.5927377710432571m,
			DecodedResult = "BAD",
			EncodedResult = -2147483446,
			Status = "SUCCESS",
			OutputRatios = new Dictionary<string, decimal> {
				{ "BAD", 0.5927377710432571m },
				{ "GOOD", 0.4072622289567429m },
			},
			Exception = "some exception",
			ErrorCode = "no error code",
			EncodingFailures = new List<Harvester.Interface.EncodingFailure> {
				new Harvester.Interface.EncodingFailure {
					ColumnName = "Bad encoded col",
					Message = "it's bad",
					Reason = "good reason",
					RowIndex = 0,
					UnencodedValue = "a value",
				},
				new Harvester.Interface.EncodingFailure {
					ColumnName = null,
					Message = null,
					Reason = null,
					RowIndex = 0,
					UnencodedValue = null,
				},
				new Harvester.Interface.EncodingFailure {
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
	} // class HarvesterReplySerialization
} // namespace
