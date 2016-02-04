namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Globalization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	internal class InferenceInputSerializer : JsonConverter {
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			InferenceInput input = value as InferenceInput;

			if (input == null)
				return;

			writer.WriteStartObject();

			writer.WritePropertyName(Payment);
			writer.WriteRawValue(input.MonthlyPayment == null
				? "0.00"
				: Math.Round(input.MonthlyPayment.Value).ToString("##.00", CultureInfo.InvariantCulture)
			);

			bool hasDirector = input.Director != null;

			WriteIfNotEmpty(writer, serializer, FirstName, hasDirector ? input.Director.FirstName : null);
			WriteIfNotEmpty(writer, serializer, LastName, hasDirector ? input.Director.LastName : null);
			WriteIfNotEmpty(writer, serializer, BirthDate, hasDirector
				? input.Director.DateOfBirth.ToString(BirthDateFormat, CultureInfo.InvariantCulture)
				: null
			);

			if (!string.IsNullOrWhiteSpace(input.EquifaxData))
				WriteIfNotEmpty(writer, serializer, Equifax, input.EquifaxData);
			else {
				WriteIfNotEmpty(writer, serializer, RegNum, input.CompanyRegistrationNumber);
				WriteIfNotEmpty(writer, serializer, Postcode, hasDirector ? input.Director.Postcode : null);
				WriteIfNotEmpty(writer, serializer, HouseNumber, hasDirector ? input.Director.HouseNumber : null);
			} // if

			writer.WriteEndObject();
		} // WriteJson

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>
		/// The object value.
		/// </returns>
		public override object ReadJson(
			JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer
		) {
			JObject jo = JObject.Load(reader);

			return new InferenceInput {
				EquifaxData = jo[Equifax].Value<string>(),
				MonthlyPayment = jo[Payment].Value<decimal>(),
				CompanyRegistrationNumber = jo[RegNum].Value<string>(),
				Director = new DirectorData {
					FirstName = jo[FirstName].Value<string>(),
					LastName = jo[LastName].Value<string>(),
					DateOfBirth = DateTime.ParseExact(
						jo[BirthDate].Value<string>(),
						BirthDateFormat,
						CultureInfo.InvariantCulture,
						DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
					),
					Postcode = jo[Postcode].Value<string>(),
					HouseNumber = jo[HouseNumber].Value<string>(),
				},
			};
		} // ReadJson

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>
		/// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type objectType) {
			return objectType == typeof(InferenceInput);
		} // CanConvert

		private static void WriteIfNotEmpty(JsonWriter writer, JsonSerializer serializer, string name, string val ) {
			if (string.IsNullOrWhiteSpace(val))
				return;

			writer.WritePropertyName(name);
			serializer.Serialize(writer, val);
		} // WriteIfNotEmpty

		private const string Equifax     = "equifax";
		private const string RegNum      = "companiesHouseRegisteredNumber";
		private const string Payment     = "monthlyPayment";
		private const string FirstName   = "directorFirstName";
		private const string LastName    = "directorLastName";
		private const string BirthDate   = "dob";
		private const string Postcode    = "postcode";
		private const string HouseNumber = "houseNumber";

		private const string BirthDateFormat = "yyyy-MM-dd";
	} // class InferenceInputSerializer
} // namespace
