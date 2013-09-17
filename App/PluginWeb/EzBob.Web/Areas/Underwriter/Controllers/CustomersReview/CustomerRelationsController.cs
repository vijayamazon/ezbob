using System.Linq;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using Models;
	using Infrastructure.csrf;
	using Scorto.Web;
	using log4net;

	public class CustomerRelationsController : Controller
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(CustomerRelationsController));
		private readonly CustomerRelationsRepository customerRelationsRepository;
		private readonly CRMActionsRepository crmActionsRepository;
		private readonly CRMStatusesRepository crmStatusesRepository;

		public CustomerRelationsController(CustomerRelationsRepository customerRelationsRepository, CRMActionsRepository crmActionsRepository, CRMStatusesRepository crmStatusesRepository)
		{
			this.customerRelationsRepository = customerRelationsRepository;
			this.crmActionsRepository = crmActionsRepository;
			this.crmStatusesRepository = crmStatusesRepository;
		}

	    [Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Index(int id)
		{
			var models = customerRelationsRepository.ByCustomer(id).Select(customerRelations => CustomerRelationsModel.Create(customerRelations)).ToList();
		    return this.JsonNet(models);
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Actions()
		{
			var actions = crmActionsRepository.GetAll();
			return this.JsonNet(actions);
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Statuses()
		{
			var actions = crmStatusesRepository.GetAll();
			return this.JsonNet(actions);
		}

		[Ajax]
		[Transactional]
		public JsonNetResult SaveEntry(bool isIncoming, int action, int status, string comment, int customerId)
		{
			try
			{
				var newEntry = new CustomerRelations();
				newEntry.CustomerId = customerId;
				newEntry.UserName = User.Identity.Name;
				newEntry.Incoming = isIncoming;
				newEntry.Action = crmActionsRepository.Get(action);
				newEntry.Status = crmStatusesRepository.Get(status);
				newEntry.Comment = comment;
				newEntry.Timestamp = DateTime.UtcNow;
				customerRelationsRepository.SaveOrUpdate(newEntry);

				return this.JsonNet(string.Empty);
			}
			catch (Exception e)
			{
				log.ErrorFormat("Exception while trying to save customer relations new entry:{0}", e);
				return this.JsonNet(new { error = "Error saving new entry" });
			}
		}
	}
}
