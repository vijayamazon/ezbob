using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.CommonLib;
using EzBob.Configuration;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using NHibernate;
using PaymentServices.Calculators;
using PaymentServices.PacNet;
using Scorto.Configuration;
using Scorto.Web;
using StructureMap;
using ZohoCRM;
using log4net;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class GetCashController : Controller
    {
        private readonly IAppCreator _appCreator;
        private readonly PayPointConfiguration _config;

        private readonly IEzbobWorkplaceContext _context;
        private readonly IPayPointFacade _payPointFacade;
        private readonly ICustomerNameValidator _validator;
        private static readonly ILog _log = LogManager.GetLogger("EzBob.Web.Areas.Customer.Controllers.GetCashController");
        private readonly IPacnetPaypointServiceLogRepository _logRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILoanCreator _loanCreator;
        private readonly IZohoFacade _crm;
        private readonly ISession _session;

        //-------------------------------------------------------------------------------
        public GetCashController(
            IEzbobWorkplaceContext context, 
            IPayPointFacade payPointFacade, 
            IAppCreator appCreator,
            ICustomerNameValidator validator,
            IPacnetPaypointServiceLogRepository logRepository,
            ICustomerRepository customerRepository,
            ILoanCreator loanCreator,
            IZohoFacade crm,
            ISession session)
        {
            _context = context;
            _payPointFacade = payPointFacade;
            _appCreator = appCreator;
            _validator = validator;
            _logRepository = logRepository;
            _customerRepository = customerRepository;
            _loanCreator = loanCreator;
            _crm = crm;
            _session = session;
            _config = ConfigurationRootBob.GetConfiguration().PayPoint;
        }

        [NoCache]
        public RedirectResult GetTransactionId(decimal loan_amount, int loanType, int repaymentPeriod)
        {
            EZBob.DatabaseLib.Model.Database.Customer customer = _context.Customer;

            CheckCustomerStatus(customer);

            if (loan_amount < 0)
            {
                loan_amount = (int) Math.Floor(customer.CreditSum.Value);
            }
            var cr = customer.LastCashRequest;

			if (customer.IsLoanTypeSelectionAllowed == 1) {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				cr.RepaymentPeriod = repaymentPeriod;
				cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

	        DateTime lastDateOfPayment = DateTime.UtcNow.AddMonths(cr.RepaymentPeriod);

            decimal fee = !cr.HasLoans && cr.UseSetupFee ? (new SetupFeeCalculator()).Calculate(loan_amount) : 0;

            string callback = Url.Action("PayPointCallback", "GetCash",
                                         new
                                             {
                                                 Area = "Customer",
                                                 loan_amount,
                                                 fee,
                                                 username = _context.User.Name,
                                                 lastDatePayment = FormattingUtils.FormatDateToString(lastDateOfPayment)
                                             },
                                         "https");
            string url = _payPointFacade.GeneratePaymentUrl(5.00m, callback);
            _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Redirect to " + url, "Successful", "");
            return Redirect(url);
        }

        private static void CheckCustomerStatus(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            if (
                !customer.CreditSum.HasValue ||
                !customer.Status.HasValue ||
                customer.Status.Value != Status.Approved ||
                customer.CollectionStatus.CurrentStatus != CollectionStatusType.Enabled)
            {
                throw new Exception("Invalid customer state");
            }
        }

        [Transactional]
        [NoCache]
        public RedirectToRouteResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, decimal loan_amount, string card_no, string customer, string expiry)
        {
            //_session.Lock(_context.Customer, LockMode.Upgrade);

            if (test_status == "true")
            {
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
				if (random4Digits.Length > 4)
				{
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);
				}
				card_no = random4Digits;
                expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
            }

            DateTime now = DateTime.UtcNow;

            try
            {
                if (!valid || code != "A")
                {
                    _log.ErrorFormat("Invalid transaction. Id = {0}, Code: {1}, Message: {2}", trans_id, code, message);

                    _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
                                       String.Format("Invalid transaction. Id = {0}, Code: {1}, Message: {2}", trans_id,
                                                     code, message));

                    _context.Customer.PayPointErrorsCount++;

                    _appCreator.GetCashFailed(_context.User, _context.Customer.PersonalInfo.FirstName);

                    TempData["code"] = code;
                    TempData["message"] = message;

                    return RedirectToAction("Error", "Paypoint", new {Area = "Customer"});
                }

                if (!_payPointFacade.CheckHash(hash, Request.Url))
                {
                    _log.ErrorFormat("Paypoint callback is not authenticated for user {0}", _context.Customer.Id);
                    _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
                                       String.Format("Paypoint callback is not authenticated for user {0}",
                                                     _context.Customer.Id));
                    //return View("Error");
                    throw new Exception("check hash failed");
                }

                EZBob.DatabaseLib.Model.Database.Customer cus = _context.Customer;

                ValidateCustomerName(customer, cus);

                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Successful", "");


                var card = cus.TryAddPayPointCard(trans_id, card_no, expiry, customer);

                var loan = _loanCreator.CreateLoan(cus, loan_amount, card, now);

                cus.PayPointErrorsCount = 0;
                cus.PayPointTransactionId = trans_id;
                cus.CreditCardNo = card_no;

                TempData["amount"] = loan_amount;
                TempData["bankNumber"] = cus.BankAccount.AccountNumber;
                TempData["card_no"] = card_no;
                
                _customerRepository.Update(cus);
                _crm.UpdateOfferOnGetCash(cus.LastCashRequest, cus);

                return RedirectToAction("Index", "PacnetStatus", new {Area = "Customer"});
            }
            catch (OfferExpiredException e)
            {
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
                                   "Invalid apply for a loan period");
                return RedirectToAction("ErrorOfferDate", "Paypoint", new {Area = "Customer"});
            }
            catch (PacnetException)
            {
                _appCreator.GetCashFailed(_context.User, _context.Customer.PersonalInfo.FirstName);
                return RedirectToAction("Error", "Pacnet", new {Area = "Customer"});
            }
            catch (TargetInvocationException)
            {
                return RedirectToAction("ErrorOfferDate", "Paypoint", new {Area = "Customer"});
            }
        }

        [Transactional]
        [HttpPost]
        public JsonNetResult Now(int cardId, decimal amount)
        {
            var cus = _context.Customer;
            var card = cus.PayPointCards.First(c => c.Id == cardId);
            DateTime now = DateTime.UtcNow;
            var loan = _loanCreator.CreateLoan(cus, amount, card, now);

            var url = Url.Action("Index", "PacnetStatus", new {Area = "Customer"}, "https");

            _crm.UpdateOfferOnGetCash(cus.LastCashRequest, cus);

            return this.JsonNet(new {url = url});
        }

        private void ValidateCustomerName(string customer, EZBob.DatabaseLib.Model.Database.Customer cus)
        {
            if (!_validator.CheckCustomerName(customer, cus.PersonalInfo.FirstName, cus.PersonalInfo.Surname))
            {
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Warning",
                                   String.Format("Name {0} did not passed validation check for {1} {2}", customer,
                                                 cus.PersonalInfo.Surname, cus.PersonalInfo.Surname));
                _log.WarnFormat("Name {0} did not passed validation check for {1} {2}", customer,
                                cus.PersonalInfo.Surname,
                                cus.PersonalInfo.Surname);
                _appCreator.PayPointNameValidationFailed(customer, _context.User, cus);
            }
        }
    }
}