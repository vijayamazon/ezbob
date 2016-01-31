namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal abstract class AApprovalBaseStep : ADecisionBaseStep {
		protected AApprovalBaseStep(
			string outerContextDescription,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag,
			bool customerStatusIsEnabled,
			bool customerStatusIsWarning,
			bool autoRejectionEnabled,
			bool autoRerejectionEnabled
		) : base(
			outerContextDescription,
			avoidAutomaticDecision,
			enabled,
			customerID,
			cashRequestID,
			nlCashRequestID,
			tag
		) {
			this.customerStatusIsEnabled = customerStatusIsEnabled;
			this.customerStatusIsWarning = customerStatusIsWarning;
			this.autoRejectionEnabled = autoRejectionEnabled;
			this.autoRerejectionEnabled = autoRerejectionEnabled;
		} // constructor

		protected override string DecisionName { get { return "approved"; } }

		protected override bool PreventAffirmativeDecision() {
			if (base.PreventAffirmativeDecision())
				return true;

			if (!this.customerStatusIsEnabled) {
				Log.Msg(
					"Preventing {1} decision for {0}: customer status is not enabled.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			if (this.customerStatusIsWarning) {
				Log.Msg(
					"Preventing {1} decision for {0}: customer status is 'warning'.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			if (!this.autoRerejectionEnabled) {
				Log.Msg(
					"Preventing {1} decision for {0}: auto re-rejections are disabled.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			if (!this.autoRejectionEnabled) {
				Log.Msg(
					"Preventing {1} decision for {0}: auto rejections are disabled.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			return false;
		} // PreventAffirmativeDecision

		private readonly bool customerStatusIsEnabled;
		private readonly bool customerStatusIsWarning;
		private readonly bool autoRejectionEnabled;
		private readonly bool autoRerejectionEnabled;
	} // class AApprovalBaseStep
} // namespace
