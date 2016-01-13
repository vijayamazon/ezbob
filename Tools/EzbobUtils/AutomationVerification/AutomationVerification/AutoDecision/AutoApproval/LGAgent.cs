namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LGAgent {
		public LGAgent(
			int customerID,
			long? cashRequestID,
			DateTime now,
			decimal systemCalculatedAmount,
			AutomationCalculator.Common.Medal medal,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			AConnection db,
			ASafeLog log
		) {
			this.db = db;
			this.log = log.Safe();
			Now = now;

			Trail = new ApprovalTrail(customerID, cashRequestID, this.log);
			Trail.SetTag("AutomationVerificationAutoApproveLGAgent");

			this.oldWayAgent = new Agent(
				customerID,
				cashRequestID,
				systemCalculatedAmount,
				medal,
				medalType,
				turnoverType,
				db,
				log
			);
		} // constructor

		public virtual DateTime Now { get; private set; }

		public virtual ApprovalTrail Trail { get; private set; }

		public virtual LGAgent Init() {
			// TODO

			return this;
		} // Init

		public virtual void MakeDecision() {
			this.oldWayAgent.MakeDecision();
			this.oldWayAgent.Trail.Save(this.db, null, TrailPrimaryStatus.OldVerification);

			ChooseInternalOrLogicalGlueFlow();

			if (LogicalGlueFlowFollowed) {
				// TODO Logical Glue flow goes here.
			} else
				Trail.AppendOverridingResults(this.oldWayAgent.Trail);
		} // MakeDecision

		public bool LogicalGlueFlowFollowed {
			get { return Trail.FindTrace<LogicalGlueFlow>() != null; }
		} // LogicalGlueFlowFollowed

		protected virtual void ChooseInternalOrLogicalGlueFlow() {
			if (false) // TODO detect from company type; insert LogicalGlueFlow or InternalFlow into Trail.
				Trail.Dunno<LogicalGlueFlow>().Init();
			else
				Trail.Dunno<InternalFlow>().Init();
		} // ChooseInternalOrLogicalGlueFlow

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly Agent oldWayAgent;
	} // class LGAgent
} // namespace
