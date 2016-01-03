using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Currencies {
    using System.Globalization;
    using Newtonsoft.Json;

    public class MoneyJsonSerializer : JsonConverter {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            Money money = value as Money;

            writer.WriteStartObject();
            writer.WritePropertyName("amount");
            writer.WriteValue(money.Amount);
            writer.WritePropertyName("region");
            writer.WriteValue(money.Region.TwoLetterISORegionName);
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
            reader.Read();
            var amount = reader.ReadAsDecimal().Value;
            reader.Read();
            var regionISOName = reader.ReadAsString();
            while (reader.Read()) {
                if (reader.TokenType == JsonToken.EndObject) {
                    break;
                }
            }

            return new Money(amount, new RegionInfo(regionISOName));
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Money);
        }
    }
}
