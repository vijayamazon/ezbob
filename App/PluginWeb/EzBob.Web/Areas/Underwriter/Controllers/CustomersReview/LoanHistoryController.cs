namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using Ezbob.Backend.Models;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using Code.ReportGenerator;
	using EzBob.Models;
	using Infrastructure;
	using PaymentServices.Calculators;
	using PaymentServices.PayPoint;
	using ServiceClientProxy;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class LoanHistoryController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(LoanHistoryController));
		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;
		private readonly CustomerRepository customerRepository;
		private readonly LoanRepository loanRepository;
		private readonly LoanScheduleRepository loanScheduleRepository;
		private readonly IPacnetPaypointServiceLogRepository logRepository;
		private readonly PaymentRolloverRepository rolloverRepository;
		private readonly PayPointApi paypoint;

		public LoanHistoryController(
			CustomerRepository customersRepository,
			PaymentRolloverRepository rolloverRepository,
			LoanScheduleRepository loanScheduleRepository, 
			IEzbobWorkplaceContext context,
			IPacnetPaypointServiceLogRepository logRepository, 
			LoanRepository loanRepository,
			PayPointApi paypoint, 
			ServiceClient serviceClient)
		{
			this.customerRepository = customersRepository;
			this.rolloverRepository = rolloverRepository;
			this.loanScheduleRepository = loanScheduleRepository;
			this.context = context;
			this.logRepository = logRepository;
			this.loanRepository = loanRepository;
			this.paypoint = paypoint;
			this.serviceClient = serviceClient;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			Customer customer = this.customerRepository.Get(id);
			var loans = customer
				.Loans
				.Select(l => LoanModel.FromLoan(l, new LoanRepaymentScheduleCalculator(l, null, CurrentValues.Instance.AmountToChargeFrom))).ToList();
			var decisions = this.serviceClient.Instance.LoadDecisionHistory(customer.Id, this.context.UserId);
			var offers = decisions.Model.Where(x => x.Action != "Escalate" && x.Action != "Pending" && x.Action != "Waiting")
				.Select(CashRequestModel.Create)
				.ToList();

			return Json(new { offers = offers, loans = loans }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Details(int customerid, int loanid)
		{
			var customer = this.customerRepository.Get(customerid);
			var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

			if (loan == null)
			{
				return Json(new { error = "loan does not exists" }, JsonRequestBehavior.AllowGet);
			}

			var loansDetailsBuilder = new LoansDetailsBuilder();
			var details = loansDetailsBuilder.Build(loan, this.rolloverRepository.GetByLoanId(loan.Id));
			decimal rolloverCharge = CurrentValues.Instance.RolloverCharge;
			var notExperiedRollover =
				this.rolloverRepository.GetByLoanId(loan.Id)
				.Where(x => x.Status != RolloverStatus.Expired)
				.Select(x => new RolloverModel
								{
									Status = x.Status,
									ExpiryDate = x.ExpiryDate,
									Payment = x.Payment,
									PaidPaymentAmount = x.PaidPaymentAmount,
									LoanScheduleId = x.LoanSchedule.Id
								});
			var rolloverCount = this.rolloverRepository.GetByLoanId(loan.Id).Count(x => x.Status != RolloverStatus.Removed && x.Status != RolloverStatus.Expired);

			bool transactionsDoneToday = loan.Transactions.Count(x => x.PostDate.Date == DateTime.UtcNow.Date && x.Status == LoanTransactionStatus.Done) > 0;
			string rolloverAvailableClass = transactionsDoneToday ? "disabled" : string.Empty;

			var model = new { details, configValues = new { rolloverCharge }, notExperiedRollover, rolloverCount, rolloverAvailableClass };

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult ExportToExel(int id)
		{
			Customer customer = this.customerRepository.Get(id);
			return new LoanHistoryExelReportResult(customer);
		}

		[Ajax]
		[Transactional]
		public void RemoveRollover(int rolloverId)
		{
			var rollover = this.rolloverRepository.GetById(rolloverId);
			rollover.Status = RolloverStatus.Removed;
			this.rolloverRepository.Update(rollover);

            //TODO remove rollover from new rollover table
		    Log.InfoFormat("remove rollover from new rollover table rollover id {0}", rolloverId);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public void AddRollover(int scheduleId, string experiedDate, bool isEditCurrent, decimal payment, int? rolloverId, int mounthCount)
		{
			var expDate = FormattingUtils.ParseDateWithoutTime(experiedDate);
			var currentLoanSchedule = this.loanScheduleRepository.GetById(scheduleId);
			var rolloverModel = isEditCurrent && rolloverId.HasValue ? this.rolloverRepository.GetById((int)rolloverId) : new PaymentRollover();
			var customer = currentLoanSchedule.Loan.Customer;

			if (expDate <= DateTime.UtcNow)
			{
				throw new Exception("Incorrect date");
			}
			if (mounthCount < 1)
			{
				throw new Exception("Month count must be at least 1");
			}

			if (isEditCurrent)
			{
				if (rolloverModel == null) throw new Exception("Loan schedule #{0} not found for editing");
			}
			else
			{
				var rollovers = this.rolloverRepository.GetByLoanId(currentLoanSchedule.Loan.Id);
				if (rollovers.Any(rollover => rollover.Status == RolloverStatus.New && rollover.ExpiryDate > DateTime.UtcNow))
				{
					throw new Exception("The loan has an unpaid rollover. Please close unpaid rollover and try again");
				}
			}

			rolloverModel.MounthCount = mounthCount;
			rolloverModel.ExpiryDate = expDate;
			rolloverModel.LoanSchedule = currentLoanSchedule;
			rolloverModel.CreatorName = this.context.User.Name;
			rolloverModel.Created = DateTime.Now;
			rolloverModel.Payment = CurrentValues.Instance.RolloverCharge;
			rolloverModel.Status = RolloverStatus.New;
			this.rolloverRepository.SaveOrUpdate(rolloverModel);


            //TODO add rollover to new rollover table
            Log.InfoFormat("add rollover to new rollover table schedule id {0}", scheduleId);

			this.serviceClient.Instance.EmailRolloverAdded(this.context.UserId, customer.Id, payment);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetRollover(int scheduleId)
		{
			return Json(new { roolover = this.rolloverRepository.GetByCheduleId(scheduleId) }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public void ManualPayment(ManualPaymentModel model)
		{
			var realAmount = model.TotalSumPaid;
			var customer = this.customerRepository.Get(model.CustomerId);

			try
			{

				Log.InfoFormat("Manual payment request for customer id {0}, amount {1}", customer.Id, realAmount);

				if (realAmount < 0)
				{
					throw new Exception("Amount is too small");
				}

				var date = FormattingUtils.ParseDateWithoutTime(model.PaymentDate);

				if (date > DateTime.UtcNow)
				{
					throw new Exception("The date is more than now");
				}

				if (date < DateTime.UtcNow.AddDays(-7)) {
					throw new Exception("The date is less than a week ago");
				}

				string payPointTransactionId = PaypointTransaction.Manual;
				
				if (model.ChargeClient) {
					var paypointCard = customer.PayPointCards.FirstOrDefault(x => x.IsDefaultCard);	
					if(paypointCard == null && customer.PayPointCards.Any()) {
						paypointCard = customer.PayPointCards.First();
					}

					if (paypointCard == null) {
						throw new Exception("No Debit card found");
					}

					payPointTransactionId = paypointCard.TransactionId;
					this.paypoint.RepeatTransactionEx(paypointCard.PayPointAccount, payPointTransactionId, realAmount);
				}

				string description = string.Format("UW Manual payment method: {0}, description: {2}{2}{1}", model.PaymentMethod,
												   model.Description, Environment.NewLine);

				var facade = new LoanPaymentFacade();

				facade.MakePayment(payPointTransactionId, realAmount, null,
												 "other", model.LoanId, customer,
												 date, description, null, model.PaymentMethod);

                //TODO add payment to new table
                Log.InfoFormat("add payment to new payment table customer {0}", customer.Id);
				var loan = customer.GetLoan(model.LoanId);
				facade.Recalculate(loan, DateTime.Now);

				if (model.SendEmail)
					this.serviceClient.Instance.PayEarly(customer.Id, realAmount, customer.GetLoan(model.LoanId).RefNumber);

				this.serviceClient.Instance.LoanStatusAfterPayment(
					this.context.UserId, 
					customer.Id, 
					customer.Name, 
					model.LoanId,
					realAmount, 
					loan.Balance, 
					loan.Status == LoanStatus.PaidOff,
					model.SendEmail);

				string requestType = string.Format("UW Manual payment for customer {0}, amount {1}",
												   customer.PersonalInfo.Fullname, realAmount);
				this.logRepository.Log(this.context.UserId, date, requestType, "Successful", "");
			}
			catch (PayPointException ex)
			{
				this.logRepository.Log(customer.Id, DateTime.UtcNow, "Paypoint Manual Payment", "Failed", ex.ToString());
			}
			catch (Exception exx)
			{
				this.logRepository.Log(customer.Id, DateTime.UtcNow, "Paypoint Manual Payment", "Failed", exx.ToString());
			}
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetPaymentInfo(string date, decimal money, int loanId)
		{
			DateTime paymentDate = FormattingUtils.ParseDateWithoutTime(date);
			Loan loan = this.loanRepository.Get(loanId);

			var hasRollover = this.rolloverRepository.GetByLoanId(loanId).Any(x => x.Status == RolloverStatus.New);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, paymentDate, CurrentValues.Instance.AmountToChargeFrom);
			var state = payEarlyCalc.GetState();

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
		public JsonResult GetRolloverInfo(int loanId, bool isEdit)
		{
			var loan = this.loanRepository.Get(loanId);
			var rollover = this.rolloverRepository.GetByLoanId(loanId).FirstOrDefault(x => x.Status == RolloverStatus.New);
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			var state = payEarlyCalc.GetState();

			var rolloverCharge = CurrentValues.Instance.RolloverCharge;

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
			var customer = this.customerRepository.Get(id);
			var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

			if (loan == null)
			{
				return Json(new { error = "loan does not exists" }, JsonRequestBehavior.AllowGet);
			}
			return new LoanScheduleReportResult(this.rolloverRepository, loan, isExcel, wError, customer);
		}
	}
}
