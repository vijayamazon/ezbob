using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EZBob.DatabaseLib.Repository;
using EzBob.Models;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using EzBob.Web.Code.ReportGenerator;
using EzBob.Web.Infrastructure;
using PaymentServices.Calculators;
using PaymentServices.PayPoint;
using Scorto.Web;
using log4net;


namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class LoanHistoryController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger("LoanHistoryController");
        private readonly IAppCreator _appCreator;
        private readonly ConfigurationVariablesRepository _configurationVariablesRepository;
        private readonly IEzbobWorkplaceContext _context;
        private readonly CustomerRepository _customerRepository;
        private readonly LoanPaymentFacade _loanRepaymentFacade;
        private readonly LoanRepository _loanRepository;
        private readonly LoanScheduleRepository _loanScheduleRepository;
        private readonly IPacnetPaypointServiceLogRepository _logRepository;
        private readonly PaymentRolloverRepository _rolloverRepository;
        private readonly LoanChargesRepository _chargesRepository;
        private readonly IUsersRepository _users;

        public LoanHistoryController(CustomerRepository customersRepository,
                                     PaymentRolloverRepository rolloverRepository,
                                     LoanScheduleRepository loanScheduleRepository, IEzbobWorkplaceContext context,
                                     LoanPaymentFacade loanRepaymentFacade, IAppCreator appCreator,
                                     IPacnetPaypointServiceLogRepository logRepository, LoanRepository loanRepository,
                                     ConfigurationVariablesRepository configurationVariablesRepository, LoanChargesRepository chargesRepository, IUsersRepository users)
        {
            _customerRepository = customersRepository;
            _rolloverRepository = rolloverRepository;
            _loanScheduleRepository = loanScheduleRepository;
            _context = context;
            _loanRepaymentFacade = loanRepaymentFacade;
            _appCreator = appCreator;
            _logRepository = logRepository;
            _loanRepository = loanRepository;
            _configurationVariablesRepository = configurationVariablesRepository;
            _chargesRepository = chargesRepository;
            _users = users;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int id)
        {
            EZBob.DatabaseLib.Model.Database.Customer customer = _customerRepository.Get(id);
            List<LoanModel> loans =
                customer.Loans.Select(l => LoanModel.FromLoan(l, new PayEarlyCalculator2(l, null))).ToList();

            List<CashRequestModel> offers = customer.CashRequests
                                                    .OrderBy(c => c.CreationDate)
                                                    .Select(c => CashRequestModel.Create(c))
                                                    .ToList();

            return this.JsonNet(new {loans, offers});
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Details(int customerid, int loanid)
        {
            var customer = _customerRepository.Get(customerid);
            var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

            if (loan == null)
            {
                return this.JsonNet(new {error = "loan does not exists"});
            }

            var loansDetailsBuilder = new LoansDetailsBuilder();
            var details = loansDetailsBuilder.Build(loan, _rolloverRepository.GetByLoanId(loan.Id));
            var rolloverCharge = _configurationVariablesRepository.GetByName("RolloverCharge").Value;
            var notExperiedRollover =
                _rolloverRepository.GetByLoanId(loan.Id)
                .Where(x => x.Status != RolloverStatus.Expired)
                .Select(x=> new RolloverModel
                                {
                                    Status = x.Status,
                                    ExpiryDate = x.ExpiryDate,
                                    Payment = x.Payment,
                                    PaidPaymentAmount = x.PaidPaymentAmount,
                                    LoanScheduleId = x.LoanSchedule.Id
                                });
            var rolloverCount = _rolloverRepository.GetByLoanId(loan.Id).Count(x => x.Status != RolloverStatus.Removed && x.Status != RolloverStatus.Expired);

            var model = new { details, configValues = new { rolloverCharge }, notExperiedRollover, rolloverCount };

            return this.JsonNet(model);
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
            var rolloverModel = isEditCurrent ? _rolloverRepository.GetById((int) rolloverId) : new PaymentRollover();
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

            _appCreator.EmailRolloverAdded(customer, payment, expDate);
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult GetRollover(int scheduleId)
        {
            return this.JsonNet(new {roolover = _rolloverRepository.GetByCheduleId(scheduleId)});
        }

        [Ajax]
        [HttpPost]
        public void ManualPayment(ManualPaymentModel model)
        {
            var realAmount = model.TotalSumPaid;
            var customer = _customerRepository.Get(model.CustomerId);

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
                var paypoint = new PayPointApi();
                payPointTransactionId = customer.PayPointTransactionId;
                paypoint.RepeatTransactionEx(payPointTransactionId, realAmount);
            }

            string description = string.Format("Manual payment method: {0}, description: {2}{2}{1}", model.PaymentMethod, model.Description, Environment.NewLine);

            _loanRepaymentFacade.MakePayment(payPointTransactionId, realAmount, null,
                                             "other", model.LoanId, customer,
                                             date, description);

            _loanRepaymentFacade.Recalculate(customer.GetLoan(model.LoanId), DateTime.Now);

            if (model.SendEmail)
            {
                _appCreator.PayEarly(_users.Get(customer.Id), date, realAmount, customer.PersonalInfo.FirstName, customer.GetLoan(model.LoanId).RefNumber);
            }

            string requestType = string.Format("Manual payment for customer {0}, amount {1}", customer.PersonalInfo.Fullname, realAmount);
            _logRepository.Log(_context.UserId, date, requestType, "Successful", "");
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult GetPaymentInfo(string date, decimal money, int loanId)
        {
            DateTime paymentDate = FormattingUtils.ParseDateWithoutTime(date);
            Loan loan = _loanRepository.Get(loanId);
            
            var hasRollover = _rolloverRepository.GetByLoanId(loanId).Any(x => x.Status == RolloverStatus.New);

            var payEarlyCalc = new PayEarlyCalculator2(loan, paymentDate);
            var state = payEarlyCalc.GetState();

            var model = new LoanPaymentDetails
                            {
                                Balance = payEarlyCalc.TotalEarlyPayment(),
                                MinValue = !hasRollover ? 0.01m : state.Fees + state.Interest
                            };

            money = ( money > model.Balance ) ? model.Balance : money;
            money = (money < model.MinValue) ? model.MinValue : money;
            model.Amount = money;

            decimal amount = Math.Min(money, state.Fees);
            model.Fee = amount;
            money = money - amount;

            amount = Math.Min(money, state.Interest);
            model.Interest = amount;
            money = money - amount;

            model.Principal = money;

            return this.JsonNet(model);
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult GetRolloverInfo(int loanId, bool isEdit)
        {
            var loan = _loanRepository.Get(loanId);
            var rollover = _rolloverRepository.GetByLoanId(loanId).FirstOrDefault(x => x.Status == RolloverStatus.New);
            var payEarlyCalc = new PayEarlyCalculator2(loan, DateTime.UtcNow);
            var state = payEarlyCalc.GetState();
            
            var rolloverCharge = Convert.ToDecimal(_configurationVariablesRepository.GetByName("RolloverCharge").Value);

            var model = new
                            {
                                rolloverAmount = state.Fees + state.Interest + (!isEdit ? rolloverCharge : 0),
                                interest = state.Interest,
                                lateCharge = state.Fees - (isEdit && state.Fees !=0 ? rolloverCharge : 0),
                                mounthAmount = rollover != null ? rollover.MounthCount : 1
                            };

            return this.JsonNet(model);
        }

        [HttpGet]
        public ActionResult ExportDetails(int id, int loanid, bool isExcel)
        {
            var customer = _customerRepository.Get(id);
            var loan = customer.Loans.SingleOrDefault(l => l.Id == loanid);

            if (loan == null)
            {
                return this.JsonNet(new { error = "loan does not exists" });
            }
            return new LoanScheduleReportResult(_rolloverRepository, loan, isExcel, customer);
        }
    }
}