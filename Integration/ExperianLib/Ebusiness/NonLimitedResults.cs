using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ExperianLib.Ebusiness {
	public class NonLimitedResults : BusinessReturnData {
		#region public

		public decimal BureauScore { get; set; }
		public bool CompanyNotFoundOnBureau { get; set; }
		public SortedSet<string> Owners { get; private set; }

		public NonLimitedResults(string inputXml) : base(inputXml) {
			CompanyNotFoundOnBureau = IsError;
		} // constructor

		public NonLimitedResults(Exception ex) : base(ex) {
		} // constructor

		#endregion public

		#region protected

		protected override void Parse(XElement root) {
			var creditrisk = root.XPathSelectElement("./REQUEST/DN40/RISKSCORE");

			if (creditrisk == null)
				Error += "Can`t read RISKSCORE section from response!";
			else
				BureauScore = Convert.ToDecimal(creditrisk.Value);

			if (Owners == null)
				Owners = new SortedSet<string>();

			var node = root.XPathSelectElement("./REQUEST/DL23/ULTPARREGNUM");

			if (node != null)
				Owners.Add(node.Value.Trim());

			IEnumerable<XElement> oNodes = root.XPathSelectElements("./REQUEST/DL23/SHAREHLDS/SHLDREGNUM");

			foreach (XElement oNode in oNodes)
				Owners.Add(oNode.Value.Trim());
		} // Parse

		#endregion protected
	} // class NonLimitedResults
} // namespace
