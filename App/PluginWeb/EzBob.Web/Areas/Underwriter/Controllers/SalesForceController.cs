namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EzBob.Models.Marketplaces;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using log4net;
	using ServiceClientProxy;

	public class SalesForceController : Controller
    {
		public SalesForceController(CustomerRepository customerRepository, 
			FraudDetectionRepository fraudDetectionLog, 
			MessagesModelBuilder messagesModelBuilder, 
			CustomerPhoneRepository customerPhoneRepository, 
			CustomerRelationsRepository customerRelationsRepository, 
			CompanyFilesMetaDataRepository companyFilesMetaDataRepository, 
			ExperianHistoryRepository experianHistoryRepository, 
			ServiceClient serviceClient, 
			IEzbobWorkplaceContext context) {
			this.customerRepository = customerRepository;
			this.fraudDetectionLog = fraudDetectionLog;
			this.messagesModelBuilder = messagesModelBuilder;
			this.customerPhoneRepository = customerPhoneRepository;
			this.customerRelationsRepository = customerRelationsRepository;
			this.companyFilesMetaDataRepository = companyFilesMetaDataRepository;
			this.experianHistoryRepository = experianHistoryRepository;
			this.serviceClient = serviceClient;
			this.context = context;
		}

		public ActionResult Main() {
			return View("SalesForce");
		}

		public ActionResult Index(string id) {
			Response.AddHeader("X-FRAME-OPTIONS", "");
			this.Log.InfoFormat("Loading sales force iframe for customer {0}", id);
			int customerId;
			Customer customer = int.TryParse(id, out customerId) ? 
				this.customerRepository.ReallyTryGet(customerId) :
				this.customerRepository.TryGetByEmail(id);

			var model = new SalesForceModel();
			if (customer == null) {
				this.Log.WarnFormat("customer not found for email {0} returning empty result", id);
				return View(model);
			}
			
			model.FromCustomer(customer);

			var decisionHistories = this.serviceClient.Instance.LoadDecisionHistory(customerId, this.context.UserId);
			model.Decisions = decisionHistories.Model.Select(DecisionHistoryModel.Create)
				.OrderBy(x => x.Date)
				.ToList();

			var experianConsumer = this.experianHistoryRepository.GetCustomerConsumerHistory(customer.Id).OrderByDescending(x => x.ServiceLogId).FirstOrDefault();

			if (experianConsumer != null) {
				model.PersonalModel.ExperianPersonalScore = experianConsumer.Score;
				model.PersonalModel.ExperianCII = experianConsumer.CII;
			}

			if (customer.Company != null) {
				var experianCompany = this.experianHistoryRepository.GetCompanyHistory(customer.Company.ExperianRefNum, customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited)
					.OrderByDescending(x => x.ServiceLogId)
					.FirstOrDefault();

				if (experianCompany != null)
					model.PersonalModel.ExperianCompanyScore = experianCompany.Score;
			}

			DateTime? lastCheckDate;
		    string customerRef;

			var fraudDetections = this.fraudDetectionLog
				.GetLastDetections(customer.Id, out lastCheckDate, out customerRef)
				.Select(x => new FraudDetectionLogRowModel(x))
				.OrderByDescending(x => x.Id)
				.ToList();

			model.Fraud = new FraudDetectionLogModel
				{
					FraudDetectionLogRows = fraudDetections,
					LastCheckDate = lastCheckDate
				};

			model.Messages = this.messagesModelBuilder.Create(customer);

			model.Phones = this.customerPhoneRepository
				.GetAll()
				.Where(x => x.CustomerId == customer.Id && x.IsCurrent)
				.Select(x => new CrmPhoneNumber {
					Type = x.PhoneType,
					IsVerified = x.IsVerified,
					Number = x.Phone
				})
				.ToList();

			model.OldCrm = this.customerRelationsRepository
				.ByCustomer(customer.Id)
				.Select(customerRelations => CustomerRelationsModel.Create(customerRelations))
				.ToList();

			model.CompanyFiles = this.companyFilesMetaDataRepository.GetByCustomerId(customer.Id).Select(x => new CompanyFile {
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
		private readonly ExperianHistoryRepository experianHistoryRepository;
		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;
		protected readonly ILog Log = LogManager.GetLogger(typeof (SalesForceController));
	}
}
