namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.LogicalGlue {
	using System;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;

	using SecondaryAgent = AutomationCalculator.AutoDecision.AutoApproval.LogicalGlue.Agent;

	public class Agent : AAutoDecisionBase, ICreateOfferInputData {
		public Agent(AutoApprovalArguments args) {
			this.args = args;

			this.trail = new ApprovalTrail(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			) {
				Amount = this.args.SystemCalculatedAmount,
			};

			this.oldWayAgent = new Approval(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				(int)Math.Truncate(this.args.SystemCalculatedAmount),
				(EZBob.DatabaseLib.Model.Database.Medal)this.args.Medal,
				this.args.MedalType,
				this.args.TurnoverType,
				this.args.DB,
				this.args.Log
			);

			this.secondaryAgent = new SecondaryAgent(this.args);
		} // constructor

		public virtual Agent Init() {
			// Nothing real to do here. This method is here just to be consistent with the old way Agent.

			using (Trail.AddCheckpoint(ProcessCheckpoints.Initializtion))
				return this;
		} // Init

		public void MakeAndVerifyDecision(string tag, bool quiet = false) {
			try {
				Trail.SetTag(tag).UniqueID = this.args.TrailUniqueID;

				RunPrimary();

				this.secondaryAgent.MakeDecision();

				WasMismatch = !Trail.EqualsTo(this.secondaryAgent.Trail, quiet);

				if (!WasMismatch && Trail.HasDecided) {
					if (Trail.RoundedAmount == this.secondaryAgent.Trail.RoundedAmount) {
						Trail.Affirmative<SameAmount>(false).Init(Trail.RoundedAmount);

						this.secondaryAgent.Trail.Affirmative<SameAmount>(false).Init(
							this.secondaryAgent.Trail.RoundedAmount
						);
					} else {
						Trail.Negative<SameAmount>(false).Init(Trail.RoundedAmount);
						this.secondaryAgent.Trail.Negative<SameAmount>(false).Init(this.secondaryAgent.Trail.RoundedAmount);
						WasMismatch = true;
					} // if
				} // if
			} catch (Exception e) {
				Log.Alert(e, "Exception during auto approval.");
				Trail.Negative<ExceptionThrown>(true).Init(e);
			} // try

			Trail.Save(DB, this.secondaryAgent.Trail);
		} // MakeAndVerifyDecision

		public bool ExceptionWhileDeciding {
			get { return Trail.FindTrace<ExceptionThrown>() != null; }
		} // ExceptionWhileDeciding

		public ApprovalTrail Trail {
			get { return this.trail; }
		} // Trail

		public bool LogicalGlueFlowFollowed { get { return this.args.FlowType == AutoDecisionFlowTypes.LogicalGlue; } }

		public DateTime Now { get { return this.args.Now; } }

		protected AConnection DB { get { return this.args.DB; } }
		protected ASafeLog Log { get { return this.args.Log; } }

		protected virtual void RunPrimary() {
			using (Trail.AddCheckpoint(ProcessCheckpoints.OldWayFlow)) {
				this.oldWayAgent.Init().RunPrimaryOnly();
				this.oldWayAgent.Trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;
				this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);
			} // old way flow step

			using (Trail.AddCheckpoint(ProcessCheckpoints.GatherData))
				GatherData();

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
		} // RunPrimary

		protected virtual void GatherData() {
			// TODO
		} // GatherData

		private readonly ApprovalTrail trail;
		private readonly Approval oldWayAgent;
		private readonly SecondaryAgent secondaryAgent;
		private readonly AutoApprovalArguments args;
	} // class Agent
} // namespace
