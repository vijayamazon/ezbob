﻿namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using OfficeOpenXml;

	public class Bar : AStrategy {
		public Bar(DateTime? startTime, DateTime? endTime) {
			this.spLoad = new SpLoadCashRequestsForBravoAutomationReport(DB, Log);
			this.customerDecisions = new SortedDictionary<int, CustomerDecisions>();

			StartTime = startTime ?? defaultStartTime;
			EndTime = endTime ?? defaultEndTime;

			this.tag = string.Format(
				"#BravoAutoRpt_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", Library.Instance.Culture),
				Guid.NewGuid().ToString("N")
			);

			this.allNonAffirmativeTraces = new SortedSet<string>();
		} // constructor

		public override string Name {
			get { return "Bravo automation report"; }
		} // Name

		public DateTime StartTime {
			get { return this.spLoad.StartTime ?? defaultStartTime; }
			private set { this.spLoad.StartTime = value; }
		} // StartTime

		public DateTime EndTime {
			get { return this.spLoad.EndTime ?? defaultEndTime; }
			private set { this.spLoad.EndTime = value; }
		} // EndTime

		public override void Execute() {
			LoadCashRequests();
			RunAutomation();

			Result = new ExcelPackage();

			CreateResult();

			SaveResult();
		} // Execute

		public ExcelPackage Result { get; private set; }

		private void ProcessManualDecision(SafeReader sr) {
			int customerID = sr["CustomerID"];

			ManualDecision md = sr.Fill<ManualDecision>();

			if (!this.customerDecisions.ContainsKey(customerID))
				this.customerDecisions[customerID] = new CustomerDecisions(customerID, sr["IsAlibaba"], this.tag);

			this.customerDecisions[customerID].ManualDecisions.Add(md);

			this.pc++;
		} // ProcessManualDecision

		private void LoadCashRequests() {
			this.pc = new ProgressCounter("{0} manual decisions processed.", Log, 25);

			spLoad.ForEachRowSafe(ProcessManualDecision);

			this.pc.Log();

			Log.Info("{0} loaded.", Grammar.Number(this.customerDecisions.Count, "customer"));
		} // LoadCashRequests

		private void RunAutomation() {
			this.pc = new ProgressCounter("{0} customers processed.", Log, 10);

			foreach (CustomerDecisions cd in this.customerDecisions.Values) {
				cd.RunAutomation(EndTime, this.allNonAffirmativeTraces);
				this.pc++;
			} // for each

			this.pc.Log();
		} // RunAutomation

		private void CreateResult() {
			Log.Debug("Generating result...");

			ExcelWorksheet sheet = Result.CreateSheet("Raw", false);

			int row = CreateResultHeader(sheet);

			var prc = new ProgressCounter("{0} customers processed.", Log, 25);

			foreach (CustomerDecisions cd in this.customerDecisions.Values) {
				int column = 1;

				column = sheet.SetCellValue(row, column, cd.CustomerID);

				column = sheet.SetCellValue(row, column, cd.Manual.First.DecisionTime);
				column = sheet.SetCellValue(row, column, cd.Manual.First.IsAuto ? "Auto" : "Manual");
				column = sheet.SetCellValue(row, column, cd.Manual.First.ApproveStatus == ApproveStatus.Yes ? "Approve" : "Reject");
				column = sheet.SetCellValue(row, column, cd.Manual.First.AutoDecision);

				column = sheet.SetCellValue(row, column, cd.ManualDecisions.Count);

				column = sheet.SetCellValue(row, column, cd.Manual.Last.DecisionTime);
				column = sheet.SetCellValue(row, column, cd.Manual.Last.IsAuto ? "Auto" : "Manual");
				column = sheet.SetCellValue(row, column, cd.Manual.Last.ApproveStatus == ApproveStatus.Yes ? "Approve" : "Reject");
				column = sheet.SetCellValue(row, column, cd.Manual.Last.AutoDecision);

				column = sheet.SetCellValue(row, column, cd.Auto.First.AutoDecision);
				column = sheet.SetCellValue(row, column, cd.Auto.Last.AutoDecision);
				column = sheet.SetCellValue(row, column, cd.Auto.Current.AutoDecision);

				foreach (string s in this.allNonAffirmativeTraces)
					column = sheet.SetCellValue(row, column, cd.NonAffirmativeTraceResult(s));

				row++;
				prc++;
			} // for each customer

			Result.AutoFitColumns();

			prc.Log();

			Log.Debug("Generating result complete.");
		} // CreateResult

		private int CreateResultHeader(ExcelWorksheet sheet) {
			sheet.SetRowTitles(1, ResultHeaders().ToArray());
			return 2;
		} // CreateResultHeader

		private IEnumerable<string> ResultHeaders() {
			foreach (string s in standardResultHeaders)
				yield return s;

			foreach (string s in this.allNonAffirmativeTraces)
				yield return s;
		} // ResultHeaders

		private string SaveResult() {
			string filePath = Path.Combine(
				Path.GetTempPath(),
				string.Format(
					"bravo-automation-report.{0}.xlsx",
					DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", Library.Instance.Culture)
				)
			);

			Result.SaveAs(new FileInfo(filePath));

			Log.Info("Result has been saved as '{0}'.", filePath);

			return filePath;
		} // SaveResult

		private ProgressCounter pc;

		private readonly SpLoadCashRequestsForBravoAutomationReport spLoad;

		private readonly SortedDictionary<int, CustomerDecisions> customerDecisions;

		private readonly string tag;

		private readonly SortedSet<string> allNonAffirmativeTraces; 

		private static readonly DateTime defaultStartTime = new DateTime(2015, 5, 11, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime defaultEndTime = new DateTime(2015, 5, 28, 0, 0, 0, DateTimeKind.Utc);

		private static readonly string[] standardResultHeaders = {
			"Customer ID",
			"First decision time",
			"First decision source",
			"First decision type",
			"First decision auto decision",
			"Decision count",
			"Last decision time",
			"Last decision source",
			"Last decision type",
			"Last decision auto decision",
			"Auto decision first",
			"Auto decision last",
			"Auto decision current",
		};
	} // class Bar
} // namespace
