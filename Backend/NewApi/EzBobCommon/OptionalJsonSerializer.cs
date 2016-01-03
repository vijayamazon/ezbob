namespace EzBobCommon {
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Converts Optional to/from json
    /// </summary>
    public class OptionalJsonSerializer : JsonConverter {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            dynamic optional = value;

            bool hasValue = optional.HasValue;

            writer.WriteStartObject();
            writer.WritePropertyName("HasValue");
            writer.WriteValue(hasValue);
            if (hasValue) {
                writer.WritePropertyName("ValueType");
                writer.WriteValue((string)optional.GetValue()
                    .GetType()
                    .FullName);
            }
            writer.WritePropertyName("Value");
            serializer.Serialize(writer, hasValue ? optional.GetValue() : string.Empty);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.Read() && reader.Read()) {
                bool hasValue = (bool)reader.Value;
                if (hasValue && reader.Read() && reader.Read()) {
                    var valueType = Type.GetType((string)reader.Value);
                    var optionalT = typeof(Optional<>).MakeGenericType(valueType);
                    reader.Read();
                    reader.Read();
                    object value = reader.Value;
                    int typeCode = (int)Type.GetTypeCode(valueType);
                    
                    //numerical type codes
                    if (typeCode >= 6 && typeCode <= 15) {
                        value = Convert.ChangeType(value, Type.GetTypeCode(valueType));
                    }

                    while (reader.Read()) {
                        if (reader.TokenType == JsonToken.EndObject) {
                            break;
                        }
                    }

                    return optionalT.GetMethod("Of")
                        .Invoke(null, new[] {
                            value
                        });
                }
            }

            return Optional<object>.Empty();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType) {
            return objectType.GetGenericTypeDefinition() == typeof(Optional<>);
        }
    }
}