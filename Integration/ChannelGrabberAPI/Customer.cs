using System;
using System.Collections.Generic;
using System.Xml;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberAPI {

	class Customer : IJsonable {

		public Customer(XmlNode oNode) : this() {
			FromXml(oNode);
		} // constructor

		public Customer(string sName = "", string sCountry = "") {
			Init();

			Name = (sName ?? "").Trim();
			Country = (sCountry ?? "").Trim();
		} // constructor

		public int Id { get { return m_oIntData[XmlUtil.IdNode]; } } // Id

		public string Name {
			get {  return m_oStrData[NameNode];         }
			private set { m_oStrData[NameNode] = value; }
		} // Name

		public string Country {
			get {  return m_oStrData[CountryNode];         }
			private set { m_oStrData[CountryNode] = value; }
		} // Country

		public XmlDocument ToXml(string sRootNodeName = "customer") {
			var doc = new XmlDocument();

			doc.LoadXml(string.Format("<{0} />", sRootNodeName));

			foreach (var pair in m_oIntData)
				AddChild(doc, pair.Key, pair.Value.ToString());

			foreach (var pair in m_oStrData)
				AddChild(doc, pair.Key, pair.Value);

			return doc;
		} // ToXml

		public void FromXml(XmlDocument oDoc) {
			Init();

			if (oDoc != null)
				FromXml(oDoc.DocumentElement);
		} // FromXml

		public void FromXml(XmlNode oNode) {
			Init();

			if (null == oNode)
				return;

			Xml2Data(m_oIntData, oNode, XmlConvert.ToInt32);

			Xml2Data(m_oStrData, oNode, s => s.Trim());
		} // FromXml

		public bool IsValid() {
			return (Name != string.Empty) && (Country != string.Empty) && (Id > 0);
		} // IsValid

		public object ToJson() {
			return this;
		} // ToJson

		private const string NameNode = "name";
		private const string CountryNode = "country";

		private Dictionary<string, string> m_oStrData;
		private Dictionary<string, int> m_oIntData;

		private void Init() {
			m_oIntData = new Dictionary<string, int>();
			m_oStrData = new Dictionary<string, string>();

			m_oIntData[XmlUtil.IdNode] = 0;
			m_oStrData[NameNode] = "";
			m_oStrData[CountryNode] = "";
		} // Init

		private void Xml2Data<TValue>(Dictionary<string, TValue> oData, XmlNode oNode, Func<string, TValue> oAction) {
			if (null == oNode)
				return;

			var oKeys = new List<string>(oData.Keys);

			foreach (var k in oKeys) {
				if (null != oNode[k])
					oData[k] = oAction(oNode[k].InnerText);
			} // foreach
		} // Xml2Data

		private void AddChild(XmlDocument xdoc, string sNodeName, string sValue) {
			XmlNode oNode = xdoc.CreateElement(sNodeName);
			oNode.InnerText = sValue;
			xdoc.DocumentElement.AppendChild(oNode);
		} // AddChild

		public override string ToString() {
			return string.Format("{0} ({1}) from {2}",
				Name == "" ? "-- no name --" : Name,
				Id,
				Country == "" ? "-- nowhere --" : Country
			);
		} // ToString

	} // class Customer

} // namespace Integration.ChannelGrabberAPI
