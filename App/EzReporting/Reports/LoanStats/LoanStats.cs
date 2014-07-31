using Ezbob.Database;
using EZBob.DatabaseLib.Model.Database;
using Ezbob.Logger;
using Ezbob.Utils;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;

namespace Reports {
	using System.IO;

	public class LoanStats : SafeLog {
		#region public

		#region constructor

		public LoanStats(AConnection oDB, ASafeLog oLog = null) : base(oLog) {
			if (oDB == null)
				throw new ArgumentNullException("oDB", "Connection to database not specified.");

			m_oDB = oDB;

			LoadCashRequests();
			LoadCustomerRegions();
			LoadCustomerMarketplaces();
			LoadPaypalTotals();
		} // constructor

		#endregion constructor

		#region method Generate

		public List<LoanStatsReportEntry> Generate() {
			var oResult = new List<LoanStatsReportEntry>();

			foreach (KeyValuePair<int, List<LoanStatsDataEntry>> pair in Data) {
				int nCustomerID = pair.Key;

				foreach (LoanStatsDataEntry lse in pair.Value) {
					var lre = new LoanStatsReportEntry();
					oResult.Add(lre);

					lre.IsFirstLoan = lse.IsLoanIssued ? ((lse.IsFirstLoan ? "" : "not ") + "first") : "";

					lre.ClientLoanOrderNo = lse.LoanSeqNo;
					lre.TypeOfLoan = lse.ApprovedType;
					lre.CustomerSelection = lse.IsLoanTypeSelectionAllowed ? 1 : 0;
					lre.DiscountPlan = lse.DiscountPlanName;
					lre.Offline = lse.IsOffline ? "offline" : "online";
					lre.LoanID = lse.IsLoanIssued ? lse.LoanID : (int?)null;
					lre.ClientID = lse.CustomerID;
					lre.ClientName = lse.CustomerName;
					lre.DateFirstApproved = lse.FirstDecisionDate.Date;
					lre.DateLastApproved = lse.LastDecisionDate.Date;
					lre.NewOrOldClient = lse.IsNewClient ? "new" : "old";
					lre.LoanOffered = lse.ApprovedSum;
					lre.InterestRate = lse.ApprovedRate;
					lre.LoanIssued = lse.IsLoanIssued ? lse.LoanAmount : (decimal?)null;
					lre.IsLoanIssued = lse.IsLoanIssued ? "yes" : "no";
					lre.LoanIssueDate = lse.IsLoanIssued ? lse.IssueDate : (DateTime?)null;
					lre.LoanDuration = lse.IsLoanIssued ? lse.LoanTerm : (int?)null;
					lre.CreditScore = lse.CreditScore;
					lre.TotalAnnualTurnover = lse.AnnualTurnover;
					lre.Medal = ((object)lse.Medal).ToString().ToLower();

					SortedDictionary<int, int> oMarketplaceCount = CustomerMarketplaces.ContainsKey(lse.CustomerID)
						? CustomerMarketplaces[lse.CustomerID].Count(lse.FirstDecisionDate)
						: LoanStatsMarketplaces.Count();

					foreach (KeyValuePair<int, int> mpc in oMarketplaceCount)
						lre.MarketplaceCount[mpc.Key] = mpc.Value;

					lre.PaypalTotal = CustomerPaypalTotals.ContainsKey(lse.CustomerID)
						? CustomerPaypalTotals[lse.CustomerID].Calculate(lse.FirstDecisionDate)
						: 0;

					lre.Gender = lse.Gender.ToString().ToLower();
					lre.YearOfBirth = lse.BirthDate.Year;

					switch (lse.MaritalStatus) {
					case MaritalStatus.Married:
						lre.FamilyStatus = 1;
						break;

					case MaritalStatus.Single:
						lre.FamilyStatus = 0;
						break;

					case MaritalStatus.Divorced:
						lre.FamilyStatus = 2;
						break;

					case MaritalStatus.Widowed:
						lre.FamilyStatus = 3;
						break;

					case MaritalStatus.LivingTogether:
						lre.FamilyStatus = 4;
						break;

					case MaritalStatus.Separated:
						lre.FamilyStatus = 5;
						break;

					case MaritalStatus.Other:
						lre.FamilyStatus = 0;
						break;

					default:
						throw new ArgumentOutOfRangeException("MaritalStatus", (object)lse.MaritalStatus, "Unsupported marital status.");
					} // switch

					if (lse.IsHomeOwner)
					{
						lre.HomeOwnership = 1;
					}
					else
					{
						switch (lse.PropertyStatusDescription.ToLower())
						{
							case "social house":
								lre.HomeOwnership = 3;
								break;

							case "renting":
								lre.HomeOwnership = 0;
								break;

							case "living with parents":
								lre.HomeOwnership = 2;
								break;

							default:
								lre.HomeOwnership = null;
								break;
						} // switch
					}

					switch (lse.TypeOfBusiness) {
					case TypeOfBusiness.Entrepreneur:
					case TypeOfBusiness.SoleTrader:
						lre.TypeOfBusiness = 0;
						break;

					case TypeOfBusiness.LLP:
					case TypeOfBusiness.Limited:
						lre.TypeOfBusiness = 2;
						break;

					case TypeOfBusiness.PShip3P:
					case TypeOfBusiness.PShip:
						lre.TypeOfBusiness = 1;
						break;

					default:
						throw new ArgumentOutOfRangeException();
					} // switch

					lre.SourceRef = lse.ReferenceSource;

					lre.Category1 = ""; // TODO
					lre.Category2 = ""; // TODO
					lre.Category3 = ""; // TODO

					lre.Region = CustomerRegions.ContainsKey(nCustomerID) ? CustomerRegions[nCustomerID] : "";
				} // for each entry
			} // for each customer

			return oResult;
		} // Generate

		#endregion method Generate

		#region method Xls

		public ExcelPackage Xls() {
			var wb = new ExcelPackage();

			ExcelWorksheet sheet = wb.Workbook.Worksheets.Add("Data");

			List<LoanStatsReportEntry> list = Generate();

			const int nFirstRow = 15;

			FillTitle(sheet, nFirstRow - 1);

			int nOffset = 0;

			foreach (LoanStatsReportEntry lre in list) {
				FillRow(sheet, nFirstRow + nOffset, lre);
				++nOffset;
			} // for each

			sheet.Cells.AutoFitColumns();

			return wb;
		} // Xls

		#endregion method Xls

		#endregion public

		#region private

		#region method FillRow

		private void FillRow(ExcelWorksheet sheet, int nRowNumber, LoanStatsReportEntry lre) {
			sheet.Cells["A"  + nRowNumber].Value = lre.IsFirstLoan;
			sheet.Cells["B"  + nRowNumber].Value = lre.ClientLoanOrderNo;
			sheet.Cells["C"  + nRowNumber].Value = lre.TypeOfLoan;
			sheet.Cells["D"  + nRowNumber].Value = lre.CustomerSelection;
			sheet.Cells["E"  + nRowNumber].Value = lre.DiscountPlan;
			sheet.Cells["F"  + nRowNumber].Value = lre.Offline;
			sheet.Cells["G"  + nRowNumber].Value = lre.LoanID;
			sheet.Cells["I"  + nRowNumber].Value = lre.ClientID;
			sheet.Cells["J"  + nRowNumber].Value = lre.ClientName;
			sheet.Cells["K"  + nRowNumber].Value = lre.DateFirstApproved;
			sheet.Cells["L"  + nRowNumber].Value = lre.DateLastApproved;
			sheet.Cells["M"  + nRowNumber].Value = lre.NewOrOldClient;
			sheet.Cells["N"  + nRowNumber].Value = lre.LoanOffered;
			sheet.Cells["P"  + nRowNumber].Value = lre.InterestRate;
			sheet.Cells["Q"  + nRowNumber].Value = lre.LoanIssued;
			sheet.Cells["R"  + nRowNumber].Value = lre.IsLoanIssued;
			sheet.Cells["T"  + nRowNumber].Value = lre.LoanIssueDate;
			sheet.Cells["X"  + nRowNumber].Value = lre.LoanDuration;
			sheet.Cells["Y"  + nRowNumber].Value = lre.CreditScore;
			sheet.Cells["AB" + nRowNumber].Value = lre.TotalAnnualTurnover;
			sheet.Cells["AC" + nRowNumber].Value = lre.Medal;
			sheet.Cells["AK" + nRowNumber].Value = lre.PaypalTotal;
			sheet.Cells["AL" + nRowNumber].Value = lre.Gender;
			sheet.Cells["AO" + nRowNumber].Value = lre.YearOfBirth;
			sheet.Cells["AQ" + nRowNumber].Value = lre.FamilyStatus;
			sheet.Cells["FN" + nRowNumber].Value = lre.HomeOwnership;
			sheet.Cells["FO" + nRowNumber].Value = lre.TypeOfBusiness;
			sheet.Cells["GC" + nRowNumber].Value = lre.SourceRef;
			sheet.Cells["GD" + nRowNumber].Value = lre.Category1;
			sheet.Cells["GE" + nRowNumber].Value = lre.Category2;
			sheet.Cells["GF" + nRowNumber].Value = lre.Category3;
			sheet.Cells["GG" + nRowNumber].Value = lre.Region;

			int nColumn = sheet.Cells["GH" + nRowNumber].Start.Column;

			foreach (KeyValuePair<int, int> pair in lre.MarketplaceCount) {
				int nMarketplaceTypeID = pair.Key;
				int nCount = pair.Value;

				string sMarketplaceType = LoanStatsMarketplaces.MarketplaceTypes[nMarketplaceTypeID].ToLower();

				if (sMarketplaceType.StartsWith("ebay"))
					sheet.Cells["AE" + nRowNumber].Value = nCount;
				else if (sMarketplaceType.StartsWith("amazon"))
					sheet.Cells["AF" + nRowNumber].Value = nCount;
				else if (sMarketplaceType.StartsWith("ekm"))
					sheet.Cells["AG" + nRowNumber].Value = nCount;
				else if (sMarketplaceType.StartsWith("volusion"))
					sheet.Cells["AH" + nRowNumber].Value = nCount;
				else if (sMarketplaceType.StartsWith("play"))
					sheet.Cells["AI" + nRowNumber].Value = nCount;
				else if (sMarketplaceType.StartsWith("pay pal"))
					sheet.Cells["AJ" + nRowNumber].Value = nCount;
				else {
					sheet.Cells[nRowNumber, nColumn].Value = nCount;
					++nColumn;
				} // if
			} // for each

			sheet.Cells["K" + nRowNumber + ":L" + nRowNumber].Style.Numberformat.Format = "dd-mmm-yy";

			sheet.Cells["T"  + nRowNumber].Style.Numberformat.Format = "dd-mmm-yy";
			sheet.Cells["P"  + nRowNumber].Style.Numberformat.Format = "0.00%";
			sheet.Cells["N"  + nRowNumber].Style.Numberformat.Format = "#,##0";
			sheet.Cells["Q"  + nRowNumber].Style.Numberformat.Format = "#,##0";
			sheet.Cells["AB" + nRowNumber].Style.Numberformat.Format = "#,##0";
			sheet.Cells["AK" + nRowNumber].Style.Numberformat.Format = "#,##0";
		} // FillRow

		#endregion method FillRow

		#region method FillTitle

		private void FillTitle(ExcelWorksheet sheet, int nRowNumber) {
			sheet.Cells["A"  + nRowNumber].Value = "Is this 'a first' loan?";
			sheet.Cells["B"  + nRowNumber].Value = "Client loan order #";
			sheet.Cells["C"  + nRowNumber].Value = "Type of loan (s-standard, n-new offer)";
			sheet.Cells["D"  + nRowNumber].Value = "Customer selection (0-no, 1-yes)";
			sheet.Cells["E"  + nRowNumber].Value = "Discount plan (0, new13, old13)";
			sheet.Cells["G"  + nRowNumber].Value = "Loan ID";
			sheet.Cells["I"  + nRowNumber].Value = "Client ID";
			sheet.Cells["J"  + nRowNumber].Value = "Name";
			sheet.Cells["K"  + nRowNumber].Value = "Date first approved";
			sheet.Cells["L"  + nRowNumber].Value = "Date last approved";
			sheet.Cells["M"  + nRowNumber].Value = "New or old client?";
			sheet.Cells["N"  + nRowNumber].Value = "Loan offered";
			sheet.Cells["P"  + nRowNumber].Value = "interest rate, % per month";
			sheet.Cells["Q"  + nRowNumber].Value = "Loan issued";
			sheet.Cells["R"  + nRowNumber].Value = "Loan issued?";
			sheet.Cells["T"  + nRowNumber].Value = "Loan issue date";
			sheet.Cells["X"  + nRowNumber].Value = "Loan duration, months";
			sheet.Cells["Y"  + nRowNumber].Value = "Credit score";
			sheet.Cells["AB" + nRowNumber].Value = "Total annual turnover, GBP";
			sheet.Cells["AC" + nRowNumber].Value = "Medal";
			sheet.Cells["AK" + nRowNumber].Value = "paypal total in";
			sheet.Cells["AL" + nRowNumber].Value = "Gender";
			sheet.Cells["AO" + nRowNumber].Value = "Year of birth";
			sheet.Cells["AQ" + nRowNumber].Value = "Family status: 0-single/other, 1-married, 2-divorced, 3-widow";
			sheet.Cells["FN" + nRowNumber].Value = "Home ownership (0-renting, 1-home owner, 2-living with parents, 3-social house)";
			sheet.Cells["FO" + nRowNumber].Value = "Type of business (0-entrepreneur, 1-partnership, 2-Ltd)";
			sheet.Cells["GC" + nRowNumber].Value = "SourceRef";
			sheet.Cells["GD" + nRowNumber].Value = "Category 1";
			sheet.Cells["GE" + nRowNumber].Value = "Category 2";
			sheet.Cells["GF" + nRowNumber].Value = "Category 3";
			sheet.Cells["GG" + nRowNumber].Value = "Region";

			int nColumn = sheet.Cells["GH" + nRowNumber].Start.Column;

			foreach (KeyValuePair<int, string> pair in LoanStatsMarketplaces.MarketplaceTypes) {
				string sTitle = "# of " + pair.Value + " stores";
				string sMarketplaceType = pair.Value.ToLower();

				if (sMarketplaceType.StartsWith("ebay"))
					sheet.Cells["AE" + nRowNumber].Value = sTitle;
				else if (sMarketplaceType.StartsWith("amazon"))
					sheet.Cells["AF" + nRowNumber].Value = sTitle;
				else if (sMarketplaceType.StartsWith("ekm"))
					sheet.Cells["AG" + nRowNumber].Value = sTitle;
				else if (sMarketplaceType.StartsWith("volusion"))
					sheet.Cells["AH" + nRowNumber].Value = sTitle;
				else if (sMarketplaceType.StartsWith("play"))
					sheet.Cells["AI" + nRowNumber].Value = sTitle;
				else if (sMarketplaceType.StartsWith("pay pal"))
					sheet.Cells["AJ" + nRowNumber].Value = sTitle;
				else {
					sheet.Cells[nRowNumber, nColumn].Value = sTitle;
					++nColumn;
				} // if
			} // for
		} // FillTitle

		#endregion method FillTitle

		#region method LoadCashRequests

		private void LoadCashRequests() {
			Msg("Loan Stats: loading cash requests...");

			Data = new SortedDictionary<int, List<LoanStatsDataEntry>>();

			Debug("Loan Stats: processing raw data...");

			var oCounter = new ProgressCounter("Loan Stats: {0} rows processed", this);

			LoanStatsDataEntry oCurrent = null;

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					if (oCurrent == null)
						oCurrent = CreateEntry(sr);
					else {
						int nCustomerID = sr["CustomerID"];

						if (oCurrent.CustomerID == nCustomerID)
							oCurrent.Update(sr);
						else
							oCurrent = CreateEntry(sr);
					} // if

					if (oCurrent.LoanID != 0)
						oCurrent = null;

					++oCounter;

					return ActionResult.Continue;
				},
				"RptLoanStats_CashRequests",
				CommandSpecies.StoredProcedure
			);

			oCounter.Log();

			Debug("Loan Stats: processing raw data complete.");

			foreach (KeyValuePair<int, List<LoanStatsDataEntry>> pair in Data) {
				int num = 1;

				foreach (LoanStatsDataEntry lse in pair.Value) {
					lse.LoanSeqNo = num;
					++num;
				} // for
			} // for

			Msg("Loan Stats: loading cash requests complete.");
		} // LoadCashRequests

		#endregion method LoadCashRequests

		#region method CreateEntry

		private LoanStatsDataEntry CreateEntry(SafeReader sr) {
			var lse = new LoanStatsDataEntry(sr);

			if (!Data.ContainsKey(lse.CustomerID))
				Data[lse.CustomerID] = new List<LoanStatsDataEntry>();

			Data[lse.CustomerID].Add(lse);

			return lse;
		} // CreateEntry

		#endregion method CreateEntry

		#region method LoadCustomerRegions

		private void LoadCustomerRegions() {
			Msg("Loan Stats: loading customer regions...");

			var ptr = new PostcodeToRegion(this);

			CustomerRegions = new SortedDictionary<int, string>();

			DataTable tbl = m_oDB.ExecuteReader("RptLoanStats_CustomerPostcodes", CommandSpecies.StoredProcedure);

			foreach (DataRow dataRow in tbl.Rows) {
				int nCustomerID = Convert.ToInt32(dataRow["CustomerID"]);
				string sRawpostcode = dataRow["Rawpostcode"].ToString().Trim().ToUpper();

				if (string.IsNullOrWhiteSpace(sRawpostcode))
					continue;
				
					string sPostcode = "";

					for (int i = 0; i < sRawpostcode.Length; ++i) {
						char c = sRawpostcode[i];

						if (('A' <= c) && (c <= 'Z'))
							sPostcode += c;
						else
							break;
					} // for

					string sRegion = ptr[sPostcode];

					if (sRegion != string.Empty)
						CustomerRegions[nCustomerID] = sRegion;
			} // for each row

			Msg("Loan Stats: loading customer regions complete.");
		} // LoadCustomerRegions

		#endregion method LoadCustomerRegions

		#region method LoadCustomerMarketplaces

		private void LoadCustomerMarketplaces() {
			Msg("Loans stats: loading customer marketplaces...");

			DataTable dataTable = m_oDB.ExecuteReader("RptLoanStats_Marketplaces", CommandSpecies.StoredProcedure);

			CustomerMarketplaces = new SortedDictionary<int, LoanStatsMarketplaces>();

			foreach (DataRow dataRow in (InternalDataCollectionBase)dataTable.Rows) {
				int nCustomerID = Convert.ToInt32(dataRow["CustomerID"]);
				int nMarketplaceTypeID = Convert.ToInt32(dataRow["MarketplaceTypeID"]);
				string sMarketplaceTypeName = dataRow["MarketplaceType"].ToString();
				DateTime oCreated = Convert.ToDateTime(dataRow["Created"]);

				if (CustomerMarketplaces.ContainsKey(nCustomerID))
					CustomerMarketplaces[nCustomerID].Add(nMarketplaceTypeID, sMarketplaceTypeName, oCreated);
				else
					CustomerMarketplaces[nCustomerID] = new LoanStatsMarketplaces(nMarketplaceTypeID, sMarketplaceTypeName, oCreated);
			} // for each row

			Msg("Loans stats: loading customer marketplaces complete.");
		} // LoadCustomerMarketplaces

		#endregion method LoadCustomerMarketplaces

		#region method LoadPaypalTotals

		private void LoadPaypalTotals() {
			Msg("Loans stats: loading customer PayPal totals...");

			DataTable dataTable = m_oDB.ExecuteReader("RptLoanStats_PaypalTotalIn", CommandSpecies.StoredProcedure);

			CustomerPaypalTotals = new SortedDictionary<int, LoanStatsPaypalTotal>();

			foreach (DataRow dataRow in dataTable.Rows) {
				int nCustomerID = Convert.ToInt32(dataRow["CustomerID"]);
				int nMarketplaceID = Convert.ToInt32(dataRow["CustomerMarketplaceId"]);
				DateTime oUpdated = Convert.ToDateTime(dataRow["Updated"]);
				decimal nTotal = Convert.ToDecimal(dataRow["ValueFloat"]);

				if (CustomerPaypalTotals.ContainsKey(nCustomerID))
					CustomerPaypalTotals[nCustomerID].Add(nMarketplaceID, oUpdated, nTotal);
				else
					CustomerPaypalTotals[nCustomerID] = new LoanStatsPaypalTotal(nMarketplaceID, oUpdated, nTotal);
			} // for each row

			Msg("Loans stats: loading customer PayPal totals complete.");
		} // LoadPaypalTotals

		#endregion method LoadPaypalTotals

		#region fields

		private AConnection m_oDB;

		private SortedDictionary<int, List<LoanStatsDataEntry>> Data { get; set; }
		private SortedDictionary<int, string> CustomerRegions { get; set; }
		private SortedDictionary<int, LoanStatsMarketplaces> CustomerMarketplaces { get; set; }
		private SortedDictionary<int, LoanStatsPaypalTotal> CustomerPaypalTotals { get; set; }

		#endregion fields

		#endregion private
	} // class LoanStats
} // namespace
