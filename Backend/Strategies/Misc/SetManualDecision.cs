namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class SetManualDecision : AStrategy {
		public SetManualDecision(DecisionModel model) {
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
				Log.Info("Decision cannot be applied to model.attemptID = {0}: '{1}.", this.decisionModel.attemptID, Error);
				break;

			case ChangeDecisionOption.BlockedByError:
				Log.Alert("Decision cannot be applied to model.attemptID = {0}: '{1}.", this.decisionModel.attemptID, Error);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			/*
			var underwriter = this.context.User;
			var customer = this.customersRepo.GetChecked(this.decisionModel.customerID);

			customer.CreditResult = this.decisionModel.status;
			customer.UnderwriterName = underwriter.Name;

			var request = customer.LastCashRequest ?? new CashRequest();
			request.IdUnderwriter = this.decisionModel.underwriterID;
			request.UnderwriterDecisionDate = DateTime.UtcNow;
			request.UnderwriterDecision = this.decisionModel.status;
			request.UnderwriterComment = this.decisionModel.reason;

			var newDecision = new NL_Decisions {
				UserID = this.decisionModel.underwriterID,
				DecisionTime = this.now,
				Notes = this.decisionModel.reason,
			};

			int numOfPrevApprovals = customer.DecisionHistory.Count(x => x.Action == DecisionActions.Approve);

			if (this.decisionModel.status != CreditResultStatus.ApprovedPending)
				customer.IsWaitingForSignature = false;

			bool runSilentAutomation = false;

			switch (this.decisionModel.status) {
			case CreditResultStatus.Approved:
				if (customer.WizardStep.TheLastOne)
					runSilentAutomation = true;

				ApproveCustomer(customer, request, newDecision, numOfPrevApprovals);
				break;

			case CreditResultStatus.Rejected:
				runSilentAutomation = true;
				RejectCustomer(customer, request, newDecision, numOfPrevApprovals);
				break;

			case CreditResultStatus.Escalated:
				EscalateCustomer(customer, request, newDecision);
				break;

			case CreditResultStatus.ApprovedPending:
				runSilentAutomation = true;
				PendCustomer(customer, request, newDecision);
				break;

			case CreditResultStatus.WaitingForDecision:
				ReturnCustomerToWaitingForDecision(customer, underwriter, request, newDecision);
				break;
			} // switch

			// send final decision data (0002) to Alibaba parther (if exists)
			if (customer.IsAlibaba && this.decisionModel.status.In(CreditResultStatus.Rejected, CreditResultStatus.Approved)) {
				this.serviceClient.Instance.DataSharing(
					customer.Id,
					ServiceClientProxy.EzServiceReference.AlibabaBusinessType.APPLICATION_REVIEW,
					this.context.UserId
				);
			} // if

			if (runSilentAutomation)
				this.serviceClient.Instance.SilentAutomation(customer.Id, underwriter.Id);
			*/

			Log.Debug("Done applying manual decision by model: {0}.", this.decisionModel.Stringify());
		} // Execute

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

			DB.GetFirst(
				"LoadCurrentCustomerDecisionState",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.decisionModel.customerID),
				new QueryParameter("@CashRequestID", this.decisionModel.cashRequestID)
			);

			return ChangeDecisionOption.Available;
		} // CanChangeDecision

		private void ReturnCustomerToWaitingForDecision(
			Customer customer,
			CashRequest oldCashRequest,
			NL_Decisions newDecision
		) {
			/*
			customer.CreditResult = CreditResultStatus.WaitingForDecision;
			this.historyRepo.LogAction(DecisionActions.Waiting, "", user, customer);
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

		private void PendCustomer(
			Customer customer,
			CashRequest oldCashRequest,
			NL_Decisions newDecision
		) {
			/*
			customer.IsWaitingForSignature = this.decisionModel.signature == 1;
			customer.CreditResult = CreditResultStatus.ApprovedPending;
			customer.PendingStatus = PendingStatus.Manual;
			customer.ManagerApprovedSum = oldCashRequest.ApprovedSum();
			this.historyRepo.LogAction(DecisionActions.Pending, "", user, customer);

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

		private void EscalateCustomer(
			Customer customer,
			CashRequest oldCashRequest,
			NL_Decisions newDecision
		) {
			/*
			customer.CreditResult = CreditResultStatus.Escalated;
			customer.DateEscalated = DateTime.UtcNow;
			customer.EscalationReason = this.decisionModel.reason;
			this.historyRepo.LogAction(DecisionActions.Escalate, this.decisionModel.reason, user, customer);
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

		private void RejectCustomer(
			Customer customer,
			CashRequest oldCashRequest,
			NL_Decisions newDecision,
			int numOfPreviousApprovals
		) {
			/*
			customer.DateRejected = this.now;
			customer.RejectedReason = this.decisionModel.reason;
			customer.Status = Status.Rejected;
			customer.NumRejects++;
			this.historyRepo.LogAction(DecisionActions.Reject, model.reason, user, customer, this.decisionModel.rejectionReasons);

			oldCashRequest.ManagerApprovedSum = null;

			bool bSendToCustomer = true;

			if (customer.FilledByBroker) {
				if (numOfPreviousApprovals == 0)
					bSendToCustomer = false;
			} // if

			if (!oldCashRequest.EmailSendingBanned) {
				try {
					this.serviceClient.Instance.RejectUser(user.Id, customer.Id, bSendToCustomer);
				} catch (Exception e) {
					Warning = "Failed to send 'reject user' email: " + e.Message;
					log.Warn(e, "Failed to send 'reject user' email.");
				} // try
			} // if

			newDecision.DecisionNameID = (int)DecisionActions.Reject;

			var rejectReasons = this.decisionModel.rejectionReasons.Select(x => new NL_DecisionRejectReasons {
				RejectReasonID = x
			}).ToArray();

			//this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, rejectReasons);

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel {
					Email = customer.Name,
					CloseDate = this.now,
					DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
					DealLostReason = customer.RejectedReason,
				}
			);
			*/
		} // RejectCustomer

		private void ApproveCustomer(
			Customer customer,
			CashRequest oldCashRequest,
			NL_Decisions newDecision,
			int numOfPreviousApprovals
		) {
			/*
			if (!customer.WizardStep.TheLastOne) {
				try {
					customer.AddAlibabaDefaultBankAccount();

					var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(
						CurrentValues.Instance.FinishWizardForApproved
					);
					oArgs.CustomerID = customer.Id;
					oArgs.CashRequestOriginator = CashRequestOriginator.Approved;

					this.serviceClient.Instance.FinishWizard(oArgs, user.Id);
				} catch (Exception e) {
					log.Alert(e, "Something went horribly not so cool while finishing customer's wizard.");
				} // try
			} // if

			customer.DateApproved = this.now;
			customer.Status = Status.Approved;
			customer.ApprovedReason = this.decisionModel.reason;

			var sum = oldCashRequest.ApprovedSum();
			if (sum <= 0)
				throw new Exception("Credit sum cannot be zero or less");

			this.loanLimit.Check(sum);

			customer.CreditSum = sum;
			customer.ManagerApprovedSum = sum;
			customer.NumApproves++;
			customer.IsLoanTypeSelectionAllowed = oldCashRequest.IsLoanTypeSelectionAllowed;
			oldCashRequest.ManagerApprovedSum = (double?)sum;

			this.historyRepo.LogAction(DecisionActions.Approve, this.decisionModel.reason, user, customer);

			bool bSendBrokerForceResetCustomerPassword = false;

			if (customer.FilledByBroker) {
				if (numOfPreviousApprovals == 0)
					bSendBrokerForceResetCustomerPassword = true;
			} // if

			bool bSendApprovedUser = !oldCashRequest.EmailSendingBanned;
			this.session.Flush();

			int validForHours = (int)(oldCashRequest.OfferValidUntil - oldCashRequest.OfferStart).Value.TotalHours;

			if (bSendBrokerForceResetCustomerPassword && bSendApprovedUser) {
				try {
					this.serviceClient.Instance.BrokerApproveAndResetCustomerPassword(
						user.Id,
						customer.Id,
						sum,
						validForHours,
						numOfPreviousApprovals == 0
						);
				} catch (Exception e) {
					Warning = "Failed to force reset customer password and send 'approved user' email: " + e.Message;
					log.Alert(e, "Failed to force reset customer password and send 'approved user' email.");
				} // try
			} else if (bSendApprovedUser) {
				try {
					this.serviceClient.Instance.ApprovedUser(
						user.Id,
						customer.Id,
						sum,
						validForHours,
						numOfPreviousApprovals == 0
						);
				} catch (Exception e) {
					Warning = "Failed to send 'approved user' email: " + e.Message;
					log.Warn(e, "Failed to send 'approved user' email.");
				} // try
			} else if (bSendBrokerForceResetCustomerPassword) {
				try {
					this.serviceClient.Instance.BrokerForceResetCustomerPassword(user.Id, customer.Id);
				} catch (Exception e) {
					log.Alert(e, "Something went horribly not so cool while resetting customer password.");
				} // try
			} // if

			newDecision.DecisionNameID = (int)DecisionActions.Approve;

			//NL_Offers lastOffer = this.serviceClient.Instance.GetLastOffer(user.Id, customer.Id);
			//var decisionID = this.serviceClient.Instance.AddDecision(user.Id, customer.Id,
			// newDecision, oldCashRequest.Id, null);

			//lastOffer.DecisionID = decisionID.Value;
			//lastOffer.CreatedTime = this.now;
			//this.serviceClient.Instance.AddOffer(user.Id, customer.Id, lastOffer);

			var stage = OpportunityStage.s90.DescriptionAttr();

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel {
					Email = customer.Name,
					Stage = stage,
					ApprovedAmount = (int)sum,
					ExpectedEndDate = oldCashRequest.OfferValidUntil
				}
			);
			*/
		} // ApproveCustomer

		private enum ChangeDecisionOption {
			Available,
			BlockedNoCashRequest,
			BlockedByConcurrency,
			BlockedByError,
		} // enum ChangeDecisionOption

		private readonly DecisionModel decisionModel;
		private readonly DateTime now;
	} // class SetManualDecision
} // namespace

