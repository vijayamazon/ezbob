namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EzBob.Models.Marketplaces;
	using EzBob.Web.Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using log4net;

	public class SalesForceController : Controller
    {
		public SalesForceController(CustomerRepository customerRepository, FraudDetectionRepository fraudDetectionLog, MessagesModelBuilder messagesModelBuilder, CustomerPhoneRepository customerPhoneRepository, CustomerRelationsRepository customerRelationsRepository, CompanyFilesMetaDataRepository companyFilesMetaDataRepository) {
			this.customerRepository = customerRepository;
			this.fraudDetectionLog = fraudDetectionLog;
			this.messagesModelBuilder = messagesModelBuilder;
			this.customerPhoneRepository = customerPhoneRepository;
			this.customerRelationsRepository = customerRelationsRepository;
			this.companyFilesMetaDataRepository = companyFilesMetaDataRepository;
		}

		public ActionResult Index(string id) {
			Response.AddHeader("X-FRAME-OPTIONS", "");
			Log.InfoFormat("Loading sales force iframe for customer {0}", id);
			int customerId;
			Customer customer = int.TryParse(id, out customerId) ? 
				customerRepository.ReallyTryGet(customerId) :
				customerRepository.TryGetByEmail(id);

			if (customer == null) {
				Log.WarnFormat("customer not found for email {0} returning empty result", id);
				
				return View();
			}
			var model = new SalesForceModel();
			model.FromCustomer(customer);

			DateTime? lastCheckDate;
		    string customerRef;

			var fraudDetections = fraudDetectionLog
				.GetLastDetections(customer.Id, out lastCheckDate, out customerRef)
				.Select(x => new FraudDetectionLogRowModel(x))
				.OrderByDescending(x => x.Id)
				.ToList();

			model.Fraud = new FraudDetectionLogModel
				{
					FraudDetectionLogRows = fraudDetections,
					LastCheckDate = lastCheckDate
				};
			
			model.Messages = messagesModelBuilder.Create(customer);

			model.Phones = customerPhoneRepository
				.GetAll()
				.Where(x => x.CustomerId == customer.Id && x.IsCurrent)
				.Select(x => new CrmPhoneNumber {
					Type = x.PhoneType,
					IsVerified = x.IsVerified,
					Number = x.Phone
				})
				.ToList();

			model.OldCrm = customerRelationsRepository
				.ByCustomer(customer.Id)
				.Select(customerRelations => CustomerRelationsModel.Create(customerRelations))
				.ToList();

			model.CompanyFiles = companyFilesMetaDataRepository.GetByCustomerId(customer.Id).Select(x => new CompanyFile{
				FileName = x.FileName,
				Uploaded = x.Created
			}).ToList();

			return View(model);
		}

		private readonly CustomerRepository customerRepository;
		private readonly FraudDetectionRepository fraudDetectionLog;
		private readonly MessagesModelBuilder messagesModelBuilder;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly CustomerRelationsRepository customerRelationsRepository;
		private readonly CompanyFilesMetaDataRepository companyFilesMetaDataRepository;
		private readonly ILog Log = LogManager.GetLogger(typeof (SalesForceController));

	}
}
