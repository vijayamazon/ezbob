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
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;
	using ServiceClientProxy;

	public class CollectionStatusController : Controller {
		private readonly ICustomerRepository customerRepository;
		private readonly CustomerStatusesRepository customerStatusesRepository;
		private readonly LoanOptionsRepository loanOptionsRepository;
		private readonly ICustomerStatusHistoryRepository customerStatusHistoryRepository;
		private readonly ISession session;
		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;
		private readonly ILoanRepository loanRepository;
		public CollectionStatusController(
			ICustomerRepository customerRepository,
			CustomerStatusesRepository customerStatusesRepository,
			LoanOptionsRepository loanOptionsRepository,
			ICustomerStatusHistoryRepository customerStatusHistoryRepository, ISession session,
			ServiceClient serviceClient,
			IEzbobWorkplaceContext context,
			ILoanRepository loanRepository) {

			this.customerRepository = customerRepository;
			this.customerStatusesRepository = customerStatusesRepository;
			this.loanOptionsRepository = loanOptionsRepository;
			this.customerStatusHistoryRepository = customerStatusHistoryRepository;
			this.session = session;
			this.serviceClient = serviceClient;
			this.context = context;
			this.loanRepository = loanRepository;
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

			var loans = customer.Loans.Select(l => LoanModel.FromLoan(l, new LoanRepaymentScheduleCalculator(l, null, CurrentValues.Instance.AmountToChargeFrom))).ToList();
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
		public JsonResult Save(int customerId, CollectionStatusModel collectionStatus) {

			var customer = this.customerRepository.Get(customerId);
			var prevStatus = customer.CollectionStatus.CurrentStatus;

			if (prevStatus.Id == collectionStatus.CurrentStatus) {
				return Json(new { });
			}

			customer.CollectionStatus.CurrentStatus = this.customerStatusesRepository.Get(collectionStatus.CurrentStatus);
			customer.CollectionStatus.CollectionDescription = collectionStatus.CollectionDescription;

			new Transactional(() => {
				this.customerRepository.SaveOrUpdate(customer);

				if (customer.CollectionStatus.CurrentStatus.IsDefault) {

					// Update loan options
					foreach (Loan loan in customer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= CurrentValues.Instance.MinDectForDefault)) {
						LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? new LoanOptions {
							LoanId = loan.Id,
							AutoPayment = true,
							ReductionFee = true,
							LatePaymentNotification = true,
							EmailSendingAllowed = false,
							MailSendingAllowed = false,
							SmsSendingAllowed = false,
							ManualCaisFlag = "Calculated value",
						};
						options.CaisAccountStatus = "8";
						this.loanOptionsRepository.SaveOrUpdate(options);
					}
				}

				DateTime now = DateTime.UtcNow;
				if (!customer.CollectionStatus.CurrentStatus.IsEnabled) {

					// Update loan options add freeze interest
					foreach (Loan loan in customer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= CurrentValues.Instance.MinDectForDefault)) {
						LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? LoanOptions.GetDefault(loan.Id);
						options.AutoLateFees = false;
						options.AutoPayment = false;
						this.loanOptionsRepository.SaveOrUpdate(options);

						loan.InterestFreeze.Add(new LoanInterestFreeze {
							Loan = loan,
							StartDate = now.Date,
							EndDate = (DateTime?)null,
							InterestRate = 0,
							ActivationDate = now,
							DeactivationDate = null
						});

						this.loanRepository.SaveOrUpdate(loan);
					}
				} else if (!prevStatus.IsEnabled && customer.CollectionStatus.CurrentStatus.IsEnabled) {
					// Update loan options add freeze interest
					foreach (Loan loan in customer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= CurrentValues.Instance.MinDectForDefault)) {
						LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? LoanOptions.GetDefault(loan.Id);
						options.AutoLateFees = true;
						options.AutoPayment = true;
						this.loanOptionsRepository.SaveOrUpdate(options);

						if (loan.InterestFreeze.Any(f => f.EndDate == null && f.DeactivationDate == null)) {
							foreach (var interestFreeze in loan.InterestFreeze.Where(f => f.EndDate == null && f.DeactivationDate == null)) {
								interestFreeze.DeactivationDate = now;
							}

							this.loanRepository.SaveOrUpdate(loan);
						}
					}

				}

				this.session.Flush();

				DateTime applyForJudgmentDate;
				bool hasApplyForJudgmentDate = DateTime.TryParseExact(collectionStatus.ApplyForJudgmentDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out applyForJudgmentDate);

				var newEntry = new CustomerStatusHistory {
					Username = User.Identity.Name,
					Timestamp = DateTime.UtcNow,
					CustomerId = customerId,
					PreviousStatus = prevStatus,
					NewStatus = customer.CollectionStatus.CurrentStatus,
					Description = collectionStatus.CollectionDescription,
					Amount = collectionStatus.Amount,
					ApplyForJudgmentDate = hasApplyForJudgmentDate ? applyForJudgmentDate : (DateTime?)null,
					Feedback = collectionStatus.Feedback,
					Type = collectionStatus.Type
				};

				this.customerStatusHistoryRepository.SaveOrUpdate(newEntry);
			}).Execute();

			if (customer.CollectionStatus.CurrentStatus.Name == "Disabled" && (collectionStatus.Unsubscribe || collectionStatus.ChangeEmail)) {
				this.serviceClient.Instance.UserDisable(this.context.UserId, customer.Id, customer.Name, collectionStatus.Unsubscribe, collectionStatus.ChangeEmail);
			}
			return Json(new { });
		}
	}
}
