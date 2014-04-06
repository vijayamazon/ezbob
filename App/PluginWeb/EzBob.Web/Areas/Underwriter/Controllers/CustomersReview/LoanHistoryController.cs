﻿namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using Code.ReportGenerator;
	using Infrastructure;
	using PaymentServices.Calculators;
	using PaymentServices.PayPoint;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class LoanHistoryController : Controller
	{
		private static readonly ILog _log = LogManager.GetLogger("LoanHistoryController");
		private readonly ServiceClient m_oServiceClient;
		private readonly ConfigurationVariablesRepository _configurationVariablesRepository;
		private readonly IEzbobWorkplaceContext _context;
		private readonly CustomerRepository _customerRepository;
		private readonly LoanPaymentFacade _loanRepaymentFacade;
		private readonly LoanRepository _loanRepository;
		private readonly LoanScheduleRepository _loanScheduleRepository;
		private readonly IPacnetPaypointServiceLogRepository _logRepository;
		private readonly PaymentRolloverRepository _rolloverRepository;
		private readonly IUsersRepository _users;
		private readonly PayPointApi _paypoint;

		public LoanHistoryController(CustomerRepository customersRepository,
									 PaymentRolloverRepository rolloverRepository,
									 LoanScheduleRepository loanScheduleRepository, IEzbobWorkplaceContext context,
									 LoanPaymentFacade loanRepaymentFacade,
									 IPacnetPaypointServiceLogRepository logRepository, LoanRepository loanRepository,
									 ConfigurationVariablesRepository configurationVariablesRepository,
									 IUsersRepository users,
									 PayPointApi paypoint)
		{
			_customerRepository = customersRepository;
			_rolloverRepository = rolloverRepository;
			_loanScheduleRepository = loanScheduleRepository;
			_context = context;
			_loanRepaymentFacade = loanRepaymentFacade;
			m_oServiceClient = new ServiceClient();
			_logRepository = logRepository;
			_loanRepository = loanRepository;
			_configurationVariablesRepository = configurationVariablesRepository;
			_users = users;
			_paypoint = paypoint;
		}

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult Index(int id)
		{
			Customer customer = _customerRepository.Get(id);
			return Json(new LoansAndOffers(customer), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult Details(int customerid, int loanid)
		{
			var customer = _customerRepository.Get(customerid);
			var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

			if (loan == null)
			{
				return Json(new { error = "loan does not exists" }, JsonRequestBehavior.AllowGet);
			}

			var loansDetailsBuilder = new LoansDetailsBuilder();
			var details = loansDetailsBuilder.Build(loan, _rolloverRepository.GetByLoanId(loan.Id));
			var rolloverCharge = _configurationVariablesRepository.GetByName("RolloverCharge").Value;
			var notExperiedRollover =
				_rolloverRepository.GetByLoanId(loan.Id)
				.Where(x => x.Status != RolloverStatus.Expired)
				.Select(x => new RolloverModel
								{
									Status = x.Status,
									ExpiryDate = x.ExpiryDate,
									Payment = x.Payment,
									PaidPaymentAmount = x.PaidPaymentAmount,
									LoanScheduleId = x.LoanSchedule.Id
								});
			var rolloverCount = _rolloverRepository.GetByLoanId(loan.Id).Count(x => x.Status != RolloverStatus.Removed && x.Status != RolloverStatus.Expired);

			var model = new { details, configValues = new { rolloverCharge }, notExperiedRollover, rolloverCount };

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult ExportToExel(int id)
		{
			EZBob.DatabaseLib.Model.Database.Customer customer = _customerRepository.Get(id);
			return new LoanHistoryExelReportResult(customer);
		}

		[Ajax]
		[Transactional]
		public void RemoveRollover(int rolloverId)
		{
			var rollover = _rolloverRepository.GetById(rolloverId);
			rollover.Status = RolloverStatus.Removed;
			_rolloverRepository.Update(rollover);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public void AddRollover(int scheduleId, string experiedDate, bool isEditCurrent, decimal payment, int? rolloverId, int mounthCount)
		{
			var expDate = FormattingUtils.ParseDateWithoutTime(experiedDate);
			var currentLoanSchedule = _loanScheduleRepository.GetById(scheduleId);
			var rolloverModel = isEditCurrent ? _rolloverRepository.GetById((int)rolloverId) : new PaymentRollover();
			var customer = currentLoanSchedule.Loan.Customer;

			if (expDate <= DateTime.UtcNow)
			{
				throw new Exception("Incorrect date");
			}
			if (mounthCount < 1)
			{
				throw new Exception("Mounth count must be more 1");
			}

			if (isEditCurrent)
			{
				if (rolloverModel == null) throw new Exception("Loan schedule #{0} not found for editing");
			}
			else
			{
				var rollovers = _rolloverRepository.GetByLoanId(currentLoanSchedule.Loan.Id);
				if (
					rollovers.Any(
						rollover => rollover.Status == RolloverStatus.New && rollover.ExpiryDate > DateTime.UtcNow))
				{
					throw new Exception("The loan has an unpaid rollover. Please close unpaid rollover and try again");
				}
			}
			rolloverModel.MounthCount = mounthCount;
			rolloverModel.ExpiryDate = expDate;
			rolloverModel.LoanSchedule = currentLoanSchedule;
			rolloverModel.CreatorName = _context.User.Name;
			rolloverModel.Created = DateTime.Now;
			rolloverModel.Payment = Convert.ToDecimal(_configurationVariablesRepository.GetByName("RolloverCharge").Value);
			rolloverModel.Status = RolloverStatus.New;
			_rolloverRepository.SaveOrUpdate(rolloverModel);

			m_oServiceClient.Instance.EmailRolloverAdded(customer.Id, payment);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetRollover(int scheduleId)
		{
			return Json(new { roolover = _rolloverRepository.GetByCheduleId(scheduleId) }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public void ManualPayment(ManualPaymentModel model)
		{
			var realAmount = model.TotalSumPaid;
			var customer = _customerRepository.Get(model.CustomerId);

			try
			{


				_log.InfoFormat("Manual payment request for customer id {0}, amount {1}", customer.Id, realAmount);

				if (realAmount < 0)
				{
					throw new Exception("Amount is too small");
				}

				var date = FormattingUtils.ParseDateWithoutTime(model.PaymentDate);

				if (date > DateTime.UtcNow)
				{
					throw new Exception("The date is more than now");
				}

				string payPointTransactionId = "--- manual ---";

				if (model.ChargeClient)
				{
					payPointTransactionId = customer.PayPointTransactionId;
					_paypoint.RepeatTransactionEx(payPointTransactionId, realAmount);
				}

				string description = string.Format("Manual payment method: {0}, description: {2}{2}{1}", model.PaymentMethod,
												   model.Description, Environment.NewLine);

				_loanRepaymentFacade.MakePayment(payPointTransactionId, realAmount, null,
												 "other", model.LoanId, customer,
												 date, description);

				_loanRepaymentFacade.Recalculate(customer.GetLoan(model.LoanId), DateTime.Now);

				if (model.SendEmail)
					m_oServiceClient.Instance.PayEarly(_users.Get(customer.Id).Id, realAmount, customer.GetLoan(model.LoanId).RefNumber);

				string requestType = string.Format("Manual payment for customer {0}, amount {1}",
												   customer.PersonalInfo.Fullname, realAmount);
				_logRepository.Log(_context.UserId, date, requestType, "Successful", "");
			}
			catch (PayPointException ex)
			{
				_logRepository.Log(customer.Id, DateTime.UtcNow, "Paypoint Manual Payment", "Failed", ex.ToString());
			}
			catch (Exception exx)
			{
				_logRepository.Log(customer.Id, DateTime.UtcNow, "Paypoint Manual Payment", "Failed", exx.ToString());
			}
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetPaymentInfo(string date, decimal money, int loanId)
		{
			DateTime paymentDate = FormattingUtils.ParseDateWithoutTime(date);
			Loan loan = _loanRepository.Get(loanId);

			var hasRollover = _rolloverRepository.GetByLoanId(loanId).Any(x => x.Status == RolloverStatus.New);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, paymentDate);
			var state = payEarlyCalc.GetState();

			var model = new LoanPaymentDetails
							{
								Balance = payEarlyCalc.TotalEarlyPayment(),
								MinValue = !hasRollover ? 0.01m : state.Fees + state.Interest
							};

			money = (money > model.Balance) ? model.Balance : money;
			money = (money < model.MinValue) ? model.MinValue : money;
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
		public JsonResult GetRolloverInfo(int loanId, bool isEdit)
		{
			var loan = _loanRepository.Get(loanId);
			var rollover = _rolloverRepository.GetByLoanId(loanId).FirstOrDefault(x => x.Status == RolloverStatus.New);
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow);
			var state = payEarlyCalc.GetState();

			var rolloverCharge = Convert.ToDecimal(_configurationVariablesRepository.GetByName("RolloverCharge").Value);

			var model = new
							{
								rolloverAmount = state.Fees + state.Interest + (!isEdit ? rolloverCharge : 0),
								interest = state.Interest,
								lateCharge = state.Fees - (isEdit && state.Fees != 0 ? rolloverCharge : 0),
								mounthAmount = rollover != null ? rollover.MounthCount : 1
							};

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult ExportDetails(int id, int loanid, bool isExcel, bool wError)
		{
			var customer = _customerRepository.Get(id);
			var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

			if (loan == null)
			{
				return Json(new { error = "loan does not exists" }, JsonRequestBehavior.AllowGet);
			}
			return new LoanScheduleReportResult(_rolloverRepository, loan, isExcel, wError, customer);
		}
	}
}