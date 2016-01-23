﻿namespace AutomationCalculator.AutoDecision.AutoApproval.LogicalGlue {
	using System;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;

	using OldWayAgent = AutomationCalculator.AutoDecision.AutoApproval.Agent;

	public class Agent {
		public Agent(AutoApprovalArguments args) {
			this.args = args;

			Trail = new ApprovalTrail(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.Log
			);

			this.oldWayAgent = new OldWayAgent(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.SystemCalculatedAmount,
				this.args.Medal,
				this.args.MedalType,
				this.args.TurnoverType,
				this.args.DB,
				this.args.Log
			);
		} // constructor

		public virtual DateTime Now { get { return this.args.Now; } }

		public virtual ApprovalTrail Trail { get; private set; }

		public virtual Agent Init() {
			// Nothing real to do here. This method is here just to be consistent with the old way Agent.

			using (Trail.AddCheckpoint(ProcessCheckpoints.Initializtion))
				return this;
		} // Init

		public virtual void MakeDecision() {
			using (Trail.AddCheckpoint(ProcessCheckpoints.OldWayFlow)) {
				this.oldWayAgent.Init().MakeDecision();
				this.oldWayAgent.Trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;
				this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldVerification);
			} // old flow step

			using (Trail.AddCheckpoint(ProcessCheckpoints.GatherData)) {
				Trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;

				GatherData();
			} // gather data step

			using (Trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				switch (this.args.FlowType) { // TODO replace with some Trail property usage
				case AutoDecisionFlowTypes.LogicalGlue:
					Trail.Dunno<LogicalGlueFlow>().Init();
					// TODO partially Trail.AppendOverridingResults(this.oldWayAgent.Trail);
					break;

				case AutoDecisionFlowTypes.Internal:
					Trail.Dunno<InternalFlow>().Init();
					Trail.AppendOverridingResults(this.oldWayAgent.Trail);
					break;

				default:
					throw new ArgumentOutOfRangeException(
						"Unsupported flow detected: " + this.args.FlowType,
						(Exception)null
					);
				} // switch
			} // make decision step
		} // MakeDecision

		protected virtual AConnection DB { get { return this.args.DB; } }
		protected virtual ASafeLog Log { get { return this.args.Log; } }

		protected virtual void GatherData() {
			// TODO
		} // GatherData

		private readonly OldWayAgent oldWayAgent;
		private readonly AutoApprovalArguments args;
	} // class Agent
} // namespace
