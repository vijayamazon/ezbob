namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using DbConstants;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using OfficeOpenXml;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		int,
		System.Collections.Generic.List<LoanMetaData>
	>;

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	public class Datum {
		public Datum() {
			Manual = new ManualMedalAndPricing();
			ManualCfg = new SetupFeeConfiguration();
			AutoMin = new AutoMedalAndPricing();
			AutoMax = new AutoMedalAndPricing();
			IsSuperseded = false;
			this.automationDecision = DecisionActions.Waiting;
			IsAutoReRejected = false;
			IsAutoRejected = false;
			IsAutoReApproved = false;
			IsAutoApproved = false;
		} // constructor

		public string Tag { get; set; }

		public int CashRequestID { get; set; }
		public int CustomerID { get; set; }
		public int BrokerID { get; set; }

		public string MedalType {
			get { return Manual.MedalName; }
			set { Manual.MedalName = value; }
		} // MedalType

		public decimal? ScorePoints {
			get { return Manual.EzbobScore; }
			set { Manual.EzbobScore = value; }
		} // ScorePoints

		[FieldName("UnderwriterDecisionDate")]
		public DateTime DecisionTime {
			get { return Manual.DecisionTime; }
			set {
				Manual.DecisionTime = value;
				AutoMin.DecisionTime = value;
				AutoMax.DecisionTime = value;
			} // set
		} // DecisionTime

		public DateTime Now { get; set; }

		[FieldName("UnderwriterDecision")]
		public string Decision {
			get { return Manual.Decision; }
			set { Manual.Decision = value; }
		} // Decision

		public AMedalAndPricing Manual { get; private set; }

		public bool IsDefault { get; set; }
		public bool IsCampaign { get; set; }
		public bool IsSuperseded { get; set; }

		public DecisionActions AutomationDecision {
			get { return this.automationDecision; }
			private set {
				if (this.automationDecision == DecisionActions.Waiting)
					this.automationDecision = value;
			} // set
		} // AutomationDecision

		public bool IsAutoReRejected { get; private set; }
		public bool IsAutoRejected { get; private set; }
		public bool IsAutoReApproved { get; private set; }
		public bool IsAutoApproved { get; private set; }

		public decimal ReapprovedAmount { get; private set; }

		public AMedalAndPricing AutoMin { get; private set; }
		public AMedalAndPricing AutoMax { get; private set; }

		public SetupFeeConfiguration ManualCfg { get; private set; }

		public void RunAutomation(bool isHomeOwner, AConnection db, ASafeLog log) {
			RunAutoRerejection(db, log);

			RunAutoReject(db, log);

			RunAutoReapproval(db, log);

			var instance = new CalculateMedal(CustomerID, DecisionTime, true, false);
			instance.Execute();

			RunAutoApprove(isHomeOwner, instance.Result, db, log);
		} // RunAutomation

		public static string CsvTitles(SortedSet<string> sources) {
			var os = new List<string>();

			foreach (var s in sources) {
				os.Add(string.Format(
					"{0} loan count;{0} worst loan status;{0} issued amount;{0} repaid amount;{0} max late days",
					s
				));
			} // for each

			return string.Join(";",
				"Cash Request ID",
				"Customer ID",
				"Broker ID",
				"Customer is default now",
				"Has default loan",
				"Is campaign",
				"Is superseded",
				AMedalAndPricing.CsvTitles("Manual"),
				"Automation decision",
				"Auto re-reject decision",
				"Auto reject decision",
				"Auto re-approve decision",
				"Auto approve decision",
				"Re-approved amount",
				AMedalAndPricing.CsvTitles("Auto min"),
				"The same max offer",
				AMedalAndPricing.CsvTitles("Auto max"),
				string.Join(";", os)
			);
		} // CsvTitles

		public string ToCsv(TCrLoans crLoans, SortedSet<string> sources) {
			List<LoanMetaData> lst = crLoans.ContainsKey(CashRequestID)
				? crLoans[CashRequestID]
				: new List<LoanMetaData>();

			var bySource = new SortedDictionary<string, LoanSummaryData>();

			foreach (string s in sources)
				bySource[s] = new LoanSummaryData();

			foreach (LoanMetaData lmd in lst)
				bySource[lmd.LoanSourceName].Add(lmd);

			var os = new List<string>();

			HasDefaultLoan = false;

			foreach (string s in sources) {
				LoanSummaryData loanStat = bySource[s];

				if (loanStat.LoanStatus == LoanStatus.Late)
					HasDefaultLoan = true;

				os.Add(loanStat.ToString());
			} // for each

			return string.Join(";",
				CashRequestID.ToString(CultureInfo.InvariantCulture),
				CustomerID.ToString(CultureInfo.InvariantCulture),
				BrokerID.ToString(CultureInfo.InvariantCulture),
				IsDefault ? "Default" : "No",
				HasDefaultLoan ? "Default" : "No",
				IsCampaign ? "Campaign" : "No",
				IsSuperseded ? "Superseded" : "No",
				Manual.ToCsv(),
				AutomationDecision.ToString(),
				IsAutoReRejected ? "Reject" : "Manual",
				IsAutoRejected ? "Reject" : "Manual",
				IsAutoReApproved ? "Approve" : "Manual",
				IsAutoApproved ? "Approve" : "Manual",
				ReapprovedAmount,
				AutoMin.ToCsv(),
				(AutoMax == null) ? "Same" : "No",
				(AutoMax ?? AutoMin).ToCsv(),
				string.Join(";", os)
			);
		} // ToCsv

		public bool HasDefaultLoan { get; private set; }

		public int ToXlsx(ExcelWorksheet sheet, int rowNum, TCrLoans crLoans, SortedSet<string> sources) {
			List<LoanMetaData> lst = crLoans.ContainsKey(CashRequestID)
				? crLoans[CashRequestID]
				: new List<LoanMetaData>();

			var bySource = new SortedDictionary<string, LoanSummaryData>();

			foreach (string s in sources)
				bySource[s] = new LoanSummaryData();

			HasDefaultLoan = false;

			foreach (LoanMetaData lmd in lst) {
				bySource[lmd.LoanSourceName].Add(lmd);

				if (lmd.LoanStatus == LoanStatus.Late)
					HasDefaultLoan = true;
			} // if

			int curColumn = 1;

			curColumn = sheet.SetCellValue(rowNum, curColumn, CashRequestID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, CustomerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, BrokerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsDefault ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, HasDefaultLoan ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsCampaign ? "Campaign" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsSuperseded ? "Superseded" : "No");

			curColumn = Manual.ToXlsx(sheet, rowNum, curColumn);

			curColumn = sheet.SetCellValue(rowNum, curColumn, AutomationDecision.ToString());

			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoReRejected ? "Reject" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoRejected ? "Reject" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoReApproved ? "Approve" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoApproved ? "Approve" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, ReapprovedAmount);

			curColumn = AutoMin.ToXlsx(sheet, rowNum, curColumn);

			curColumn = sheet.SetCellValue(rowNum, curColumn, (AutoMax == null) ? "Same" : "No");

			curColumn = (AutoMax ?? AutoMin).ToXlsx(sheet, rowNum, curColumn);

			foreach (string s in sources) {
				LoanSummaryData loanStat = bySource[s];
				curColumn = loanStat.ToXlsx(sheet, rowNum, curColumn);
			} // for each

			return curColumn;
		} // ToXlsx

		private void RunAutoRerejection(AConnection db, ASafeLog log) {
			var agent = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(
				CustomerID,
				DecisionTime,
				db,
				log
			).Init();

			agent.MakeDecision();

			agent.Trail.Save(db, null, CashRequestID, Tag);

			if (agent.Trail.HasDecided)
				AutomationDecision = DecisionActions.ReReject;

			IsAutoReRejected = agent.Trail.HasDecided;
		} // RunAutoRerejection

		private void RunAutoReject(AConnection db, ASafeLog log) {
			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent agent =
				new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(db, log, CustomerID);

			agent.MakeDecision(agent.GetRejectionInputData(DecisionTime));

			agent.Trail.Save(db, null, CashRequestID, Tag);

			if (agent.Trail.HasDecided)
				AutomationDecision = DecisionActions.Reject;

			IsAutoRejected = agent.Trail.HasDecided;
		} // RunAutoReject

		private void RunAutoReapproval(AConnection db, ASafeLog log) {
			ReapprovedAmount = 0;

			var agent = new
				Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.
				ReApproval.ManAgainstAMachine.SameDataAgent(CustomerID, DecisionTime, db, log);

			agent.Init();

			agent.Decide(CashRequestID, Tag);

			if (agent.Trail.HasDecided) {
				AutomationDecision = DecisionActions.ReApprove;
				ReapprovedAmount = agent.ApprovedAmount;
			} // if

			IsAutoReApproved = agent.Trail.HasDecided;
		} // RunAutoReapproval

		private void RunAutoApprove(
			bool isHomeOwner,
			Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal,
			AConnection db,
			ASafeLog log
		) {
			AutoMin.Calculate(CustomerID, isHomeOwner, medal, true, CashRequestID, Tag, db, log);

			if (AutoMin.Amount > 0)
				AutomationDecision = DecisionActions.Approve;

			IsAutoApproved = AutoMin.Amount > 0;

			if (medal.OfferedAmountsDiffer())
				AutoMax.Calculate(CustomerID, isHomeOwner, medal, false, CashRequestID, Tag, db, log);
			else
				AutoMax = null;
		} // RunAutoApprove

		private DecisionActions automationDecision;
	} // class Datum
} // namespace
