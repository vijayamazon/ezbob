namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory.Trails.ApprovalInput;
	using DbConstants;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		long,
		System.Collections.Generic.List<LoanMetaData>
	>;

	public class AutoDatumItem : ADatumItem {
		public AutoDatumItem(
			SpLoadCashRequestsForAutomationReport.ResultRow sr,
			string tag,
			AConnection db,
			ASafeLog log
		) : base(tag, log.Safe()) {
			this.db = db;

			MinLoanCount = new LoanCount(true, log.Safe());

			MaxOffer = new MaxOfferResult(log.Safe()) {
				ApprovedAmount = 0,
				RepaymentPeriod = 0,
				InterestRate = 0,
				SetupFeeAmount = 0,
				SetupFeePct = 0,
			};

			this.automationDecision = DecisionActions.Waiting;

			IsAutoRejected = false;
			IsApproved = false;

			OutstandingPrincipalOnDecisionDate = 0;

			CustomerID = sr.CustomerID;
			DecisionTime = sr.DecisionTime;
			PreviousLoanCount = sr.PreviousLoanCount;
			CashRequestID = sr.CashRequestID;
		} // constructor

		public LoanCount MinLoanCount { get; private set; }

		public override string DecisionStr {
			get {
				return AutomationDecision == DecisionActions.Waiting ? "Manual" : AutomationDecision.ToString();
			} // get
		} // DecisionStr

		public bool HasDecided {
			get { return AutomationDecision != DecisionActions.Waiting; }
		} // HasDecided

		public DecisionActions AutomationDecision {
			get { return this.automationDecision; }
			private set {
				if (this.automationDecision == DecisionActions.Waiting)
					this.automationDecision = value;
			} // set
		} // AutomationDecision

		public class OfferResult {
			public OfferResult() {
				ScenarioName = string.Empty;
				ApprovedAmount = 0;
				RepaymentPeriod = 0;
				InterestRate = 0;
				SetupFeePct = 0;
				SetupFeeAmount = 0;
			} // constructor

			public string ScenarioName { get; set; }
			public int ApprovedAmount { get; set; }
			public int RepaymentPeriod { get; set; }
			public decimal InterestRate { get; set; }
			public decimal SetupFeePct { get; set; }
			public decimal SetupFeeAmount { get; set; }
		} // class OfferResult

		public class MaxOfferResult : OfferResult {
			public MaxOfferResult(ASafeLog log) {
				LoanCount = new LoanCount(false, log.Safe());
			} // constructor

			public LoanCount LoanCount { get; set; }
		} // class MaxOfferResult

		public MaxOfferResult MaxOffer { get; private set; }

		public bool IsAutoRejected { get; private set; }

		public bool IsAutoReRejected { get; private set; }
		public bool IsAutoReApproved { get; private set; }

		public decimal OutstandingPrincipalOnDecisionDate { get; private set; }

		public static new string CsvTitles(string prefix) {
			prefix += " auto";

			return string.Format(
				"{1} decision;{1} reject decision;{1} approve decision;" +
				"{0};{1} turnover type; {1} min medal amount; {1} max medal amount",
				ADatumItem.CsvTitles(prefix),
				prefix
			);
		} // CsvTitles

		public override int ToXlsx(ExcelWorksheet sheet, int rowNum, int colNum) {
			colNum = sheet.SetCellValue(rowNum, colNum, AutomationDecision.ToString());
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoRejected ? "Reject" : "Manual");
			colNum = sheet.SetCellValue(rowNum, colNum, IsApproved ? "Approve" : "Manual");

			colNum = base.ToXlsx(sheet, rowNum, colNum);

			var medalRes = this.medal ?? new MedalResult(CustomerID, Log);

			colNum = sheet.SetCellValue(
				rowNum,
				colNum,
				medalRes.TurnoverType.HasValue ? medalRes.TurnoverType.ToString() : "N/A"
			);
			colNum = sheet.SetCellValue(rowNum, colNum, medalRes.RoundOfferedAmount());
			colNum = sheet.SetCellValue(rowNum, colNum, medalRes.RoundMaxOfferedAmount());

			return colNum;
		} // ToXlsx

		public void SetAdjustedLoanCount(LoanCount manualLoanCount, int manuallyApprovedAmount) {
			MinLoanCount = AdjustLoanCount(
				manualLoanCount,
				manuallyApprovedAmount,
				ApprovedAmount,
				(lc, ratio) => lc.SetMinRatio(ratio)
			);

			MaxOffer.LoanCount = AdjustLoanCount(
				manualLoanCount,
				manuallyApprovedAmount,
				MaxOffer.ApprovedAmount,
				(lc, ratio) => lc.SetMaxRatio(ratio)
			);
		} // SetAdjustedLoanCount

		public bool IsHomeOwner { get; set; }
		public string PricingCalculatorScenarioName { get; private set; }

		public void RunAutomation(SortedDictionary<long, AutomationTrails> automationTrails) {
			ApprovedAmount = 0;
			RepaymentPeriod = 0;
			InterestRate = 0;
			SetupFeeAmount = 0;
			SetupFeePct = 0;

			var atra = new AutomationTrails();

			automationTrails[CashRequestID] = atra;

			RunAutoReject(atra);

			this.medal = RunCalculateMedal();

			RunAutoApprove(atra);

			atra.AutomationDecision = AutomationDecision;

			RunAutoRerejection();
			RunAutoReapproval();
		} // RunAutomation

		private void RunAutoRerejection() {
			var agent = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(
				CustomerID,
				CashRequestID,
				NLCashRequestID,
				DecisionTime,
				this.db,
				Log
			).Init();

			agent.MakeDecision();

			agent.Trail.SetTag(Tag).Save(this.db, null);

			IsAutoReRejected = agent.Trail.HasDecided;
		} // RunAutoRerejection

		private void RunAutoReapproval() {
			var agent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval.ManAgainstAMachine.
				SameDataAgent(
					CustomerID,
					CashRequestID,
					NLCashRequestID,
					DecisionTime,
					Tag,
					this.db,
					Log
				);

			agent.Init();

			agent.Decide();

			IsAutoReApproved = agent.AffirmativeDecisionMade;
		} // RunAutoReapproval

		private void RunAutoReject(AutomationTrails atra) {
			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent agent =
				new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(
					this.db,
					Log,
					CustomerID,
					CashRequestID,
					NLCashRequestID
				);

			agent.MakeDecision(agent.GetRejectionInputData(DecisionTime));

			agent.Trail.SetTag(Tag).Save(this.db, null);

			atra.Rejection = agent.Trail;

			if (agent.Trail.HasDecided)
				AutomationDecision = DecisionActions.Reject;

			IsAutoRejected = agent.Trail.HasDecided;
		} // RunAutoReject

		private MedalResult RunCalculateMedal() {
			CalculateMedal instance = new CalculateMedal(
				CustomerID,
				CashRequestID,
				NLCashRequestID,
				DecisionTime,
				true,
				true
			) { Tag = Tag, };
			instance.Execute();
			return instance.Result;
		} // RunCalculateMedal

		private void RunAutoApprove(AutomationTrails atra) {
			IsApproved = false;

			MedalName = this.medal.MedalClassification.ToString();

			EzbobScore = this.medal.TotalScoreNormalized;

			var approveAgent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
				CustomerID,
				CashRequestID,
				NLCashRequestID,
				120000, // this is currently max loan amount
				(AutomationCalculator.Common.Medal)this.medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)this.medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)this.medal.TurnoverType,
				DecisionTime, this.db,
				Log
			).Init();

			approveAgent.MakeDecision();

			approveAgent.Trail.SetTag(Tag).Save(this.db, null);

			atra.Approval = approveAgent.Trail;

			// Currently we don't check this logic hence 0.
			OutstandingPrincipalOnDecisionDate = 0; // approveAgent.Trail.MyInputData.MetaData.OutstandingPrincipal;

			if (!approveAgent.Trail.HasDecided)
				return;

			OfferResult minOffer = CalculateOffer(this.medal.RoundOfferedAmount(), approveAgent.Trail.MyInputData);

			// Currently we check "what if" scenario and not "how automation exactly works" scenario.
			// if (minOffer.ApprovedAmount <= 0)
				// return;

			// Due to previous comment there can be approve with amount of 0.

			IsApproved = true;
			AutomationDecision = DecisionActions.Approve;

			PricingCalculatorScenarioName = minOffer.ScenarioName;
			ApprovedAmount = minOffer.ApprovedAmount;
			RepaymentPeriod = minOffer.RepaymentPeriod;
			InterestRate = minOffer.InterestRate;
			SetupFeePct = minOffer.SetupFeePct;
			SetupFeeAmount = minOffer.SetupFeeAmount;

			OfferResult maxOffer = this.medal.OfferedAmountsDiffer()
				? CalculateOffer(this.medal.RoundMaxOfferedAmount(), approveAgent.Trail.MyInputData)
				: minOffer;

			MaxOffer.ScenarioName = maxOffer.ScenarioName;
			MaxOffer.ApprovedAmount = maxOffer.ApprovedAmount;
			MaxOffer.RepaymentPeriod = maxOffer.RepaymentPeriod;
			MaxOffer.InterestRate = maxOffer.InterestRate;
			MaxOffer.SetupFeeAmount = maxOffer.SetupFeeAmount;
			MaxOffer.SetupFeePct = maxOffer.SetupFeePct;
		} // RunAutoApprove

		private OfferResult CalculateOffer(decimal medalAmount, ApprovalInputData aid) {
			// decimal outstandingPrincipal = aid.MetaData.OutstandingPrincipal;

			decimal roundTo = aid.Configuration.GetCashSliderStep;

			// Currently we don't check this functionality.
			// decimal approvedAmount = medalAmount - outstandingPrincipal;
			decimal approvedAmount = medalAmount;

			if (approvedAmount <= 0)
				return new OfferResult();

			if (roundTo < 0.00000001m)
				roundTo = 1m;

			approvedAmount = roundTo * Math.Round(
				approvedAmount / roundTo, 0, MidpointRounding.AwayFromZero
			);

			return null;

			/* TODO: use standard calculator (if and when)...
			var odc = new OfferDualCalculator(
				CustomerID,
				DecisionTime,
				(int)approvedAmount,
				PreviousLoanCount > 0,
				this.medal.MedalClassification
			);

			odc.CalculateOffer();

			var res = new OfferResult {
				ScenarioName = odc.VerificationResult.ScenarioName,
				ApprovedAmount = (int)approvedAmount,
				RepaymentPeriod = odc.VerificationResult.RepaymentPeriod,
				InterestRate = odc.VerificationResult.InterestRate / 100.0m,
				SetupFeePct = odc.VerificationResult.SetupFee / 100.0m,
			};

			res.SetupFeeAmount = res.ApprovedAmount * res.SetupFeePct;

			return res;
			*/
		} // CalculateOffer

		private LoanCount AdjustLoanCount(
			LoanCount manualLoanCount,
			int manuallyApprovedAmount,
			decimal autoApprovedAmount,
			Action<LoanCount, decimal> setRatio
		) {
			LoanCount loanCount = manualLoanCount.Clone(false);

			decimal ratio = 0;

			if (manuallyApprovedAmount > 0) {
				if (AutomationDecision == DecisionActions.Approve)
					ratio = autoApprovedAmount / manuallyApprovedAmount;
			} // if

			setRatio(loanCount, ratio);

			return loanCount;
		} // AdjustLoanCount

		private DecisionActions automationDecision;
		private Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal;
		private readonly AConnection db;
	} // AutoDatumItem
} // namespace
