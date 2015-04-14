namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using AutomationCalculator.ProcessHistory.Trails;
	// using AutomationCalculator.Common;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Strategies.Extensions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
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
			ASafeLog log
		) : base(tag, log.Safe()) {
			MinLoanCount = new LoanCount(true, log.Safe());

			this.automationDecision = DecisionActions.Waiting;
			IsAutoReRejected = false;
			IsAutoRejected = false;
			IsAutoReApproved = false;
			IsAutoApproved = false;

			CustomerID = sr.CustomerID;
			DecisionTime = sr.DecisionTime;
			PreviousLoanCount = sr.PreviousLoanCount;
			CashRequestID = sr.CashRequestID;
		} // constructor

		public override string DecisionStr {
			get {
				return AutomationDecision == DecisionActions.Waiting ? "Manual" : AutomationDecision.ToString();
			} // get
		} // DecisionStr

		public override bool IsAuto { get { return true; } }

		public LoanCount MinLoanCount { get; private set; }

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

		public class MaxOfferResult {
			public MaxOfferResult(ASafeLog log) {
				LoanCount = new LoanCount(false, log.Safe());
			} // constructor

			public LoanCount LoanCount { get; set; }
			public int ApprovedAmount { get; set; }
			public int RepaymentPeriod { get; set; }
			public decimal InterestRate { get; set; }
			public decimal SetupFeePct { get; set; }
			public decimal SetupFeeAmount { get; set; }
		} // class MaxOfferResult

		public MaxOfferResult MaxOffer { get; private set; }

		public bool IsAutoReRejected { get; private set; }
		public bool IsAutoRejected { get; private set; }
		public bool IsAutoReApproved { get; private set; }
		public bool IsAutoApproved { get; private set; }

		public decimal ReapprovedAmount { get; private set; }

		public static new string CsvTitles(string prefix) {
			prefix += " auto";

			return string.Format(
				"{1} decision;{1} re-reject decision;{1} reject decision;{1} re-approve decision;{1} approve decision;" +
				"{0};{1} re-approved amount;{1} turnover type; {1} min medal amount; {1} max medal amount",
				ADatumItem.CsvTitles(prefix),
				prefix
			);
		} // CsvTitles

		public override int ToXlsx(ExcelWorksheet sheet, int rowNum, int colNum) {
			colNum = sheet.SetCellValue(rowNum, colNum, AutomationDecision.ToString());
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoReRejected ? "Reject" : "Manual");
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoRejected ? "Reject" : "Manual");
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoReApproved ? "Approve" : "Manual");
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoApproved ? "Approve" : "Manual");

			colNum = base.ToXlsx(sheet, rowNum, colNum);

			colNum = sheet.SetCellValue(rowNum, colNum, ReapprovedAmount);

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
			MinLoanCount = AdjustLoanCount(manualLoanCount, manuallyApprovedAmount, ApprovedAmount);
			MaxOffer.LoanCount = AdjustLoanCount(manualLoanCount, manuallyApprovedAmount, MaxOffer.ApprovedAmount);
		} // SetAdjustedLoanCount

		private LoanCount AdjustLoanCount(
			LoanCount manualLoanCount,
			int manuallyApprovedAmount,
			decimal autoApprovedAmount
		) {
			LoanCount loanCount = manualLoanCount.Clone();

			loanCount.Cap(autoApprovedAmount);

			decimal takenAmount = manualLoanCount.Total.Amount;

			if ((takenAmount > 0) && (takenAmount == manuallyApprovedAmount))
				loanCount.AssumedLoanAmount = autoApprovedAmount;

			return loanCount;
		} // AdjustLoanCount

		public void RunAutomation(bool isHomeOwner, AConnection db) {
			Log.Info(
				"RunAutomation({0}) started for customer {1} with decision time '{2}'...",
				isHomeOwner,
				CustomerID,
				DecisionTime.MomentStr()
			);

			RunAutoRerejection(db);

			RunAutoReject(db);

			RunAutoReapproval(db);

			this.medal = RunCalculateMedal();

			RunAutoApprove(isHomeOwner, db);

			Log.Info(
				"RunAutomation({0}) complete for customer {1} with decision time '{2}'.",
				isHomeOwner,
				CustomerID,
				DecisionTime.MomentStr()
			);
		} // RunAutomation

		private void RunAutoRerejection(AConnection db) {
			Log.Info(
				"RunAutomation-RunAutoRerejection() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			var agent = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(
				CustomerID,
				DecisionTime,
				db,
				Log
			).Init();

			agent.MakeDecision();

			agent.Trail.Save(db, null, CashRequestID, Tag);

			if (agent.Trail.HasDecided)
				AutomationDecision = DecisionActions.ReReject;

			IsAutoReRejected = agent.Trail.HasDecided;

			Log.Info(
				"RunAutomation-RunAutoRerejection() complete for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto re-rejected is '{3}'.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoReRejected
			);
		} // RunAutoRerejection

		private void RunAutoReject(AConnection db) {
			Log.Info(
				"RunAutomation-RunAutoReject() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent agent =
				new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(db, Log, CustomerID);

			agent.MakeDecision(agent.GetRejectionInputData(DecisionTime));

			agent.Trail.Save(db, null, CashRequestID, Tag);

			if (agent.Trail.HasDecided)
				AutomationDecision = DecisionActions.Reject;

			IsAutoRejected = agent.Trail.HasDecided;

			Log.Info(
				"RunAutomation-RunAutoReject() complete for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto rejected is '{3}'.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoRejected
			);
		} // RunAutoReject

		private void RunAutoReapproval(AConnection db) {
			Log.Info(
				"RunAutomation-RunAutoReapproval() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			ReapprovedAmount = 0;

			var agent = new
				Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.
				ReApproval.ManAgainstAMachine.SameDataAgent(CustomerID, DecisionTime, db, Log);

			agent.Init();

			agent.Decide(true, CashRequestID, Tag);

			if (agent.Trail.HasDecided) {
				AutomationDecision = DecisionActions.ReApprove;
				ReapprovedAmount = agent.ApprovedAmount;
			} // if

			IsAutoReApproved = agent.Trail.HasDecided;

			Log.Info(
				"RunAutomation-RunAutoReapproval() complete for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto re-approved is '{3}', re-approved amount is {4}.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoReApproved,
				ReapprovedAmount
			);
		} // RunAutoReapproval

		private MedalResult RunCalculateMedal() {
			Log.Info(
				"RunAutomation-CalculateMedal() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			CalculateMedal instance = new CalculateMedal(CustomerID, DecisionTime, true, true) { Tag = Tag, };
			instance.Execute();

			Log.Info(
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

		private void RunAutoApprove(bool isHomeOwner, AConnection db) {
			Log.Info(
				"RunAutomation-RunAutoApprove() started for customer {0} with decision time '{1}'...",
				CustomerID,
				DecisionTime.MomentStr()
			);

			Calculate(isHomeOwner, CashRequestID, db);

			if (ApprovedAmount > 0)
				AutomationDecision = DecisionActions.Approve;

			IsAutoApproved = ApprovedAmount > 0;

			Log.Info(
				"RunAutomation-RunAutoApprove()-AutoMin done for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto approved is '{3}', approved amount is {4}.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoApproved,
				ApprovedAmount
			);
		} // RunAutoApprove

		private void Calculate(
			bool isHomeOwner,
			long cashRequestID,
			AConnection db
		) {
			Log.Info(
				"RunAutomation-Auto.Calculate(customer {0}, cash request {1}, has home '{2}', medal '{3}') started...",
				CustomerID,
				cashRequestID,
				isHomeOwner,
				this.medal.MedalClassification
			);

			MedalName = this.medal.MedalClassification.ToString();

			EzbobScore = this.medal.TotalScoreNormalized;

			int amountBeforeApproval = this.medal.RoundOfferedAmount();

			Log.Info(
				"RunAutomation-Auto.Calculate() before capping: medal '{0}', amount {1}\n{2}",
				MedalName,
				amountBeforeApproval,
				this.medal
			);

			amountBeforeApproval = Math.Min(
				amountBeforeApproval,
				isHomeOwner ? CurrentValues.Instance.MaxCapHomeOwner : CurrentValues.Instance.MaxCapNotHomeOwner
			);

			Log.Info(
				"RunAutomation-Auto.Calculate(), after capping: medal name '{0}', amount {1}, " +
				"medal '{2}', medal type '{3}', turnover type '{4}', decision time '{5}'.",
				MedalName,
				amountBeforeApproval,
				(AutomationCalculator.Common.Medal)this.medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)this.medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)this.medal.TurnoverType,
				DecisionTime.MomentStr()
			);

			var approveAgent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
				CustomerID,
				amountBeforeApproval,
				(AutomationCalculator.Common.Medal)this.medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)this.medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)this.medal.TurnoverType,
				DecisionTime,
				db,
				Log
			).Init();

			approveAgent.MakeDecision();

			approveAgent.Trail.Save(db, null, CashRequestID, Tag);

			ApprovedAmount = approveAgent.Trail.RoundedAmount;
			IsApproved = approveAgent.Trail.HasDecided;

			if (!IsApproved) {
				Log.Info(
					"RunAutomation-Auto.Calculate(), after decision: amount {0} - not calculating offer.",
					ApprovedAmount
				);

				RepaymentPeriod = 0;
				InterestRate = 0;
				SetupFeeAmount = 0;
				SetupFeePct = 0;

				MaxOffer = new MaxOfferResult(Log) {
					ApprovedAmount = 0,
					RepaymentPeriod = 0,
					InterestRate = 0,
					SetupFeeAmount = 0,
					SetupFeePct = 0,
				};
			} else {
				Log.Info(
					"RunAutomation-Auto.Calculate(), after decision: amount {0}, " +
					"loan count {1}, medal '{2}', decision time '{3}' - going for offer calculation.",
					ApprovedAmount,
					PreviousLoanCount,
					this.medal.MedalClassification,
					DecisionTime.MomentStr()
				);

				var odc = new OfferDualCalculator(
					CustomerID,
					DecisionTime,
					ApprovedAmount,
					PreviousLoanCount > 0,
					this.medal.MedalClassification
				);

				odc.CalculateOffer();

				RepaymentPeriod = odc.VerifySeek.RepaymentPeriod;
                InterestRate = odc.VerifySeek.InterestRate / 100.0m;
                SetupFeePct = odc.VerifySeek.SetupFee / 100.0m;
				SetupFeeAmount = ApprovedAmount * SetupFeePct;

				Log.Info(
					"RunAutomation-Auto.Calculate(), offer: amount {0}, " +
					"repayment period {1}, interest rate {2}, setup fee {3} ({4}).",
					ApprovedAmount,
					RepaymentPeriod,
					InterestRate,
					SetupFeePct.ToString("P6", Library.Instance.Culture),
					SetupFeeAmount.ToString("C2", Library.Instance.Culture)
				);

				if (this.medal.OfferedAmountsDiffer())
					CalculateMaxOffer(isHomeOwner, approveAgent.Trail);
				else {
					MaxOffer = new MaxOfferResult(Log) {
						ApprovedAmount = ApprovedAmount,
						RepaymentPeriod = RepaymentPeriod,
						InterestRate = InterestRate,
						SetupFeeAmount = SetupFeeAmount,
						SetupFeePct = SetupFeePct,
					};
				} // if
			} // if

			Log.Info(
				"RunAutomation-Auto.Calculate(customer {0}, cash request {1}, has home '{2}', medal '{3}') complete.",
				CustomerID,
				cashRequestID,
				isHomeOwner,
				this.medal.MedalClassification
			);
		} // Calculate

		private void CalculateMaxOffer(
			bool isHomeOwner,
			ApprovalTrail trail
		) {
			decimal approvedAmount = Math.Min(
				this.medal.RoundMaxOfferedAmount(),
				isHomeOwner ? CurrentValues.Instance.MaxCapHomeOwner : CurrentValues.Instance.MaxCapNotHomeOwner
			);

			approvedAmount -= trail.MyInputData.MetaData.OutstandingPrincipal;

			if (approvedAmount < 0)
				approvedAmount = 0;

			if (approvedAmount == 0) {
				MaxOffer = new MaxOfferResult(Log) {
					ApprovedAmount = 0,
					RepaymentPeriod = 0,
					InterestRate = 0,
					SetupFeeAmount = 0,
					SetupFeePct = 0,
				};
			} else {
				decimal roundTo = trail.MyInputData.Configuration.GetCashSliderStep;

				if (roundTo < 0.00000001m)
					roundTo = 1m;

				approvedAmount = roundTo * Math.Round(
					approvedAmount / roundTo, 0, MidpointRounding.AwayFromZero
				);

				MaxOffer = new MaxOfferResult(Log) {
					ApprovedAmount = (int)approvedAmount,
				};

				var odc = new OfferDualCalculator(
					CustomerID,
					DecisionTime,
					MaxOffer.ApprovedAmount,
					PreviousLoanCount > 0,
					this.medal.MedalClassification
				);

				odc.CalculateOffer();

				MaxOffer.RepaymentPeriod = odc.VerifySeek.RepaymentPeriod;
                MaxOffer.InterestRate = odc.VerifySeek.InterestRate / 100.0m;
                MaxOffer.SetupFeePct = odc.VerifySeek.SetupFee / 100.0m;
				MaxOffer.SetupFeeAmount = MaxOffer.ApprovedAmount * MaxOffer.SetupFeePct;
			} // if
		} // CalculateMaxOffer

		private DecisionActions automationDecision;
		private Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal;
	} // AutoDatumItem
} // namespace
