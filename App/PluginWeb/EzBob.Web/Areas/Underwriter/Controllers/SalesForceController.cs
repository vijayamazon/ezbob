namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using EzBob.Models.Marketplaces;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using log4net;
	using ServiceClientProxy;

	public class SalesForceController : Controller {
		public SalesForceController(CustomerRepository customerRepository, 
			FraudDetectionRepository fraudDetectionLog, 
			CustomerPhoneRepository customerPhoneRepository, 
			CustomerRelationsRepository customerRelationsRepository, 
			CompanyFilesMetaDataRepository companyFilesMetaDataRepository, 
			ExperianHistoryRepository experianHistoryRepository, 
			ServiceClient serviceClient, 
			IEzbobWorkplaceContext context, 
			IUsersRepository userRepo) {
			this.customerRepository = customerRepository;
			this.fraudDetectionLog = fraudDetectionLog;
			this.customerPhoneRepository = customerPhoneRepository;
			this.customerRelationsRepository = customerRelationsRepository;
			this.companyFilesMetaDataRepository = companyFilesMetaDataRepository;
			this.experianHistoryRepository = experianHistoryRepository;
			this.serviceClient = serviceClient;
			this.context = context;
			this.userRepo = userRepo;
		}

		public ActionResult Main() {
			return View("SalesForce");
		} // Main

		public ActionResult Index(string id, string origin) {
			Response.AddHeader("X-FRAME-OPTIONS", "");

			this.Log.InfoFormat("Loading sales force iframe for customer {0}, origin '{1}'.", id, origin);

			Customer customer = null;
			var model = new SalesForceModel();

			try {
				int customerId;
				if (int.TryParse(id, out customerId))
					customer = this.customerRepository.ReallyTryGet(customerId);
				else if (!string.IsNullOrWhiteSpace(origin)) {
					CustomerOriginEnum coe;

					if (Enum.TryParse(origin, true, out coe)) {
						User user = this.userRepo.GetUserByLogin(id, coe);

						if (user != null)
							customer = this.customerRepository.ReallyTryGet(user.Id);
					} // if
				} else if (string.IsNullOrWhiteSpace(origin) && !string.IsNullOrWhiteSpace(id)) {
					int numOfUsersWithEmail = this.userRepo.GetAll()
						.Count(x => x.Name == id);
					if (numOfUsersWithEmail == 1) {
						User user = this.userRepo.GetAll()
							.FirstOrDefault(x => x.Name == id);
						if (user != null) {
							customer = this.customerRepository.ReallyTryGet(user.Id);
						} //if
					} //if

					if (numOfUsersWithEmail > 1) {
						this.Log.WarnFormat("{0} customers found for email {1} returning empty result", numOfUsersWithEmail, id);
					} //if
				} // if
			} catch (Exception ex) {
				this.Log.WarnFormat("Failed to loan sales force iframe for customer {0}and origin '{1}' returning empty result", id, origin, ex);
			}
			if (customer == null) {
				this.Log.WarnFormat("customer not found for email {0} and origin '{1}' returning empty result", id, origin);
				return View(model);
			} // if
			
			model.FromCustomer(customer);

			var experianConsumer = this.experianHistoryRepository
				.GetCustomerConsumerHistory(customer.Id)
				.OrderByDescending(x => x.ServiceLogId)
				.FirstOrDefault();

			var decisionHistories = this.serviceClient.Instance.LoadDecisionHistory(customer.Id, this.context.UserId);
			model.Decisions = decisionHistories.Model.Select(DecisionHistoryModel.Create)
				.OrderBy(x => x.Date)
				.ToList();

			if (experianConsumer != null) {
				model.PersonalModel.ExperianPersonalScore = experianConsumer.Score;
				model.PersonalModel.ExperianCII = experianConsumer.CII;
			} // if

			if (customer.Company != null) {
				var experianCompany = this.experianHistoryRepository
					.GetCompanyHistory(
						customer.Company.ExperianRefNum,
						customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited
					)
					.OrderByDescending(x => x.ServiceLogId)
					.FirstOrDefault();

				if (experianCompany != null)
					model.PersonalModel.ExperianCompanyScore = experianCompany.Score;
			} // if

			DateTime? lastCheckDate;
		    string customerRef;

			var fraudDetections = this.fraudDetectionLog
				.GetLastDetections(customer.Id, out lastCheckDate, out customerRef)
				.Select(x => new FraudDetectionLogRowModel(x))
				.OrderByDescending(x => x.Id)
				.ToList();

			model.Fraud = new FraudDetectionLogModel {
				FraudDetectionLogRows = fraudDetections,
				LastCheckDate = lastCheckDate,
			};

			try {
				model.Messages = new List<MessagesModel>(
					new ServiceClient().Instance.LoadMessagesSentToUser(customer.Id).Messages
				);
			} catch (Exception e) {
				Log.Error("Failed to load messages sent to customer " + customer.Id, e);
				model.Messages = new List<MessagesModel>();
			} // try

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

			model.CompanyFiles = this.companyFilesMetaDataRepository
				.GetByCustomerId(customer.Id)
				.Select(x => new CompanyFile {
					FileName = x.FileName,
					Uploaded = x.Created
				})
				.ToList();

			return View(model);
		} // Index

		private readonly CustomerRepository customerRepository;
		private readonly IUsersRepository userRepo;
		private readonly FraudDetectionRepository fraudDetectionLog;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly CustomerRelationsRepository customerRelationsRepository;
		private readonly CompanyFilesMetaDataRepository companyFilesMetaDataRepository;
		private readonly ExperianHistoryRepository experianHistoryRepository;
		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;
		protected readonly ILog Log = LogManager.GetLogger(typeof (SalesForceController));
	} // class SalesForceController
} // namespace
