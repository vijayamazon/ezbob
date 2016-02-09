namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Linq;
	using System.Reflection;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Alibaba;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.Investor;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.NewLoan;
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

		/// <exception cref="ArgumentOutOfRangeException">Condition. </exception>
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
				this.decisionModel.cashRequestID,
				this.currentState.CashRequestTimestamp
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

			bool notifyAlibaba = false;

			switch (this.decisionModel.status) {
			case CreditResultStatus.Approved:
				if (ApproveCustomer(newDecision)) {
					if (this.currentState.LastWizardStep)
						silentAutomationCaller = SilentAutomation.Callers.ManuallyApproved;

					notifyAlibaba = this.currentState.IsAlibaba;
				} // if
				break;

			case CreditResultStatus.PendingInvestor:
				if (ApproveCustomer(newDecision)) {
					notifyAlibaba = this.currentState.IsAlibaba;
				} // if
				break;

			case CreditResultStatus.Rejected:
				if (RejectCustomer(newDecision)) {
					silentAutomationCaller = SilentAutomation.Callers.ManuallyRejected;
					notifyAlibaba = this.currentState.IsAlibaba;
				} // if
				break;

			case CreditResultStatus.Escalated:
				EscalateCustomer(newDecision);
				break;

			case CreditResultStatus.ApprovedPending:
				if (SuspendCustomer(newDecision))
					silentAutomationCaller = SilentAutomation.Callers.ManuallySuspended;
				break;

			case CreditResultStatus.WaitingForDecision:
				ReturnCustomerToWaitingForDecision(newDecision);
				break;
			} // switch

			if (notifyAlibaba)
				FireToBackground(new DataSharing(this.decisionModel.customerID, AlibabaBusinessType.APPLICATION_REVIEW));

			if (silentAutomationCaller.HasValue) {
				FireToBackground(
					new SilentAutomation(this.decisionModel.customerID)
						.SetTag(silentAutomationCaller.Value)
						.PreventMainStrategy()
				);
			} // if

			Log.Debug("Done applying manual decision by model: {0}.", this.decisionModel.Stringify());
		} // Execute

		private ChangeDecisionOption CanChangeDecision() {
			var checker = new DecisionIsChangable(this.decisionModel);

			var checkResult = checker.Precheck();

			if (checkResult != ChangeDecisionOption.Available) {
				Error = checker.Error;
				Warning = checker.Warning;
				return checkResult;
			} // if

			this.currentState = DB.FillFirst<CurrentCustomerDecisionState>(
				"LoadCurrentCustomerDecisionState",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@UnderwriterID", this.decisionModel.underwriterID),
				new QueryParameter("@CustomerID", this.decisionModel.customerID),
				new QueryParameter("@CashRequestID", this.decisionModel.cashRequestID)
			);

			checkResult = checker.ValidateAgainstCurrentState(this.currentState);

			Error = checker.Error;
			Warning = checker.Warning;

			return checkResult;
		} // CanChangeDecision

		private void ReturnCustomerToWaitingForDecision(NL_Decisions newDecision) {
			if (!SaveDecision<ManuallyUnsuspend>())
				return;

			newDecision.DecisionNameID = (int)DecisionActions.Waiting;

			AddDecision nlAddDecision = new AddDecision(newDecision, this.decisionToApply.CashRequest.ID, null);
			nlAddDecision.Context.CustomerID = this.decisionModel.customerID;
			nlAddDecision.Context.UserID = this.decisionModel.underwriterID;

			try {
				nlAddDecision.Execute();
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Error("Failed to add NL_decision. Err: {0}", ex.Message);
			}

			UpdateSalesForceOpportunity(OpportunityStage.s40);
		} // ReturnCustomerToWaitingForDecision

		private bool SuspendCustomer(NL_Decisions newDecision) {
			this.decisionToApply.Customer.IsWaitingForSignature = this.decisionModel.signature == 1;
			this.decisionToApply.Customer.ManagerApprovedSum = this.currentState.OfferedCreditLine;

			if (!SaveDecision<ManuallySuspend>())
				return false;

			newDecision.DecisionNameID = (int)DecisionActions.Pending;

			AddDecision nlAddDecision = new AddDecision(newDecision, this.decisionToApply.CashRequest.ID, null);
			nlAddDecision.Context.CustomerID = this.decisionModel.customerID;
			nlAddDecision.Context.UserID = this.decisionModel.underwriterID;
			nlAddDecision.Execute();

			UpdateSalesForceOpportunity((this.decisionModel.signature == 1) ? OpportunityStage.s75 : OpportunityStage.s50);

			return true;
		} // SuspendCustomer

		private void EscalateCustomer(NL_Decisions newDecision) {
			this.decisionToApply.Customer.DateEscalated = this.now;
			this.decisionToApply.Customer.EscalationReason = this.decisionModel.reason;

			if (!SaveDecision<ManuallyEscalate>())
				return;

			FireToBackground(
				new Escalated(this.decisionModel.customerID),
				e => Warning = "Failed to send 'escalated' email: " + e.Message
			);

			newDecision.DecisionNameID = (int)DecisionActions.Escalate;

			AddDecision nlAddDecision = new AddDecision(newDecision, this.decisionToApply.CashRequest.ID, null);
			nlAddDecision.Context.CustomerID = this.decisionModel.customerID;
			nlAddDecision.Context.UserID = this.decisionModel.underwriterID;
			try {
				nlAddDecision.Execute();
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Error("Failed to add NL_decision. Err: {0}", ex.Message);
			}

			UpdateSalesForceOpportunity(OpportunityStage.s20);
		} // EscalateCustomer

		private bool RejectCustomer(NL_Decisions newDecision) {
			this.decisionToApply.Customer.DateRejected = this.now;
			this.decisionToApply.Customer.RejectedReason = this.decisionModel.reason;
			this.decisionToApply.Customer.NumRejects = 1 + this.currentState.NumOfPrevRejections;

			this.decisionToApply.CashRequest.RejectionReasons.Clear();
			this.decisionToApply.CashRequest.RejectionReasons.AddRange(this.decisionModel.rejectionReasons);

			if (!SaveDecision<ManuallyReject>())
				return false;

			bool bSendToCustomer = !(this.currentState.FilledByBroker && (this.currentState.NumOfPrevApprovals == 0));

			if (!this.currentState.EmailSendingBanned) {
				FireToBackground(
					new RejectUser(this.decisionModel.customerID, bSendToCustomer),
					e => Warning = "Failed to send 'reject user' email: " + e.Message
				);
			} // if

			newDecision.DecisionNameID = (int)DecisionActions.Reject;

			AddDecision nlAddDecision = new AddDecision(newDecision, this.decisionToApply.CashRequest.ID, this.decisionModel.rejectionReasons.Select(x => new NL_DecisionRejectReasons { RejectReasonID = x }).ToList());
			nlAddDecision.Context.CustomerID = this.decisionModel.customerID;
			nlAddDecision.Context.UserID = this.decisionModel.underwriterID;
			try {
				nlAddDecision.Execute();
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Error("Failed to add NL_decision. Err: {0}", ex.Message);
			}

			UpdateSalesForceOpportunity(null, model => {
				model.CloseDate = this.now;
				model.DealCloseType = OpportunityDealCloseReason.Lost.ToString();
				model.DealLostReason = this.decisionModel.reason;
			});

			return true;
		} // RejectCustomer


		private bool ApproveCustomer(NL_Decisions newDecision) {
			LinkOfferToInvestor linkOfferToInvestor = new LinkOfferToInvestor(this.decisionToApply.Customer.ID, this.decisionToApply.CashRequest.ID, this.decisionModel.ForceInvestor, this.decisionModel.InvestorID, this.decisionModel.underwriterID);
			linkOfferToInvestor.Execute();

			Log.Info("ApproveCustomer Decision {0} for Customer {1} cr {2} OP {3} FoundInvestor {4}",
				this.decisionToApply.CashRequest.UnderwriterDecision,
				this.decisionToApply.Customer.ID,
				this.decisionToApply.CashRequest.ID,
				linkOfferToInvestor.IsForOpenPlatform,
				linkOfferToInvestor.FoundInvestor);

			// "open paltform" case; was approved and investor not found 
			if (this.decisionModel.status == CreditResultStatus.Approved && linkOfferToInvestor.IsForOpenPlatform && !linkOfferToInvestor.FoundInvestor) {
				PendingInvestor(newDecision);
				return false;
			}

			// "open paltform" case; investor not found again
			if (this.decisionModel.status == CreditResultStatus.PendingInvestor && linkOfferToInvestor.IsForOpenPlatform && !linkOfferToInvestor.FoundInvestor) {
				Log.Info("Investor not found for customer {0} cr {1}, the underwriter decision / credit result are not changed",
					this.decisionToApply.Customer.ID,
					this.decisionToApply.CashRequest.ID);
				return false;
			}

			this.decisionToApply.CashRequest.UnderwriterDecision = CreditResultStatus.Approved.ToString();

			this.decisionToApply.Customer.DateApproved = this.now;
			this.decisionToApply.Customer.ApprovedReason = this.decisionModel.reason;

			this.decisionToApply.Customer.CreditSum = this.currentState.OfferedCreditLine;
			this.decisionToApply.Customer.ManagerApprovedSum = this.currentState.OfferedCreditLine;
			this.decisionToApply.Customer.NumApproves = 1 + this.currentState.NumOfPrevApprovals;
			this.decisionToApply.Customer.IsLoanTypeSelectionAllowed = this.currentState.IsLoanTypeSelectionAllowed;

			this.decisionToApply.CashRequest.ManagerApprovedSum = (int)this.currentState.OfferedCreditLine;

			if (!SaveDecision<ManuallyApprove>())
				return false;

			bool bSendBrokerForceResetCustomerPassword =
				this.currentState.FilledByBroker &&
				(this.currentState.NumOfPrevApprovals == 0);

			bool bSendApprovedUser = !this.currentState.EmailSendingBanned;

			int validForHours = (int)(this.currentState.OfferValidUntil - this.currentState.OfferStart).TotalHours;

			if (bSendBrokerForceResetCustomerPassword && bSendApprovedUser) {
				FireToBackground(
					 new ApprovedUser(
						this.decisionModel.customerID,
						this.currentState.OfferedCreditLine,
						validForHours,
						this.currentState.NumOfPrevApprovals == 0
					) { SendToCustomer = false, },
					e => Warning = "Failed to force reset customer password and send 'approved user' email: " + e.Message
				);
			} else if (bSendApprovedUser) {
				FireToBackground(
					new ApprovedUser(
						this.decisionModel.customerID,
						this.currentState.OfferedCreditLine,
						validForHours,
						this.currentState.NumOfPrevApprovals == 0
					),
					e => Warning = "Failed to send 'approved user' email: " + e.Message
				);
			} else if (bSendBrokerForceResetCustomerPassword)
				FireToBackground(new BrokerForceResetCustomerPassword(this.decisionModel.customerID));

			newDecision.DecisionNameID = (int)DecisionActions.Approve;

			AddDecision nlAddDecision = new AddDecision(newDecision, this.decisionToApply.CashRequest.ID, null);
			nlAddDecision.Context.CustomerID = this.decisionModel.customerID;
			nlAddDecision.Context.UserID = this.decisionModel.underwriterID;

			try {
				try {
					nlAddDecision.Execute();
					Log.Debug("nl AddDecision {0}, Error: {1}", nlAddDecision.DecisionID, nlAddDecision.Error);

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Log.Error("Failed to add NL_decision. Err: {0}", ex.Message);
				}

				NL_Offers nlOffer = new NL_Offers() {
					DecisionID = nlAddDecision.DecisionID,
					CreatedTime = this.currentState.CreationDate,
					Amount = this.currentState.OfferedCreditLine,
					BrokerSetupFeePercent = this.currentState.BrokerSetupFeePercent,
					//IsAmountSelectionAllowed = this.currentState.
					SendEmailNotification = !this.currentState.EmailSendingBanned,
					StartTime = this.currentState.OfferStart,
					EndTime = this.currentState.OfferValidUntil,
					IsLoanTypeSelectionAllowed = this.currentState.IsLoanTypeSelectionAllowed == 1,
					Notes = this.decisionToApply.CashRequest.UnderwriterComment + " old cr " + this.decisionToApply.CashRequest.ID,
					MonthlyInterestRate = this.currentState.InterestRate,
					LoanSourceID = this.currentState.LoanSourceID,
					DiscountPlanID = this.currentState.DiscountPlanID,
					LoanTypeID = this.currentState.LoanTypeID,
					RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month,
					RepaymentCount = this.currentState.RepaymentPeriod, // ApprovedRepaymentPeriod???
					IsRepaymentPeriodSelectionAllowed = this.currentState.IsCustomerRepaymentPeriodSelectionAllowed
				};

				Log.Debug("Adding nl offer: {0}", nlOffer);

				NL_OfferFees setupFee = new NL_OfferFees() {
					LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
					Percent = this.currentState.ManualSetupFeePercent,
					OneTimePartPercent = 1,
					DistributedPartPercent = 0
				};

				if (this.currentState.SpreadSetupFee) {
					setupFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
					setupFee.OneTimePartPercent = 0;
					setupFee.DistributedPartPercent = 1;
				}
				NL_OfferFees[] ofeerFees = { setupFee };

				AddOffer sAddOffer = new AddOffer(nlOffer, ofeerFees);
				sAddOffer.Context.CustomerID = this.decisionToApply.Customer.ID;
				sAddOffer.Context.UserID = this.decisionModel.underwriterID;

				try {
					sAddOffer.Execute();
					Log.Debug("nl offer added: {0}, Error: {1}", sAddOffer.OfferID, sAddOffer.Error);
					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Log.Error("Failed to AddOffer. Err: {0}", ex.Message);
				}

				// ReSharper disable once CatchAllClause
			} catch (Exception nlException) {
				Log.Error("Failed to run NL offer/decision Err: {0}", nlException.Message);
			}

			UpdateSalesForceOpportunity(OpportunityStage.s90, model => {
				model.ApprovedAmount = (int)this.currentState.OfferedCreditLine;
				model.ExpectedEndDate = this.currentState.OfferValidUntil;
			});

			return true;
		}// ApproveCustomer

		private void PendingInvestor(NL_Decisions newDecision) {
			Log.Info("Investor not found for customer {0} cr {1}, mark as pending investor",
				this.decisionToApply.Customer.ID,
				this.decisionToApply.CashRequest.ID);

			this.decisionToApply.Customer.CreditResult = CreditResultStatus.PendingInvestor.ToString();
			this.decisionToApply.CashRequest.UnderwriterDecision = CreditResultStatus.PendingInvestor.ToString();
			SaveDecision<ManuallySuspend>();

			var notifyRiskPendingInvestorCustomer = new NotifyRiskPendingInvestorOffer(
				this.decisionToApply.Customer.ID,
				this.currentState.OfferedCreditLine,
				this.currentState.OfferValidUntil
			);
			notifyRiskPendingInvestorCustomer.Execute();

			//TODO newDecision PendingInvestor status
		}

		private bool SaveDecision<T>() where T : AApplyManualDecisionBase {
			string result;

			try {
				ConstructorInfo constructorInfo = typeof(T).GetConstructors().FirstOrDefault(
					ci => ci.GetParameters().Length == 3
				);

				if (constructorInfo == null)
					result = "Failed to initialize stored procedure " + typeof(T).Name;
				else {
					T sp = (T)constructorInfo.Invoke(new object[] {
						this.decisionToApply, DB, Log
					});

					if (sp == null)
						result = "Failed to create stored procedure " + typeof(T).Name;
					else
						result = sp.ExecuteScalar<string>();
				} // if
			} catch (Exception e) {
				Log.Alert(e, "Failed to save approval to DB.");
				result = "Failed to save approval to the database: " + e.Message;
			} // try

			if (result == OK)
				return true;

			Error = result;
			return false;
		} // SaveDecision

		private void UpdateSalesForceOpportunity(OpportunityStage? stage, Action<OpportunityModel> setMoreFields = null) {
			var model = new OpportunityModel { Email = this.currentState.Email, Origin = this.currentState.Origin, };

			if (stage != null)
				model.Stage = stage.Value.DescriptionAttr();

			if (setMoreFields != null)
				setMoreFields(model);

			FireToBackground(new UpdateOpportunity(this.decisionModel.customerID, model));
		} // UpdateSalesForceOpportunity

		private DecisionToApply decisionToApply;
		private CurrentCustomerDecisionState currentState;


		private readonly DecisionModel decisionModel;
		private readonly DateTime now;

		private const string OK = "OK";
	} // class ApplyManualDecision
} // namespace

