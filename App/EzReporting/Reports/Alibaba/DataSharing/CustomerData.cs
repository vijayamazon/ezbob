namespace Reports.Alibaba.DataSharing {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.ParsedValue;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	public class CustomerData {
		#region static constructor

		static CustomerData() {
			ms_oColumnTitles = new Dictionary<string, Dictionary<string, string>>();

			ms_oColumnTitles["VatAccount"] = new Dictionary<string, string> {
				{ "Linked", "Automatic upload" },
				{ "Uploaded", "Sales call upload" },
			};
		} // static constructor

		#endregion static constructor

		#region public

		public const string RowType = "RowType";
		public const string LoanDataCustomerIDField = "CustomerID";
		public const string ApprovalPhaseFeedback = "ApprovalPhaseFeedback";

		#region constructor

		public CustomerData(SafeReader sr) {
			Data = new Dictionary<string, Dictionary<string, ParsedValue>>();
			CustomerID = 0;
			CustomerRefNum = string.Empty;
			AlibabaID = string.Empty;

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
				else
					AddSectionItem(sFieldName, oValue);
			} // for

			m_oLoans = new SortedDictionary<int, LoanData>();
			m_oCashRequests = new SortedDictionary<int, CashRequestData>();
		} // constructor

		#endregion constructor

		public Dictionary<string, Dictionary<string, ParsedValue>> Data { get; private set; }

		public int CustomerID { get; private set; }

		public string CustomerRefNum { get; private set; }

		public string AlibabaID { get; private set; }

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

				Action<ExcelWorksheet> onCreate = (sSectionName == FinancialDetails) ? new Action<ExcelWorksheet>(ws => {
					int nColumn = 1;

					while (ws.Cells[1, nColumn].Value != null)
						nColumn++;

					ws.SetCellValue(1, nColumn, "The data is provided by customers.", false);
					ws.Cells[1, nColumn].Style.Font.Bold = true;
					ws.Cells[1, nColumn].Style.Font.Color.SetColor(Color.Red);
				}) : null;

				ExcelWorksheet oSheet = oReport.FindOrCreateSheet(
					sSectionName,
					true,
					onCreate,
					oSection.Keys.Select(sTitle => GetColumnDisplayName(sSectionName, sTitle)).ToArray()
				);

				int nCustomerRow = 2;

				while (oSheet.Cells[nCustomerRow, 1].Value != null)
					nCustomerRow++;

				var lst = new List<object> {
					new ParsedValue(CustomerRefNum),
					new ParsedValue(AlibabaID),
				};

				lst.AddRange(oSection.Values);

				oSheet.SetRowValues(nCustomerRow, true, lst.ToArray());
			} // for each section

			SaveLoanServicingSection(oReport);
		} // SaveTo

		#endregion method SaveTo

		#endregion public

		#region private

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

		#endregion method SaveLoanServicingSection

		private const string CustomerIDField = "CustomerID";
		private const string CustomerRefNumField = "CustomerRefnum";
		private const string AlibabaIDField = "AlibabaID";

		private const string SignedField = "LoanAgreementPhase_SignOnlineFinancingAgreement";
		private static readonly ParsedValue Yes = new ParsedValue("yes");

		private const string FinancialDetails = "FinancialDetails";

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

		private static readonly Dictionary<string, Dictionary<string, string>> ms_oColumnTitles;

		#region method GetColumnDisplayName

		private static string GetColumnDisplayName(string sSectionName, string sColumnTitle) {
			if (!ms_oColumnTitles.ContainsKey(sSectionName))
				return sColumnTitle;

			var dic = ms_oColumnTitles[sSectionName];

			return dic.ContainsKey(sColumnTitle) ? dic[sColumnTitle] : sColumnTitle;
		} // GetColumnDisplayName

		#endregion method GetColumnDisplayName

		#endregion private
	} // class CustomerData
} // namespace
