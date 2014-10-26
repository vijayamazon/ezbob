﻿namespace Reports.Alibaba.DataSharing {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.ParsedValue;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	public class CustomerData {
		#region public

		public const string RowType = "RowType";
		public const string LoanDataCustomerIDField = "CustomerID";

		#region constructor

		public CustomerData(SafeReader sr) {
			Data = new Dictionary<string, Dictionary<string, ParsedValue>>();
			CustomerID = 0;
			CustomerRefNum = string.Empty;
			BuyerID = string.Empty;

			for (int nIdx = 0; nIdx < sr.Count; nIdx++) {
				string sFieldName = sr.GetName(nIdx);

				if (sFieldName == RowType)
					continue;

				ParsedValue oValue = sr[nIdx];

				if (sFieldName == CustomerIDField)
					CustomerID = oValue;
				else if (sFieldName == CustomerRefNumField)
					CustomerRefNum = oValue;
				else if (sFieldName == BuyerIDField)
					BuyerID = oValue;

				AddSectionItem(sFieldName, oValue);
			} // for

			m_oLoans = new SortedDictionary<int, LoanData>();
			m_oCashRequests = new SortedDictionary<int, CashRequestData>();
		} // constructor

		#endregion constructor

		public Dictionary<string, Dictionary<string, ParsedValue>> Data { get; private set; }

		public int CustomerID { get; private set; }

		public string CustomerRefNum { get; private set; }

		public string BuyerID { get; private set; }

		#region method AddLoanData

		public void AddLoanData(SafeReader sr) {
			for (int nIdx = 0; nIdx < sr.Count; nIdx++) {
				string sFieldName = sr.GetName(nIdx);

				if ((sFieldName == RowType) || (sFieldName == LoanDataCustomerIDField))
					continue;

				AddSectionItem(sFieldName, sr[nIdx]);
			} // for

			AddSectionItem(SignedField, Yes);
		} // AddLoanData

		#endregion method AddLoanData

		#region method AddRepayment

		public void AddRepayment(SafeReader sr, SortedSet<int> oLateLoans) {
			LoanData ld = sr.Fill<LoanData>();

			ld.IsLate = oLateLoans.Contains(ld.ID);

			if (m_oLoans.ContainsKey(ld.ID))
				m_oLoans[ld.ID] += ld;
			else {
				m_oLoans[ld.ID] = ld;

				if (m_oCashRequests.ContainsKey(ld.CashRequestID))
					m_oCashRequests[ld.CashRequestID].AmountTaken += ld.Amount;
				else {
					m_oCashRequests[ld.CashRequestID] = new CashRequestData {
						ApprovedAmount = sr["ManagerApprovedSum"],
						AmountTaken = ld.Amount,
					};
				} // if
			} // if
		} // AddRepayment

		#endregion method AddRepayment

		#region method SaveTo

		public void SaveTo(ExcelPackage oReport) {
			foreach (KeyValuePair<string, Dictionary<string, ParsedValue>> pair in Data) {
				string sSectionName = pair.Key;
				Dictionary<string, ParsedValue> oSection = pair.Value;

				ExcelWorksheet oSheet = oReport.Workbook.Worksheets[sSectionName] ?? CreateSheet(oReport, sSectionName, oSection.Keys.ToArray());

				int nCustomerRow = 2;

				while (oSheet.Cells[nCustomerRow, 1].Value != null)
					nCustomerRow++;

				SetCellValue(oSheet, nCustomerRow, 1, CustomerRefNum);

				int nColumn = 2;

				foreach (var oValuePair in oSection)
					nColumn = SetCellValue(oSheet, nCustomerRow, nColumn, oValuePair.Value.Raw);
			} // for each section

			SaveLoanServicingSection(oReport);
		} // SaveTo

		#endregion method SaveTo

		#endregion public

		#region private

		#region method SetCellValue

		private int SetCellValue(ExcelWorksheet oSheet, int nRow, int nColumn, object oRaw) {
			if (oRaw == null)
				oSheet.Cells[nRow, nColumn].Value = null;
			else if (oRaw is DateTime)
				oSheet.Cells[nRow, nColumn].Value = ((DateTime)oRaw).ToString("dd/MMMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
			else
				oSheet.Cells[nRow, nColumn].Value = oRaw;

			nColumn++;

			return nColumn;
		} // SetCellValue

		#endregion method SetCellValue

		#region method AddSectionItem

		private void AddSectionItem(string sFieldName, ParsedValue oValue) {
			int nPos = sFieldName.IndexOf('_');

			string sSectionName = sFieldName.Substring(0, nPos);

			string sFieldInSection = sFieldName.Substring(nPos + 1);

			if (!Data.ContainsKey(sSectionName))
				Data[sSectionName] = new Dictionary<string, ParsedValue>();

			Data[sSectionName][sFieldInSection] = oValue;
		} // AddSectionItem

		#endregion method AddSectionItem

		#region method SaveLoanServicingSection

		private void SaveLoanServicingSection(ExcelPackage oReport) {
			const string sSectionName = "LoanServicing";

			ExcelWorksheet oSheet = oReport.Workbook.Worksheets[sSectionName] ?? CreateSheet(
				oReport,
				sSectionName,
				"OrderNo",
				"BuyerRepayDate",
				"BuyerRepayAmount",
				"TotalInterestAmount",
				"Overdue",
				"UnusedCreditAmount"
			);

			int nLoanRow = 2;

			while (oSheet.Cells[nLoanRow, 1].Value != null)
				nLoanRow++;

			foreach (KeyValuePair<int, LoanData> pair in m_oLoans) {
				LoanData oLoan = pair.Value;

				SetCellValue(oSheet, nLoanRow, 1, CustomerRefNum);

				int nColumn = 2;

				nColumn = SetCellValue(oSheet, nLoanRow, nColumn, oLoan.RefNum);
				nColumn = SetCellValue(oSheet, nLoanRow, nColumn, oLoan.DateClosed);
				nColumn = SetCellValue(oSheet, nLoanRow, nColumn, oLoan.TotalRepaid);
				nColumn = SetCellValue(oSheet, nLoanRow, nColumn, oLoan.InterestRepaid);
				nColumn = SetCellValue(oSheet, nLoanRow, nColumn, oLoan.IsLate ? "yes" : "no");
				nColumn = SetCellValue(oSheet, nLoanRow, nColumn, m_oCashRequests[oLoan.CashRequestID].UnusedAmount);

				nLoanRow++;
			} // for each loan
		} // SaveLoanServicingSection 

		#endregion method SaveLoanServicingSection

		#region method CreateSheet

		private ExcelWorksheet CreateSheet(ExcelPackage oReport, string sSheetName, params string[] oColumnNames) {
			var oSheet = oReport.Workbook.Worksheets.Add(sSheetName);

			SetCellValue(oSheet, 1, 1, "Customer ID");

			int nColumn = 2;

			foreach (var sName in oColumnNames) {
				SetCellValue(oSheet, 1, nColumn, sName);
				nColumn++;
			} // for each

			return oSheet;
		} // CreateSheet

		#endregion method CreateSheet

		private const string CustomerIDField = "CreditAccount_CustomerID";
		private const string CustomerRefNumField = "ApprovalPhaseVerify_EzbobMemberID";
		private const string BuyerIDField = "ApprovalPhaseVerify_BuyerID";

		private const string SignedField = "LoanAgreementPhase_SignOnlineFinancingAgreement";
		private static readonly ParsedValue Yes = new ParsedValue("yes");

		private readonly SortedDictionary<int, LoanData> m_oLoans; 
		private readonly SortedDictionary<int, CashRequestData> m_oCashRequests; 

		#region class LoanData

		private class LoanData {
			public static LoanData operator +(LoanData a, LoanData b) {
				if (b == null)
					return a;

				a.TotalRepaid += b.TotalRepaid;
				a.InterestRepaid += b.InterestRepaid;

				return a;
			} // operator +

			[UsedImplicitly]
			[FieldName("LoanID")]
			public int ID { get; set; }

			[UsedImplicitly]
			[FieldName("LoanRefNum")]
			public string RefNum { get; set; }

			[UsedImplicitly]
			public DateTime? DateClosed { get; set; }

			[UsedImplicitly]
			[FieldName("LoanAmount")]
			public decimal Amount { get; set; }

			[UsedImplicitly]
			[FieldName("Amount")]
			public decimal TotalRepaid { get; set; }

			[UsedImplicitly]
			[FieldName("Interest")]
			public decimal InterestRepaid { get; set; }

			[UsedImplicitly]
			[FieldName("RequestCashID")]
			public int CashRequestID { get; set; }

			[NonTraversable]
			public bool IsLate { get; set; }
		} // LoanData

		#endregion class LoanData

		#region class CashRequestData

		private class CashRequestData {
			public decimal ApprovedAmount { get; set; }
			public decimal AmountTaken { get; set; }

			public decimal UnusedAmount {
				get { return ApprovedAmount - AmountTaken; }
			} // UnusedAmount
		} // CashRequestData

		#endregion class CashRequestData

		#endregion private
	} // class CustomerData
} // namespace
