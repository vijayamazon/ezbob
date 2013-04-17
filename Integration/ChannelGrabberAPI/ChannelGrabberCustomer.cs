using System;
using System.Collections.Generic;
using System.Xml;

namespace Integration.ChannelGrabberAPI {
	#region class ChannelGrabberCustomer

	public class ChannelGrabberCustomer : AJsonable {
		#region public

		#region constructors

		public ChannelGrabberCustomer(XmlNode oNode) : this() {
			this.FromXml(oNode);
		} // constructor

		public ChannelGrabberCustomer(string sName = "", string sCountry = "") {
			this.Init();

			Name = (sName ?? "").Trim();
			Country = (sCountry ?? "").Trim();
		} // constructor

		#endregion constructors

		#region public properties

		#region property Id

		public int Id { get { return m_oIntData[IdNode]; } } // Id

		#endregion property Id

		#region property Name

		public string Name {
			get {  return m_oStrData[NameNode];         }
			private set { m_oStrData[NameNode] = value; }
		} // Name

		#endregion property Name

		#region property Country

		public string Country {
			get {  return m_oStrData[CountryNode];         }
			private set { m_oStrData[CountryNode] = value; }
		} // Country

		#endregion property Country

		#endregion public properties

		#region public methods

		#region method ToXml

		public XmlDocument ToXml(string sRootNodeName = "customer") {
			var doc = new XmlDocument();

			doc.LoadXml(string.Format("<{0} />", sRootNodeName));

			foreach (var pair in m_oIntData)
				AddChild(doc, pair.Key, pair.Value.ToString());

			foreach (var pair in m_oStrData)
				AddChild(doc, pair.Key, pair.Value);

			return doc;
		} // ToXml

		#endregion method ToXml

		#region method FromXml

		public void FromXml(XmlDocument oDoc) {
			this.Init();

			if (oDoc != null)
				FromXml(oDoc.DocumentElement);
		} // FromXml

		public void FromXml(XmlNode oNode) {
			this.Init();

			if (null == oNode)
				return;

			this.Xml2Data(m_oIntData, oNode, XmlConvert.ToInt32);

			this.Xml2Data(m_oStrData, oNode, s => s.Trim());
		} // FromXml

		#endregion method FromXml

		#region method IsValid

		public bool IsValid() {
			return (Name != string.Empty) && (Country != string.Empty) && (Id > 0);
		} // IsValid

		#endregion method IsValid

		#endregion public methods

		#endregion public

		#region private

		#region private const

		private const string IdNode = "id";
		private const string NameNode = "name";
		private const string CountryNode = "country";

		#endregion private const

		#region private properties

		private Dictionary<string, string> m_oStrData;
		private Dictionary<string, int> m_oIntData;

		#endregion private properties

		#region private methods

		#region method Init

		private void Init() {
			m_oIntData = new Dictionary<string, int>();
			m_oStrData = new Dictionary<string, string>();

			m_oIntData[IdNode] = 0;
			m_oStrData[NameNode] = "";
			m_oStrData[CountryNode] = "";
		} // Init

		#endregion method Init

		#region method Xml2Data

		private void Xml2Data<TValue>(Dictionary<string, TValue> oData, XmlNode oNode, Func<string, TValue> oAction) {
			if (null == oNode)
				return;

			var oKeys = new List<string>(oData.Keys);

			foreach (var k in oKeys) {
				if (null != oNode[k])
					oData[k] = oAction(oNode[k].InnerText);
			} // foreach
		} // Xml2Data

		#endregion method Xml2Data

		#region method AddChild

		private void AddChild(XmlDocument xdoc, string sNodeName, string sValue) {
			XmlNode oNode = xdoc.CreateElement(sNodeName);
			oNode.InnerText = sValue;
			xdoc.DocumentElement.AppendChild(oNode);
		} // AddChild

		#endregion method AddChild

		#region method ToString

		public override string ToString() {
			return string.Format("{0} ({1}) from {2}",
				Name == "" ? "-- no name --" : Name,
				Id,
				Country == "" ? "-- nowhere --" : Country
			);
		} // ToString

		#endregion method ToString

		#endregion private methods

		#endregion private
	} // class ChannelGrabberCustomer

	#endregion class ChannelGrabberCustomer
} // namespace Integration.ChannelGrabberAPI
