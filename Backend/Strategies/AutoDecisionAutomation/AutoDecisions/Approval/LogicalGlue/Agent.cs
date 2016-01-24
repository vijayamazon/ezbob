namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.LogicalGlue {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
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

			this.trail = new LGApprovalTrail(
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

			using (this.trail.AddCheckpoint(ProcessCheckpoints.Initializtion))
				return this;
		} // Init

		public void MakeAndVerifyDecision(bool quiet = false) {
			try {
				this.trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;

				RunPrimary();

				this.secondaryAgent.MakeDecision();

				WasMismatch = !this.trail.EqualsTo(this.secondaryAgent.Trail, quiet);

				if (!WasMismatch && this.trail.HasDecided) {
					if (this.trail.RoundedAmount == this.secondaryAgent.Trail.RoundedAmount) {
						this.trail.Affirmative<SameAmount>(false).Init(this.trail.RoundedAmount);

						this.secondaryAgent.Trail.Affirmative<SameAmount>(false).Init(
							this.secondaryAgent.Trail.RoundedAmount
						);
					} else {
						this.trail.Negative<SameAmount>(false).Init(this.trail.RoundedAmount);
						this.secondaryAgent.Trail.Negative<SameAmount>(false).Init(this.secondaryAgent.Trail.RoundedAmount);
						WasMismatch = true;
					} // if
				} // if
			} catch (Exception e) {
				Log.Alert(e, "Exception during auto approval.");
				this.trail.Negative<ExceptionThrown>(true).Init(e);
			} // try

			this.trail.Save(DB, this.secondaryAgent.Trail);
		} // MakeAndVerifyDecision

		public bool ExceptionWhileDeciding {
			get { return this.trail.FindTrace<ExceptionThrown>() != null; }
		} // ExceptionWhileDeciding

		public ApprovalTrail Trail {
			get { return this.trail; }
		} // Trail

		public bool LogicalGlueFlowFollowed { get { return this.args.FlowType == AutoDecisionFlowTypes.LogicalGlue; } }

		public DateTime Now { get { return this.args.Now; } }

		protected AConnection DB { get { return this.args.DB; } }
		protected ASafeLog Log { get { return this.args.Log; } }

		protected virtual void RunPrimary() {
			using (this.trail.AddCheckpoint(ProcessCheckpoints.OldWayFlow)) {
				this.oldWayAgent.Init().RunPrimaryOnly();
				this.oldWayAgent.Trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;
				this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);
			} // old way flow step

			using (this.trail.AddCheckpoint(ProcessCheckpoints.GatherData))
				GatherData();

			using (this.trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				switch (this.trail.MyInputData.FlowType) {
				case AutoDecisionFlowTypes.LogicalGlue:
					this.trail.Dunno<LogicalGlueFlow>().Init();

					if (this.trail.MyInputData.ErrorInLGData)
						this.trail.Negative<LGWithoutError>(true).Init(false);
					else {
						this.trail.Affirmative<LGWithoutError>(false).Init(true);

						List<ATrail.StepWithDecision> subtrail = this.oldWayAgent.Trail.FindSubtrail(
							typeof(FraudSuspect),
							typeof(IsBrokerCustomer),
							typeof(TodayApprovalCount),
							typeof(TodayLoans),
							typeof(HourlyApprovalCount),
							typeof(LastHourApprovalCount),
							typeof(OutstandingOffers),
							typeof(AmlCheck),
							typeof(CustomerStatus),
							typeof(ThreeMonthsTurnover),
							typeof(DefaultAccounts),
							typeof(Rollovers),
							typeof(LatePayment),
							typeof(OutstandingLoanCount),
							typeof(OutstandingRepayRatio)
						);

						foreach (ATrail.StepWithDecision sd in subtrail)
							this.trail.Add(sd, sd.Decision == DecisionStatus.Negative);
					} // if

					break;

				case AutoDecisionFlowTypes.Internal:
					this.trail.Dunno<InternalFlow>().Init();
					this.trail.AppendOverridingResults(this.oldWayAgent.Trail);
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
			this.trail.MyInputData.FullInit(this.args.FlowType, this.args.ErrorInLGData, this.oldWayAgent.Trail.MyInputData);
		} // GatherData

		private readonly LGApprovalTrail trail;
		private readonly Approval oldWayAgent;
		private readonly SecondaryAgent secondaryAgent;
		private readonly AutoApprovalArguments args;
	} // class Agent
} // namespace
