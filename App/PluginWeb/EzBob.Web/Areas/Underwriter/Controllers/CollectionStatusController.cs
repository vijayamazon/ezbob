namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using NHibernate;
	using PaymentServices.Calculators;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;

	public class CollectionStatusController : Controller {
		private readonly ICustomerRepository customerRepository;
		private readonly CustomerStatusesRepository customerStatusesRepository;
		private readonly LoanOptionsRepository loanOptionsRepository;
		private readonly ICustomerStatusHistoryRepository customerStatusHistoryRepository;
		private readonly ISession session;
		public CollectionStatusController(
			ICustomerRepository customerRepository,
			CustomerStatusesRepository customerStatusesRepository,
			LoanOptionsRepository loanOptionsRepository,
			ICustomerStatusHistoryRepository customerStatusHistoryRepository, ISession session) {

			this.customerRepository = customerRepository;
			this.customerStatusesRepository = customerStatusesRepository;
			this.loanOptionsRepository = loanOptionsRepository;
			this.customerStatusHistoryRepository = customerStatusHistoryRepository;
			this.session = session;
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetStatuses() {
			return Json(this.customerStatusesRepository.GetAll().ToList(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult Index(int id) {
			var customer = this.customerRepository.Get(id);

			var collectionStatus = customer.CollectionStatus;

			var loans = customer.Loans.Select(l => LoanModel.FromLoan(l, new LoanRepaymentScheduleCalculator(l, null))).ToList();
			var loansNonClosed = loans.Where(l => l.DateClosed == null).ToList();

			var data = new CollectionStatusModel {
				CurrentStatus = collectionStatus.CurrentStatus.Id,
				CollectionDescription = collectionStatus.CollectionDescription,
				Items = loansNonClosed.Select(loan => new CollectionStatusItem {
					LoanId = loan.Id,
					LoanRefNumber = loan.RefNumber
				}).ToList()
			};

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Permission(Name = "CustomerStatus")]
		[Transactional]
		public JsonResult Save(int customerId, CollectionStatusModel collectionStatus) {
			var customer = this.customerRepository.Get(customerId);
			var prevStatus = customer.CollectionStatus.CurrentStatus.Id;

			if (prevStatus == collectionStatus.CurrentStatus) {
				return Json(new {});
			}

			customer.CollectionStatus.CurrentStatus = this.customerStatusesRepository.Get(collectionStatus.CurrentStatus);
			customer.CollectionStatus.CollectionDescription = collectionStatus.CollectionDescription;

			customerRepository.SaveOrUpdate(customer);

			if (customer.CollectionStatus.CurrentStatus.IsDefault) {
				

				// Update loan options
				foreach (Loan loan in customer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= CurrentValues.Instance.MinDectForDefault)) {
					LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? new LoanOptions {
						LoanId = loan.Id,
						AutoPayment = true,
						ReductionFee = true,
						LatePaymentNotification = true,
						StopSendingEmails = true,
						ManulCaisFlag = "Calculated value",
					};

					options.CaisAccountStatus = "8";
					loanOptionsRepository.SaveOrUpdate(options);
				}
			}

			this.session.Flush();

			DateTime applyForJudgmentDate;
			bool hasApplyForJudgmentDate = DateTime.TryParseExact(collectionStatus.ApplyForJudgmentDate, "dd/MM/yyyy",CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out applyForJudgmentDate);

			var newEntry = new CustomerStatusHistory {
				Username = User.Identity.Name,
				Timestamp = DateTime.UtcNow,
				CustomerId = customerId,
				PreviousStatus = prevStatus,
				NewStatus = collectionStatus.CurrentStatus,
				Description = collectionStatus.CollectionDescription,
				Amount = collectionStatus.Amount,
				ApplyForJudgmentDate = hasApplyForJudgmentDate ? applyForJudgmentDate : (DateTime?)null,
				Feedback = collectionStatus.Feedback,
				Type = collectionStatus.Type
			};

			this.customerStatusHistoryRepository.SaveOrUpdate(newEntry);

			return Json(new { });
		}
	}
}
