namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using AutomationCalculator.ProcessHistory;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	internal class Digger {
		public Digger() {
			Result = new SortedDictionary<int, DeepData>();
		} // constructor

		public void Execute() {
			LoadCashRequests();
			LoadMarketplacesData();
			LoadConsumerScores();
			LoadPersonalLates();

			CreateXlsx();
			SaveXlsx();
		} // Execute

		public SortedDictionary<int, DeepData> Result { get; private set; }

		public ExcelPackage Xlsx { get; private set; }

		private void LoadCashRequests() {
			var pc = new ProgressCounter("{0} raw lines processed.", Log, 100);

			DB.ForEachResult<RawSource>(
				rs => {
					if (Result.ContainsKey(rs.CustomerID))
						Result[rs.CustomerID].Add(rs);
					else
						Result[rs.CustomerID] = new DeepData(rs);

					pc.Next();
				},
				RawSource.SpName,
				CommandSpecies.StoredProcedure
			);

			pc.Log();
		} // LoadCashRequests

		private void LoadPersonalLates() {
			var pc = new ProgressCounter("{0} CAIS accounts loaded.", Log, 20);

			DB.ForEachResult<CaisAccount>(
				ca => {
					if (Result.ContainsKey(ca.CustomerID))
						Result[ca.CustomerID].CaisAccounts.Add(ca);
					else
						Log.Warn("Customer {0} not found for CAIS account.", ca.CustomerID);

					pc.Next();
				},
				CaisAccount.SpName,
				CommandSpecies.StoredProcedure
			);

			pc.Log();
		} // LoadPersonalLates

		private void LoadConsumerScores() {
			var pc = new ProgressCounter("{0} consumer score items loaded.", Log, 20);

			DB.ForEachRowSafe(
				sr => {
					int customerID = sr["CustomerID"];
					int score = sr["ConsumerScore"];

					if (Result.ContainsKey(customerID))
						Result[customerID].ConsumerScore = score;
					else
						Log.Warn("Customer {0} not found for consumer score.", customerID);

					pc.Next();
				},
				"BAR_LoadConsumerScores",
				CommandSpecies.StoredProcedure
			);

			pc.Log();
		} // LoadConsumerScores

		private void LoadMarketplacesData() {
			var pc = new ProgressCounter("{0} marketplaces processed.", Log, 20);

			foreach (var dd in Result.Values)
				dd.LoadMonthTurnover(pc);

			pc.Log();

			Log.Info("All standard reject reason count: {0}.", DeepData.AllStandardRejectReasons.Count);
			Log.Info("All non-affirmative traces count: {0}.", DeepData.AllNonAffirmativeTraces.Count);

			Log.Info("Max marketplace count: {0}.", Result.Values.Select(dd => dd.Marketplaces.Count).Max());
			Log.Info("Max standard reject reason count: {0}.", Result.Values.Select(dd => dd.StandardRejectReasons.Count).Max());
			Log.Info("Max non-affirmative traces count: {0}.", Result.Values.Select(dd => dd.NonAffirmativeTraces.Count).Max());
		} // LoadMarketplacesData

		private void CreateXlsx() {
			Xlsx = new ExcelPackage();

			CreateXlsxRejectReasonsSheet();
			CreateXlsxMarketplacesSheet();
			CreateXlsxNotApprovedReasonsSheet();
			CreateXlsxLateAccountsSheet();

			Xlsx.AutoFitColumns();
		} // CreateXlsx

		private void CreateXlsxRejectReasonsSheet() {
			ExcelWorksheet sheet = Xlsx.CreateSheet("Reject reasons", false);

			int firstDataColumn = FillCommonHeaders(sheet);

			int column = firstDataColumn;

			column = sheet.SetCellTitle(1, column, "UW note");

			foreach (string reason in DeepData.AllStandardRejectReasons)
				column = sheet.SetCellTitle(1, column, reason);

			int row = 2;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd);

				column = firstDataColumn;

				column = sheet.SetCellValue(row, column, dd.ManualRejectReason);

				foreach (string reason in DeepData.AllStandardRejectReasons)
					column = sheet.SetCellValue(row, column, dd.StandardRejectReasons.Contains(reason) ? "yes" : "no");

				row++;
			} // for each customer
		} // CreateXlsxRejectReasonsSheet

		private void CreateXlsxMarketplacesSheet() {
			ExcelWorksheet sheet = Xlsx.CreateSheet("Marketplaces", false);

			int firstDataColumn = FillCommonHeaders(sheet);

			int column = firstDataColumn;

			column = sheet.SetCellTitle(1, column, "Mp ID");
			column = sheet.SetCellTitle(1, column, "Mp type");
			column = sheet.SetCellTitle(1, column, "Last month");

			for (int i = 0; i < 12; i++)
				column = sheet.SetCellTitle(1, column, i);

			int row = 2;

			Color rowColour = ExcelRangeExt.ZebraOddBgColour;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd, dd.Marketplaces.Count, rowColour);

				if (dd.Marketplaces.Count < 1) {
					column = firstDataColumn;

					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);

					for (int i = 0; i < 12; i++)
						column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);

					row++;
				} else {
					foreach (MarketplaceData mp in dd.Marketplaces.Values) {
						column = firstDataColumn;

						column = sheet.SetCellValue(row, column, mp.ID, bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetCellValue(row, column, mp.Type.Name, bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetCellValue(row, column, mp.TotalsMonth.ToString("MMM-yyyy"), bSetZebra: false, oBgColour: rowColour);

						for (int i = 0; i < 12; i++)
							column = sheet.SetCellValue(row, column, mp.GetTurnover(i), bSetZebra: false, oBgColour: rowColour, sNumberFormat: "£#,##0.00");

						row++;
					} // for each marketplace
				} // if

				rowColour = rowColour == ExcelRangeExt.ZebraOddBgColour
					? ExcelRangeExt.ZebraEvenBgColour
					: ExcelRangeExt.ZebraOddBgColour;
			} // for each customer
		} // CreateXlsxMarketplacesSheet

		private void CreateXlsxNotApprovedReasonsSheet() {
			ExcelWorksheet sheet = Xlsx.CreateSheet("Not approved reasons", false);

			int firstDataColumn = FillCommonHeaders(sheet);

			int column = firstDataColumn;

			foreach (string reason in DeepData.AllNonAffirmativeTraces)
				column = sheet.SetCellTitle(1, column, reason);

			int row = 2;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd);

				column = firstDataColumn;

				foreach (string reason in DeepData.AllNonAffirmativeTraces) {
					string display = (dd.AutoApproved == DecisionStatus.Negative)
						? (dd.NonAffirmativeTraces.Contains(reason) ? "yes" : "no")
						: null;

					column = sheet.SetCellValue(row, column, display);
				} // for each

				row++;
			} // for each customer
		} // CreateXlsxNotApprovedReasonsSheet

		private void CreateXlsxLateAccountsSheet() {
			ExcelWorksheet sheet = Xlsx.CreateSheet("Late accounts", false);

			int firstDataColumn = FillCommonHeaders(sheet);

			int column = firstDataColumn;

			column = sheet.SetCellTitle(1, column, "Total balance");
			column = sheet.SetCellTitle(1, column, "Max balance");
			column = sheet.SetCellTitle(1, column, "Total count");

			column = sheet.SetCellTitle(1, column, "Late for approve count");
			column = sheet.SetCellTitle(1, column, "Total for reject count");
			column = sheet.SetCellTitle(1, column, "Late for reject count");

			column = sheet.SetCellTitle(1, column, "For reject");
			column = sheet.SetCellTitle(1, column, "Last update date");
			column = sheet.SetCellTitle(1, column, "Worst status");
			column = sheet.SetCellTitle(1, column, "Account codes");

			int row = 2;

			Color rowColour = ExcelRangeExt.ZebraOddBgColour;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd, dd.CaisAccounts.Count, rowColour);

				column = firstDataColumn;

				if (dd.CaisAccounts.Count < 1) {
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);

					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);

					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetCellValue(row, column, null, bSetZebra: false, oBgColour: rowColour);

					row++;
				} else {
					var values = new List<object> {
						dd.CaisAccounts.Balance.Total,
						dd.CaisAccounts.Balance.Max,
						dd.CaisAccounts.Count.Total,

						dd.CaisAccounts.Count.LateForApprove,
						dd.CaisAccounts.Count.ForReject,
						dd.CaisAccounts.Count.LateForReject,
					};

					for (int i = 0; i < values.Count; i++) {
						var range = sheet.Cells[row, column, row + dd.CaisAccounts.Count - 1, column];
						range.Merge = true;

						column = sheet.SetCellValue(row, column, values[i], bSetZebra: false);

						range.Style.Fill.PatternType = ExcelFillStyle.Solid;
						range.Style.Fill.BackgroundColor.SetColor(rowColour);
						range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					} // for

					int firstAccountColumn = column;

					foreach (CaisAccount ca in dd.CaisAccounts) {
						column = firstAccountColumn;

						column = sheet.SetCellValue(row, column, ca.IsForReject ? "yes" : "no", bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetCellValue(row, column, ca.LastUpdatedDate.ToString("d-MMM-yyyy"), bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetCellValue(row, column, ca.WorstStatus, bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetCellValue(row, column, ca.AccountStatusCodes.Replace(" ", "-"), bSetZebra: false, oBgColour: rowColour);

						row++;
					} // for each marketplace
				} // if

				rowColour = rowColour == ExcelRangeExt.ZebraOddBgColour
					? ExcelRangeExt.ZebraEvenBgColour
					: ExcelRangeExt.ZebraOddBgColour;
			} // for each customer
		} // CreateXlsxLateAccountsSheet

		private static int FillCommonHeaders(ExcelWorksheet sheet) {
			const int row = 1;
			int column = 1;

			column = sheet.SetCellTitle(row, column, "Customer ID");
			column = sheet.SetCellTitle(row, column, "Cash request ID");
			column = sheet.SetCellTitle(row, column, "Underwriter");
			column = sheet.SetCellTitle(row, column, "Decision time");
			column = sheet.SetCellTitle(row, column, "Consumer score");
			column = sheet.SetCellTitle(row, column, "Decision");
			column = sheet.SetCellTitle(row, column, "Auto approve");

			return column;
		} // FillCommonHeaders

		private void FillCommonValues(
			ExcelWorksheet sheet,
			int row,
			DeepData dd,
			int rowSpan = 1,
			Color? bgColour = null
		) {
			int column = 1;

			bool zebra = bgColour == null;

			var values = new List<object> {
				dd.CustomerID,
				dd.CashRequestID,
				dd.UnderwriterName,
				dd.DecisionTime,
				dd.ConsumerScore,
				dd.ManualDecision.ToString(),
				dd.AutoApproved.HasValue ? dd.AutoApproved.ToString() : null,
			};

			for (int i = 0; i < values.Count; i++) {
				if (rowSpan <= 1) {
					column = sheet.SetCellValue(row, column, values[i], bSetZebra: zebra, oBgColour: bgColour);
					continue;
				} // if

				var range = sheet.Cells[row, column, row + rowSpan - 1, column];
				range.Merge = true;

				column = sheet.SetCellValue(row, column, values[i], bSetZebra: false);

				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(bgColour ?? Color.White);
				range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
			} // for
		} // FillCommonValues

		private string SaveXlsx() {
			string filePath = Path.Combine(
				Path.GetTempPath(),
				string.Format(
					"bravo-automation-report.manual.no-sign.enough-data.{0}.xlsx",
					DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", Library.Instance.Culture)
				)
			);

			Xlsx.SaveAs(new FileInfo(filePath));

			Log.Info("Result has been saved as '{0}'.", filePath);

			return filePath;
		} // SaveXlsx

		private AConnection DB { get { return Library.Instance.DB; } }
		private ASafeLog Log { get { return Library.Instance.Log; } }
	} // class Digger
} // namespace
