namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using Models;
	using Infrastructure.csrf;
	using Scorto.Web;
	using System.Linq;
	using log4net;

	public class CustomerRelationsController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerRelationsController));
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly CRMActionsRepository _crmActionsRepository;
		private readonly CRMStatusesRepository _crmStatusesRepository;

		public CustomerRelationsController(CustomerRelationsRepository customerRelationsRepository, CRMActionsRepository crmActionsRepository, CRMStatusesRepository crmStatusesRepository)
		{
			_customerRelationsRepository = customerRelationsRepository;
			_crmActionsRepository = crmActionsRepository;
			_crmStatusesRepository = crmStatusesRepository;
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Index(int id)
		{
			var models = _customerRelationsRepository.ByCustomer(id).Select(customerRelations => CustomerRelationsModel.Create(customerRelations)).ToList();
			return this.JsonNet(models);
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Actions()
		{
			var actions = _crmActionsRepository.GetAll();
			return this.JsonNet(actions);
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Statuses()
		{
			var actions = _crmStatusesRepository.GetAll();
			return this.JsonNet(actions);
		}

		[Ajax]
		[Transactional]
		public JsonNetResult SaveEntry(bool isIncoming, int action, int status, string comment, int customerId)
		{
			try
			{
				var newEntry = new CustomerRelations
					{
						CustomerId = customerId,
						UserName = User.Identity.Name,
						Incoming = isIncoming,
						Action = _crmActionsRepository.Get(action),
						Status = _crmStatusesRepository.Get(status),
						Comment = comment,
						Timestamp = DateTime.UtcNow
					};

				_customerRelationsRepository.SaveOrUpdate(newEntry);

				return this.JsonNet(string.Empty);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Exception while trying to save customer relations new entry:{0}", e);
				return this.JsonNet(new { error = "Error saving new entry" });
			}
		}
	}
}
