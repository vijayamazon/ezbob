namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Newtonsoft.Json;
	using NUnit.Framework;

	[TestFixture]
	class ModelOutputSerialization : ABaseTest {
		[Test]
		public void DoTest() {
			ModelOutput mo = CreateInternalModelOutput();

			string serializedClass = JsonConvert.SerializeObject(mo);

			Console.WriteLine("Serialized model as class: {0}", serializedClass);

			ModelOutput dcc = JsonConvert.DeserializeObject<ModelOutput>(serializedClass);

			Assert.AreEqual(serializedClass, JsonConvert.SerializeObject(dcc));
		} // DoTest
	} // class ModelOutputSerialization
} // namespace
