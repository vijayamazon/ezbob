using System.Xml;

namespace Integration.ChannelGrabberAPI {
	#region class ChannelGrabberCustomer

	public class ChannelGrabberCustomer : IJsonable {
		#region constructors

		public ChannelGrabberCustomer(XmlNode oNode) : this() {
			this.FromXml(oNode);
		} // constructor

		public ChannelGrabberCustomer(string sName = "", string sCountry = "") {
			this.Clear();
			Name = (sName ?? "").Trim();
			Country = (sCountry ?? "").Trim();
		} // constructor

		#endregion constructors

		#region method ToXml

		public XmlDocument ToXml(string sRootNodeName = "customer") {
			var doc = new XmlDocument();

			doc.LoadXml(string.Format("<{0}><{1} /><{2} /><{3} /></{0}>",
				sRootNodeName, IdNode, NameNode, CountryNode
			));

			doc.SelectSingleNode(string.Format("/{0}/{1}", sRootNodeName, IdNode)).InnerText = Id.ToString();
			doc.SelectSingleNode(string.Format("/{0}/{1}", sRootNodeName, NameNode)).InnerText = Name;
			doc.SelectSingleNode(string.Format("/{0}/{1}", sRootNodeName, CountryNode)).InnerText = Country;

			return doc;
		} // ToXml

		#endregion method ToXml

		#region method ToJson

		public object ToJson() {
			return new {
				name = this.Name,
				country = this.Country
			};
		} // ToJson

		#endregion method ToJson

		#region method Clear

		private void Clear() {
			Id = 0;
			Name = "";
			Country = "";
			IsValid = false;
		} // Clear

		#endregion method Clear

		#region method FromXml

		public void FromXml(XmlDocument oDoc) {
			this.Clear();

			if (oDoc != null)
				FromXml(oDoc.DocumentElement);
		} // FromXml

		public void FromXml(XmlNode oNode) {
			this.Clear();

			if (null == oNode)
				return;

			if (null == oNode[IdNode])
				return;

			Id = XmlConvert.ToInt32(oNode[IdNode].InnerText);

			if (null == oNode[NameNode])
				return;

			Name = oNode[NameNode].InnerText.Trim();

			if (null == oNode[CountryNode])
				return;

			Country = oNode[CountryNode].InnerText.Trim();

			IsValid = (Name != string.Empty) && (Country != string.Empty) && (Id > 0);
		} // FromXml

		#endregion method FromXml

		#region properties

		public int    Id      { get; private set; }
		public string Name    { get; private set; }
		public string Country { get; private set; }
		public bool   IsValid { get; private set; }

		#endregion properties

		#region method ToString

		public override string ToString() {
			return string.Format("{0} ({1}) from {2}",
				Name == "" ? "-- no name --" : Name,
				Id,
				Country == "" ? "-- nowhere --" : Country
			);
		} // ToString

		#endregion method ToString

		#region private const

		private const string IdNode = "id";
		private const string NameNode = "name";
		private const string CountryNode = "country";

		#endregion private const
	} // class ChannelGrabberCustomer

	#endregion class ChannelGrabberCustomer
} // namespace Integration.ChannelGrabberAPI
