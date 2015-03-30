namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using AutomationCalculator.Common;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Strategies.Extensions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	public class AutoDatumItem : ADatumItem {
		public AutoDatumItem(int customerID, DateTime decisionTime, int previousLoanCount) {
			this.automationDecision = DecisionActions.Waiting;
			IsAutoReRejected = false;
			IsAutoRejected = false;
			IsAutoReApproved = false;
			IsAutoApproved = false;

			CustomerID = customerID;
			DecisionTime = decisionTime;
			PreviousLoanCount = previousLoanCount;
		} // constructor

		public override string DecisionStr {
			get {
				return AutomationDecision == DecisionActions.Waiting ? "Manual" : AutomationDecision.ToString();
			} // get
		} // DecisionStr

		public override bool IsAuto { get { return true; } }

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

		public static new string CsvTitles(string prefix) {
			return string.Format(
				"{1} decision;{1} re-reject decision;{1} reject decision;{1} re-approve decision;{1} approve decision;" +
				"{0};{1} re-approved amount", ADatumItem.CsvTitles(prefix), prefix);
		} // CsvTitles

		public override int ToXlsx(ExcelWorksheet sheet, int rowNum, int colNum) {
			colNum = sheet.SetCellValue(rowNum, colNum, AutomationDecision.ToString());
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoReRejected ? "Reject" : "Manual");
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoRejected ? "Reject" : "Manual");
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoReApproved ? "Approve" : "Manual");
			colNum = sheet.SetCellValue(rowNum, colNum, IsAutoApproved ? "Approve" : "Manual");

			colNum = base.ToXlsx(sheet, rowNum, colNum);

			colNum = sheet.SetCellValue(rowNum, colNum, ReapprovedAmount);
			return colNum;
		} // ToXlsx

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

			agent.Decide(false, CashRequestID);

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

			Calculate(isHomeOwner, medal, true, CashRequestID, db, log);

			if (ApprovedAmount > 0)
				AutomationDecision = DecisionActions.Approve;

			IsAutoApproved = ApprovedAmount > 0;

			log.Info(
				"RunAutomation-RunAutoApprove()-AutoMin done for customer {0} with decision time '{1}': " +
				"automation decision is '{2}', auto approved is '{3}', approved amount is {4}.",
				CustomerID,
				DecisionTime.MomentStr(),
				AutomationDecision,
				IsAutoApproved,
				ApprovedAmount
			);

			/*
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
			*/
		} // RunAutoApprove

		private void Calculate(
			bool isHomeOwner,
			Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal,
			bool takeMinOffer,
			long cashRequestID,
			AConnection db,
			ASafeLog log
		) {
			log.Info(
				"RunAutomation-Auto{4}.Calculate(customer {0}, cash request {1}, " +
				"has home '{2}', medal '{3}', take min '{4}') started...",
				CustomerID,
				cashRequestID,
				isHomeOwner,
				medal.MedalClassification,
				takeMinOffer ? "Min" : "Max"
			);

			MedalName = medal.MedalClassification.ToString();

			EzbobScore = medal.TotalScoreNormalized;

			int amountBeforeApproval = takeMinOffer ? medal.RoundOfferedAmount() : medal.RoundMaxOfferedAmount();

			log.Info(
				"RunAutomation-Auto{0}.Calculate() before capping: medal '{1}', amount {2}\n{3}",
				takeMinOffer ? "Min" : "Max",
				MedalName,
				amountBeforeApproval,
				medal
			);

			amountBeforeApproval = Math.Min(
				amountBeforeApproval,
				isHomeOwner ? CurrentValues.Instance.MaxCapHomeOwner : CurrentValues.Instance.MaxCapNotHomeOwner
			);

			log.Info(
				"RunAutomation-Auto{0}.Calculate(), after capping: medal name '{1}', amount {2}, " +
				"medal '{3}', medal type '{4}', turnover type '{5}', decision time '{6}'.",
				takeMinOffer ? "Min" : "Max",
				MedalName,
				amountBeforeApproval,
				(AutomationCalculator.Common.Medal)medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DecisionTime.MomentStr()
			);

			var approveAgent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
				CustomerID,
				amountBeforeApproval,
				(AutomationCalculator.Common.Medal)medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DecisionTime,
				db,
				log
			).Init();

			approveAgent.MakeDecision();

			ApprovedAmount = approveAgent.Trail.RoundedAmount;
			IsApproved = approveAgent.Trail.HasDecided;

			if (!IsApproved) {
				log.Info(
					"RunAutomation-Auto{0}.Calculate(), after decision: amount {1} - not calculating offer.",
					takeMinOffer ? "Min" : "Max",
					ApprovedAmount
				);

				RepaymentPeriod = 0;
				InterestRate = 0;
				SetupFeeAmount = 0;
				SetupFeePct = 0;
			} else {
				log.Info(
					"RunAutomation-Auto{0}.Calculate(), after decision: amount {1}, " +
					"loan count {2}, medal '{3}', decision time '{4}' - going for offer calculation.",
					takeMinOffer ? "Min" : "Max",
					ApprovedAmount,
					PreviousLoanCount,
					medal.MedalClassification,
					DecisionTime.MomentStr()
				);

				var odc = new OfferDualCalculator(
					CustomerID,
					DecisionTime,
					ApprovedAmount,
					PreviousLoanCount > 0,
					medal.MedalClassification
				);

				odc.CalculateOffer();

				RepaymentPeriod = odc.VerifyBoundaries.RepaymentPeriod;
				InterestRate = odc.VerifyBoundaries.InterestRate / 100.0m;
				SetupFeePct = odc.VerifyBoundaries.SetupFee / 100.0m;
				SetupFeeAmount = ApprovedAmount * SetupFeePct;

				log.Info(
					"RunAutomation-Auto{0}.Calculate(), offer: amount {1}, " +
					"repayment period {2}, interest rate {3}, setup fee {4} ({5}).",
					takeMinOffer ? "Min" : "Max",
					ApprovedAmount,
					RepaymentPeriod,
					InterestRate,
					SetupFeePct.ToString("P6", Library.Instance.Culture),
					SetupFeeAmount.ToString("C2", Library.Instance.Culture)
				);
			} // if

			log.Info(
				"RunAutomation-Auto{4}.Calculate(customer {0}, cash request {1}, " +
				"has home '{2}', medal '{3}', take min '{4}') complete.",
				CustomerID,
				cashRequestID,
				isHomeOwner,
				medal.MedalClassification,
				takeMinOffer ? "Min" : "Max"
			);
		} // Calculate

		private DecisionActions automationDecision;
	} // AutoDatumItem
} // namespace
