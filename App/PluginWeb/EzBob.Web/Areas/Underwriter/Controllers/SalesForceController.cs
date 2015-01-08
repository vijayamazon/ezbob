namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EzBob.Web.Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using log4net;

	public class SalesForceController : Controller
    {
		public SalesForceController(CustomerRepository customerRepository, FraudDetectionRepository fraudDetectionLog, MessagesModelBuilder messagesModelBuilder, CustomerPhoneRepository customerPhoneRepository, CustomerRelationsRepository customerRelationsRepository) {
			this.customerRepository = customerRepository;
			this.fraudDetectionLog = fraudDetectionLog;
			this.messagesModelBuilder = messagesModelBuilder;
			this.customerPhoneRepository = customerPhoneRepository;
			this.customerRelationsRepository = customerRelationsRepository;
		}

		public ActionResult Index(string id) {
			Response.AddHeader("X-FRAME-OPTIONS", "");
			Log.InfoFormat("Loading sales force iframe for customer email {0}", id);
			var customer = customerRepository.TryGetByEmail(id);
			var model = new SalesForceModel();
			if (customer == null) {
				Log.WarnFormat("customer not found for email {0} returning empty result", id);
				
				return View();
			}
			model.FromCustomer(customer);

			DateTime? lastCheckDate;
			var rows = fraudDetectionLog
				.GetLastDetections(customer.Id, out lastCheckDate)
				.Select(x => new FraudDetectionLogRowModel(x))
				.OrderByDescending(x => x.Id)
				.ToList();

			var fraudModel = new FraudDetectionLogModel
				{
					FraudDetectionLogRows = rows,
					LastCheckDate = lastCheckDate
				};
			model.Fraud = fraudModel;

			var messagesModel = messagesModelBuilder.Create(customer);
			model.Messages = messagesModel;

			var phoneNumbers = customerPhoneRepository
				.GetAll()
				.Where(x => x.CustomerId == customer.Id && x.IsCurrent)
				.Select(x => new CrmPhoneNumber {
					Type = x.PhoneType,
					IsVerified = x.IsVerified,
					Number = x.Phone
				})
				.ToList();

			model.Phones = phoneNumbers;
			var crm = customerRelationsRepository
				.ByCustomer(customer.Id)
				.Select(customerRelations => CustomerRelationsModel.Create(customerRelations))
				.ToList();
			model.OldCrm = crm;

			return View(model);
		}

		private readonly CustomerRepository customerRepository;
		private readonly FraudDetectionRepository fraudDetectionLog;
		private readonly MessagesModelBuilder messagesModelBuilder;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly CustomerRelationsRepository customerRelationsRepository;
		private readonly ILog Log = LogManager.GetLogger(typeof (SalesForceController));
	}
}
