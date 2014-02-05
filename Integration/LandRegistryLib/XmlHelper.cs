﻿namespace LandRegistryLib
{
	using System;
	using System.Text;
	using System.IO;
	using System.Xml;
	using System.Xml.Serialization;

	public static class XmlHelper
	{
		public static string SerializeObject<T>(T serializableObject)
		{
			if (serializableObject == null) { return null; }

			try
			{
				var serializer = new XmlSerializer(serializableObject.GetType());
				var stream = new MemoryStream();

				using (var xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8) { Formatting = Formatting.Indented })
				{
					serializer.Serialize(xmlTextWriter, serializableObject);
					stream = (MemoryStream)xmlTextWriter.BaseStream;
					string xmlString = new UTF8Encoding().GetString(stream.ToArray());
					stream.Dispose();
					return xmlString;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static T XmlDeserializeFromString<T>(string objectData)
		{
			return (T)XmlDeserializeFromString(objectData, typeof(T));
		}

		public static object XmlDeserializeFromString(string objectData, Type type)
		{
			var serializer = new XmlSerializer(type);
			object result;

			using (TextReader reader = new StringReader(objectData))
			{
				result = serializer.Deserialize(reader);
			}

			return result;
		}

		public static T DeserializeObject<T>(string xml)
		{
			try
			{
				var xs = new XmlSerializer(typeof(T));
				var memoryStream = new MemoryStream(StringToUnicodeByteArray(xml));
				T obj = (T)xs.Deserialize(memoryStream);
				return obj;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private static Byte[] StringToUnicodeByteArray(string pXmlString)
		{
			UnicodeEncoding encoding = new UnicodeEncoding();
			byte[] byteArray = encoding.GetBytes(pXmlString);
			return byteArray;
		}
	}
}
