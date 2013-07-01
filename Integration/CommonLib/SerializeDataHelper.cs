using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace EzBob.CommonLib
{
	public static class SerializeDataHelper
	{
		public static byte[] Serialize( object info )
		{
			byte[] data;

			var modelSerializer = SerializerCache.GetSerializer( info.GetType() );
			using ( var mem = new MemoryStream() )
			{
				modelSerializer.Serialize( mem, info );
				data = mem.GetBuffer();
			}

			return data;
		}

		public static void SerializeStreamToFile( string fileName, MemoryStream inputStream )
		{
			using ( var stream = new FileStream( fileName, FileMode.Create ) )
			{
				inputStream.WriteTo( stream );
			}
		}

		public static string SerializeToString<T>( T info )
		{
			var bytes = Serialize( info );

			return Encoding.UTF8.GetString( bytes );

		}

		public static void SerializeToFile<T>( string fileName, T type )
		{
			var serializer = SerializerCache.GetSerializer( typeof( T ) );
			using ( Stream fs = new FileStream( fileName, FileMode.Create ) )
			{
				using ( var writer = new XmlTextWriter( fs, Encoding.UTF8 ) )
				{
					serializer.Serialize( writer, type );
					writer.Close();
				}
			}


		}

		public static T DeserializeTypeFromString<T>( string data )
		{
			var bytes = Encoding.UTF8.GetBytes( data );
			using ( var stream = new MemoryStream( bytes ) )
			{
				return DeserializeType<T>( stream );
			}
		}

		public static T DeserializeTypeFromFile<T>( string fileName )
		{
			T rez;

			var serializer = SerializerCache.GetSerializer( typeof( T ) );

			using ( var fs = new FileStream( fileName, FileMode.Open ) )
			{

				using ( var reader = XmlReader.Create( fs ) )
				{
					rez = (T)serializer.Deserialize( reader );
					fs.Close();
				}
			}
			return rez;
		}

		public static T DeserializeType<T>( byte[] data )
		{
			using ( var stream = new MemoryStream( data ) )
			{
				return DeserializeType<T>( stream );
			}
		}

		public static T DeserializeType<T>( Stream stream )
		{
			var serializer = SerializerCache.GetSerializer( typeof( T ) );

		    var reader = new StreamReader(stream);
		    var xml = reader.ReadToEnd();

            string cleanXml = Regex.Replace(xml, @"<[a-zA-Z].[^(><.)]+/>", new MatchEvaluator(RemoveText));
            var memoryStream = new MemoryStream((new UTF8Encoding()).GetBytes(cleanXml));
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

            return (T)serializer.Deserialize(memoryStream);
			/*using ( var strmReader = new StreamReader( stream ) )
			{
				
				using ( var xmlReader = XmlReader.Create( strmReader ) )
				{
					return (T)serializer.Deserialize( xmlReader );
				}
			}*/
		}

        static string RemoveText(Match m) { return ""; }
	}
}