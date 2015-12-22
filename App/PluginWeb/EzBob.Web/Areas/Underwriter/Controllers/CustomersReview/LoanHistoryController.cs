﻿namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Code.ReportGenerator;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using log4net;
	using PaymentServices.Calculators;
	using PaymentServices.PayPoint;
	using ServiceClientProxy;

	public class LoanHistoryController : Controller {
		private static readonly ILog Log = LogManager.GetLogger(typeof(LoanHistoryController));
		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext _context;
		private readonly CustomerRepository _customerRepository;
		private readonly LoanRepository _loanRepository;
		private readonly LoanScheduleRepository _loanScheduleRepository;
		private readonly IPacnetPaypointServiceLogRepository _logRepository;
		private readonly PaymentRolloverRepository _rolloverRepository;
		private readonly IUsersRepository _users;
		private readonly PayPointApi _paypoint;

		public LoanHistoryController(CustomerRepository customersRepository,
									 PaymentRolloverRepository rolloverRepository,
									 LoanScheduleRepository loanScheduleRepository, IEzbobWorkplaceContext context,

									 IPacnetPaypointServiceLogRepository logRepository, LoanRepository loanRepository,
									 IUsersRepository users,
									 PayPointApi paypoint) {
			_customerRepository = customersRepository;
			_rolloverRepository = rolloverRepository;
			_loanScheduleRepository = loanScheduleRepository;
			_context = context;

			m_oServiceClient = new ServiceClient();
			_logRepository = logRepository;
			_loanRepository = loanRepository;
			_users = users;
			_paypoint = paypoint;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id) {
			Customer customer = _customerRepository.Get(id);
			return Json(new LoansAndOffers(customer), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Details(int customerid, int loanid) {
			var customer = _customerRepository.Get(customerid);
			var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

			if (loan == null) {
				return Json(new { error = "loan does not exists" }, JsonRequestBehavior.AllowGet);
			}

			var loansDetailsBuilder = new LoansDetailsBuilder();
			var details = loansDetailsBuilder.Build(loan, _rolloverRepository.GetByLoanId(loan.Id));
			decimal rolloverCharge = CurrentValues.Instance.RolloverCharge;
			var notExperiedRollover =
				_rolloverRepository.GetByLoanId(loan.Id)
				.Where(x => x.Status != RolloverStatus.Expired)
				.Select(x => new RolloverModel {
					Status = x.Status,
					ExpiryDate = x.ExpiryDate,
					Payment = x.Payment,
					PaidPaymentAmount = x.PaidPaymentAmount,
					LoanScheduleId = x.LoanSchedule.Id
				});
			var rolloverCount = _rolloverRepository.GetByLoanId(loan.Id).Count(x => x.Status != RolloverStatus.Removed && x.Status != RolloverStatus.Expired);

			bool transactionsDoneToday = loan.Transactions.Count(x => x.PostDate.Date == DateTime.UtcNow.Date && x.Status == LoanTransactionStatus.Done) > 0;
			string rolloverAvailableClass = transactionsDoneToday ? "disabled" : string.Empty;

			var model = new { details, configValues = new { rolloverCharge }, notExperiedRollover, rolloverCount, rolloverAvailableClass };

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult ExportToExel(int id) {
			Customer customer = _customerRepository.Get(id);
			return new LoanHistoryExelReportResult(customer);
		}

		[Ajax]
		[Transactional]
		public void RemoveRollover(int rolloverId) {
			var rollover = _rolloverRepository.GetById(rolloverId);
			rollover.Status = RolloverStatus.Removed;
			_rolloverRepository.Update(rollover);

			//TODO remove rollover from new rollover table
			Log.InfoFormat("remove rollover from new rollover table rollover id {0}", rolloverId);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public void AddRollover(int scheduleId, string experiedDate, bool isEditCurrent, decimal payment, int? rolloverId, int mounthCount) {
			var expDate = FormattingUtils.ParseDateWithoutTime(experiedDate);
			var currentLoanSchedule = _loanScheduleRepository.GetById(scheduleId);
			var rolloverModel = isEditCurrent && rolloverId.HasValue ? _rolloverRepository.GetById((int)rolloverId) : new PaymentRollover();
			var customer = currentLoanSchedule.Loan.Customer;

			if (expDate <= DateTime.UtcNow) {
				throw new Exception("Incorrect date");
			}
			if (mounthCount < 1) {
				throw new Exception("Month count must be at least 1");
			}

			if (isEditCurrent) {
				if (rolloverModel == null)
					throw new Exception("Loan schedule #{0} not found for editing");
			} else {
				var rollovers = _rolloverRepository.GetByLoanId(currentLoanSchedule.Loan.Id);
				if (rollovers.Any(rollover => rollover.Status == RolloverStatus.New && rollover.ExpiryDate > DateTime.UtcNow)) {
					throw new Exception("The loan has an unpaid rollover. Please close unpaid rollover and try again");
				}
			}

			rolloverModel.MounthCount = mounthCount;
			rolloverModel.ExpiryDate = expDate;
			rolloverModel.LoanSchedule = currentLoanSchedule;
			rolloverModel.CreatorName = _context.User.Name;
			rolloverModel.Created = DateTime.Now;
			rolloverModel.Payment = CurrentValues.Instance.RolloverCharge;
			rolloverModel.Status = RolloverStatus.New;
			_rolloverRepository.SaveOrUpdate(rolloverModel);


			//TODO add rollover to new rollover table
			Log.InfoFormat("add rollover to new rollover table schedule id {0}", scheduleId);

			//long nlLoanID = m_oServiceClient.Instance

			//  + GetRollover
			//NL_LoanRollovers nlRollover = new NL_LoanRollovers() {CreatedByUserID = _context.UserId, CreationTime = DateTime.Now, ExpirationTime =  expDate, LoanHistoryID = 

			m_oServiceClient.Instance.EmailRolloverAdded(_context.UserId, customer.Id, payment);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetRollover(int scheduleId) {
			return Json(new { roolover = _rolloverRepository.GetByCheduleId(scheduleId) }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public void ManualPayment(ManualPaymentModel model) {
			var realAmount = model.TotalSumPaid;
			var customer = _customerRepository.Get(model.CustomerId);

			try {

				Log.InfoFormat("Manual payment request for customer id {0}, amount {1}", customer.Id, realAmount);

				if (realAmount < 0) {
					throw new Exception("Amount is too small");
				}

				var date = FormattingUtils.ParseDateWithoutTime(model.PaymentDate);

				if (date > DateTime.UtcNow) {
					throw new Exception("The date is more than now");
				}

				if (date < DateTime.UtcNow.AddDays(-7)) {
					throw new Exception("The date is less than a week ago");
				}

				string payPointTransactionId = PaypointTransaction.Manual;

				if (model.ChargeClient) {
					var paypointCard = customer.PayPointCards.FirstOrDefault(x => x.IsDefaultCard);
					if (paypointCard == null && customer.PayPointCards.Any()) {
						paypointCard = customer.PayPointCards.First();
					}

					if (paypointCard == null) {
						throw new Exception("No Debit card found");
					}

					payPointTransactionId = paypointCard.TransactionId;

					this._paypoint.RepeatTransactionEx(paypointCard.PayPointAccount, payPointTransactionId, realAmount);
				}

				string description = string.Format("UW Manual payment method: {0}, description: {2}{2}{1}", model.PaymentMethod,
												   model.Description, Environment.NewLine);

				//if (Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value)) {
				var method = Enum.GetName(typeof(NLLoanTransactionMethods), model.PaymentMethod);
				var nlPayment = new NL_Payments() {
					CreatedByUserID = this._context.UserId,
					Amount = realAmount,
					PaymentMethodID = (int)Enum.Parse(typeof(NLLoanTransactionMethods), method),
					PaymentSystemType = NLPaymentSystemTypes.None,
				};

				Log.InfoFormat("ManualPayment: Sending nlPayment: {0} for customer {1}", nlPayment, customer.Id);
				//}

				var facade = new LoanPaymentFacade();
				facade.MakePayment(payPointTransactionId, realAmount, null,
												 "other", model.LoanId, customer,
												 date, description, null, model.PaymentMethod, nlPayment);

				Log.InfoFormat("add payment to new payment table customer {0}", customer.Id);
				var loan = customer.GetLoan(model.LoanId);
				facade.Recalculate(loan, DateTime.Now);

				if (model.SendEmail)
					this.m_oServiceClient.Instance.PayEarly(customer.Id, realAmount, customer.GetLoan(model.LoanId).RefNumber);

				this.m_oServiceClient.Instance.LoanStatusAfterPayment(
					this._context.UserId,
					customer.Id,
					customer.Name,
					model.LoanId,
					realAmount,
					model.SendEmail,
					loan.Balance,
					loan.Status == LoanStatus.PaidOff
				);

				string requestType = string.Format("UW Manual payment for customer {0}, amount {1}",
												   customer.PersonalInfo.Fullname, realAmount);
				_logRepository.Log(_context.UserId, date, requestType, "Successful", "");
			} catch (PayPointException ex) {
				_logRepository.Log(customer.Id, DateTime.UtcNow, "Paypoint Manual Payment", "Failed", ex.ToString());
			} catch (Exception exx) {
				_logRepository.Log(customer.Id, DateTime.UtcNow, "Paypoint Manual Payment", "Failed", exx.ToString());
			}
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetPaymentInfo(string date, decimal money, int loanId) {
			DateTime paymentDate = FormattingUtils.ParseDateWithoutTime(date);
			Loan loan = _loanRepository.Get(loanId);

			var hasRollover = _rolloverRepository.GetByLoanId(loanId).Any(x => x.Status == RolloverStatus.New);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, paymentDate, CurrentValues.Instance.AmountToChargeFrom);
			var state = payEarlyCalc.GetState();

			try {
				long nlLoanId = this.m_oServiceClient.Instance.GetLoanByOldID(loan.Id, 1, 1).Value;
				if (nlLoanId > 0) {
					var nlModel = this.m_oServiceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, paymentDate, 1, true).Value;
					Log.InfoFormat("<<< NL_Compare: nlModel : {0} loan: {1}  >>>", nlModel, loan);
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.InfoFormat("<<< NL_Compare Fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
			}

			var model = new LoanPaymentDetails {
				Balance = payEarlyCalc.TotalEarlyPayment(),
				MinValue = !hasRollover ? 0.01m : state.Fees + state.Interest
			};

			model.Amount = money;

			decimal amount = Math.Min(money, state.Fees);
			model.Fee = amount;
			money = money - amount;

			amount = Math.Min(money, state.Interest);
			model.Interest = amount;
			money = money - amount;

			model.Principal = money;

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetRolloverInfo(int loanId, bool isEdit) {
			var loan = _loanRepository.Get(loanId);
			var rollover = _rolloverRepository.GetByLoanId(loanId).FirstOrDefault(x => x.Status == RolloverStatus.New);
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			var state = payEarlyCalc.GetState();

			try {

				long nlLoanId = this.m_oServiceClient.Instance.GetLoanByOldID(loan.Id, 1, 1).Value;
				if (nlLoanId > 0) {
					var nlModel = this.m_oServiceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, DateTime.UtcNow, 1, true).Value;
					Log.InfoFormat("<<< NL_Compare: {0}\n===============loan: {1}  >>>", nlModel, loan);
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.InfoFormat("<<< NL_Compare fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
			}

			var rolloverCharge = CurrentValues.Instance.RolloverCharge;

			// TODO FOR NL: newcalc.Interest - interest at rollover info t'; calc.Fees - "lateCharge" == late fees at t'; rolloverCharge as now (from conf. variables)

			var model = new {
				rolloverAmount = state.Fees + state.Interest + (!isEdit ? rolloverCharge : 0),
				interest = state.Interest,
				lateCharge = state.Fees - (isEdit && state.Fees != 0 ? rolloverCharge : 0),
				mounthAmount = rollover != null ? rollover.MounthCount : 1
			};

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult ExportDetails(int id, int loanid, bool isExcel, bool wError) {
			var customer = _customerRepository.Get(id);
			var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

			if (loan == null) {
				return Json(new { error = "loan does not exists" }, JsonRequestBehavior.AllowGet);
			}
			return new LoanScheduleReportResult(_rolloverRepository, loan, isExcel, wError, customer);
		}
	}
}
