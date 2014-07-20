namespace Reports {
	using System;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class LoanDateScoreItem

	public class LoanDateScoreItem {
		#region public

		#region constructor

		public LoanDateScoreItem(SafeReader oRow, ASafeLog oLog) {
			m_oLog = new SafeLog(oLog);

			CustomerID = oRow["CustomerID"];
			m_oLastLoanDate = oRow["LoanIssueDate"];
			m_sCompanyRefNum = oRow["LimitedRefNum"];

			if (string.IsNullOrWhiteSpace(m_sCompanyRefNum))
				m_sCompanyRefNum = oRow["NonLimitedRefNum"];

			m_oLog.Debug("Customer {0} took last loan on {1}", CustomerID, m_oLastLoanDate);
		} // constructor

		#endregion constructor

		public int CustomerID { get; set; }

		#region method Add

		public void Add(SafeReader oRow, AConnection oDB)
		{
			XmlDocument doc = ExtractXml(oRow["ResponseData"]);

			if (doc == null)
				return;

			DateTime oInsertDate = oRow["InsertDate"];

			string sServiceType = oRow["ServiceType"];

			switch (sServiceType) {
			case "Consumer Request":
				if (DateFits(ref m_oNdspciiDataDate, oInsertDate))
					AddNdspciiData(doc);
				break;

			case "E-SeriesLimitedData":
				if (DateFits(ref m_oCompanyDataDate, oInsertDate))
					AddLtdData(doc);
				break;

			case "E-SeriesNonLimitedData":
				if (DateFits(ref m_oCompanyDataDate, oInsertDate))
					AddCompanyData(oDB);
				break;
			} // switch
		} // Add

		#endregion method Add

		#region method LoadLastScore

		public void LoadLastScore(AConnection oDB) {
			if (!ReferenceEquals(m_sCompanyName, null) && !ReferenceEquals(m_sCompanyNumber, null) && m_oIncorporationDate.HasValue && m_nCompanyScore.HasValue && !ReferenceEquals(m_sCreditLimit, null))
				return;

			if (string.IsNullOrWhiteSpace(m_sCompanyRefNum))
				return;

			// TODO: Remove this query once limited company is done
			var sXml = oDB.ExecuteScalar<string>("SELECT TOP 1 JsonPacket FROM MP_ExperianDataCache WHERE CompanyRefNumber = '" + m_sCompanyRefNum + "' ORDER BY LastUpdateDate DESC", CommandSpecies.Text);

			var doc = ExtractXml(sXml);

			if (doc != null) {
				AddLtdData(doc);
			} // if

			AddCompanyData(oDB);
		} // LoadLastScore

		#endregion method LoadLastScore

		#region method ToOutput

		public void ToOutput(StreamWriter fout) {
			fout.WriteLine(string.Join(";", new string[] {
				CustomerID.ToString(), m_oLastLoanDate.ToString("yyyy-MM-dd"),
				(m_oIncorporationDate.HasValue ? m_oIncorporationDate.Value.ToString("yyyy-MM-dd") : ""),
				m_nCompanyScore.ToString(), (m_oCompanyDataDate.HasValue ? m_oCompanyDataDate.Value.ToString("yyyy-MM-dd") : ""),
				m_nNdspcii.ToString(), (m_oNdspciiDataDate.HasValue ? m_oNdspciiDataDate.Value.ToString("yyyy-MM-dd") : ""),
				m_sCompanyNumber, m_sCompanyName, m_sCreditLimit, m_sNonLimDelphiScore, m_sNonLimDefaultChance, m_sNonLimStabilityOdds
			}));
		} // ToOutput

		#endregion method ToOutput

		#endregion public

		#region private

		private readonly DateTime m_oLastLoanDate;
		private readonly string m_sCompanyRefNum;

		private readonly ASafeLog m_oLog;

		private DateTime? m_oNdspciiDataDate;
		private int? m_nNdspcii;

		private DateTime? m_oCompanyDataDate;

		private int? m_nCompanyScore;
		private DateTime? m_oIncorporationDate;
		private string m_sCompanyNumber;
		private string m_sCompanyName;
		private string m_sCreditLimit;

		private string m_sNonLimDelphiScore;
		private string m_sNonLimDefaultChance;
		private string m_sNonLimStabilityOdds;

		#region method ExtractXml

		private XmlDocument ExtractXml(string sXml) {
			var doc = new XmlDocument();

			try {
				doc.LoadXml(sXml);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Failed to parse Experian output as XML for customer {0}", CustomerID);
				return null;
			} // try

			if (doc.DocumentElement == null) {
				m_oLog.Warn("Failed to parse Experian output (root node) for customer {0}", CustomerID);
				return null;
			} // if

			return doc;
		} // ExtractXml

		#endregion method ExtractXml

		#region method DateFits

		private bool DateFits(ref DateTime? oSavedDate, DateTime oInsertDate) {
			if (oInsertDate > m_oLastLoanDate)
				return false;

			bool b = !oSavedDate.HasValue || (oInsertDate > oSavedDate.Value);

			if (b)
				oSavedDate = oInsertDate;

			return b;
		} // DateFits

		#endregion method DateFits

		#region method AddNdspciiData

		private void AddNdspciiData(XmlDocument doc) {
			XmlNode oNode = doc.DocumentElement.SelectSingleNode("//NDSPCII");

			if (oNode == null)
				m_oLog.Warn("Failed to parse Experian output (NDSPCII) for customer {0}", CustomerID);
			else
				m_nNdspcii = Convert.ToInt32(oNode.InnerText.Trim());
		} // AddNdspciiDAta

		#endregion method AddNdspciiData

		#region method AddLtdData

		private void AddLtdData(XmlDocument doc) {
			XmlNode oNode = doc.DocumentElement.SelectSingleNode("./REQUEST/DL12/REGNUMBER");

			if (oNode == null)
				m_oLog.Warn("Failed to parse Experian output (company number) for customer {0}", CustomerID);
			else if (m_sCompanyNumber == null)
				m_sCompanyNumber = oNode.InnerText;

			oNode = doc.DocumentElement.SelectSingleNode("./REQUEST/DL12/COMPANYNAME");

			if (oNode == null)
				m_oLog.Warn("Failed to parse Experian output (company name) for customer {0}", CustomerID);
			else if (m_sCompanyName == null)
				m_sCompanyName = oNode.InnerText;

			if (!m_oIncorporationDate.HasValue)
				m_oIncorporationDate = ExtractDate(doc.DocumentElement, "./REQUEST/DL12/DATEINCORP");

			if (!m_oIncorporationDate.HasValue)
				m_oLog.Warn("Failed to parse Experian output (incorporation date) for customer {0}", CustomerID);

			oNode = doc.DocumentElement.SelectSingleNode("./REQUEST/DL76/RISKSCORE");

			if (oNode != null) {
				int nCompanyScore;

				if (int.TryParse(oNode.InnerText, out nCompanyScore))
					if (!m_nCompanyScore.HasValue)
						m_nCompanyScore = nCompanyScore;
			} // if

			oNode = doc.DocumentElement.SelectSingleNode("./REQUEST/DL78/CREDITLIMIT");
			if (!ReferenceEquals(oNode, null) && ReferenceEquals(m_sCreditLimit, null))
				m_sCreditLimit = oNode.InnerText;
		} // AddLtdData

		#endregion method AddLtdData

		#region method AddCompanyData

		private void AddCompanyData(AConnection oDB)
		{
			DataTable nonLimitedDataTable = oDB.ExecuteReader("GetNonLimitedDataForLoanDateScoreReport",
			                 CommandSpecies.StoredProcedure,
							 new QueryParameter("CustomerId", CustomerID),
			                 new QueryParameter("RefNumber", m_sCompanyRefNum));


			if (nonLimitedDataTable.Rows.Count == 1)
			{
				var sr = new SafeReader(nonLimitedDataTable.Rows[0]);
				m_sNonLimDelphiScore = sr["CommercialDelphiScore"];
				m_sNonLimDefaultChance = sr["ProbabilityOfDefaultScore"];
				m_sNonLimDelphiScore = sr["StabilityOdds"];
			}
		} // AddCompanyData

		#endregion method AddCompanyData

		#region method ExtractDate

		private static DateTime? ExtractDate(XmlNode oNode, string sFieldNamePrefix) {
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

		#region method MD

		private static string MD(XmlNode oNode) {
			string s = oNode.InnerText;

			if (s.Length < 2)
				return "0" + s;

			return s;
		} // MD

		#endregion method MD

		#endregion private
	} // class LoanDateScoreItem

	#endregion class LoanDateScoreItem
} // namespace Reports
