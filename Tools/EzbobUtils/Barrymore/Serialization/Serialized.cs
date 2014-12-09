namespace Ezbob.Utils.Serialization {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Xml.Serialization;

	public class Serialized {

		public static string AsBase64String<T>(T oData) {
			return Convert.ToBase64String(new Serialized(oData));
		} // AsBase64String

		public static byte[] AsBase64<T>(T oData) {
			return Convert.FromBase64String(AsBase64String(oData));
		} // AsBase64

		public static void Serialize<T>(string fileName, T type) {
			var serializer = GetSerializer(typeof(T));

			using (Stream fs = new FileStream(fileName, FileMode.Create)) {
				using (var writer = new XmlTextWriter(fs, Encoding.UTF8)) {
					serializer.Serialize(writer, type);
					writer.Close();
				} // using writer
			} // using stream
		} // Serialize

		public static T Deserialize<T>(Serialized data) {
			return Deserialize<T>(data.m_oData);
		} // Deserialize

		public static T Deserialize<T>(string data) {
			return Deserialize<T>(Encoding.UTF8.GetBytes(data));
		} // Deserialize

		public static T Deserialize<T>(FileStream fs) {
			T rez;

			var serializer = GetSerializer(typeof(T));
				using (var reader = XmlReader.Create(fs)) {
					rez = (T)serializer.Deserialize(reader);
					fs.Close();
				} // using reader

			return rez;
		} // Deserialize

		public static T Deserialize<T>(byte[] data) {
			using (var stream = new MemoryStream(data)) {
				return Deserialize<T>(stream);
			} // using stream
		} // Deserialize

		public static T Deserialize<T>(Stream stream) {
			var serializer = GetSerializer(typeof(T));
			return (T)serializer.Deserialize(stream);
		} // Deserialize

		public static implicit operator byte[](Serialized obj) {
			return obj.m_oData;
		} // to byte[]

		public static implicit operator string(Serialized obj) {
			return Encoding.UTF8.GetString(obj.m_oData);
		} // to byte[]

		public Serialized(object obj) {
			var modelSerializer = GetSerializer(obj.GetType());

			using (var mem = new MemoryStream()) {
				modelSerializer.Serialize(mem, obj);
				m_oData = mem.GetBuffer();
			} // using stream
		} // constructor

		private readonly byte[] m_oData;

		private static XmlSerializer GetSerializer(Type type) {
			XmlSerializer res = null;

			lock (ms_oHash) {
				if (ms_oHash.ContainsKey(type.FullName))
					res = ms_oHash[type.FullName];
				else {
					res = new XmlSerializer(type);
					ms_oHash[type.FullName] = res;
				}
			} // lock

			return res;
		} // GetSerializer

		private static readonly SortedDictionary<string, XmlSerializer> ms_oHash = new SortedDictionary<string, XmlSerializer>();

	} // class Serialized
} // namespace
