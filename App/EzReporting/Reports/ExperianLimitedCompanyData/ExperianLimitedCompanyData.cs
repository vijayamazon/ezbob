//*****************************************************************************
//
// July 31, 2014
// This class was not changed to use ExperianLtd* tables, and it still parses
// XML. The only change done while moving from using MP-ExperianDataCache to
// using ExperianLtd* is to replace MP-ExperianDataCache with MP_ServiceLog.
// Parsing XML here is all right because this class is only used to create
// some report. The report is not part of the report system. It is
// manually created by Adi once in a while.
//
//*****************************************************************************

namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.IO;
	using System.Text;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianLimitedCompanyData : SafeLog {
		#region public

		#region method ToOutput

		public static void ToOutput(string sFileName, Tuple<List<ExperianLimitedCompanyReportItem>, SortedSet<string>> oData) {
			List<ExperianLimitedCompanyReportItem> oReportItems = oData.Item1;
			SortedSet<string> oFieldNames = oData.Item2;

			var oFieldCaptions = new List<string> {
				"Company reg #",
				"Company name",
				"Incorporation date",
				"Company score",
				"Customer ID",
				"Date"
			};

			oFieldCaptions.AddRange(ExperianLimitedCompanyReportItem.RelevantFieldNames);

			string[] aryFieldCaptions = oFieldCaptions.ToArray();

			var oOutput = new List<string[]> { aryFieldCaptions };

			foreach (ExperianLimitedCompanyReportItem ri in oReportItems)
				oOutput.AddRange(ri.ToOutput());

			var fout = new StreamWriter(sFileName, false, Encoding.UTF8);

			for (int i = 0; i < aryFieldCaptions.Length; i++) {
				bool bFirst = true;

				foreach (string[] ary in oOutput) {
					if (bFirst)
						bFirst = false;
					else
						fout.Write(",");

					fout.Write(ary[i]);
				} // for each array

				fout.WriteLine();
			} // for i

			fout.Close();
		} // ToOutput

		#endregion method ToOutput

		#region constructor

		public ExperianLimitedCompanyData(AConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;
			VerboseLogging = false;
		} // constructor

		#endregion constructor

		#region method Run

		public Tuple<List<ExperianLimitedCompanyReportItem>, SortedSet<string>> Run() {
			m_oResult = new List<ExperianLimitedCompanyReportItem>();
			m_oFieldNames = new SortedSet<string>();

			m_oDB.ForEachRow(
				HandleRow,
				"RptExperianLimitedCompanyData",
				CommandSpecies.StoredProcedure
			);

			return new Tuple<List<ExperianLimitedCompanyReportItem>, SortedSet<string>>(m_oResult, m_oFieldNames);
		} // Run

		#endregion method Run

		#region property VerboseLogging

		public bool VerboseLogging { get; set; }

		#endregion property VerboseLogging

		#endregion public

		#region private

		#region method HandleRow

		private ActionResult HandleRow(DbDataReader oRow, bool bStartOfRowset) {
			int nCustomerID = Convert.ToInt32(oRow["Id"]);
			string sXml = oRow["JsonPacket"].ToString();

			if (VerboseLogging)
				Debug("Customer ID: {0}", nCustomerID);

			XmlDocument doc = new XmlDocument();

			try {
				doc.LoadXml(sXml);
			}
			catch (Exception e) {
				Warn(e, "Failed to parse Experian output as XML for customer {0}", nCustomerID);
				return ActionResult.Continue;
			} // try

			if (doc.DocumentElement == null) {
				Warn("Failed to parse Experian output (root node) for customer {0}", nCustomerID);
				return ActionResult.Continue;
			} // if

			XmlNode oNode = doc.DocumentElement.SelectSingleNode("./REQUEST/DL12/REGNUMBER");

			if (oNode == null) {
				Warn("Failed to parse Experian output (company number) for customer {0}", nCustomerID);
				return ActionResult.Continue;
			} // if

			string sCompanyNumber = oNode.InnerText;

			oNode = doc.DocumentElement.SelectSingleNode("./REQUEST/DL12/COMPANYNAME");

			if (oNode == null) {
				Warn("Failed to parse Experian output (company name) for customer {0}", nCustomerID);
				return ActionResult.Continue;
			} // if

			string sCompanyName = oNode.InnerText;

			DateTime? oIncorporationDate = ExperianLimitedCompanyReportItem.ExtractDate(doc.DocumentElement, "./REQUEST/DL12/DATEINCORP");

			if (!oIncorporationDate.HasValue) {
				Warn("Failed to parse Experian output (incorporation date) for customer {0}", nCustomerID);
				return ActionResult.Continue;
			} // if

			oNode = doc.DocumentElement.SelectSingleNode("./REQUEST/DL76/RISKSCORE");
			int nCompanyScore = -1;

			if (oNode != null)
				int.TryParse(oNode.InnerText, out nCompanyScore);

			var oItem = new ExperianLimitedCompanyReportItem(
				nCustomerID,
				sCompanyNumber,
				sCompanyName,
				oIncorporationDate.Value,
				nCompanyScore,
				doc.DocumentElement.SelectNodes("./REQUEST/DL99"),
				m_oFieldNames,
				VerboseLogging ? this : null
			);

			oItem.Validate();

			m_oResult.Add(oItem);

			return ActionResult.Continue;
		} // HandleRow

		#endregion method HandleRow

		private readonly AConnection m_oDB;
		private List<ExperianLimitedCompanyReportItem> m_oResult;
		private SortedSet<string> m_oFieldNames;

		#endregion private
	} // class ExperianLimitedCompanyData
} // namespace Reports
