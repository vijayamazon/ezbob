namespace AutomationCalculator.AutoDecision.AutoApproval.LogicalGlue {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;

	using OldWayAgent = AutomationCalculator.AutoDecision.AutoApproval.Agent;

	public class Agent {
		public Agent(AutoApprovalArguments args) {
			this.args = args;

			Trail = new LGApprovalTrail(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.Log
			) {
				Amount = this.args.SystemCalculatedAmount,
			};

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

		public virtual LGApprovalTrail Trail { get; private set; }

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
				switch (Trail.MyInputData.FlowType) {
				case AutoDecisionFlowTypes.LogicalGlue:
					Trail.Affirmative<LogicalGlueFlow>(false).Init();

					if (Trail.MyInputData.ErrorInLGData) {
						Trail.Negative<LGWithoutError>(true).Init(false);
						Trail.Amount = 0;
					} else {
						Trail.Affirmative<LGWithoutError>(false).Init(true);

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

						bool dropToZero = false;

						foreach (ATrail.StepWithDecision sd in subtrail) {
							Trail.Add(sd, sd.Decision == DecisionStatus.Negative);

							if (sd.Decision == DecisionStatus.Negative)
								dropToZero = true;
						} // for each

						if (dropToZero)
							Trail.Amount = 0;
					} // if

					break;

				case AutoDecisionFlowTypes.Internal:
					Trail.Affirmative<InternalFlow>(false).Init();
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
			Trail.MyInputData.FullInit(this.args.FlowType, this.args.ErrorInLGData, this.oldWayAgent.Trail.MyInputData);
		} // GatherData

		private readonly OldWayAgent oldWayAgent;
		private readonly AutoApprovalArguments args;
	} // class Agent
} // namespace
