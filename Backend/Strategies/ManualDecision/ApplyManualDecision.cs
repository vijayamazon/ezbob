namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Threading.Tasks;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Alibaba;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using SalesForceLib.Models;

	public class ApplyManualDecision : AStrategy {
		public ApplyManualDecision(DecisionModel model) {
			this.now = DateTime.UtcNow;
			Warning = string.Empty;
			Error = string.Empty;

			this.decisionModel = model;
		} // constructor

		public override string Name {
			get { return "Set manual decision"; }
		} // Name

		public string Warning { get; private set; }

		public string Error { get; private set; }

		public override void Execute() {
			Log.Debug("Applying manual decision by model: {0}.", this.decisionModel.Stringify());

			switch (CanChangeDecision()) {
			case ChangeDecisionOption.Available:
				Log.Debug("Decision can be applied to model.attemptID = {0}.", this.decisionModel.attemptID);
				break;

			case ChangeDecisionOption.BlockedByConcurrency:
			case ChangeDecisionOption.BlockedNoCashRequest:
			case ChangeDecisionOption.BlockedByFinalDecision:
			case ChangeDecisionOption.BlockedByMainStrategy:
			case ChangeDecisionOption.BlockedByApprovedAmount:
			case ChangeDecisionOption.BlockedByMaxLoanAmount:
				Log.Info("Decision cannot be applied to model {0}: '{1}'.", this.decisionModel.Stringify(), Error);
				return;

			case ChangeDecisionOption.BlockedByError:
				Log.Alert("Decision cannot be applied to model {0}: '{1}'.", this.decisionModel.Stringify(), Error);
				return;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			this.decisionToApply = new DecisionToApply(
				this.decisionModel.underwriterID,
				this.decisionModel.customerID,
				this.decisionModel.cashRequestID
			);

			this.decisionToApply.Customer.CreditResult = this.decisionModel.status.ToString();

			this.decisionToApply.Customer.UnderwriterName =
				this.currentState.UnderwriterID == this.decisionModel.underwriterID
					? this.currentState.UnderwriterName
					: null;

			this.decisionToApply.CashRequest.UnderwriterDecisionDate = this.now;
			this.decisionToApply.CashRequest.UnderwriterDecision = this.decisionModel.status.ToString();
			this.decisionToApply.CashRequest.UnderwriterComment = this.decisionModel.reason;

			var newDecision = new NL_Decisions {
				UserID = this.decisionModel.underwriterID,
				DecisionTime = this.now,
				Notes = this.decisionModel.reason,
			};

			if (this.decisionModel.status != CreditResultStatus.ApprovedPending)
				this.decisionToApply.Customer.IsWaitingForSignature = false;

			SilentAutomation.Callers? silentAutomationCaller = null;

			switch (this.decisionModel.status) {
			case CreditResultStatus.Approved:
				if (this.currentState.LastWizardStep)
					silentAutomationCaller = SilentAutomation.Callers.ManuallyApproved;

				ApproveCustomer(newDecision);
				break;

			case CreditResultStatus.Rejected:
				silentAutomationCaller = SilentAutomation.Callers.ManuallyRejected;
				RejectCustomer(newDecision);
				break;

			case CreditResultStatus.Escalated:
				EscalateCustomer(newDecision);
				break;

			case CreditResultStatus.ApprovedPending:
				silentAutomationCaller = SilentAutomation.Callers.ManuallyPending;
				PendCustomer(newDecision);
				break;

			case CreditResultStatus.WaitingForDecision:
				ReturnCustomerToWaitingForDecision(newDecision);
				break;
			} // switch

			// send final decision data (0002) to Alibaba parther (if exists)
			bool notifyAlibaba =
				this.currentState.IsAlibaba &&
				this.decisionModel.status.In(CreditResultStatus.Rejected, CreditResultStatus.Approved);

			if (notifyAlibaba) {
				FireToBackground(
					"notify Alibaba",
					() => new DataSharing(this.decisionModel.customerID, AlibabaBusinessType.APPLICATION_REVIEW).Execute()
				);
			} // if

			if (silentAutomationCaller.HasValue) {
				FireToBackground(
					"silent automation",
					() => {
						new SilentAutomation(this.decisionModel.customerID)
							.SetTag(silentAutomationCaller.Value)
							.PreventMainStrategy()
							.Execute();
					}
				);
			} // if

			Log.Debug("Done applying manual decision by model: {0}.", this.decisionModel.Stringify());
		} // Execute

		private void FireToBackground(string description, Action task, Action<Exception> onFailedToStart = null) {
			if (task == null)
				return;

			string taskID = Guid.NewGuid().ToString("N");

			StrategyLog log = Log;

			log.Debug("Starting background task '{1}' with id '{0}'...", taskID, description);

			try {
				Task.Run(() => {
					try {
						task();

						log.Debug("Background task '{1}' (id: '{0}') completed successfully.", taskID, description);
					} catch (Exception e) {
						log.Alert(e, "Background task '{1}' (id: '{0}') failed.", taskID, description);
					} // try
				});
			} catch (Exception e) {
				Log.Alert(e, "Failed to fire task '{1}' (id: '{0}') to background.", taskID, description);

				if (onFailedToStart != null)
					onFailedToStart(e);
			} // try
		} // FireToBackground

		private ChangeDecisionOption CanChangeDecision() {
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

			this.currentState = DB.FillFirst<CurrentCustomerDecisionState>(
				"LoadCurrentCustomerDecisionState",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@UnderwriterID", this.decisionModel.underwriterID),
				new QueryParameter("@CustomerID", this.decisionModel.customerID),
				new QueryParameter("@CashRequestID", this.decisionModel.cashRequestID)
			);

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
		} // CanChangeDecision

		private void ReturnCustomerToWaitingForDecision(NL_Decisions newDecision) {
			/*
			customer.CreditResult = CreditResultStatus.WaitingForDecision;
			// TODO: this.historyRepo.LogAction(DecisionActions.Waiting, "", user, customer);
			var stage = OpportunityStage.s40.DescriptionAttr();

			newDecision.DecisionNameID = (int)DecisionActions.Waiting;

			// this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage, }
			);
			*/
		} // ReturnCustomerToWaitingForDecision

		private void PendCustomer(NL_Decisions newDecision) {
			/*
			customer.IsWaitingForSignature = this.decisionModel.signature == 1;
			customer.CreditResult = CreditResultStatus.ApprovedPending;
			customer.PendingStatus = PendingStatus.Manual;
			customer.ManagerApprovedSum = oldCashRequest.ApprovedSum();
			// TODO: this.historyRepo.LogAction(DecisionActions.Pending, "", user, customer);

			newDecision.DecisionNameID = (int)DecisionActions.Pending;
			// this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

			var stage = this.decisionModel.signature == 1
				? OpportunityStage.s75.DescriptionAttr()
				: OpportunityStage.s50.DescriptionAttr();

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage }
			);
			*/
		} // PendCustomer

		private void EscalateCustomer(NL_Decisions newDecision) {
			/*
			customer.CreditResult = CreditResultStatus.Escalated;
			customer.DateEscalated = DateTime.UtcNow;
			customer.EscalationReason = this.decisionModel.reason;
			// TODO: this.historyRepo.LogAction(DecisionActions.Escalate, this.decisionModel.reason, user, customer);
			var stage = OpportunityStage.s20.DescriptionAttr();

			try {
				this.serviceClient.Instance.Escalated(customer.Id, this.context.UserId);
			} catch (Exception e) {
				Warning = "Failed to send 'escalated' email: " + e.Message;
				log.Warn(e, "Failed to send 'escalated' email.");
			} // try

			newDecision.DecisionNameID = (int)DecisionActions.Escalate;
			//this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage }
			);
			*/
		} // EscalateCustomer

		private void RejectCustomer(NL_Decisions newDecision) {
			this.decisionToApply.Customer.DateRejected = this.now;
			this.decisionToApply.Customer.RejectedReason = this.decisionModel.reason;
			this.decisionToApply.Customer.NumRejects = this.currentState.NumOfPrevRejections;

			this.decisionToApply.CashRequest.RejectionReasons.Clear();
			this.decisionToApply.CashRequest.RejectionReasons.AddRange(this.decisionModel.rejectionReasons);

			new ManuallyReject(this.decisionToApply, DB, Log).ExecuteNonQuery();

			bool bSendToCustomer = !(this.currentState.FilledByBroker && (this.currentState.NumOfPrevApprovals == 0));

			if (!this.currentState.EmailSendingBanned) {
				FireToBackground(
					"send 'rejected' email",
					() => new RejectUser(this.decisionModel.customerID, bSendToCustomer).Execute(),
					e => Warning = "Failed to send 'reject user' email: " + e.Message
				);
			} // if

			newDecision.DecisionNameID = (int)DecisionActions.Reject;

			//this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, this.decisionModel.rejectionReasons.Select(x => new NL_DecisionRejectReasons { RejectReasonID = x }).ToArray());

			FireToBackground(
				"update Sales Force opportunity",
				() =>
					new UpdateOpportunity(
						this.decisionModel.customerID,
						new OpportunityModel {
							Email = this.currentState.Email,
							CloseDate = this.now,
							DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
							DealLostReason = this.decisionModel.reason,
						}
					).Execute()
			);
		} // RejectCustomer

		private void ApproveCustomer(NL_Decisions newDecision) {
			this.decisionToApply.Customer.DateApproved = this.now;
			this.decisionToApply.Customer.ApprovedReason = this.decisionModel.reason;

			this.decisionToApply.Customer.CreditSum = this.currentState.OfferedCreditLine;
			this.decisionToApply.Customer.ManagerApprovedSum = this.currentState.OfferedCreditLine;
			this.decisionToApply.Customer.NumApproves = 1 + this.currentState.NumOfPrevApprovals;
			this.decisionToApply.Customer.IsLoanTypeSelectionAllowed = this.currentState.IsLoanTypeSelectionAllowed;

			this.decisionToApply.CashRequest.ManagerApprovedSum = (int)this.currentState.OfferedCreditLine;

			new ManuallyApprove(this.decisionToApply, DB, Log).ExecuteNonQuery();

			bool bSendBrokerForceResetCustomerPassword =
				this.currentState.FilledByBroker &&
				(this.currentState.NumOfPrevApprovals == 0);

			bool bSendApprovedUser = !this.currentState.EmailSendingBanned;

			int validForHours = (int)(this.currentState.OfferValidUntil - this.currentState.OfferStart).TotalHours;

			if (bSendBrokerForceResetCustomerPassword && bSendApprovedUser) {
				FireToBackground(
					"send 'approved' and force reset customer password",
					() => {
						var stra = new ApprovedUser(
							this.decisionModel.customerID,
							this.currentState.OfferedCreditLine,
							validForHours,
							this.currentState.NumOfPrevApprovals == 0
						);
						stra.SendToCustomer = false;
						stra.Execute();
					},
					e => Warning = "Failed to force reset customer password and send 'approved user' email: " + e.Message
				);
			} else if (bSendApprovedUser) {
				FireToBackground(
					"send 'approved' email",
					() =>
						new ApprovedUser(
							this.decisionModel.customerID,
							this.currentState.OfferedCreditLine,
							validForHours,
							this.currentState.NumOfPrevApprovals == 0
						).Execute(),
					e => Warning = "Failed to send 'approved user' email: " + e.Message
				);
			} else if (bSendBrokerForceResetCustomerPassword) {
				FireToBackground(
					"force reset customer password",
					() => new BrokerForceResetCustomerPassword(this.decisionModel.customerID).Execute()
				);
			} // if

			newDecision.DecisionNameID = (int)DecisionActions.Approve;

			// NL_Offers lastOffer = this.serviceClient.Instance.GetLastOffer(
			// 	this.decisionToApply.CashRequest.UnderwriterID,
			// 	this.decisionToApply.Customer.ID
			// );

			// var decisionID = this.serviceClient.Instance.AddDecision(
			// 	this.decisionToApply.CashRequest.UnderwriterID,
			// 	this.decisionToApply.Customer.ID
			// 	newDecision,
			// 	this.decisionToApply.CashRequest.ID,
			// 	null
			// );

			// lastOffer.DecisionID = decisionID.Value;
			// lastOffer.CreatedTime = this.now;
			// this.serviceClient.Instance.AddOffer(
			// 	this.decisionToApply.CashRequest.UnderwriterID,
			// 	this.decisionToApply.Customer.ID
			// 	lastOffer
			// );

			FireToBackground("update Sales Force opportunity", () =>
				new UpdateOpportunity(
					this.decisionModel.customerID,
					new OpportunityModel {
						Email = this.currentState.Email,
						Stage = OpportunityStage.s90.DescriptionAttr(),
						ApprovedAmount = (int)this.currentState.OfferedCreditLine,
						ExpectedEndDate = this.currentState.OfferValidUntil
					}
				).Execute()
			);
		} // ApproveCustomer

		private DecisionToApply decisionToApply;
		private CurrentCustomerDecisionState currentState;

		private readonly DecisionModel decisionModel;
		private readonly DateTime now;
	} // class ApplyManualDecision
} // namespace

