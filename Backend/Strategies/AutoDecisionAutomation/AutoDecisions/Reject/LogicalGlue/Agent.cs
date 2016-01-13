namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue {
	using System;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Agent : AAutoDecisionBase {
		public virtual RejectionTrail Trail { get; private set; }

		public Agent(int nCustomerID, long? cashRequestID, DateTime? now, AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = oLog.Safe();
			Args = new Arguments(nCustomerID, cashRequestID, now);

			this.oldWayAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				nCustomerID,
				cashRequestID,
				oDB,
				oLog
			);
		} // constructor

		public virtual Agent Init() {
			Trail = new RejectionTrail(
				Args.CustomerID,
				Args.CashRequestID,
				Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);

			Cfg = InitCfg();

			return this;
		} // Init

		public virtual void MakeAndVerifyDecision(string tag, bool quiet = false) {
			AutomationCalculator.AutoDecision.AutoRejection.LGAgent oSecondary = null;

			try {
				Trail.SetTag(tag);

				RunPrimary();

				oSecondary = RunSecondary();

				if (Trail.HasApprovalChance == oSecondary.Trail.HasApprovalChance) {
					Trail.Negative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
					oSecondary.Trail.Negative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
				} else {
					Trail.Affirmative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
					oSecondary.Trail.Affirmative<SameApprovalChance>(false)
						.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
				} // if

				WasMismatch = !Trail.EqualsTo(oSecondary.Trail, quiet);
			} catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				Trail.Negative<ExceptionThrown>(true).Init(e);
			} // try

			Trail.Save(DB, oSecondary == null ? null : oSecondary.Trail);
		} // MakeAndVerifyDecision

		public bool LogicalGlueFlowFollowed {
			get { return Trail.FindTrace<LogicalGlueFlow>() != null; }
		} // LogicalGlueFlowFollowed

		protected virtual void RunPrimary() {
			this.oldWayAgent.RunPrimaryOnly();
			this.oldWayAgent.Trail.SetTag(Trail.Tag);
			this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);

			ChooseInternalOrLogicalGlueFlow();

			if (LogicalGlueFlowFollowed) {
				// TODO Logical Glue flow goes here.
			} else
				Trail.AppendOverridingResults(this.oldWayAgent.Trail);
		} // RunPrimary

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }

		private AutomationCalculator.AutoDecision.AutoRejection.LGAgent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoRejection.LGAgent(
					DB,
					Log,
					Trail.Tag,
					Args.CustomerID,
					Trail.InputData.DataAsOf,
					Args.CashRequestID,
					Cfg.Values
				);

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		protected virtual void ChooseInternalOrLogicalGlueFlow() {
			if (false) // TODO choose from company type
				Trail.Dunno<LogicalGlueFlow>().Init();
			else
				Trail.Dunno<InternalFlow>().Init();
		} // ChooseInternalOrLogicalGlueFlow

		private readonly Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent oldWayAgent;
	} // class Agent
} // namespace
