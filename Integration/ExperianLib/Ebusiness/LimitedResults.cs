namespace ExperianLib.Ebusiness {
	using System;
	using System.Collections.Generic;
	using System.Xml.Linq;
	using System.Xml.XPath;

	public class LimitedResults : BusinessReturnData {
		#region public

		#region properties
		
		public decimal ExistingBusinessLoans { get; set; }
		public SortedSet<string> Owners { get; protected set; }

		#endregion properties

		#region constructors

		public LimitedResults(long nServiceLogID, string inputXml, DateTime lastCheckDate) : base(nServiceLogID, inputXml, lastCheckDate) {
			Owners = new SortedSet<string>();
		} // constructor

		public LimitedResults(Exception exception) : base(exception) {
			Owners = new SortedSet<string>();
		} // constructor

		#endregion constructors

		#region property IsLimited

		public override bool IsLimited {
			get { return true; }
		} // IsLimited

		#endregion property IsLimited

		#endregion public

		#region protected

		#region method Parse

		protected override void Parse(XElement root) {
			var node = root.XPathSelectElement("./REQUEST/DL76/RISKSCORE");

			if (node == null)
				Error += "There is no RISKSCORE in the experian response! ";
			else
				BureauScore = Convert.ToDecimal(node.Value);

			node = root.XPathSelectElement("./REQUEST/DL78/CREDITLIMIT");
			if (node != null)
				CreditLimit = Convert.ToDecimal(node.Value);

			node = root.XPathSelectElement("./REQUEST/DL95/NUMACTIVEACCS");

			if (node != null)
				ExistingBusinessLoans = Convert.ToDecimal(node.Value);

			node = root.XPathSelectElement("./REQUEST/DL12/COMPANYNAME");
			if (node != null)
				CompanyName = node.Value;

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR1");
			if (node != null)
				AddressLine1 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR2");
			if (node != null)
				AddressLine2 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR3");
			if (node != null)
				AddressLine3 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR4");
			if (node != null)
				AddressLine4 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DL12/REGPOSTCODE");
			if (node != null)
				PostCode = node.Value;

			if (Owners == null)
				Owners = new SortedSet<string>();

			node = root.XPathSelectElement("./REQUEST/DL23/ULTPARREGNUM");
			if (node != null)
				Owners.Add(node.Value.Trim());

			IEnumerable<XElement> oNodes = root.XPathSelectElements("./REQUEST/DL23/SHAREHLDS/SHLDREGNUM");

			foreach (XElement oNode in oNodes)
				Owners.Add(oNode.Value.Trim());

			node = root.XPathSelectElement("./REQUEST/DL12");
			if (node != null)
				IncorporationDate = ExtractDate(node, "DATEINCORP", "incorporation date");
		} // Parse

		#endregion method Parse

		#endregion protected
	} // class LimitedResults
} // namespace
