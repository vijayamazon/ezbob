namespace Ezbob.Backend.Strategies.ManualDecision {
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	using PendingStatusEnum = EZBob.DatabaseLib.Model.Database.PendingStatus;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class ManuallySuspend : AApplyManualDecisionBase {
		public ManuallySuspend(DecisionToApply decision, AConnection db, ASafeLog log) : base(decision, db, log) {
		} // constructor

		public decimal CustomerManagerApprovedSum { get { return this.decision.Customer.ManagerApprovedSum; } set { } }
		public string PendingStatus { get { return PendingStatusEnum.Manual.ToString(); } set { } }
	} // class ManuallySuspend
} // namespace
