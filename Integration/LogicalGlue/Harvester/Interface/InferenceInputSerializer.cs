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

			writer.WritePropertyName(Equifax);
			serializer.Serialize(writer, input.EquifaxData);

			writer.WritePropertyName(RegNum);
			serializer.Serialize(writer, input.CompanyRegistrationNumber);

			writer.WritePropertyName(Payment);
			serializer.Serialize(writer, input.MonthlyPayment);

			bool hasDirector = input.Director != null;

			writer.WritePropertyName(FirstName);
			serializer.Serialize(writer, hasDirector ? input.Director.FirstName : string.Empty);

			writer.WritePropertyName(LastName);
			serializer.Serialize(writer, hasDirector ? input.Director.LastName : string.Empty);

			writer.WritePropertyName(BirthDate);

			serializer.Serialize(writer, hasDirector
				? input.Director.DateOfBirth.ToString(BirthDateFormat, CultureInfo.InvariantCulture)
				: string.Empty
			);

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
				Director = new InferenceInput.DirectorData {
					FirstName = jo[FirstName].Value<string>(),
					LastName = jo[LastName].Value<string>(),
					DateOfBirth = DateTime.ParseExact(
						jo[BirthDate].Value<string>(),
						BirthDateFormat,
						CultureInfo.InvariantCulture,
						DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
					),
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

		private const string Equifax   = "equifax";
		private const string RegNum    = "companiesHourseRegisteredNumber";
		private const string Payment   = "monthlyPayment";
		private const string FirstName = "directorFirstName";
		private const string LastName  = "directorLastName";
		private const string BirthDate = "dob";

		private const string BirthDateFormat = "yyyy-MM-dd";
	} // class InferenceInputSerializer
} // namespace
