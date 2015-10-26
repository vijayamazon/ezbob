namespace Tests {
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
			var mo = new ModelOutput {
				Status = "Status of the model",
				Score = 28,
				EncodedResult = -1,
				DecodedResult = "Goooooooooooood",
				ListRangeErrors = new List<string>(new [] { "err 0", "err 1", }),
				MapOutputRatios = new Dictionary<string, decimal>(),
			};

			mo.MapOutputRatios[mo.DecodedResult] = 0.75m;
			mo.MapOutputRatios["Bad"] = 0.25m;

			return mo;
		} // CreateModelOutput
	} // class TestSerialization
} // namespace
