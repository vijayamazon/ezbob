﻿namespace Reports {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Xml;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoanDateScoreItem {

		public LoanDateScoreItem(SafeReader oRow, ASafeLog oLog) {
			m_oLog = oLog.Safe();

			CustomerID = oRow["CustomerID"];
			m_oLastLoanDate = oRow["LoanIssueDate"];
			m_sCompanyRefNum = oRow["LimitedRefNum"];

			if (string.IsNullOrWhiteSpace(m_sCompanyRefNum))
				m_sCompanyRefNum = oRow["NonLimitedRefNum"];

			m_oLog.Debug("Customer {0} took last loan on {1}", CustomerID, m_oLastLoanDate);
		} // constructor

		public int CustomerID { get; set; }

		public void Add(SafeReader oRow, AConnection oDB) {
			DateTime oInsertDate = oRow["InsertDate"];

			switch ((string)oRow["ServiceType"]) {
			case "Consumer Request":
				if (DateFits(ref m_oNdspciiDataDate, oInsertDate)) {
					ExperianConsumerData data = ExperianConsumerData.Load((long) oRow["ServiceLogID"], oDB, m_oLog);

					if (data != null && data.ServiceLogId != null) {
						AddNdspciiData(data.CII);
					}
				} // if
				break;

			case "E-SeriesLimitedData":
				if (DateFits(ref m_oCompanyDataDate, oInsertDate))
					AddLtdData(ExperianLtd.Load((long)oRow["ServiceLogID"], oDB, m_oLog));
				break;

			case "E-SeriesNonLimitedData":
				if (DateFits(ref m_oCompanyDataDate, oInsertDate))
					AddCompanyData(oDB);
				break;
			} // switch
		} // Add

		public void LoadLastScore(AConnection oDB) {
			if (!ReferenceEquals(m_sCompanyName, null) && !ReferenceEquals(m_sCompanyNumber, null) && m_oIncorporationDate.HasValue && m_nCompanyScore.HasValue && !ReferenceEquals(m_sCreditLimit, null))
				return;

			if (string.IsNullOrWhiteSpace(m_sCompanyRefNum))
				return;

			ExperianLtd oExperianLtd = ExperianLtd.Load(m_sCompanyRefNum, oDB, m_oLog);

			if (oExperianLtd.RegisteredNumber == m_sCompanyRefNum)
				AddLtdData(oExperianLtd);

			AddCompanyData(oDB);
		} // LoadLastScore

		public void ToOutput(StreamWriter fout) {
			fout.WriteLine(string.Join(",", new string[] {
				CustomerID.ToString(), m_oLastLoanDate.ToString("yyyy-MM-dd"),
				(m_oIncorporationDate.HasValue ? m_oIncorporationDate.Value.ToString("yyyy-MM-dd") : ""),
				m_nCompanyScore.ToString(), (m_oCompanyDataDate.HasValue ? m_oCompanyDataDate.Value.ToString("yyyy-MM-dd") : ""),
				m_nNdspcii.ToString(), (m_oNdspciiDataDate.HasValue ? m_oNdspciiDataDate.Value.ToString("yyyy-MM-dd") : ""),
				m_sCompanyNumber, m_sCompanyName, m_sCreditLimit, m_sNonLimDelphiScore, m_sNonLimDefaultChance, m_sNonLimStabilityOdds
			}));
		} // ToOutput

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

		private bool DateFits(ref DateTime? oSavedDate, DateTime oInsertDate) {
			if (oInsertDate > m_oLastLoanDate)
				return false;

			bool b = !oSavedDate.HasValue || (oInsertDate > oSavedDate.Value);

			if (b)
				oSavedDate = oInsertDate;

			return b;
		} // DateFits

		private void AddNdspciiData(int? ndspcii) {
			m_nNdspcii = ndspcii;
		} // AddNdspciiDAta

		private void AddLtdData(ExperianLtd oExperianLtd) {
			if (m_sCompanyNumber == null)
				m_sCompanyNumber = oExperianLtd.RegisteredNumber;

			m_sCompanyName = oExperianLtd.CompanyName;

			if (!m_oIncorporationDate.HasValue)
				m_oIncorporationDate = oExperianLtd.IncorporationDate;

			if (!m_oIncorporationDate.HasValue)
				m_oLog.Warn("Failed to parse Experian output (incorporation date) for customer {0}", CustomerID);

			m_nCompanyScore = oExperianLtd.CommercialDelphiScore;

			if (ReferenceEquals(m_sCreditLimit, null)) {
				m_sCreditLimit = oExperianLtd.CommercialDelphiCreditLimit.HasValue
					? oExperianLtd.CommercialDelphiCreditLimit.Value.ToString()
					: null;
			} // if
		} // AddLtdData

		private void AddCompanyData(AConnection oDB)
		{
			SafeReader sr = oDB.GetFirst(
				"GetNonLimitedDataForLoanDateScoreReport",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", m_sCompanyRefNum)
			);

			if (!sr.IsEmpty) {
				m_sNonLimDelphiScore = sr["CommercialDelphiScore"];
				m_sNonLimDefaultChance = sr["ProbabilityOfDefaultScore"];
				m_sNonLimDelphiScore = sr["StabilityOdds"];
			}
		} // AddCompanyData

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

		private static string MD(XmlNode oNode) {
			string s = oNode.InnerText;

			if (s.Length < 2)
				return "0" + s;

			return s;
		} // MD

	} // class LoanDateScoreItem

} // namespace Reports
