namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using NHibernate;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using Models;
	using Code;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Infrastructure.csrf;
	using SalesForceLib.Models;

	// In order to block sales from UW dashboard uncomment and add this permission to all relevant roles:
	// [Permission(Name = "Underwriter")]

	public class CustomersController : Controller {
		public CustomersController(
			ISession session,
			CustomerStatusesRepository customerStatusesRepo,
			CustomerRepository customersRepo,
			IDecisionHistoryRepository historyRepo,
			IWorkplaceContext context,
			LoanLimit limit,
			MarketPlaceRepository mpType,
			RejectReasonRepository rejectReasonRepo
		) {
			this.context = context;
			this.session = session;
			this.customersRepo = customersRepo;
			this.serviceClient = new ServiceClient();
			this.historyRepo = historyRepo;
			this.loanLimit = limit;
			this.mpType = mpType;

			this.customerStatusesRepo = customerStatusesRepo;

			this.rejectReasonRepo = rejectReasonRepo;
		} // constructor

		public ViewResult Index() {
			var grids = new LoansGrids {
				IsEscalated = this.context.User.Roles.Any(r => r.Name == "manager"),
				MpTypes = this.mpType.GetAll().ToList(),
				CollectionStatuses = this.customerStatusesRepo.GetVisible().ToList(),
				MaxLoan = this.loanLimit.GetMaxLimit(),
				ManagerMaxLoan = CurrentValues.Instance.ManagerMaxLoan
			};

			return View(grids);
		} // Index

		[Ajax]
		[HttpGet]
		public JsonResult RejectReasons() {
			return Json(new { reasons = this.rejectReasonRepo.GetAll().ToList() }, JsonRequestBehavior.AllowGet);
		} // RejectReasons

		[Transactional]
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SetDecision(DecisionModel model) {
			var user = this.context.User;
			var customer = this.customersRepo.GetChecked(model.id);

			DateTime now = DateTime.UtcNow;
			customer.CreditResult = model.status;
			customer.UnderwriterName = user.Name;

			var request = customer.LastCashRequest ?? new CashRequest();
			request.IdUnderwriter = user.Id;
			request.UnderwriterDecisionDate = DateTime.UtcNow;
			request.UnderwriterDecision = model.status;
			request.UnderwriterComment = model.reason;

			var newDecision = new NL_Decisions {
				UserID = user.Id,
				DecisionTime = now,
				Notes = model.reason
			};

			string sWarning = string.Empty;
			int numOfPreviousApprovals = customer.DecisionHistory.Count(x => x.Action == DecisionActions.Approve);

			if (model.status != CreditResultStatus.ApprovedPending)
				customer.IsWaitingForSignature = false;

			bool runSilentAutomation = false;

			switch (model.status) {
			case CreditResultStatus.Approved:
				if (customer.WizardStep.TheLastOne)
					runSilentAutomation = true;

				ApproveCustomer(model, customer, user, now, request, newDecision, numOfPreviousApprovals, ref sWarning);
				break;

			case CreditResultStatus.Rejected:
				runSilentAutomation = true;
				RejectCustomer(model, customer, now, user, request, newDecision, numOfPreviousApprovals, ref sWarning);
				break;

			case CreditResultStatus.Escalated:
				EscalateCustomer(model, customer, user, request, newDecision, ref sWarning);
				break;

			case CreditResultStatus.ApprovedPending:
				runSilentAutomation = true;
				PendCustomer(model, customer, request, user, newDecision);
				break;

			case CreditResultStatus.WaitingForDecision:
				ReturnCustomerToWaitingForDecision(customer, user, request, newDecision);
				break;
			} // switch

			log.Debug(
				"update decision for customer {0} with decision {1} signature {2}",
				customer.Id,
				model.status,
				model.signature
			);

			// send final decision data (0002) to Alibaba parther (if exists)
			if (customer.IsAlibaba && model.status.In(CreditResultStatus.Rejected, CreditResultStatus.Approved)) {
				this.serviceClient.Instance.DataSharing(
					customer.Id,
					ServiceClientProxy.EzServiceReference.AlibabaBusinessType.APPLICATION_REVIEW,
					this.context.UserId
				);
			} // if

			if (runSilentAutomation)
				this.serviceClient.Instance.SilentAutomation(customer.Id, user.Id);

			return Json(new { warning = sWarning });
		} // SetDecision

		private void ReturnCustomerToWaitingForDecision(
			Customer customer,
			User user,
			CashRequest oldCashRequest,
			NL_Decisions newDecision
		) {
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
		} // ReturnCustomerToWaitingForDecision

		private void PendCustomer(
			DecisionModel model,
			Customer customer,
			CashRequest oldCashRequest,
			User user,
			NL_Decisions newDecision
		) {
			customer.IsWaitingForSignature = model.signature == 1;
			customer.CreditResult = CreditResultStatus.ApprovedPending;
			customer.PendingStatus = PendingStatus.Manual;
			customer.ManagerApprovedSum = oldCashRequest.ApprovedSum();
			this.historyRepo.LogAction(DecisionActions.Pending, "", user, customer);

			newDecision.DecisionNameID = (int)DecisionActions.Pending;
			// this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

			var stage = model.signature == 1
				? OpportunityStage.s75.DescriptionAttr()
				: OpportunityStage.s50.DescriptionAttr();

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage }
			);
		} // PendCustomer

		private void EscalateCustomer(
			DecisionModel model,
			Customer customer,
			User user,
			CashRequest oldCashRequest,
			NL_Decisions newDecision,
			ref string sWarning
		) {
			customer.CreditResult = CreditResultStatus.Escalated;
			customer.DateEscalated = DateTime.UtcNow;
			customer.EscalationReason = model.reason;
			this.historyRepo.LogAction(DecisionActions.Escalate, model.reason, user, customer);
			var stage = OpportunityStage.s20.DescriptionAttr();

			try {
				this.serviceClient.Instance.Escalated(customer.Id, this.context.UserId);
			} catch (Exception e) {
				sWarning = "Failed to send 'escalated' email: " + e.Message;
				log.Warn(e, "Failed to send 'escalated' email.");
			} // try

			newDecision.DecisionNameID = (int)DecisionActions.Escalate;
			//this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage }
			);
		} // EscalateCustomer

		private void RejectCustomer(
			DecisionModel model,
			Customer customer,
			DateTime now,
			User user,
			CashRequest oldCashRequest,
			NL_Decisions newDecision,
			int numOfPreviousApprovals,
			ref string sWarning
		) {
			customer.DateRejected = now;
			customer.RejectedReason = model.reason;
			customer.Status = Status.Rejected;
			customer.NumRejects++;
			this.historyRepo.LogAction(DecisionActions.Reject, model.reason, user, customer, model.rejectionReasons);

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
					sWarning = "Failed to send 'reject user' email: " + e.Message;
					log.Warn(e, "Failed to send 'reject user' email.");
				} // try
			} // if

			newDecision.DecisionNameID = (int)DecisionActions.Reject;

			var rejectReasons = model.rejectionReasons.Select(x => new NL_DecisionRejectReasons {
				RejectReasonID = x
			}).ToArray();

			//this.serviceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, rejectReasons);

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				this.context.UserId,
				customer.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel {
					Email = customer.Name,
					CloseDate = now,
					DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
					DealLostReason = customer.RejectedReason
				}
			);
		} // RejectCustomer

		private void ApproveCustomer(
			DecisionModel model,
			Customer customer,
			User user,
			DateTime now,
			CashRequest oldCashRequest,
			NL_Decisions newDecision,
			int numOfPreviousApprovals,
			ref string sWarning
		) {
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

			customer.DateApproved = now;
			customer.Status = Status.Approved;
			customer.ApprovedReason = model.reason;

			var sum = oldCashRequest.ApprovedSum();
			if (sum <= 0)
				throw new Exception("Credit sum cannot be zero or less");

			this.loanLimit.Check(sum);

			customer.CreditSum = sum;
			customer.ManagerApprovedSum = sum;
			customer.NumApproves++;
			customer.IsLoanTypeSelectionAllowed = oldCashRequest.IsLoanTypeSelectionAllowed;
			oldCashRequest.ManagerApprovedSum = (double?)sum;

			this.historyRepo.LogAction(DecisionActions.Approve, model.reason, user, customer);

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
					sWarning = "Failed to force reset customer password and send 'approved user' email: " + e.Message;
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
					sWarning = "Failed to send 'approved user' email: " + e.Message;
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
			//lastOffer.CreatedTime = now;
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
		} // ApproveCustomer

		private readonly ISession session;
		private readonly CustomerRepository customersRepo;
		private readonly ServiceClient serviceClient;
		private readonly IDecisionHistoryRepository historyRepo;
		private readonly LoanLimit loanLimit;
		private readonly IWorkplaceContext context;
		private readonly MarketPlaceRepository mpType;

		private readonly CustomerStatusesRepository customerStatusesRepo;
		private readonly RejectReasonRepository rejectReasonRepo;

		private static readonly ASafeLog log = new SafeILog(typeof(CustomersController));
	} // class CustomersController
} // namespace
