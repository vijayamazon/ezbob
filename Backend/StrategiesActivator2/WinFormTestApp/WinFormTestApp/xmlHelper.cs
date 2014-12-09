using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Data;
using System.Reflection;

namespace WinFormTestApp
{
	class xmlHelper
	{
		/// <summary>
		/// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
		/// </summary>
		/// <param name="characters">Unicode Byte Array to be converted to String</param>
		/// <returns>String converted from Unicode Byte Array</returns>
		private static string UnicodeByteArrayToString(byte[] characters)
		{
			UnicodeEncoding encoding = new UnicodeEncoding();
			string constructedString = encoding.GetString(characters);
			return (constructedString);
		}

		/// <summary>
		/// Converts the String to Unicode Byte array and is used in De serialization
		/// </summary>
		/// <param name="pXmlString"></param>
		/// <returns></returns>
		private static Byte[] StringToUnicodeByteArray(string pXmlString)
		{
			UnicodeEncoding encoding = new UnicodeEncoding();
			byte[] byteArray = encoding.GetBytes(pXmlString);
			return byteArray;
		}

		/// <summary>
		/// Serialize an object into an XML string
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string SerializeObject<T>(T obj, string name = "")
		{
			try
			{
				string xmlString = null;
				MemoryStream memoryStream = new MemoryStream();
				XmlSerializer xs = new XmlSerializer(typeof(T));
				XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.Unicode);
				xmlTextWriter.Formatting = Formatting.Indented;

				xs.Serialize(xmlTextWriter, obj);
				memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
				xmlString = UnicodeByteArrayToString(memoryStream.ToArray());

				//StringWriter sw = new StringWriter();
				//XmlSerializer serializer = new XmlSerializer(type);

				string time = "" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + "T" + DateTime.Now.Hour.ToString("00") + "_" + DateTime.Now.Minute.ToString("00") + "_" + DateTime.Now.Second.ToString("00") + "ms" + DateTime.Now.Millisecond;
				XmlTextWriter writer = new XmlTextWriter("..\\..\\xmls\\" + typeof(T).Name + name + time + ".xml", Encoding.Unicode);
				writer.Formatting = Formatting.Indented;
				xs.Serialize(writer, obj);
				writer.Close();
				return xmlString;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static T NullStringToEmpty<T>(T myObject) where T : class
		{
			PropertyInfo[] properties = typeof(T).GetProperties();
			foreach (var info in properties)
			{
				// if a string and null, set to String.Empty
				if (info.PropertyType == typeof(string) &&
				   info.GetValue(myObject, null) == null)
				{
					info.SetValue(myObject, String.Empty, null);
				}
			}
			return myObject;
		}

		/// <summary>
		/// Reconstruct an object from an XML string
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static T DeserializeObject<T>(string xml, string name = "")
		{
			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				MemoryStream memoryStream = new MemoryStream(StringToUnicodeByteArray(xml));
				XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.Unicode);

				T obj = (T)xs.Deserialize(memoryStream);

				string time = "" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + "T" + DateTime.Now.Hour.ToString("00") + "_" + DateTime.Now.Minute.ToString("00") + "_" + DateTime.Now.Second.ToString("00") + "ms" + DateTime.Now.Millisecond;
				XmlTextWriter writer = new XmlTextWriter("..\\..\\xmls\\" + typeof(T).Name + name + time + ".xml", Encoding.Unicode);
				writer.Formatting = Formatting.Indented;
				xs.Serialize(writer, obj);
				writer.Close();
				return obj;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static void ConvertXmlNodeToTreeNode(XmlNode xmlNode, TreeNodeCollection treeNodes)
		{
			TreeNode newTreeNode = treeNodes.Add(xmlNode.Name);

			switch (xmlNode.NodeType)
			{
				case XmlNodeType.ProcessingInstruction:
				case XmlNodeType.XmlDeclaration:
					newTreeNode.Text = "<?" + xmlNode.Name + " " +
					  xmlNode.Value + "?>";
					break;
				case XmlNodeType.Element:
					newTreeNode.Text = "<" + xmlNode.Name + ">";
					break;
				//case XmlNodeType.Attribute:
				//    newTreeNode.Text = "ATTRIBUTE: " + xmlNode.Name;
				//    break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
					newTreeNode.Text = xmlNode.Value;
					break;
				case XmlNodeType.Comment:
					newTreeNode.Text = "<!--" + xmlNode.Value + "-->";
					break;
			}

			//if (xmlNode.Attributes != null)
			//{
			//    foreach (XmlAttribute attribute in xmlNode.Attributes)
			//    {
			//        ConvertXmlNodeToTreeNode(attribute, newTreeNode.Nodes);
			//    }
			//}
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				ConvertXmlNodeToTreeNode(childNode, newTreeNode.Nodes);
			}
		}

		public static void fillTree(string xml, TreeView tree)
		{
			XmlDocument xmlDocument = new XmlDocument();
			System.IO.StringReader stringReader = new System.IO.StringReader(xml);
			stringReader.Read();
			xmlDocument.LoadXml(stringReader.ReadToEnd());
			xmlHelper.ConvertXmlNodeToTreeNode(xmlDocument, tree.Nodes);
			tree.Nodes[0].ExpandAll();
		}

		public static void saveSummary(DataTable dt, string outputFilePath)
		{
			int[] maxLengths = new int[dt.Columns.Count];

			for (int i = 0; i < dt.Columns.Count; i++)
			{
				maxLengths[i] = dt.Columns[i].ColumnName.Length;

				foreach (DataRow row in dt.Rows)
				{
					if (!row.IsNull(i))
					{
						int length = row[i].ToString().Length;

						if (length > maxLengths[i])
						{
							maxLengths[i] = length;
						}
					}
				}
			}

			using (StreamWriter sw = new StreamWriter(outputFilePath, false))
			{
				for (int i = 0; i < dt.Columns.Count; i++)
				{
					sw.Write(dt.Columns[i].ColumnName.PadRight(maxLengths[i] + 2));
				}

				sw.WriteLine();

				foreach (DataRow row in dt.Rows)
				{
					for (int i = 0; i < dt.Columns.Count; i++)
					{
						if (!row.IsNull(i))
						{
							sw.Write(row[i].ToString().PadRight(maxLengths[i] + 2));
						}
						else
						{
							sw.Write(new string(' ', maxLengths[i] + 2));
						}
					}

					sw.WriteLine();
				}

				sw.Close();
			}
		}
	}
}
