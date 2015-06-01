namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using OfficeOpenXml;

	public class Bar : AStrategy {
		public Bar(DateTime? startTime, DateTime? endTime) {
			this.spLoad = new SpLoadCashRequestsForBravoAutomationReport(DB, Log);
			this.customerDecisions = new SortedDictionary<int, CustomerDecisions>();

			StartTime = startTime;
			EndTime = endTime;

			this.tag = string.Format(
				"#BravoAutoRpt_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", Library.Instance.Culture),
				Guid.NewGuid().ToString("N")
			);

			this.now = new DateTime(2015, 6, 1, 0, 0, 0, DateTimeKind.Utc);
		} // constructor

		public override string Name {
			get { return "Bravo automation report"; }
		} // Name

		public DateTime? StartTime {
			get { return this.spLoad.StartTime; }
			private set { this.spLoad.StartTime = value; }
		} // StartTime

		public DateTime? EndTime {
			get { return this.spLoad.EndTime; }
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
				this.customerDecisions[customerID] = new CustomerDecisions(customerID, sr["IsAlibaba"]);

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
				cd.AutoDecisions.Add(RunAutomationOnce(cd.CustomerID, cd.Manual.First.DecisionTime, cd.IsAlibaba));

				if (cd.ManualDecisions.Count > 1)
					cd.AutoDecisions.Add(RunAutomationOnce(cd.CustomerID, cd.Manual.Last.DecisionTime, cd.IsAlibaba));

				cd.CurrentAutoDecision = RunAutomationOnce(cd.CustomerID, this.now, cd.IsAlibaba);

				this.pc++;
			} // for each

			this.pc.Log();
		} // RunAutomation

		private AutoDecision RunAutomationOnce(int customerID, DateTime decisionTime, bool isAlibaba) {
			AutoDecision result = null;

			bool doNext = true;

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// Just for code consistency.
			if (doNext) { // Re-reject area
				if (IsRerejected(customerID, decisionTime)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.ReReject);
				} // if
			} // if; Re-reject area

			if (doNext) {
				if (!isAlibaba) {
					if (IsRejected(customerID, decisionTime)) {
						doNext = false;
						result = new AutoDecision(DecisionActions.Reject);
					} // if
				} // if not alibaba
			} // if; Reject area

			Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal = null;

			if (doNext) { // Medal area
				var instance = new Ezbob.Backend.Strategies.MedalCalculations.CalculateMedal(
					customerID,
					decisionTime,
					false,
					true
				);
				instance.Tag = this.tag;
				instance.Execute();
				medal = instance.Result;
			} // if; Medal area

			int offeredCreditLine = medal == null ? 0 : medal.RoundOfferedAmount();

			if (doNext)
				offeredCreditLine = CapOffer(customerID, offeredCreditLine);

			if (doNext) { // Re-approve area
				if (IsReapproved(customerID, decisionTime)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.ReApprove);
				} // if
			} // if; Re-approve area

			if (doNext) { // Approve area
				if (IsApproved(customerID, offeredCreditLine, medal, decisionTime)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.Approve);
				} // if
			} // if; Approve area

			if (doNext)
				result = new AutoDecision(null);

			return result;
		} // RunAutomationOnce

		private bool IsRerejected(int customerID, DateTime decisionTime) {
			var agent = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(customerID, decisionTime, DB, Log);
			agent.Init();
			agent.MakeDecision();

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsRerejected

		private bool IsRejected(int customerID, DateTime decisionTime) {
			var agent = new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(DB, Log, customerID);

			agent.MakeDecision(agent.GetRejectionInputData(decisionTime));

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsRejected

		private int CapOffer(int customerID, int offeredCreditLine) {
			Log.Debug("Capping offer for customer {0} with uncapped offer {1}.", customerID, offeredCreditLine);

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapHomeOwner);
			} else {
				Log.Info("Capped for not home owner");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapNotHomeOwner);
			} // if

			Log.Debug("Capped offer for customer {0} is {1}.", customerID, offeredCreditLine);

			return offeredCreditLine;
		} // CapOffer

		private bool IsReapproved(int customerID, DateTime decisionTime) {
			var agent = new AutomationCalculator.AutoDecision.AutoReApproval.Agent(DB, Log, customerID, decisionTime);

			agent.MakeDecision(agent.GetInputData());

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsReapproved

		private bool IsApproved(
			int customerID,
			int offeredCreditLine,
			Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal,
			DateTime decisionTime
		) {
			if (medal == null)
				return false;

			var agent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
				customerID,
				offeredCreditLine,
				(AutomationCalculator.Common.Medal)medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				decisionTime,
				DB,
				Log
			);

			agent.Init();

			agent.MakeDecision();

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsApproved

		private void CreateResult() {
			Log.Debug("Generating result...");

			ExcelWorksheet sheet = Result.CreateSheet(Name, false);

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

				row++;
				prc++;
			} // for each customer

			Result.AutoFitColumns();

			prc.Log();

			Log.Debug("Generating result complete.");
		} // CreateResult

		private int CreateResultHeader(ExcelWorksheet sheet) {
			ExcelRange range;

			range = sheet.Cells[1, 1, 2, 1];
			range.Merge = true;
			range.SetCellTitle("Customer ID");

			range = sheet.Cells[1, 2, 1, 5];
			range.Merge = true;
			range.SetCellTitle("First known decision");

			sheet.SetCellTitle(2, 2, "Time");
			sheet.SetCellTitle(2, 3, "Source");
			sheet.SetCellTitle(2, 4, "Type");
			sheet.SetCellTitle(2, 5, "Auto decision");

			range = sheet.Cells[1, 6, 2, 6];
			range.Merge = true;
			range.SetCellTitle("Decision count");

			range = sheet.Cells[1, 7, 1, 10];
			range.Merge = true;
			range.SetCellTitle("Last known decision");

			sheet.SetCellTitle(2, 7, "Time");
			sheet.SetCellTitle(2, 8, "Source");
			sheet.SetCellTitle(2, 9, "Type");
			sheet.SetCellTitle(2, 10, "Auto decision");

			range = sheet.Cells[1, 11, 1, 13];
			range.Merge = true;
			range.SetCellTitle("Auto decision");

			sheet.SetCellTitle(2, 11, "First");
			sheet.SetCellTitle(2, 12, "Last");
			sheet.SetCellTitle(2, 13, "Current");

			return 3;
		} // CreateResultHeader

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

		private static int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }

		private static int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

		private ProgressCounter pc;

		private readonly SpLoadCashRequestsForBravoAutomationReport spLoad;

		private readonly SortedDictionary<int, CustomerDecisions> customerDecisions;

		private readonly string tag;
		private readonly DateTime now;
	} // class Bar
} // namespace
