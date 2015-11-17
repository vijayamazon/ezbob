namespace AutomationCalculator {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using NUnit.Framework;

	[TestFixture]
	class TestAdjustCompanyName {
		[Test]
		public void DoTest() {
			XmlNode rows = null;

			try {
				var reader = new StreamReader(Assembly
					.GetExecutingAssembly()
					.GetManifestResourceStream("AutomationCalculator.business-names.xml")
				);

				var xml = new XmlDocument();
				xml.Load(reader);

				reader.Dispose();

				if (xml.DocumentElement == null)
					throw new Exception("Failed to parse business names XML.");

				rows = xml.DocumentElement.SelectSingleNode("rows");

				if (rows == null)
					throw new Exception("List of rows not found.");
			} catch (Exception e) {
				Assert.True(false, "Failed to read business names XML: {0}", e.Message);
			} // try

			var stats = new Dictionary<Row.Results, int>();

			foreach (XmlNode xmlRow in rows.ChildNodes) {
				var row = new Row(xmlRow).Test();

				if (stats.ContainsKey(row.Result))
					stats[row.Result]++;
				else
					stats[row.Result] = 1;
			} // for

			Console.WriteLine("{0}", string.Join(" ", stats.Select(p => string.Format("{0}: {1}.", p.Key, p.Value))));

			Assert.True(true, "Everything went smooth.");
		} // DoTest

		private class Row {
			public enum Results {
				KeptMatch,
				KeptMismatch,
				ShouldBelong,
				Mismatched,
			} // enum Results

			public Row(XmlNode row) {
				foreach (XmlNode cell in row.ChildNodes) {
					switch (cell.Attributes["columnNumber"].InnerText) {
					case "0":
						CustomerID = int.Parse(cell.InnerText);
						break;

					case "1":
						BusinessID = int.Parse(cell.InnerText);
						break;

					case "2":
						BusinessName = cell.InnerText;
						break;

					case "3":
						BelongsToCustomer = !string.IsNullOrWhiteSpace(cell.InnerText) && bool.Parse(cell.InnerText);
						break;

					case "4":
						CompanyID = int.Parse(cell.InnerText);
						break;

					case "5":
						ExperianCompanyName = cell.InnerText;
						break;
					} // switch
				} // for each cell
			} // constructor

			public Row Test() {
				if (Result == Results.KeptMatch)
					return this;

				Console.WriteLine(
					"Customer ID: {0} Business ID: {1} Company ID: {9} " +
					"Belongs to customer: {2} Name match: {3} Result: {4}\n" +
					"\tBusiness name:          '{5}'\n" +
					"\tExperian name:          '{7}'\n" +
					"\n" +
					"\tAdjusted business name: '{6}'\n" +
					"\tAdjusted experian name: '{8}'\n" +
					"\n" +
					"\tNo stops business name: '{10}'\n" +
					"\tNo stops experian name: '{11}'\n" +
					"\n",
					CustomerID,
					BusinessID,
					BelongsToCustomer ? "yes" : "no",
					NameMatches ? "yes" : "no",
					Result,
					BusinessName,
					this.businessNameForComparison.AdjustedName,
					ExperianCompanyName,
					this.experianCompanyNameForComparison.AdjustedName,
					CompanyID,
					this.businessNameForComparison.NoStopwordsName,
					this.experianCompanyNameForComparison.NoStopwordsName
				);

				return this;
			} // Test

			public Results Result {
				get {
					if (BelongsToCustomer)
						return NameMatches ? Results.KeptMatch : Results.Mismatched;

					return NameMatches ? Results.ShouldBelong : Results.KeptMismatch;
				} // get
			} // Result

			private int CustomerID { get; set; }
			private int BusinessID { get; set; }
			private bool BelongsToCustomer { get; set; }
			private int CompanyID { get; set; }

			private string BusinessName {
				get { return this.businessName; }
				set {
					this.businessName = value;
					this.businessNameForComparison = new NameForComparison(value);
					this.nameMatches = null;
				} // set
			} // BusinessName

			private string ExperianCompanyName {
				get { return this.experianCompanyName; }
				set {
					this.experianCompanyName = value;
					this.experianCompanyNameForComparison = new NameForComparison(value);
					this.nameMatches = null;
				} // set
			} // ExperianCompanyName

			private bool NameMatches {
				get {
					if (this.nameMatches.HasValue)
						return this.nameMatches.Value;

					if ((this.businessNameForComparison == null) || (this.experianCompanyNameForComparison == null))
						this.nameMatches = false;
					else {
						this.nameMatches = this.businessNameForComparison.SameAsCompany(
							this.experianCompanyNameForComparison
						);
					} // if

					return this.nameMatches.Value;
				} // get
			} // NameMatches

			private string businessName;
			private string experianCompanyName;

			private NameForComparison businessNameForComparison;
			private NameForComparison experianCompanyNameForComparison;

			private bool? nameMatches;
		} // class Row
	} // class TestAdjustCompanyName
} // namespace
