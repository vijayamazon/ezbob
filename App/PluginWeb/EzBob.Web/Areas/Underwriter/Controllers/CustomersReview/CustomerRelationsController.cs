namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Data;
	using System.Web.Mvc;

	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure.csrf;

	using log4net;
	using Scorto.Web;

	public class CustomerRelationsController : Controller {
		#region public

		#region constructor

		public CustomerRelationsController(
			CustomerRelationsRepository customerRelationsRepository,
			CRMActionsRepository crmActionsRepository,
			CRMStatusesRepository crmStatusesRepository,
			LoanRepository loanRepository
		) {
			_customerRelationsRepository = customerRelationsRepository;
			_crmActionsRepository = crmActionsRepository;
			_crmStatusesRepository = crmStatusesRepository;
			_loanRepository = loanRepository;
		} // constructor

		#endregion constructor

		#region action Index

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id) {
			var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository);
			return Json(crm.Create(id), JsonRequestBehavior.AllowGet);
		} // Index

		#endregion action Index

		#region action Actions

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Actions() {
			var actions = _crmActionsRepository.GetAll();
			return Json(actions, JsonRequestBehavior.AllowGet);
		} // Actions

		#endregion action Actions

		#region action Statuses

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Statuses() {
			var actions = _crmStatusesRepository.GetAll();
			return Json(actions, JsonRequestBehavior.AllowGet);
		} // Statuses

		#endregion action Statuses

		#region action SaveEntry

		[Ajax]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SaveEntry(bool isIncoming, int action, int status, string comment, int customerId) {
			try {
				var newEntry = new CustomerRelations {
					CustomerId = customerId,
					UserName = User.Identity.Name,
					Incoming = isIncoming,
					Action = _crmActionsRepository.Get(action),
					Status = _crmStatusesRepository.Get(status),
					Comment = comment,
					Timestamp = DateTime.UtcNow
				};

				_customerRelationsRepository.SaveOrUpdate(newEntry);

				return Json(new { success = true, error = "" });
			}
			catch (Exception e) {
				Log.ErrorFormat("Exception while trying to save customer relations new entry:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations entry." });
			} // try
		} // SaveEntry

		#endregion action SaveEntry

		#endregion public

		#region private

		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly CRMActionsRepository _crmActionsRepository;
		private readonly CRMStatusesRepository _crmStatusesRepository;
		private readonly LoanRepository _loanRepository;

		private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerRelationsController));

		#endregion private
	} // class CustomerRelationsController
} // namespace
