using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
//using System.Windows.Forms;

namespace BiztalkTestClient
{
    class xmlHelper
    {
        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        private static string UTF8ByteArrayToString(byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        private static Byte[] StringToUTF8ByteArray(string pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
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
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                xmlTextWriter.Formatting = Formatting.Indented;
                xs.Serialize(xmlTextWriter, obj);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                xmlString = UTF8ByteArrayToString(memoryStream.ToArray());

                //StringWriter sw = new StringWriter();
                //XmlSerializer serializer = new XmlSerializer(type);
                XmlTextWriter writer = new XmlTextWriter("..\\..\\xmls\\" + typeof(T).Name + name + DateTime.Now.ToFileTime() + ".xml", Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                xs.Serialize(writer, obj);

                return xmlString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reconstruct an object from an XML string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string xml, string name = "")
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xml));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            
            T obj = (T)xs.Deserialize(memoryStream);
            
            XmlTextWriter writer = new XmlTextWriter("..\\..\\xmls\\" + typeof(T).Name + name + DateTime.Now.ToFileTime() + ".xml", Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xs.Serialize(writer, obj);

            return obj;
        }

/*
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
        */
    }
}
