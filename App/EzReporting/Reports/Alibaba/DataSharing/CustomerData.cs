namespace Reports.Alibaba.DataSharing {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using Ezbob.Utils.ParsedValue;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	public class CustomerData {
		static CustomerData() {
			TotalApprovedAmount = 0;

			ms_oAllMarketplaces = new SortedSet<string>();

			ms_oColumnTitles = new Dictionary<string, Dictionary<string, string>>();

			ms_oColumnTitles["DataPoint"] = new Dictionary<string, string> {
				{ "VatLinked", "Automatic VAT upload" },
				{ "VatUploaded", "VAT Sales call upload" },
			};

			ms_oColumnNumberFormats = new Dictionary<string, Dictionary<string, string>>();

			ms_oColumnNumberFormats[ApprovalPhaseFeedback] = new Dictionary<string, string> {
				{ ApprovedAmount, "£#,##0.00" },
			};
		} // static constructor

		public const string RowType = "RowType";
		public const string LoanDataCustomerIDField = "CustomerID";

		public CustomerData(SafeReader sr) {
			Data = new Dictionary<string, Dictionary<string, ParsedValue>>();
			CustomerID = 0;
			CustomerRefNum = string.Empty;
			AlibabaID = string.Empty;

			m_oMarketplaces = new SortedSet<string>();

			for (int nIdx = 0; nIdx < sr.Count; nIdx++) {
				string sFieldName = sr.GetName(nIdx);

				if (sFieldName == RowType)
					continue;

				ParsedValue oValue = sr[nIdx];

				if (sFieldName == CustomerIDField)
					CustomerID = oValue;
				else if (sFieldName == AlibabaIDField)
					AlibabaID = oValue;
				else if (sFieldName == CustomerRefNumField)
					CustomerRefNum = oValue;
				else {
					if ((sFieldName == ApprovedAmountField) && (oValue != null) && (oValue.Raw != null))
						TotalApprovedAmount += (decimal)oValue;

					AddSectionItem(sFieldName, oValue);
				} // if
			} // for

			m_oLoans = new SortedDictionary<int, LoanData>();
			m_oCashRequests = new SortedDictionary<int, CashRequestData>();
		} // constructor

		public Dictionary<string, Dictionary<string, ParsedValue>> Data { get; private set; }

		public int CustomerID { get; private set; }

		public string CustomerRefNum { get; private set; }

		public string AlibabaID { get; private set; }

		public void AddLoanData(SafeReader sr) {
			for (int nIdx = 0; nIdx < sr.Count; nIdx++) {
				string sFieldName = sr.GetName(nIdx);

				if ((sFieldName == RowType) || (sFieldName == LoanDataCustomerIDField))
					continue;

				AddSectionItem(sFieldName, sr[nIdx]);
			} // for

			AddSectionItem(SignedField, Yes);
		} // AddLoanData

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

		public void SaveTo(ExcelPackage oReport) {
			foreach (KeyValuePair<string, Dictionary<string, ParsedValue>> pair in Data) {
				string sSectionName = pair.Key;

				if (sSectionName == DataPoint) {
					SaveDataPointSection(oReport);
					continue;
				} // if

				Dictionary<string, ParsedValue> oSection = pair.Value;

				ExcelWorksheet oSheet = oReport.FindOrCreateSheet(
					sSectionName,
					true,
					null,
					oSection.Keys.Select(sTitle => GetColumnDisplayName(sSectionName, sTitle)).ToArray()
				);

				int nCustomerRow = 2;

				while (oSheet.Cells[nCustomerRow, 1].Value != null)
					nCustomerRow++;

				int nColumn = 1;

				nColumn = oSheet.SetCellValue(nCustomerRow, nColumn, CustomerRefNum);
				nColumn = oSheet.SetCellValue(nCustomerRow, nColumn, AlibabaID);

				Dictionary<string, string> oFormats = null;

				if (ms_oColumnNumberFormats.ContainsKey(sSectionName))
					oFormats = ms_oColumnNumberFormats[sSectionName];

				foreach (KeyValuePair<string, ParsedValue> sectionPair in oSection) {
					string sFormat = ((oFormats != null) && oFormats.ContainsKey(sectionPair.Key))
						? oFormats[sectionPair.Key]
						: null;

					nColumn = oSheet.SetCellValue(
						nCustomerRow,
						nColumn,
						sectionPair.Value,
						sNumberFormat: sFormat
					);
				} // for each
			} // for each section

			SaveLoanServicingSection(oReport);
		} // SaveTo

		public void AddApprovalPhaseTotal(ExcelPackage oReport) {
			var ws = oReport.FindSheet(ApprovalPhaseFeedback);

			if (ws == null)
				return;

			int nTotalRow = 1;

			while (ws.Cells[nTotalRow, 1].Value != null)
				nTotalRow++;

			Color oFontColour = Color.White;
			Color oBgColour = Color.Black;

			int nColumn = 1;

			nColumn = ws.SetCellValue(nTotalRow, nColumn, "Total", oFontColour: oFontColour, oBgColour: oBgColour, bIsBold: true, bSetZebra: false);
			nColumn = ws.SetCellValue(nTotalRow, nColumn, null, oFontColour: oFontColour, oBgColour: oBgColour, bSetZebra: false);

			var oSection = Data[ApprovalPhaseFeedback];

			foreach (KeyValuePair<string, ParsedValue> pair in oSection) {
				decimal? oValue = pair.Key == ApprovedAmount ? TotalApprovedAmount : (decimal?)null;
				string sFormat = pair.Key == ApprovedAmount ? ms_oColumnNumberFormats[ApprovalPhaseFeedback][ApprovedAmount] : null;

				nColumn = ws.SetCellValue(
					nTotalRow,
					nColumn,
					oValue,
					bIsBold: true,
					bSetZebra: false,
					oFontColour: oFontColour,
					oBgColour: oBgColour,
					sNumberFormat: sFormat
				);
			} // for each

		} // AddApprovalPhaseTotal

		public void AddMarketplace(SafeReader sr) {
			string sMpName = sr["Marketplace"];

			if (string.IsNullOrWhiteSpace(sMpName))
				return;

			ms_oAllMarketplaces.Add(sMpName);
			m_oMarketplaces.Add(sMpName);
		} // AddMarketplace

		private void AddSectionItem(string sFieldName, ParsedValue oValue) {
			int nPos = sFieldName.IndexOf('_');

			string sSectionName = sFieldName.Substring(0, nPos);

			string sFieldInSection = sFieldName.Substring(nPos + 1);

			if (!Data.ContainsKey(sSectionName))
				Data[sSectionName] = new Dictionary<string, ParsedValue>();

			Data[sSectionName][sFieldInSection] = oValue;
		} // AddSectionItem

		private void SaveLoanServicingSection(ExcelPackage oReport) {
			ExcelWorksheet oSheet = oReport.FindOrCreateSheet(
				"LoanServicing",
				true,
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

				oSheet.SetRowValues(nLoanRow, true,
					CustomerRefNum,
					AlibabaID,

					oLoan.RefNum,
					oLoan.DateClosed,
					oLoan.TotalRepaid,
					oLoan.InterestRepaid,
					oLoan.IsLate ? "yes" : "no",
					m_oCashRequests[oLoan.CashRequestID].UnusedAmount
				);

				nLoanRow++;
			} // for each loan
		} // SaveLoanServicingSection 

		private void SaveDataPointSection(ExcelPackage oReport) {
			if (!Data.ContainsKey(DataPoint))
				return;

			Dictionary<string, ParsedValue> oSection = Data[DataPoint];

			var oTitles = new List<string>();

			foreach (var pair in oSection)
				oTitles.Add(GetColumnDisplayName(DataPoint, pair.Key));

			foreach (var sMpName in ms_oAllMarketplaces)
				oTitles.Add(GetColumnDisplayName(DataPoint, sMpName));

			ExcelWorksheet oSheet = oReport.FindOrCreateSheet(
				DataPoint,
				true,
				oTitles.ToArray()
			);

			int nMpRow = 2;

			while (oSheet.Cells[nMpRow, 1].Value != null)
				nMpRow++;

			int nColumn = 1;

			nColumn = oSheet.SetCellValue(nMpRow, nColumn, CustomerRefNum);
			nColumn = oSheet.SetCellValue(nMpRow, nColumn, AlibabaID);

			foreach (var sMpName in oTitles) {
				if (oSection.ContainsKey(sMpName))
					nColumn = oSheet.SetCellValue(nMpRow, nColumn, oSection[sMpName] ? "yes" : null);
				else
					nColumn = oSheet.SetCellValue(nMpRow, nColumn, m_oMarketplaces.Contains(sMpName) ? "yes" : null);
			} // for each
		} // SaveDataPointSection

		private const string CustomerIDField = "CustomerID";
		private const string CustomerRefNumField = "CustomerRefnum";
		private const string AlibabaIDField = "AlibabaID";

		private const string SignedField = "LoanAgreementPhase_SignOnlineFinancingAgreement";
		private static readonly ParsedValue Yes = new ParsedValue("yes");

		private const string ApprovedAmount = "ApprovedAmount";
		private const string ApprovedAmountField = "ApprovalPhaseFeedback_" + ApprovedAmount;

		private const string ApprovalPhaseFeedback = "ApprovalPhaseFeedback";

		private const string DataPoint = "DataPoint";

		private readonly SortedDictionary<int, LoanData> m_oLoans; 
		private readonly SortedDictionary<int, CashRequestData> m_oCashRequests; 

		private readonly SortedSet<string> m_oMarketplaces;

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

		private class CashRequestData {
			public decimal ApprovedAmount { get; set; }
			public decimal AmountTaken { get; set; }

			public decimal UnusedAmount {
				get { return ApprovedAmount - AmountTaken; }
			} // UnusedAmount
		} // CashRequestData

		private static readonly SortedSet<string> ms_oAllMarketplaces;

		private static readonly Dictionary<string, Dictionary<string, string>> ms_oColumnNumberFormats;
		private static readonly Dictionary<string, Dictionary<string, string>> ms_oColumnTitles;
		private static decimal TotalApprovedAmount { get; set; }

		private static string GetColumnDisplayName(string sSectionName, string sColumnTitle) {
			if (!ms_oColumnTitles.ContainsKey(sSectionName))
				return sColumnTitle;

			var dic = ms_oColumnTitles[sSectionName];

			return dic.ContainsKey(sColumnTitle) ? dic[sColumnTitle] : sColumnTitle;
		} // GetColumnDisplayName

	} // class CustomerData
} // namespace
