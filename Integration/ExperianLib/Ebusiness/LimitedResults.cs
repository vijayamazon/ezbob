using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ExperianLib.Ebusiness {
	public class LimitedResults : BusinessReturnData {
		#region public

		#region properties

		public decimal BureauScore { get; set; }
		public decimal ExistingBusinessLoans { get; set; }

		public SortedSet<string> Owners { get; private set; }

		#endregion properties

		#region constructors

		public LimitedResults(string inputXml) : base(inputXml) {
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
