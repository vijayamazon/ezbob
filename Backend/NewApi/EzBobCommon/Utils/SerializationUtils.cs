using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Utils {
    using System.IO;
    using System.Xml.Serialization;
    using Newtonsoft.Json;

    public static class SerializationUtils {
        /// <summary>
        /// Serializes to binary XML.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">got null object to serialize</exception>
        public static byte[] SerializeToBinaryXml(object obj) {
            if (obj == null) {
                throw new ArgumentException("got null object to serialize");
            }

            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            using (var memory = new MemoryStream()) {
                xmlSerializer.Serialize(memory, obj);
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Deserializes from binary XML which was serialized by <see cref="SerializeToBinaryXml"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">got empty data to deserialze</exception>
        public static T DeserializeFromBinaryXml<T>(byte[] data) {
            if (CollectionUtils.IsEmpty(data)) {
                throw new ArgumentException("got empty data to deserialze");
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (var memory = new MemoryStream(data)) {
                return (T)xmlSerializer.Deserialize(memory);
            }
        }

        /// <summary>
        /// Deserializes the binary json to {T}.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static T DeserializeBinaryJson<T>(byte[] data) where T : class {
            using (var stream = new MemoryStream(data)) {
                using (var reader = new StreamReader(stream))
                    return JsonSerializer.Create()
                        .Deserialize(reader, typeof(T)) as T;
            }
        }
    }
}
