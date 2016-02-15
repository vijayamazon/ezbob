namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using ServiceClientProxy;
	using Models;
	using Code;
	using Infrastructure.csrf;

	// In order to block sales from UW dashboard uncomment and add this permission to all relevant roles:
	// [Permission(Name = "Underwriter")]

	public class CustomersController : Controller {
		public CustomersController(
			CustomerStatusesRepository customerStatusesRepo,
			IWorkplaceContext context,
			LoanLimit limit,
			MarketPlaceRepository mpType,
			RejectReasonRepository rejectReasonRepo
		) {
			this.context = context;
			this.loanLimit = limit;
			this.mpType = mpType;
			this.customerStatusesRepo = customerStatusesRepo;
			this.rejectReasonRepo = rejectReasonRepo;
			this.serviceClient = new ServiceClient();
		} // constructor

		public ViewResult Index() {
			var grids = new LoansGrids {
				IsEscalated = this.context.UserRoles.Any(r => r == "manager"),
				MpTypes = this.mpType.GetAll().ToList(),
				CollectionStatuses = this.customerStatusesRepo.GetVisible().ToList(),
				MaxLoan = this.loanLimit.GetMaxLimit(),
				ManagerMaxLoan = CurrentValues.Instance.ManagerMaxLoan,
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
		[Permissions(Names = new [] {"ApproveReject" ,"Escalate", "SuspendBtn", "ReturnBtn"})]
		public JsonResult SetDecision(DecisionModel model) {
			model.attemptID = Guid.NewGuid().ToString("N");
			model.underwriterID = this.context.UserId;

			log.Debug("Update decision for {0}.", model);

			try {
				var mdar = this.serviceClient.Instance.SetManualDecision(model);

				var reply = new {
					error = mdar.Map["error"],
					warning = mdar.Map["warning"],
				};

				log.Debug(
					"Reply on update decision for {0}: error is '{1}', warning is '{2}'.",
					model,
					reply.error,
					reply.warning
				);

				return Json(reply);
			} catch (Exception e) {
				log.Alert(e, "Failed to set manual decision for {0}.", model);

				return Json(new {
					error = string.Format(
						"Failed to apply your decision, please retry. Error code is '{0}'.",
						model.attemptID
					),
					warning = string.Empty,
				});
			} // try
		} // SetDecision

		private readonly LoanLimit loanLimit;
		private readonly IWorkplaceContext context;
		private readonly MarketPlaceRepository mpType;
		private readonly CustomerStatusesRepository customerStatusesRepo;
		private readonly RejectReasonRepository rejectReasonRepo;
		private readonly ServiceClient serviceClient;

		private static readonly ASafeLog log = new SafeILog(typeof(CustomersController));
	} // class CustomersController
} // namespace
