namespace Ezbob.Backend.Strategies.ManualDecision {
	using ConfigManager;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;

	internal class DecisionIsChangable {
		public DecisionIsChangable(DecisionModel model) {
			this.decisionModel = model;
			Warning = string.Empty;
			Error = string.Empty;
		} // constructor

		public string Warning { get; private set; }

		public string Error { get; private set; }

		public ChangeDecisionOption Precheck() {
			if (this.decisionModel == null) { // Should never happen but just in case.
				Error = "No decision data provided.";
				return ChangeDecisionOption.BlockedByError;
			} // if

			if (this.decisionModel.customerID <= 0) { // Should never happen but just in case.
				Error = "Customer ID not specified.";
				return ChangeDecisionOption.BlockedByError;
			} // if

			if (this.decisionModel.underwriterID <= 0) { // Should never happen but just in case.
				Error = "Underwriter ID not specified.";
				return ChangeDecisionOption.BlockedByError;
			} // if

			if (this.decisionModel.cashRequestID <= 0) {
				Error = string.Format(
					"There is no open cash request for customer {0}, decision cannot be made.",
					this.decisionModel.customerID
				);
				return ChangeDecisionOption.BlockedNoCashRequest;
			} // if

			// At this point should never happen but just in case.
			if (string.IsNullOrWhiteSpace(this.decisionModel.cashRequestRowVersion)) {
				Error = string.Format(
					"Please refresh your browser page: decision cannot be applied " +
					"to customer {0} with cash request {1}, (cash request row version is not specified).",
					this.decisionModel.customerID,
					this.decisionModel.cashRequestID
				);
				return ChangeDecisionOption.BlockedByError;
			} // if
			
			return ChangeDecisionOption.Available;
		} // Precheck

		public ChangeDecisionOption ValidateAgainstCurrentState(CurrentCustomerDecisionState curState) {
			this.currentState = curState;

			if (this.currentState.CustomerID != this.decisionModel.customerID) { // Should never happen but just in case.
				Error = string.Format(
					"Decision cannot be applied " +
					"to customer {0} with cash request {1}, (failed to load current customer state from DB).",
					this.decisionModel.customerID,
					this.decisionModel.cashRequestID
				);
				return ChangeDecisionOption.BlockedByError;
			} // if

			if (!this.currentState.CashRequestMatches) { // Should never happen but just in case.
				Error = string.Format(
					"Something went terribly wrong: cash request {1} belongs to customer {2} " +
					"rather than to customer {0}.",
					this.decisionModel.customerID,
					this.decisionModel.cashRequestID,
					this.currentState.CashRequestCustomerID
				);
				return ChangeDecisionOption.BlockedByError;
			} // if

			// Should never happen but just in case.
			if (this.decisionModel.cashRequestID != this.currentState.CashRequestID) {
				Error = string.Format(
					"Please retry later: decision cannot be applied " +
					"to customer {0} with cash request {1}, (failed to load current cash request details from DB).",
					this.decisionModel.customerID,
					this.decisionModel.cashRequestID
				);
				return ChangeDecisionOption.BlockedByError;
			} // if

			if (this.currentState.IsFinalDecision) {
				Error = string.Format(
					"Cash request {1} of customer {0} is already '{2}', please refresh your browser page.",
					this.decisionModel.customerID,
					this.decisionModel.cashRequestID,
					this.currentState.DecisionStr.ToLowerInvariant()
				);
				return ChangeDecisionOption.BlockedByFinalDecision;
			} // if

			if (this.currentState.RowVersionChanged(this.decisionModel.cashRequestRowVersion)) {
				Error = string.Format(
					"Please refresh your browser page: cash request {1} of customer {0} was changed by someone else.",
					this.decisionModel.customerID,
					this.decisionModel.cashRequestID
				);
				return ChangeDecisionOption.BlockedByConcurrency;
			} // if

			if (string.IsNullOrWhiteSpace(this.currentState.CreditResult)) {
				Error = string.Format(
					"Please retry later: main strategy is being executed on customer {0}.",
					this.decisionModel.customerID
				);
				return ChangeDecisionOption.BlockedByMainStrategy;
			} // if

			if (this.decisionModel.status == CreditResultStatus.Approved) {
				if (this.currentState.OfferedCreditLine <= 0) {
					Error = "Please specify approved amount.";
					return ChangeDecisionOption.BlockedByApprovedAmount;
				} // if

				int minAmount = CurrentValues.Instance.XMinLoan;

				if (this.currentState.OfferedCreditLine < minAmount) {
					Error = string.Format(
						"Approved amount is too small (should be at least {0}).",
						minAmount.ToString("C0", Library.Instance.Culture)
					);
					return ChangeDecisionOption.BlockedByApprovedAmount;
				} // if

				int maxAmount = this.currentState.IsManager
					? CurrentValues.Instance.ManagerMaxLoan
					: CurrentValues.Instance.MaxLoan;

				if (this.currentState.OfferedCreditLine > maxAmount) {
					Error = string.Format(
						"Approved amount is too big (should be at most {0}).",
						maxAmount.ToString("C0", Library.Instance.Culture)
					);
					return ChangeDecisionOption.BlockedByApprovedAmount;
				} // if
			} // if

			return ChangeDecisionOption.Available;
		} // ValidateAgainstCurrentState

		private CurrentCustomerDecisionState currentState;
		private readonly DecisionModel decisionModel;
	} // class DecisionIsChangable
} // namespace
