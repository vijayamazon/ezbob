namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.Strategies.Extensions;
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

		public AMedalAndPricing AutoMaxOrMin { get { return AutoMax ?? AutoMin; } } // AutoMaxOrMin

		public SetupFeeConfiguration ManualCfg { get; private set; }

		public void RunAutomation(bool isHomeOwner, AConnection db, ASafeLog log) {
			log.Info(
				"RunAutomation({0}) started for customer {1} with decision time '{2}'...",
				isHomeOwner,
				CustomerID,
				DecisionTime.MomentStr()
			);

			RunAutoRerejection(db, log);

			RunAutoReject(db, log);

			RunAutoReapproval(db, log);

			MedalResult medal = RunCalculateMedal(log);

			RunAutoApprove(isHomeOwner, medal, db, log);

			log.Info(
				"RunAutomation({0}) complete for customer {1} with decision time '{2}'.",
				isHomeOwner,
				CustomerID,
				DecisionTime.MomentStr()
			);
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
				"Loan was default",
				"Is campaign",
				"Is superseded",
				"Decision time",
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

		public int CustomerLoanCount {
			get { return Manual.LoanCount; }
			private set {
				Manual.LoanCount = value;
				AutoMin.LoanCount = value;

				if (AutoMax != null)
					AutoMax.LoanCount = value;
			} // set
		} // CustomerLoanCount

		public int LoanCount { get; private set; }
		public decimal LoanAmount { get; private set; }

		public bool HasDefaultLoan { get { return DefaultLoanCount > 0; } }
		public int DefaultLoanCount { get; private set; }
		public decimal DefaultLoanAmount { get; private set; }

		public bool HasBadLoan { get { return BadLoanCount > 0; } }
		public int BadLoanCount { get; private set; }
		public decimal BadLoanAmount { get; private set; }

		public void SetCustomerLoanCount(TCrLoans customerLoans) {
			List<LoanMetaData> lst = customerLoans.ContainsKey(CustomerID)
				? customerLoans[CustomerID]
				: new List<LoanMetaData>();

			CustomerLoanCount = lst.Count(lmd =>
				(lmd.LoanDate < DecisionTime) &&
				((lmd.DateClosed == null) || (lmd.DateClosed > DecisionTime))
			);
		} // SetCustomerLoanCount

		public int ToXlsx(ExcelWorksheet sheet, int rowNum, TCrLoans crLoans, SortedSet<string> sources) {
			List<LoanMetaData> lst = crLoans.ContainsKey(CashRequestID)
				? crLoans[CashRequestID]
				: new List<LoanMetaData>();

			var bySource = new SortedDictionary<string, LoanSummaryData>();

			foreach (string s in sources)
				bySource[s] = new LoanSummaryData();

			foreach (LoanMetaData lmd in lst)

			LoanCount = 0;
			LoanAmount = 0;

			DefaultLoanCount = 0;
			DefaultLoanAmount = 0;

			BadLoanCount = 0;
			BadLoanAmount = 0;

			foreach (LoanMetaData lmd in lst) {
				bySource[lmd.LoanSourceName].Add(lmd);

				LoanCount++;
				LoanAmount += lmd.LoanAmount;

				if (lmd.LoanStatus == LoanStatus.Late) {
					DefaultLoanCount++;
					DefaultLoanAmount += lmd.LoanAmount;
				} // if

				if (lmd.MaxLateDays > 13) {
					BadLoanCount++;
					BadLoanAmount += lmd.LoanAmount;
				} // if
			} // for

			int curColumn = 1;

			curColumn = sheet.SetCellValue(rowNum, curColumn, CashRequestID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, CustomerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, BrokerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsDefault ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, HasDefaultLoan ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, HasBadLoan ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsCampaign ? "Campaign" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsSuperseded ? "Superseded" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, DecisionTime.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

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
			log.Info(
				"RunAutomation-RunAutoRerejection() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

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

			log.Info(
				"RunAutomation-RunAutoRerejection() complete for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto re-rejected is '{3}'.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoReRejected
			);
		} // RunAutoRerejection

		private void RunAutoReject(AConnection db, ASafeLog log) {
			log.Info(
				"RunAutomation-RunAutoReject() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent agent =
				new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(db, log, CustomerID);

			agent.MakeDecision(agent.GetRejectionInputData(DecisionTime));

			agent.Trail.Save(db, null, CashRequestID, Tag);

			if (agent.Trail.HasDecided)
				AutomationDecision = DecisionActions.Reject;

			IsAutoRejected = agent.Trail.HasDecided;

			log.Info(
				"RunAutomation-RunAutoReject() complete for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto rejected is '{3}'.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoRejected
			);
		} // RunAutoReject

		private void RunAutoReapproval(AConnection db, ASafeLog log) {
			log.Info(
				"RunAutomation-RunAutoReapproval() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

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

			log.Info(
				"RunAutomation-RunAutoReapproval() complete for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto re-approved is '{3}', re-approved amount is {4}.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoReApproved,
				ReapprovedAmount
			);
		} // RunAutoReapproval

		private MedalResult RunCalculateMedal(ASafeLog log) {
			log.Info(
				"RunAutomation-CalculateMedal() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			CalculateMedal instance = new CalculateMedal(CustomerID, DecisionTime, true, false);
			instance.Execute();

			log.Info(
				"RunAutomation-CalculateMedal() complete for customer {0} with decision time '{1}': " +
				"medal is '{2}', offered amount is {3}, max offered amount is {4}.",
				CustomerID,
				DecisionTime.MomentStr(),
				instance.Result.MedalClassification,
				instance.Result.RoundOfferedAmount(),
				instance.Result.RoundMaxOfferedAmount()
			);

			return instance.Result;
		} // RunCalculateMedal

		private void RunAutoApprove(
			bool isHomeOwner,
			Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal,
			AConnection db,
			ASafeLog log
		) {
			log.Info(
				"RunAutomation-RunAutoApprove() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			AutoMin.Calculate(CustomerID, isHomeOwner, medal, true, CashRequestID, Tag, db, log);

			if (AutoMin.Amount > 0)
				AutomationDecision = DecisionActions.Approve;

			IsAutoApproved = AutoMin.Amount > 0;

			log.Info(
				"RunAutomation-RunAutoApprove()-AutoMin done for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto approved is '{3}', approved amount is {4}.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoApproved,
				AutoMin.Amount
			);

			if (medal.OfferedAmountsDiffer()) {
				AutoMax.Calculate(CustomerID, isHomeOwner, medal, false, CashRequestID, Tag, db, log);

				log.Info(
					"RunAutomation-RunAutoApprove()-AutoMax done for customer {0} with decision time '{1}': " +
					"approved amount is {2}.",
					CustomerID,
					DecisionTime.MomentStr(),
					AutoMax.Amount
				);
			} else {
				AutoMax = null;

				log.Info(
					"RunAutomation-RunAutoApprove()-AutoMax not done for customer {0} with decision time '{1}': " +
					"same min and max amounts.",
					CustomerID,
					DecisionTime.MomentStr()
				);
			} // if

			log.Info(
				"RunAutomation-RunAutoApprove() complete for customer {0} with decision time '{1}'.",
				CustomerID,
				DecisionTime.MomentStr()
			);
		} // RunAutoApprove

		private DecisionActions automationDecision;
	} // class Datum
} // namespace
