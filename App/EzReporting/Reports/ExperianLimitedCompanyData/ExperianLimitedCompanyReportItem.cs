﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Reports {
	#region class ExperianLimitedCompanyReportItem

	public class ExperianLimitedCompanyReportItem {
		#region public

		#region method ExtractDate

		public static DateTime? ExtractDate(XmlNode oNode, string sFieldNamePrefix) {
			XmlNode oYear = oNode.SelectSingleNode(sFieldNamePrefix + "-YYYY");
			if (oYear == null)
				return null;

			XmlNode oMonth = oNode.SelectSingleNode(sFieldNamePrefix + "-MM");
			if (oMonth == null)
				return null;
			
			XmlNode oDay = oNode.SelectSingleNode(sFieldNamePrefix + "-DD");
			if (oDay == null)
				return null;

			DateTime oDate;

			if (!DateTime.TryParseExact(oYear.InnerText + MD(oMonth) + MD(oDay), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out oDate))
				return null;

			return oDate;
		} // ExtractDate

		#endregion method ExtractDate

		#region constructor

		public ExperianLimitedCompanyReportItem(
			int nCustomerID,
			string sRegNumber,
			string sCompanyName,
			DateTime oIncorporationDate,
			int nCompanyScore,
			XmlNodeList oDL99,
			SortedSet<string> oFieldNames
		) {
			CustomerID = nCustomerID;
			RegNumber = sRegNumber;
			CompanyName = sCompanyName;
			IncorporationDate = oIncorporationDate;
			CompanyScore = nCompanyScore;

			Data = new SortedList<DateTime, SortedDictionary<string, string>>();

			LoadFields(oDL99, oFieldNames);
		} // constructor

		#endregion constructor

		#region properties

		public int CustomerID { get; private set; }
		public string RegNumber { get; private set; }
		public string CompanyName { get; private set; }
		public DateTime IncorporationDate { get; private set; }
		public int CompanyScore { get; private set; }
		public SortedList<DateTime, SortedDictionary<string, string>> Data { get; private set; }

		#endregion properties

		#region method Validate

		public bool Validate() {
			if (Data.Count < 1)
				return false;

			while (Data.Count > 3)
				Data.RemoveAt(0);

			return true;
		} // Validate

		#endregion method Validate

		#region method ToOutput

		public List<string[]> ToOutput(SortedSet<string> oFieldNames) {
			var oResult = new List<string[]>();

			foreach (KeyValuePair<DateTime, SortedDictionary<string, string>> pair in Data) {
				var oRow = new List<string>();

				oRow.Add(RegNumber);
				oRow.Add('"' + CompanyName + '"');
				oRow.Add(IncorporationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				oRow.Add(CompanyScore < 0 ? "" : CompanyScore.ToString());
				oRow.Add(CustomerID.ToString());
				oRow.Add(pair.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

				foreach (string sFieldName in oFieldNames)
					oRow.Add(pair.Value.ContainsKey(sFieldName) ? pair.Value[sFieldName] : "");

				oResult.Add(oRow.ToArray());
			} // foreach

			return oResult;
		} // ToOutput

		#endregion method ToOutput

		#endregion public

		#region private

		#region method LoadFields

		private void LoadFields(XmlNodeList oDL99, SortedSet<string> oFieldNames) {
			if (oDL99 == null)
				return;

			if (oDL99.Count < 1)
				return;

			var oIgnoredNames = new SortedSet<string> { "DATEOFACCOUNTS-YYYY", "DATEOFACCOUNTS-MM", "DATEOFACCOUNTS-DD", "REGNUMBER", "EXPERIANREF" };

			foreach (XmlNode oNode in oDL99) {
				DateTime? oRecordDate = ExtractDate(oNode, "DATEOFACCOUNTS");

				if (!oRecordDate.HasValue)
					continue;

				LoadOne(oNode, oRecordDate.Value, oIgnoredNames, oFieldNames);
			} // foreach
		} // LoadFields

		#endregion method LoadFields

		#region method LoadOne

		private void LoadOne(XmlNode oDL99, DateTime oRecordDate, SortedSet<string> oIgnoredNames, SortedSet<string> oFieldNames) {
			var oResult = new SortedDictionary<string, string>();

			for (XmlNode oNode = oDL99.FirstChild; oNode != null; oNode = oNode.NextSibling) {
				if (oIgnoredNames.Contains(oNode.Name))
					continue;

				oFieldNames.Add(oNode.Name);

				oResult[oNode.Name] = oNode.InnerText;
			} // for

			Data[oRecordDate] = oResult;
		} // LoadOne

		#endregion method LoadOne

		#region method MD

		private static string MD(XmlNode oNode) {
			string s = oNode.InnerText;

			if (s.Length < 2)
				return "0" + s;

			return s;
		} // MD

		#endregion method MD

		#endregion private
	} // class ExperianLimitedCompanyReportItem

	#endregion class ExperianLimitedCompanyReportItem
} // namespace Reports
