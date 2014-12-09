namespace Reports.Alibaba.DataSharing {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	public class DataSharing : IAlibaba {

		public DataSharing(bool bIncludeTestCustomers, AConnection oDB, ASafeLog oLog) {
			if (oDB == null)
				throw new Exception("Database connection not specified for DataSharing report.");

			m_oLog = oLog ?? new SafeLog();

			m_oSp = new RptAlibabaDataSharing(bIncludeTestCustomers, oDB, m_oLog);

			m_oLateLoans = new SortedSet<int>();
			m_oData = new SortedDictionary<int, CustomerData>();

			m_oAlibabaIDs = new SortedSet<string>();
		} // constructor

		public void Generate() {
			Report = new ExcelPackage();

			m_oSp.ForEachRowSafe(ProcessRow);

			CustomerData oRandomCustomer = null;

			foreach (KeyValuePair<int, CustomerData> pair in m_oData) {
				pair.Value.SaveTo(Report);
				oRandomCustomer = pair.Value;
			} // for each

			if (oRandomCustomer != null)
				oRandomCustomer.AddApprovalPhaseTotal(Report);

			SaveAlibabaIDs();

			Report.AutoFitColumns();
		} // Generate

		public ExcelPackage Report { get; private set; }

		private void ProcessRow(SafeReader sr) {
			string sRowType = sr[CustomerData.RowType];

			RptAlibabaDataSharing.RowTypes nRowType;

			if (!Enum.TryParse(sRowType, out nRowType)) {
				m_oLog.Alert("Failed to parse DataSharing row type '{0}'.", sRowType);
				return;
			} // if

			switch (nRowType) {
			case RptAlibabaDataSharing.RowTypes.MetaData:
				var cd = new CustomerData(sr);

				if (cd.CustomerID < 1) {
					m_oLog.Alert("Invalid customer id detected.");
					break;
				} // if

				m_oData[cd.CustomerID] = cd;

				break;

			case RptAlibabaDataSharing.RowTypes.Loan: {
				int nCustomerID = sr[CustomerData.LoanDataCustomerIDField];

				if (!m_oData.ContainsKey(nCustomerID)) {
					m_oLog.Alert("Ignoring loan data for customer {0} because customer not found.", nCustomerID);
					break;
				} // if

				m_oData[nCustomerID].AddLoanData(sr);
			} break;

			case RptAlibabaDataSharing.RowTypes.Repayment: {
				int nCustomerID = sr[CustomerData.LoanDataCustomerIDField];

				if (!m_oData.ContainsKey(nCustomerID)) {
					m_oLog.Alert("Ignoring loan data for customer {0} because customer not found.", nCustomerID);
					break;
				} // if

				m_oData[nCustomerID].AddRepayment(sr, m_oLateLoans);
			} break;

			case RptAlibabaDataSharing.RowTypes.Marketplace: {
				int nCustomerID = sr[CustomerData.LoanDataCustomerIDField];

				if (!m_oData.ContainsKey(nCustomerID)) {
					m_oLog.Alert("Ignoring marketplace for customer {0} because customer not found.", nCustomerID);
					break;
				} // if

				m_oData[nCustomerID].AddMarketplace(sr);
			} break;

			case RptAlibabaDataSharing.RowTypes.IsLate:
				m_oLateLoans.Add(sr["LoanID"]);
				break;

			case RptAlibabaDataSharing.RowTypes.AlibabaID:
				ExtractAlibabaID(sr["Source"]);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		private void ExtractAlibabaID(string sSource) {
			if (string.IsNullOrWhiteSpace(sSource))
				return;

			int nPos = sSource.IndexOf(AlibabaID, StringComparison.InvariantCultureIgnoreCase);

			if (nPos < 0)
				return;

			int nIDStart = nPos + AlibabaID.Length;

			int nAmpPos = sSource.IndexOf("&", nIDStart, StringComparison.InvariantCultureIgnoreCase);

			string sAlibabaID = (nAmpPos < 0) ? sSource.Substring(nIDStart) : sSource.Substring(nIDStart, nAmpPos - nIDStart);

			m_oAlibabaIDs.Add(sAlibabaID);
		} // ExtractAlibabaID

		private void SaveAlibabaIDs() {
			ExcelWorksheet oSheet = Report.FindOrCreateSheet("Unique Alibaba ID on LP", false, "Alibaba ID");

			int nRow = 2;

			foreach (string sAlibabaID in m_oAlibabaIDs)
				oSheet.SetCellValue(nRow++, 1, sAlibabaID);
		} // SaveAlibabaIDs

		private readonly SortedDictionary<int, CustomerData> m_oData;
		private readonly SortedSet<int> m_oLateLoans;
		private readonly SortedSet<string> m_oAlibabaIDs;
		private readonly ASafeLog m_oLog;
		private readonly RptAlibabaDataSharing m_oSp;

		private const string AlibabaID = "alibaba_id=";

		private class RptAlibabaDataSharing : AStoredProcedure {
			public enum RowTypes {
				MetaData,
				Loan,
				IsLate,
				Repayment,
				Marketplace,
				AlibabaID,
			};

			public RptAlibabaDataSharing(bool bIncludeTestCustomers, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				IncludeTest = bIncludeTestCustomers;
			} // constructor

			public override bool HasValidParameters() {
				return true;
			} // HasValidParameters

			[UsedImplicitly]
			public bool IncludeTest { get; set; }
		} // class RptAlibabaDataSharing 

	} // class DataSharing
} // namespace
