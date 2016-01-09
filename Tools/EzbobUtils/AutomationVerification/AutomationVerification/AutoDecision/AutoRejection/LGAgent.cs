namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Detects whether customer should be rejected on specific time using Logical Glue data.
	/// Detection should work for any specified time (i.e. can be run retrospectively).
	/// Should fill <see cref="Trail"/> properties.
	/// </summary>
	/// <remarks>
	/// For regulated (non-limited) companies old flow should be applied.
	/// Logical Glue flow applies to non-regulated (limited) companies only;
	/// old flow should also be executed and results stored in DB for comparison.
	/// </remarks>
	/// <example>
	/// Intended usage:
	/// var agent = new LGAgent(db, log, customer ID, time, cash request id, configuration);
	/// agent.MakeDecision();
	/// </example>
	public class LGAgent {
		/// <summary>
		/// Creates an instance.
		/// </summary>
		/// <param name="oDB">Database connection.</param>
		/// <param name="oLog">Logger.</param>
		/// <param name="nCustomerID">Customer to inspect.</param>
		/// <param name="now">Inspection date and time.</param>
		/// <param name="cashRequestID">Link to customer's cash request. Can be null.
		/// DO NOT USE THIS ARGUMENT to deduce inspection time, it is used for logging only.</param>
		/// <param name="configs">Auto-rejection configuration. Can be null. Should be loaded from DB if null.</param>
		public LGAgent(
			AConnection oDB,
			ASafeLog oLog,
			int nCustomerID,
			DateTime now,
			long? cashRequestID,
			RejectionConfigs configs = null
		) {
			this.customerId = nCustomerID;
			this.now = now;

			this.log = oLog;
			this.db = oDB;

			this.configs = configs; // Can be null (thus should be loaded from DB somehow).

			Trail = new RejectionTrail(nCustomerID, cashRequestID, oLog);

			this.oldWayAgent = new RejectionAgent(this.db, this.log, this.customerId, Trail.CashRequestID, this.configs);
		} // constructor

		public RejectionTrail Trail { get; private set; }

		public RejectionTrail OldWayTrail {
			get { return this.oldWayAgent.Trail; }
		} // OldWayTrail

		/// <summary>
		/// Makes decision: to reject or not to reject.
		/// </summary>
		public void MakeDecision() {
			this.oldWayAgent.MakeDecision(this.oldWayAgent.GetRejectionInputData(this.now));
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
			if (false) // TODO choose from company type
				StepNoDecision<LogicalGlueFlow>().Init();
			else
				StepNoDecision<InternalFlow>().Init();
		} // ChooseInternalOrLogicalGlueFlow

		private T StepReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		} // StepReject

		private T StepNoReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Negative<T>(bLockDecisionAfterAddingAStep);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepReject

		private readonly AConnection db;
		private readonly ASafeLog log;

		private readonly int customerId;
		private readonly RejectionConfigs configs;
		private readonly DateTime now;
		private readonly RejectionAgent oldWayAgent;
	} // class LGAgent
} // namespace
