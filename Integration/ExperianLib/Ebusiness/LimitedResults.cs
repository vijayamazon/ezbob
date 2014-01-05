using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ExperianLib.Ebusiness {
	public class LimitedResults : BusinessReturnData {
		#region public

		#region properties
		
		public decimal ExistingBusinessLoans { get; set; }

		#endregion properties

		#region constructors

		public LimitedResults(string inputXml, DateTime lastCheckDate) : base(inputXml, lastCheckDate) {
		} // constructor

		public LimitedResults(Exception exception) : base(exception) {
		} // constructor

		#endregion constructors

		#endregion public

		#region protected

		#region method Parse

		protected override void Parse(XElement root) {
			var node = root.XPathSelectElement("./REQUEST/DL76/RISKSCORE");

			if (node == null)
				Error += "There is no RISKSCORE in the experian response! ";
			else
				BureauScore = Convert.ToDecimal(node.Value);

			node = root.XPathSelectElement("./REQUEST/DL95/NUMACTIVEACCS");

			if (node != null)
				ExistingBusinessLoans = Convert.ToDecimal(node.Value);


			node = root.XPathSelectElement("./REQUEST/DL12/COMPANYNAME");
			if (node != null)
			{
				CompanyName = node.Value;
			}

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR1");
			if (node != null)
			{
				AddressLine1 = node.Value;
			}

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR2");
			if (node != null)
			{
				AddressLine2 = node.Value;
			}

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR3");
			if (node != null)
			{
				AddressLine3 = node.Value;
			}

			node = root.XPathSelectElement("./REQUEST/DL12/REGADDR4");
			if (node != null)
			{
				AddressLine4 = node.Value;
			}

			node = root.XPathSelectElement("./REQUEST/DL12/REGPOSTCODE");
			if (node != null)
			{
				PostCode = node.Value;
			}

			if (Owners == null)
				Owners = new SortedSet<string>();

			node = root.XPathSelectElement("./REQUEST/DL23/ULTPARREGNUM");

			if (node != null)
				Owners.Add(node.Value.Trim());

			IEnumerable<XElement> oNodes = root.XPathSelectElements("./REQUEST/DL23/SHAREHLDS/SHLDREGNUM");

			foreach (XElement oNode in oNodes)
				Owners.Add(oNode.Value.Trim());
		} // Parse

		#endregion method Parse

		#endregion protected
	} // class LimitedResults
} // namespace
