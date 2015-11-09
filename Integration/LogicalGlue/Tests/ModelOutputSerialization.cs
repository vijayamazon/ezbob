namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue.Processor.Interface;
	using Newtonsoft.Json;
	using NUnit.Framework;

	[TestFixture]
	class ModelOutputSerialization {
		[Test]
		public void DoTest() {
			ModelOutput mo = CreateModelOutput();

			string serializedClass = JsonConvert.SerializeObject(mo);

			Console.WriteLine("Serialized model as class: {0}", serializedClass);

			ModelOutput dcc = JsonConvert.DeserializeObject<ModelOutput>(serializedClass);

			Assert.AreEqual(serializedClass, JsonConvert.SerializeObject(dcc));
		} // DoTest

		private ModelOutput CreateModelOutput() {
			var mo = new ModelOutput {
				Status = "Status of the model",
				Grade = new Grade {
					Score = 28,
					EncodedResult = -1,
					DecodedResult = "Goooooooooooood",
					MapOutputRatios = new Dictionary<string, decimal>(),
					Warnings = new List<Warning> {
						new Warning { FeatureName = "feature", MaxValue = "100", MinValue = "0", Value = "ab", },
						new Warning { FeatureName = null, MaxValue = null, MinValue = null, Value = null, },
						new Warning { FeatureName = "FEATURE", MaxValue = "900", MinValue = "1", Value = "-1", },
					},
				},
				Error = new Error {
					ErrorCode = "error code",
					Exception = "some exception",
					Uuid = uuid,
					EncodingFailures = new List<EncodingFailure> {
						new EncodingFailure {
							ColumnName = "Bad encoded col",
							Message = "it's bad",
							Reason = "good reason",
							RowIndex = 0,
							UnencodedValue = "a value",
						},
						new EncodingFailure {
							ColumnName = null,
							Message = null,
							Reason = null,
							RowIndex = 0,
							UnencodedValue = null,
						},
						new EncodingFailure {
							ColumnName = "Another bad encoded col",
							Message = "it's really bad",
							Reason = "bad reason",
							RowIndex = 1,
							UnencodedValue = "another value",
						},
					},
					MissingColumns = new List<string> { "missing 0", "", "missing 1", null, "  ", "missing 2", },
				},
			};

			mo.Grade.MapOutputRatios[mo.Grade.DecodedResult] = 0.75m;
			mo.Grade.MapOutputRatios["Bad"] = 0.25m;

			return mo;
		} // CreateModelOutput

		private static readonly Guid uuid = Guid.NewGuid();
	} // class ModelOutputSerialization
} // namespace
