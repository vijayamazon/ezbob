using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reports {
	#region class ExperianLimitedCompanyReportItem

	public class ExperianLimitedCompanyReportItem {
		#region public

		#region constructor

		public ExperianLimitedCompanyReportItem(int nCustomerID, string sRegNumber, string sCompanyName, XmlNodeList oDL99, SortedSet<string> oFieldNames) {
			CustomerID = nCustomerID;
			RegNumber = sRegNumber;
			CompanyName = sCompanyName;

			Data = new SortedList<DateTime, SortedDictionary<string, string>>();

			LoadFields(oDL99, oFieldNames);
		} // constructor

		#endregion constructor

		public int CustomerID { get; private set; }
		public string RegNumber { get; private set; }
		public string CompanyName { get; private set; }
		public SortedList<DateTime, SortedDictionary<string, string>> Data { get; private set; }

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
				oRow.Add(CompanyName);
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
				XmlNode oYear = oNode.SelectSingleNode("DATEOFACCOUNTS-YYYY");
				if (oYear == null)
					continue;

				XmlNode oMonth = oNode.SelectSingleNode("DATEOFACCOUNTS-MM");
				if (oMonth == null)
					continue;
				
				XmlNode oDay = oNode.SelectSingleNode("DATEOFACCOUNTS-DD");
				if (oDay == null)
					continue;

				DateTime oRecordDate;

				if (!DateTime.TryParseExact(oYear.InnerText + MD(oMonth) + MD(oDay), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out oRecordDate))
					continue;

				LoadOne(oNode, oRecordDate, oIgnoredNames, oFieldNames);
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

		private string MD(XmlNode oNode) {
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
