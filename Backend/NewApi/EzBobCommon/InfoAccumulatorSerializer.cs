namespace EzBobCommon {
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class InfoAccumulatorSerializer : JsonConverter {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            InfoAccumulator info = value as InfoAccumulator;;

            writer.WriteStartObject();
            writer.WritePropertyName("errors");
            serializer.Serialize(writer, info.GetErrors());
            writer.WritePropertyName("warnings");
            serializer.Serialize(writer, info.GetWarning());
            writer.WritePropertyName("infos");
            serializer.Serialize(writer, info.GetInfo());
            writer.WritePropertyName("exceptions");
            serializer.Serialize(writer, info.GetExceptions());
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

            InfoAccumulator info = Activator.CreateInstance(objectType) as InfoAccumulator;

            string currentSection = null;
            while (reader.Read()) {
                if (reader.TokenType == JsonToken.EndObject) {
                    break;
                }

                if (reader.ValueType == typeof(string)) {
                    string curValue = (string)reader.Value;
                    if (curValue == currentSection) {
                        currentSection = "";
                    } else {
                        currentSection = (string)reader.Value;
                    }
                    continue;
                }

                switch (currentSection) {
                case "errors":
                    var errors = (IEnumerable<string>)serializer.Deserialize(reader, typeof(IEnumerable<string>));
                    foreach (var error in errors) {
                        info.AddError(error);
                    }
                    break;
                case "warnings":
                    var warnings = (IEnumerable<string>)serializer.Deserialize(reader, typeof(IEnumerable<string>));
                    foreach (var warning in warnings) {
                        info.AddWarning(warning);
                    }
                    break;
                case "infos":
                    var infos = (IEnumerable<string>)serializer.Deserialize(reader, typeof(IEnumerable<string>));
                    foreach (var inf in infos) {
                        info.AddInfo(inf);
                    }
                    break;
                case "exceptions":
                    var exceptions = (IEnumerable<Exception>)serializer.Deserialize(reader, typeof(IEnumerable<Exception>));
                    foreach (var exception in exceptions) {
                        info.AddException(exception);
                    }
                    break;
                }
            }

            return info;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType) {
            return typeof(InfoAccumulator).IsAssignableFrom(objectType);
        }
    }
}