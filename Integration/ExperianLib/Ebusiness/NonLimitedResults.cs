﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ExperianLib.Ebusiness {
	public class NonLimitedResults : BusinessReturnData {
		#region public

		public bool CompanyNotFoundOnBureau { get; set; }

		public NonLimitedResults(string inputXml, DateTime lastCheckDate) : base(inputXml, lastCheckDate)
		{
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

			var node = root.XPathSelectElement("./REQUEST/DN10/BUSINESSNAME");
			if (node != null) CompanyName = node.Value;

			node = root.XPathSelectElement("./REQUEST/DN10/BUSADDR1");
			if (node != null) AddressLine1 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DN10/BUSADDR2");
			if (node != null) AddressLine2 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DN10/BUSADDR3");
			if (node != null) AddressLine3 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DN10/BUSADDR4");
			if (node != null) AddressLine4 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DN10/BUSADDR5");
			if (node != null) AddressLine5 = node.Value;

			node = root.XPathSelectElement("./REQUEST/DN10/BUSPOSTCODE");
			if (node != null) PostCode = node.Value;
			
		} // Parse

		#endregion protected
	} // class NonLimitedResults
} // namespace
