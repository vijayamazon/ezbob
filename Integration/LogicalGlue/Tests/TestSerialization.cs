namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.Models;
	using Newtonsoft.Json;
	using NUnit.Framework;

	[TestFixture]
	class TestSerialization {
		[Test]
		public void TestModelOutput() {
			ModelOutput mo = CreateModelOutput();
			IModelOutput imo = CreateModelOutput();

			string serializedClass = JsonConvert.SerializeObject(mo);
			string serializedIface = JsonConvert.SerializeObject(imo);

			Console.WriteLine("Serialized model as class: {0}", serializedClass);
			Console.WriteLine("Serialized model as iface: {0}", serializedIface);

			Assert.AreEqual(serializedClass, serializedIface);

			IModelOutput dic = JsonConvert.DeserializeObject<ModelOutput>(serializedClass);
			ModelOutput dcc = JsonConvert.DeserializeObject<ModelOutput>(serializedClass);
			IModelOutput dii = JsonConvert.DeserializeObject<ModelOutput>(serializedIface);
			ModelOutput dci = JsonConvert.DeserializeObject<ModelOutput>(serializedIface);

			Assert.AreEqual(serializedClass, JsonConvert.SerializeObject(dic));
			Assert.AreEqual(serializedClass, JsonConvert.SerializeObject(dcc));
			Assert.AreEqual(serializedClass, JsonConvert.SerializeObject(dii));
			Assert.AreEqual(serializedClass, JsonConvert.SerializeObject(dci));
		} // TestModelOutput

		private ModelOutput CreateModelOutput() {
			var lst = new List<Warning> {
				new Warning { FeatureName = "feature", MaxValue = "100", MinValue = "0", Value = "ab", },
				new Warning { FeatureName = null, MaxValue = null, MinValue = null, Value = null, },
				new Warning { FeatureName = "FEATURE", MaxValue = "900", MinValue = "1", Value = "-1", },
			};

			var mo = new ModelOutput {
				Status = "Status of the model",
				Grade = new Grade {
					Score = 28,
					EncodedResult = -1,
					DecodedResult = "Goooooooooooood",
					ListRangeErrors = new List<string>(new [] { "err 0", "err 1", }),
					MapOutputRatios = new Dictionary<string, decimal>(),
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
					Warnings = lst,
				},
			};

			// mo.Error.Warnings = lst;

			mo.Grade.MapOutputRatios[mo.Grade.DecodedResult] = 0.75m;
			mo.Grade.MapOutputRatios["Bad"] = 0.25m;

			return mo;
		} // CreateModelOutput

		private static readonly Guid uuid = Guid.NewGuid();
	} // class TestSerialization
} // namespace
