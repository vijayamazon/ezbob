namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using Models;
	using Infrastructure.csrf;
	using Infrastructure.Attributes;
	using NHibernate;
	using ServiceClientProxy;
	using log4net;
	using ServiceClientProxy.EzServiceReference;

	public class CustomerRelationsController : Controller {

		public CustomerRelationsController(
			CustomerRelationsRepository customerRelationsRepository,
			LoanRepository loanRepository,
			ISession session,
			CRMRanksRepository crmRanksRepository,
			CRMStatusesRepository crmStatusesRepository,
			CRMActionsRepository crmActionsRepository,
			CustomerRelationFollowUpRepository customerRelationFollowUpRepository,
			CustomerRelationStateRepository customerRelationStateRepository, 
			CustomerRepository customerRepository,
			IWorkplaceContext context) {
			_customerRelationsRepository = customerRelationsRepository;
			_loanRepository = loanRepository;
			_session = session;
			_crmRanksRepository = crmRanksRepository;
			_crmStatusesRepository = crmStatusesRepository;
			_crmActionsRepository = crmActionsRepository;
			_customerRelationFollowUpRepository = customerRelationFollowUpRepository;
			_customerRelationStateRepository = customerRelationStateRepository;
			this.customerRepository = customerRepository;
			frequentActionItemsForCustomerRepository = new FrequentActionItemsForCustomerRepository(session);
			_context = context;
			_serviceClient = new ServiceClient();
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id) {
			var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository, _session);
			return Json(crm.Create(id), JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult CrmStatic() {
			try {
				var ar = _serviceClient.Instance.CrmLoadLookups();

				return Json(new {
					CrmActions = ar.Actions,
					CrmStatuses = ar.Statuses,
					CrmRanks = ar.Ranks,
				}, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				Log.Warn("Failed to load CRM static data.", e);

				return Json(new {
					CrmActions = new IdNameModel[0],
					CrmStatuses = new CrmStatusGroup[0],
					CrmRanks = new IdNameModel[0],
				}, JsonRequestBehavior.AllowGet);
			}
		} // CrmStatic

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveEntry(string type, int action, int status, int? rank, string comment, int customerId, bool isBroker, string phoneNumber) {
			try {
				var actionItem = _crmActionsRepository.Get(action);
				var statusItem = _crmStatusesRepository.Get(status);
				var rankItem = rank.HasValue ? _crmRanksRepository.Get(rank.Value) : null;
				var newEntry = new CustomerRelations {
					CustomerId = customerId,
					UserName = User.Identity.Name,
					Type = type,
					Action = actionItem,
					Status = statusItem,
					Rank =  rankItem,
					Comment = comment,
					Timestamp = DateTime.UtcNow,
					IsBroker = isBroker,
					PhoneNumber = phoneNumber
				};

				_customerRelationsRepository.SaveOrUpdate(newEntry);
				_session.Flush();
				_customerRelationStateRepository.SaveUpdateState(customerId, false, null, newEntry);

				//Add SF activity
				var customer = this.customerRepository.ReallyTryGet(customerId);
				if (customer != null) {
					this._serviceClient.Instance.SalesForceAddActivity(_context.UserId, customerId, new ActivityModel {
						Description = comment,
						Email = customer.Name,
						StartDate = DateTime.UtcNow,
						EndDate = DateTime.UtcNow,
						IsOpportunity = false,
						Originator = _context.User.Name,
						Type = actionItem.Name,
					});
				}
				return Json(new { success = true, error = "" });
			}
			catch (Exception e) {
				Log.ErrorFormat("Exception while trying to save customer relations new entry:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations entry." });
			} // try
		} // SaveEntry

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveFollowUp(DateTime followUpDate, string comment, int customerId, bool isBroker) {
			try {
				var lastCrm = _customerRelationsRepository.GetLastCrm(customerId);

				var followUp = new CustomerRelationFollowUp {
					Comment = comment,
					CustomerId = customerId,
					DateAdded = DateTime.UtcNow,
					FollowUpDate = followUpDate,
					IsBroker = isBroker,
				};

				_customerRelationFollowUpRepository.SaveOrUpdate(followUp);
				_session.Flush();
				_customerRelationStateRepository.SaveUpdateState(customerId, true, followUp, lastCrm);

				return Json(new { success = true, error = "" });
			}
			catch (Exception e) {
				Log.Error("Exception while trying to save customer relations new entry.", e);
				return Json(new { success = false, error = "Error saving new customer relations follow up." });
			} // try
		} // SaveFollowUp

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult ChangeRank(int customerId, int rankId) {
			try {
				var crm = new CustomerRelations {
					Rank = _crmRanksRepository.Get(rankId),
					Timestamp = DateTime.UtcNow,
					Comment = "Rank change",
					UserName = User.Identity.Name,
				};

				_customerRelationsRepository.Save(crm);
				_session.Flush();
				_customerRelationStateRepository.SaveUpdateRank(customerId, crm);

				return Json(new { success = true, error = "" });
			}
			catch (Exception e) {
				Log.ErrorFormat("Exception while trying to change customer relations rank:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations rank." });
			} // try
		} // ChangeRank

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult CloseFollowUp(int customerId, int? followUpId = null) {
			try {
				var lastCrm = _customerRelationsRepository.GetLastCrm(customerId);
				CustomerRelationFollowUp lastFollowUp = followUpId == null
					? _customerRelationFollowUpRepository.GetLastFollowUp(customerId)
					: _customerRelationFollowUpRepository.Get(followUpId);

				if (lastFollowUp == null)
					return Json(new { success = false, error = "customer don't have any open follow ups please add one." });

				lastFollowUp.IsClosed = true;
				lastFollowUp.CloseDate = DateTime.UtcNow;
				_customerRelationStateRepository.SaveUpdateState(customerId, false, lastFollowUp, lastCrm);

				return Json(new { success = true, error = "" });
			}
			catch (Exception e) {
				Log.ErrorFormat("Exception while trying to close customer relations follow up:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations follow up." });
			} // try
		} // CloseFollowUp

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SendSms(int customerId, string phone, string content, bool isBroker)
		{
			try {
				var sendSmsResult = _serviceClient.Instance.SendSms(customerId, _context.UserId, phone, content);
				if (!sendSmsResult.Value)
				{
					return Json(new { success = false, error = "Failed to send SMS via twilio" });
				}

				var action = _crmActionsRepository.GetAll().FirstOrDefault(x => x.Name == "SMS");
				var status = _crmStatusesRepository.GetAll().FirstOrDefault(x => x.Name == "Note for underwriting");
				return SaveEntry("Out", 
					action != null ? action.Id : 1, 
					status != null ? status.Id : 1, 
					null,
					content, 
					customerId, 
					isBroker,
					phone);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Exception while trying to close customer relations follow up:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations follow up." });
			} // try
		} // CloseFollowUp

		[Ajax]
		[HttpPost]
		public void MarkAsPending(int customerId, string actionItems, string costumeActionItemValue)
		{
			DateTime now = DateTime.UtcNow;
			List<int> checkedIds = GetCheckedActionItemIds(actionItems);
			bool changed = false;

			// "Close" action items
			var openActionItemsInDb = frequentActionItemsForCustomerRepository.GetAll().Where(x => x.CustomerId == customerId && x.UnmarkedDate == null);
			foreach (var openActionItem in openActionItemsInDb)
			{
				if (!checkedIds.Contains(openActionItem.ItemId))
				{
					changed = true;
					openActionItem.UnmarkedDate = now;
					frequentActionItemsForCustomerRepository.SaveOrUpdate(openActionItem);
				}
			}

			// Insert new action items
			foreach (int checkedId in checkedIds)
			{
				if (!openActionItemsInDb.Any(x => x.ItemId == checkedId))
				{
					changed = true;
					var newCheckedItem = new FrequentActionItemsForCustomer { CustomerId = customerId, ItemId = checkedId, MarkedDate = now };
					frequentActionItemsForCustomerRepository.SaveOrUpdate(newCheckedItem);
				}
			}

			// Update costume action item
			Customer customer = customerRepository.Get(customerId);
			if (customer.CostumeActionItem != costumeActionItemValue)
			{
				changed = true;
				customer.CostumeActionItem = costumeActionItemValue;
			}

			if (changed)
			{
				var entry = new CustomerRelations
					{
						Action = _crmActionsRepository.GetAll().FirstOrDefault(x => x.Name == "Action items change"),
						CustomerId = customerId,
						IsBroker = false,
						Rank = _crmRanksRepository.GetAll().FirstOrDefault(x => x.Name == "High"),
						Status = _crmStatusesRepository.GetAll().FirstOrDefault(x => x.Name == "Pending"),
						Timestamp = DateTime.UtcNow,
						Type = "Internal",
						UserName = User.Identity.Name
					};
				_customerRelationsRepository.SaveOrUpdate(entry);

				if (checkedIds.Count == 0 && string.IsNullOrEmpty(customer.CostumeActionItem))
				{
					// Mark as waiting for decision
					customer.CreditResult = CreditResultStatus.WaitingForDecision;
					customerRepository.SaveOrUpdate(customer);
				}
				else
				{
					// Mark as pending
					customer.CreditResult = CreditResultStatus.ApprovedPending;
					customerRepository.SaveOrUpdate(customer);

					// Send mail
					_serviceClient.Instance.SendPendingMails(_context.UserId, customerId);
				}
			}
		}

		// TODO: improve the way this is done - pass list or model or use binding
		private List<int> GetCheckedActionItemIds(string actionItemsIds)
		{
			string[] idsStr = actionItemsIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			var checkedIds = new List<int>();
			foreach (string idStr in idsStr)
			{
				int idInt;
				if (int.TryParse(idStr, out idInt))
				{
					checkedIds.Add(idInt);
				}
			}

			return checkedIds;
		}

		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly CRMActionsRepository _crmActionsRepository;
		private readonly CRMStatusesRepository _crmStatusesRepository;
		private readonly CRMRanksRepository _crmRanksRepository;
		private readonly CustomerRelationFollowUpRepository _customerRelationFollowUpRepository;
		private readonly CustomerRelationStateRepository _customerRelationStateRepository;
		private readonly LoanRepository _loanRepository;
		private readonly CustomerRepository customerRepository;
		private readonly FrequentActionItemsForCustomerRepository frequentActionItemsForCustomerRepository;
		private readonly ISession _session;
		private readonly ServiceClient _serviceClient;
		private readonly IWorkplaceContext _context;
		private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerRelationsController));
	
	} // class CustomerRelationsController
} // namespace
