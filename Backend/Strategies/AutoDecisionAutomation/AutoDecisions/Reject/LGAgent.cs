namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LGAgent : AAutoDecisionBase {
		public virtual RejectionTrail Trail { get; private set; }

		public LGAgent(int nCustomerID, long? cashRequestID, AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = oLog.Safe();
			Args = new Arguments(nCustomerID, cashRequestID);

			this.oldWayAgent = new Agent(nCustomerID, cashRequestID, oDB, oLog);
		} // constructor

		public virtual LGAgent Init() {
			Trail = new RejectionTrail(
				Args.CustomerID,
				Args.CashRequestID,
				Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);

			Now = DateTime.UtcNow;
			Cfg = InitCfg();

			return this;
		} // Init

		public virtual bool MakeAndVerifyDecision(string tag, bool quiet = false) {
			try {
				Trail.SetTag(tag);

				RunPrimary();

				AutomationCalculator.AutoDecision.AutoRejection.LGAgent oSecondary = RunSecondary();

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

				Trail.Save(DB, oSecondary.Trail);

				return !WasMismatch;
			} catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				Trail.Negative<ExceptionThrown>(true).Init(e);
				return false;
			} // try
		} // MakeAndVerifyDecision

		protected virtual void RunPrimary() {
			this.oldWayAgent.RunPrimaryOnly();
			this.oldWayAgent.Trail.SetTag(Trail.Tag);
			this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);

			bool followLogicalGlueFlow = FollowLogicalGlueFlow();

			if (followLogicalGlueFlow) {
				Trail.Dunno<LogicalGlueFlow>().Init();
				// TODO Logical Glue flow goes here.
			} else {
				Trail.Dunno<InternalFlow>().Init();
				Trail.AppendOverridingResults(this.oldWayAgent.Trail);
			} // if
		} // RunPrimary

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected virtual DateTime Now { get; set; }

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }

		private AutomationCalculator.AutoDecision.AutoRejection.LGAgent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoRejection.LGAgent(
					DB,
					Log,
					Args.CustomerID,
					Trail.InputData.DataAsOf,
					Args.CashRequestID,
					Cfg.Values
				);

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		private bool FollowLogicalGlueFlow() {
			return false; // TODO: detect from company type	
		} // FollowLogicalGlueFlow

		private readonly Agent oldWayAgent;
	} // class LGAgent
} // namespace
