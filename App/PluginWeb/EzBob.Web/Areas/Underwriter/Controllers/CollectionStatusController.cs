namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using EzBob.Models;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using NHibernate;
	using PaymentServices.Calculators;
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

		private static readonly ASafeLog log = new SafeILog(typeof(CollectionStatusController));

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
				CurrentStatus = collectionStatus.Id,
				CollectionDescription = customer.CollectionDescription,
				Items = loansNonClosed.Select(loan => new CollectionStatusItem {
					LoanId = loan.Id,
					LoanRefNumber = loan.RefNumber
				}).ToList()
			};

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		private void NL_SaveLoanOptions(Customer customer, LoanOptions options) {
			NL_LoanOptions nlOptions = new NL_LoanOptions() {
				LoanID = options.LoanId,
				CaisAccountStatus = options.CaisAccountStatus,
				EmailSendingAllowed = options.EmailSendingAllowed,
				LatePaymentNotification = options.LatePaymentNotification,
				LoanOptionsID = options.Id,
				MailSendingAllowed = options.MailSendingAllowed,
				ManualCaisFlag = options.ManualCaisFlag,
				PartialAutoCharging = options.ReductionFee,
				SmsSendingAllowed = options.SmsSendingAllowed,
				StopAutoChargeDate = MiscUtils.NL_GetStopAutoChargeDate(options.AutoPayment, options.StopAutoChargeDate),
				StopLateFeeFromDate = MiscUtils.NL_GetLateFeeDates(options.AutoLateFees, options.StopLateFeeFromDate, options.StopLateFeeToDate).Item1,
				StopLateFeeToDate = MiscUtils.NL_GetLateFeeDates(options.AutoLateFees, options.StopLateFeeFromDate, options.StopLateFeeToDate).Item2,
				UserID = this.context.UserId,
				InsertDate = DateTime.Now,
				IsActive = true,
				Notes = "From Collection Status",
			};

			var PropertiesUpdateList = new List<String>() {
		        "StopAutoChargeDate",
                "StopLateFeeFromDate",
		        "StopLateFeeToDate",
		    };

			var nlStrategy = this.serviceClient.Instance.AddLoanOptions(this.context.UserId, customer.Id, nlOptions, options.LoanId, PropertiesUpdateList.ToArray());
		}

		[Ajax]
		[HttpPost]
		[Permission(Name = "CustomerStatus")]
		public JsonResult Save(int customerId, CollectionStatusModel collectionStatus) {

			var customer = this.customerRepository.Get(customerId);
			var prevStatus = customer.CollectionStatus;

			if (prevStatus.Id == collectionStatus.CurrentStatus) {
				return Json(new { });
			}

			customer.CollectionStatus = this.customerStatusesRepository.Get(collectionStatus.CurrentStatus);
			customer.CollectionDescription = collectionStatus.CollectionDescription;
			List<int> addFreeseLoans = new List<int>();

			new Transactional(() => {
				this.customerRepository.SaveOrUpdate(customer);

				if (customer.CollectionStatus.IsDefault) {

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
						NL_SaveLoanOptions(customer, options);
					}
				}

				DateTime now = DateTime.UtcNow;
				if (!customer.CollectionStatus.IsEnabled) {

					// Update loan options add freeze interest
					foreach (Loan loan in customer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= CurrentValues.Instance.MinDectForDefault)) {
						LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? LoanOptions.GetDefault(loan.Id);
						options.AutoLateFees = false;
						options.AutoPayment = false;
						this.loanOptionsRepository.SaveOrUpdate(options);
						NL_SaveLoanOptions(customer, options);

						loan.InterestFreeze.Add(new LoanInterestFreeze {
							Loan = loan,
							StartDate = now.Date,
							EndDate = (DateTime?)null,
							InterestRate = 0,
							ActivationDate = now,
							DeactivationDate = null
						});

						this.loanRepository.SaveOrUpdate(loan);

						addFreeseLoans.Add(loan.Id);
					}

					//collection and external status is ok
				} else if (!prevStatus.IsEnabled && customer.CollectionStatus.IsEnabled && customer.ExternalCollectionStatus == null) {
					// Update loan options add remove freeze interest
					foreach (Loan loan in customer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= CurrentValues.Instance.MinDectForDefault)) {
						LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? LoanOptions.GetDefault(loan.Id);
						options.AutoLateFees = true;
						options.AutoPayment = true;
						this.loanOptionsRepository.SaveOrUpdate(options);
						NL_SaveLoanOptions(customer, options);

						if (loan.InterestFreeze.Any(f => f.EndDate == null && f.DeactivationDate == null)) {
							foreach (var interestFreeze in loan.InterestFreeze.Where(f => f.EndDate == null && f.DeactivationDate == null)) {
								interestFreeze.DeactivationDate = now;
								DeactivateLoanInterestFreeze(interestFreeze, customerId, loan.Id);
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
					NewStatus = customer.CollectionStatus,
					Description = collectionStatus.CollectionDescription,
					Amount = collectionStatus.Amount,
					ApplyForJudgmentDate = hasApplyForJudgmentDate ? applyForJudgmentDate : (DateTime?)null,
					Feedback = collectionStatus.Feedback,
					Type = collectionStatus.Type
				};

				this.customerStatusHistoryRepository.SaveOrUpdate(newEntry);
			}).Execute();

			log.Debug("AaddFreeseLoans {0}", addFreeseLoans);

			foreach (int loanID in addFreeseLoans) {
				var loan = this.loanRepository.Get(loanID);
				SaveLoanInterestFreeze(loan.InterestFreeze.Last(), customerId, loan.Id);
			}

			if (customer.CollectionStatus.Name == "Disabled" && (collectionStatus.Unsubscribe || collectionStatus.ChangeEmail)) {
				this.serviceClient.Instance.UserDisable(this.context.UserId, customer.Id, customer.Name, collectionStatus.Unsubscribe, collectionStatus.ChangeEmail);
			}

			this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(this.context.UserId, customer.Name, customerId, false, false);
			return Json(new { });
		}

		private void DeactivateLoanInterestFreeze(LoanInterestFreeze loanInterestFreeze, int customerId, int loanID) {
			long nlLoanId = this.serviceClient.Instance.GetLoanByOldID(loanID, customerId, this.context.UserId).Value;
			if (nlLoanId == 0)
				return;
			NL_LoanInterestFreeze nlLoanInterestFreeze = new NL_LoanInterestFreeze() {
				OldID = loanInterestFreeze.Id,
				DeactivationDate = loanInterestFreeze.DeactivationDate,
				LoanID = nlLoanId,
				AssignedByUserID = this.context.UserId,
				DeletedByUserID = null,
			};
			var nlStrategy = this.serviceClient.Instance.DeactivateLoanInterestFreeze(this.context.UserId,
																					  customerId,
																					  nlLoanInterestFreeze).Value;
		}

		private void SaveLoanInterestFreeze(LoanInterestFreeze loanInterestFreeze, int customerId, int loanID) {
			long nlLoanId = this.serviceClient.Instance.GetLoanByOldID(loanID, customerId, this.context.UserId).Value;
			if (nlLoanId == 0)
				return;
			NL_LoanInterestFreeze nlLoanInterestFreeze = new NL_LoanInterestFreeze() {
				StartDate = loanInterestFreeze.StartDate,
				OldID = loanInterestFreeze.Id,
				ActivationDate = loanInterestFreeze.ActivationDate,
				DeactivationDate = loanInterestFreeze.DeactivationDate,
				EndDate = loanInterestFreeze.EndDate,
				InterestRate = loanInterestFreeze.InterestRate,
				LoanID = nlLoanId,
				AssignedByUserID = this.context.UserId,
				DeletedByUserID = null,
			};

			log.Debug("ADDING NL FREESE: {0} \n olffreeze: {1}", nlLoanInterestFreeze, loanInterestFreeze);

			var nlStrategy = this.serviceClient.Instance.AddLoanInterestFreeze(this.context.UserId, customerId, nlLoanInterestFreeze).Value;
		}
	}
}
