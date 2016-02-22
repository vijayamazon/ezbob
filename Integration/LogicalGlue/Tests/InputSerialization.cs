namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Newtonsoft.Json;
	using NUnit.Framework;

	[TestFixture]
	class InputSerialization {
		[Test]
		public void Serialize() {
			InferenceInput ii = new InferenceInput {
				CompanyRegistrationNumber = "12121234",
				Director = new DirectorData {
					FirstName = "Basil",
					LastName = "Poop-kind",
					DateOfBirth = new DateTime(1980, 3, 8, 0, 0, 0, DateTimeKind.Utc),
					Postcode = "AB101BA",
					HouseNumber = "61",
				},
				EquifaxData = "<xml>equi</xml>",
				MonthlyPayment = 1265,
			};

			Console.WriteLine("Serialize this: {0}", ii);

			string serialized = JsonConvert.SerializeObject(ii, Formatting.Indented);

			Console.WriteLine("Serialized: {0}", serialized);

			InferenceInput deserialized = JsonConvert.DeserializeObject<InferenceInput>(serialized);

			Console.WriteLine("Deserialized: {0}", deserialized);

			Assert.AreEqual(ii, deserialized);
		} // Serialize
	} // class InputSerialization
} // namespace
