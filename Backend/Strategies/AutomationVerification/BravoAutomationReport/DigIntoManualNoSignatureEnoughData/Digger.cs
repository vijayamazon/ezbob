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

	public class Digger {
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

		protected virtual void LoadPersonalLates() {
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

		protected AConnection DB { get { return Library.Instance.DB; } }
		protected ASafeLog Log { get { return Library.Instance.Log; } }

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

			column = sheet.SetBorder(1, column).SetCellTitle("UW note");

			foreach (string reason in DeepData.AllStandardRejectReasons)
				column = sheet.SetBorder(1, column).SetCellTitle(reason);

			int row = 2;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd);

				column = firstDataColumn;

				column = sheet.SetBorder(row, column).SetCellValue(dd.ManualRejectReason);

				foreach (string reason in DeepData.AllStandardRejectReasons)
					column = sheet.SetBorder(row, column).SetCellValue(dd.StandardRejectReasons.Contains(reason) ? "yes" : "no");

				row++;
			} // for each customer
		} // CreateXlsxRejectReasonsSheet

		private void CreateXlsxMarketplacesSheet() {
			ExcelWorksheet sheet = Xlsx.CreateSheet("Marketplaces", false);

			int firstDataColumn = FillCommonHeaders(sheet);

			int column = firstDataColumn;

			column = sheet.SetBorder(1, column).SetCellTitle("Mp ID");
			column = sheet.SetBorder(1, column).SetCellTitle("Mp type");
			column = sheet.SetBorder(1, column).SetCellTitle("Last month");

			for (int i = 0; i < 12; i++)
				column = sheet.SetBorder(1, column).SetCellTitle(i);

			int row = 2;

			Color rowColour = ExcelRangeExt.ZebraOddBgColour;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd, dd.Marketplaces.Count, rowColour);

				if (dd.Marketplaces.Count < 1) {
					column = firstDataColumn;

					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);

					for (int i = 0; i < 12; i++)
						column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);

					row++;
				} else {
					foreach (MarketplaceData mp in dd.Marketplaces.Values) {
						column = firstDataColumn;

						column = sheet.SetBorder(row, column).SetCellValue(mp.ID, bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetBorder(row, column).SetCellValue(mp.Type.Name, bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetBorder(row, column).SetCellValue(mp.TotalsMonth.ToString("MMM-yyyy"), bSetZebra: false, oBgColour: rowColour);

						for (int i = 0; i < 12; i++)
							column = sheet.SetBorder(row, column).SetCellValue(mp.GetTurnover(i), bSetZebra: false, oBgColour: rowColour, sNumberFormat: "£#,##0.00");

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
				column = sheet.SetBorder(1, column).SetCellTitle(reason);

			int row = 2;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd);

				column = firstDataColumn;

				foreach (string reason in DeepData.AllNonAffirmativeTraces) {
					string display = (dd.AutoApproved == DecisionStatus.Negative)
						? (dd.NonAffirmativeTraces.Contains(reason) ? "yes" : "no")
						: null;

					column = sheet.SetBorder(row, column).SetCellValue(display);
				} // for each

				row++;
			} // for each customer
		} // CreateXlsxNotApprovedReasonsSheet

		private void CreateXlsxLateAccountsSheet() {
			ExcelWorksheet sheet = Xlsx.CreateSheet("Late and default accounts", false);

			int firstDataColumn = FillCommonHeaders(sheet);

			int column = firstDataColumn;

			column = sheet.SetBorder(1, column).SetCellTitle("Total balance");
			column = sheet.SetBorder(1, column).SetCellTitle("Max balance");
			column = sheet.SetBorder(1, column).SetCellTitle("Total count");
			column = sheet.SetBorder(1, column).SetCellTitle("Late for approve count");
			column = sheet.SetBorder(1, column).SetCellTitle("Total for reject count");
			column = sheet.SetBorder(1, column).SetCellTitle("Late for reject count");
			column = sheet.SetBorder(1, column).SetCellTitle("Personal default count");
			column = sheet.SetBorder(1, column).SetCellTitle("Late for approve");
			column = sheet.SetBorder(1, column).SetCellTitle("For reject");
			column = sheet.SetBorder(1, column).SetCellTitle("Late for reject");
			column = sheet.SetBorder(1, column).SetCellTitle("Personal default");
			column = sheet.SetBorder(1, column).SetCellTitle("Last update date");
			column = sheet.SetBorder(1, column).SetCellTitle("Worst status");
			column = sheet.SetBorder(1, column).SetCellTitle("Account codes");

			int row = 2;

			Color rowColour = ExcelRangeExt.ZebraOddBgColour;

			foreach (DeepData dd in Result.Values) {
				FillCommonValues(sheet, row, dd, dd.CaisAccounts.Count, rowColour);

				column = firstDataColumn;

				if (dd.CaisAccounts.Count < 1) {
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);

					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);

					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);

					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);
					column = sheet.SetBorder(row, column).SetCellValue(null, bSetZebra: false, oBgColour: rowColour);

					row++;
				} else {
					var values = new List<object> {
						dd.CaisAccounts.Balance.Total,
						dd.CaisAccounts.Balance.Max,
						dd.CaisAccounts.Count.Total,

						dd.CaisAccounts.Count.LateForApprove,
						dd.CaisAccounts.Count.ForReject,
						dd.CaisAccounts.Count.LateForReject,
						dd.CaisAccounts.Count.PersonalDefault,
					};

					for (int i = 0; i < values.Count; i++) {
						var range = sheet.Cells[row, column, row + dd.CaisAccounts.Count - 1, column];
						range.Merge = true;

						column = range.SetBorder().SetCellValue(values[i], bSetZebra: false);

						range.Style.Fill.PatternType = ExcelFillStyle.Solid;
						range.Style.Fill.BackgroundColor.SetColor(rowColour);
						range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					} // for

					int firstAccountColumn = column;

					foreach (CaisAccount ca in dd.CaisAccounts) {
						column = firstAccountColumn;

						column = sheet.SetBorder(row, column).SetCellValue(ca.IsLateForApprove ? "yes" : "no", bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetBorder(row, column).SetCellValue(ca.IsForReject ? "yes" : "no", bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetBorder(row, column).SetCellValue(ca.IsLateForReject ? "yes" : "no", bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetBorder(row, column).SetCellValue(ca.IsPersonalDefault ? "yes" : "no", bSetZebra: false, oBgColour: rowColour);

						column = sheet.SetBorder(row, column).SetCellValue(ca.LastUpdatedDate.ToString("d-MMM-yyyy"), bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetBorder(row, column).SetCellValue(ca.WorstStatus, bSetZebra: false, oBgColour: rowColour);
						column = sheet.SetBorder(row, column).SetCellValue(ca.AccountStatusCodes.Replace(' ', '-'), bSetZebra: false, oBgColour: rowColour);

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

			column = sheet.SetBorder(row, column).SetCellTitle("Customer ID");
			column = sheet.SetBorder(row, column).SetCellTitle("Is broker");
			column = sheet.SetBorder(row, column).SetCellTitle("Cash request ID");
			column = sheet.SetBorder(row, column).SetCellTitle("Underwriter");
			column = sheet.SetBorder(row, column).SetCellTitle("Decision time");
			column = sheet.SetBorder(row, column).SetCellTitle("Consumer score");
			column = sheet.SetBorder(row, column).SetCellTitle("Decision");
			column = sheet.SetBorder(row, column).SetCellTitle("Auto approve");

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
				dd.IsBroker,
				dd.CashRequestID,
				dd.UnderwriterName,
				dd.DecisionTime,
				dd.ConsumerScore,
				dd.ManualDecision.ToString(),
				dd.AutoApproved.HasValue ? dd.AutoApproved.ToString() : null,
			};

			for (int i = 0; i < values.Count; i++) {
				if (rowSpan <= 1) {
					column = sheet.SetBorder(row, column).SetCellValue(values[i], bSetZebra: zebra, oBgColour: bgColour);
					continue;
				} // if

				var range = sheet.Cells[row, column, row + rowSpan - 1, column];
				range.Merge = true;

				column = range.SetBorder().SetCellValue(values[i], bSetZebra: false);

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
	} // class Digger
} // namespace
