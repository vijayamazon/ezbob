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

	using CreditResultStatus = EZBob.DatabaseLib.Model.Database.CreditResultStatus;

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
			this._customerRelationsRepository = customerRelationsRepository;
			this._loanRepository = loanRepository;
			this._session = session;
			this._crmRanksRepository = crmRanksRepository;
			this._crmStatusesRepository = crmStatusesRepository;
			this._crmActionsRepository = crmActionsRepository;
			this._customerRelationFollowUpRepository = customerRelationFollowUpRepository;
			this._customerRelationStateRepository = customerRelationStateRepository;
			this.customerRepository = customerRepository;
			this.frequentActionItemsForCustomerRepository = new FrequentActionItemsForCustomerRepository(session);
			this._context = context;
			this._serviceClient = new ServiceClient();
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id) {
			var crm = new CustomerRelationsModelBuilder(this._loanRepository, this._customerRelationsRepository, this._session);
			return Json(crm.Create(id), JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SalesForceActivity(int id) {
			var activity = this._serviceClient.Instance.SalesForceGetActivity(this._context.UserId, id);
			return Json(new {
				Activities = activity.Value.Activities.OrderByDescending(x => x.StartDate),
				Error = activity.Value.Error
			}, JsonRequestBehavior.AllowGet);
		} // SalesForceActivity

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult CrmStatic() {
			try {
				var ar = this._serviceClient.Instance.CrmLoadLookups();

				return Json(new {
					CrmActions = ar.Actions,
					CrmStatuses = ar.Statuses,
					CrmRanks = ar.Ranks,
				}, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
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
		[Permission(Name = "CRM")]
		public JsonResult SaveEntry(string type, int action, int status, int? rank, string comment, int customerId, bool isBroker, string phoneNumber) {
			try {
				var actionItem = this._crmActionsRepository.Get(action);
				var statusItem = this._crmStatusesRepository.Get(status);
				var rankItem = rank.HasValue ? this._crmRanksRepository.Get(rank.Value) : null;
				var newEntry = new CustomerRelations {
					CustomerId = customerId,
					UserName = User.Identity.Name,
					Type = type,
					Action = actionItem,
					Status = statusItem,
					Rank = rankItem,
					Comment = comment,
					Timestamp = DateTime.UtcNow,
					IsBroker = isBroker,
					PhoneNumber = phoneNumber
				};

				this._customerRelationsRepository.SaveOrUpdate(newEntry);
				this._customerRelationStateRepository.SaveUpdateState(customerId, false, null, newEntry);

				//Add SF activity
				var customer = this.customerRepository.ReallyTryGet(customerId);
				if (customer != null) {
					this._serviceClient.Instance.SalesForceAddActivity(this._context.UserId, customerId, new ActivityModel {
						Description = string.Format("{0}, {1}, {2}, {3}", type, actionItem.Name, statusItem.Name, comment),
						Email = customer.Name,
						Origin = customer.CustomerOrigin.Name,
						StartDate = DateTime.UtcNow,
						EndDate = DateTime.UtcNow,
						IsOpportunity = false,
						Originator = this._context.User.Name,
						Type = actionItem.Name,
					});
				}
				return Json(new { success = true, error = "" });
			} catch (Exception e) {
				Log.ErrorFormat("Exception while trying to save customer relations new entry:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations entry." });
			} // try
		} // SaveEntry

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "CRM")]
		public JsonResult SaveFollowUp(DateTime followUpDate, string comment, int customerId, bool isBroker) {
			try {
				var lastCrm = this._customerRelationsRepository.GetLastCrm(customerId);

				var followUp = new CustomerRelationFollowUp {
					Comment = comment,
					CustomerId = customerId,
					DateAdded = DateTime.UtcNow,
					FollowUpDate = followUpDate,
					IsBroker = isBroker,
				};

				this._customerRelationFollowUpRepository.SaveOrUpdate(followUp);
				this._customerRelationStateRepository.SaveUpdateState(customerId, true, followUp, lastCrm);

				return Json(new { success = true, error = "" });
			} catch (Exception e) {
				Log.Error("Exception while trying to save customer relations new entry.", e);
				return Json(new { success = false, error = "Error saving new customer relations follow up." });
			} // try
		} // SaveFollowUp

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "CRM")]
		public JsonResult ChangeRank(int customerId, int rankId) {
			try {
				var crm = new CustomerRelations {
					Rank = this._crmRanksRepository.Get(rankId),
					Timestamp = DateTime.UtcNow,
					Comment = "Rank change",
					UserName = User.Identity.Name,
				};

				this._customerRelationsRepository.Save(crm);
				this._customerRelationStateRepository.SaveUpdateRank(customerId, crm);

				return Json(new { success = true, error = "" });
			} catch (Exception e) {
				Log.ErrorFormat("Exception while trying to change customer relations rank:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations rank." });
			} // try
		} // ChangeRank

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "CRM")]
		public JsonResult CloseFollowUp(int customerId, int? followUpId = null) {
			try {
				var lastCrm = this._customerRelationsRepository.GetLastCrm(customerId);
				CustomerRelationFollowUp lastFollowUp = followUpId == null
					? this._customerRelationFollowUpRepository.GetLastFollowUp(customerId)
					: this._customerRelationFollowUpRepository.Get(followUpId);

				if (lastFollowUp == null)
					return Json(new { success = false, error = "customer don't have any open follow ups please add one." });

				lastFollowUp.IsClosed = true;
				lastFollowUp.CloseDate = DateTime.UtcNow;
				this._customerRelationStateRepository.SaveUpdateState(customerId, false, lastFollowUp, lastCrm);

				return Json(new { success = true, error = "" });
			} catch (Exception e) {
				Log.ErrorFormat("Exception while trying to close customer relations follow up:{0}", e);
				return Json(new { success = false, error = "Error saving new customer relations follow up." });
			} // try
		} // CloseFollowUp

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "SendSMS")]
		public JsonResult SendSms(int customerId, string phone, string content, bool isBroker) {
			try {
				var sendSmsResult = this._serviceClient.Instance.SendSms(customerId, this._context.UserId, phone, content);
				if (!sendSmsResult.Value) {
					return Json(new { success = false, error = "Failed to send SMS via twilio" });
				}

				var action = this._crmActionsRepository.GetAll().FirstOrDefault(x => x.Name == "SMS");
				var status = this._crmStatusesRepository.GetAll().FirstOrDefault(x => x.Name == "Note for underwriting");
				return SaveEntry("Out",
					action != null ? action.Id : 1,
					status != null ? status.Id : 1,
					null,
					content,
					customerId,
					isBroker,
					phone);
			} catch (Exception e) {
				Log.ErrorFormat("Exception while trying to SendSms :{0}", e);
				return Json(new { success = false, error = "Error Sending Sms." });
			} // try
		} // CloseFollowUp

		[Ajax]
		[HttpPost]
		[Permission(Name = "CRM")]
		public void MarkAsPending(int customerId, string actionItems, string costumeActionItemValue) {
			DateTime now = DateTime.UtcNow;
			List<int> checkedIds = GetCheckedActionItemIds(actionItems);
			bool changed = false;

			// "Close" action items
			var openActionItemsInDb = this.frequentActionItemsForCustomerRepository.GetAll().Where(x => x.CustomerId == customerId && x.UnmarkedDate == null);
			foreach (var openActionItem in openActionItemsInDb) {
				if (!checkedIds.Contains(openActionItem.ItemId)) {
					changed = true;
					openActionItem.UnmarkedDate = now;
					this.frequentActionItemsForCustomerRepository.SaveOrUpdate(openActionItem);
				}
			}

			// Insert new action items
			foreach (int checkedId in checkedIds) {
				if (!openActionItemsInDb.Any(x => x.ItemId == checkedId)) {
					changed = true;
					var newCheckedItem = new FrequentActionItemsForCustomer { CustomerId = customerId, ItemId = checkedId, MarkedDate = now };
					this.frequentActionItemsForCustomerRepository.SaveOrUpdate(newCheckedItem);
				}
			}

			// Update costume action item
			Customer customer = this.customerRepository.Get(customerId);
			if (customer.CostumeActionItem != costumeActionItemValue) {
				changed = true;
				customer.CostumeActionItem = costumeActionItemValue;
			}

			if (changed) {
				var entry = new CustomerRelations {
					Action = this._crmActionsRepository.GetAll().FirstOrDefault(x => x.Name == "Action items change"),
					CustomerId = customerId,
					IsBroker = false,
					Rank = this._crmRanksRepository.GetAll().FirstOrDefault(x => x.Name == "High"),
					Status = this._crmStatusesRepository.GetAll().FirstOrDefault(x => x.Name == "Pending"),
					Timestamp = DateTime.UtcNow,
					Type = "Internal",
					UserName = User.Identity.Name
				};
				this._customerRelationsRepository.SaveOrUpdate(entry);

				if (checkedIds.Count == 0 && string.IsNullOrEmpty(customer.CostumeActionItem)) {
					// Mark as waiting for decision
					customer.CreditResult = CreditResultStatus.WaitingForDecision;
					this.customerRepository.SaveOrUpdate(customer);
				} else {
					// Mark as pending
					customer.CreditResult = CreditResultStatus.ApprovedPending;
					this.customerRepository.SaveOrUpdate(customer);

					// Send mail
					this._serviceClient.Instance.SendPendingMails(this._context.UserId, customerId);
				}
			}
		}

		public FileResult DownloadSnailMail(int id) {
			var result = this._serviceClient.Instance.GetCollectionSnailMail(this._context.UserId, id);
			FileResult fs = File(result.SnailMail.Content, result.SnailMail.ContentType);
			
			//	fs.FileDownloadName = fileName;
			var cd = new System.Net.Mime.ContentDisposition {
				FileName = result.SnailMail.Name,
				Inline = true,
			};

			Response.AppendHeader("Content-Disposition", cd.ToString());
			
			return fs;
		}

		// TODO: improve the way this is done - pass list or model or use binding
		private List<int> GetCheckedActionItemIds(string actionItemsIds) {
			string[] idsStr = actionItemsIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			var checkedIds = new List<int>();
			foreach (string idStr in idsStr) {
				int idInt;
				if (int.TryParse(idStr, out idInt)) {
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
