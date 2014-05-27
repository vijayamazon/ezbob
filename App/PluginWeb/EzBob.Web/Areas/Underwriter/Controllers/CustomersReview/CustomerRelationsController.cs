namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using Models;
	using Infrastructure.csrf;
	using Infrastructure.Attributes;
	using NHibernate;
	using log4net;

	public class CustomerRelationsController : Controller {
		#region public

		#region constructor

		public CustomerRelationsController(
			CustomerRelationsRepository customerRelationsRepository,
			LoanRepository loanRepository,
			ISession session, CRMRanksRepository crmRanksRepository, CRMStatusesRepository crmStatusesRepository, CRMActionsRepository crmActionsRepository) {
			_customerRelationsRepository = customerRelationsRepository;
			_loanRepository = loanRepository;
			_session = session;
			_crmRanksRepository = crmRanksRepository;
			_crmStatusesRepository = crmStatusesRepository;
			_crmActionsRepository = crmActionsRepository;
			} // constructor

		#endregion constructor

		#region action Index

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id) {
			var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository, _session);
			return Json(crm.Create(id), JsonRequestBehavior.AllowGet);
		} // Index

		#endregion action Index
		
		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult CrmStatic()
		{
			var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository, _session);
			return Json(crm.GetStaticCrmModel(), JsonRequestBehavior.AllowGet);
		} // CrmStatic

		#region action SaveEntry

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveEntry(bool isIncoming, int action, int status, int rank, string comment, int customerId) {
			try {
				var newEntry = new CustomerRelations {
					CustomerId = customerId,
					UserName = User.Identity.Name,
					Incoming = isIncoming,
					Action = _crmActionsRepository.Get(action),
					Status = _crmStatusesRepository.Get(status),
					Rank = _crmRanksRepository.Get(rank),
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
		private readonly CRMRanksRepository _crmRanksRepository;
		private readonly LoanRepository _loanRepository;
		private readonly ISession _session;

		private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerRelationsController));

		#endregion private
	} // class CustomerRelationsController
} // namespace
