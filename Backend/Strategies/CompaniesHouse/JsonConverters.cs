
namespace Ezbob.Backend.Strategies.CompaniesHouse {
	using System;
	using System.Linq;
	using Newtonsoft.Json;

	public class OfficerRoleConvertor : JsonConverter {
		public override bool CanConvert(Type objectType) {
			//assume we can convert to anything for now
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			//explicitly specify the concrete type we want to create
			if (Dictionaries.officer_role.ContainsKey(reader.Value.ToString())) {
				return Dictionaries.officer_role[reader.Value.ToString()];
			}
			return serializer.Deserialize<string>(reader);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (Dictionaries.officer_role.ContainsValue(value.ToString())) {
				var role = Dictionaries.officer_role.First(x => x.Value == value.ToString());
				serializer.Serialize(writer, role.Key);
			} else {
				serializer.Serialize(writer, value);
			}
		}
	}

	public class ErrorConvertor : JsonConverter {
		public override bool CanConvert(Type objectType) {
			//assume we can convert to anything for now
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			//explicitly specify the concrete type we want to create
			if (Dictionaries.serviceErrors.ContainsKey(reader.Value.ToString())) {
				return Dictionaries.serviceErrors[reader.Value.ToString()];
			}
			return serializer.Deserialize<string>(reader);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (Dictionaries.serviceErrors.ContainsValue(value.ToString())) {
				var role = Dictionaries.serviceErrors.First(x => x.Value == value.ToString());
				serializer.Serialize(writer, role.Key);
			} else {
				serializer.Serialize(writer, value);
			}
		}
	}

	public class IdentificationTypeConvertor : JsonConverter {
		public override bool CanConvert(Type objectType) {
			//assume we can convert to anything for now
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			//explicitly specify the concrete type we want to create
			if (Dictionaries.identification_type.ContainsKey(reader.Value.ToString())) {
				return Dictionaries.identification_type[reader.Value.ToString()];
			}
			return serializer.Deserialize<string>(reader);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (Dictionaries.identification_type.ContainsValue(value.ToString())) {
				var role = Dictionaries.identification_type.First(x => x.Value == value.ToString());
				serializer.Serialize(writer, role.Key);
			} else {
				serializer.Serialize(writer, value);
			}
		}
	}

	public class CompanyStatusConvertor : JsonConverter {
		public override bool CanConvert(Type objectType) {
			//assume we can convert to anything for now
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			//explicitly specify the concrete type we want to create
			if (Dictionaries.company_status.ContainsKey(reader.Value.ToString())) {
				return Dictionaries.company_status[reader.Value.ToString()];
			}
			return serializer.Deserialize<string>(reader);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (Dictionaries.company_status.ContainsValue(value.ToString())) {
				var role = Dictionaries.company_status.First(x => x.Value == value.ToString());
				serializer.Serialize(writer, role.Key);
			} else {
				serializer.Serialize(writer, value);
			}
		}
	}

}
