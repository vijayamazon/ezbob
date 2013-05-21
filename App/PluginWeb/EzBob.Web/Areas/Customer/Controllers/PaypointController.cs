﻿using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using EzBob.Web.Models;
using PaymentServices.Calculators;
using PaymentServices.PayPoint;
using Scorto.Web;
using ZohoCRM;
using log4net;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class PaypointController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly PayPointFacade _payPointFacade;
        private readonly IAppCreator _appCreator;
        private readonly IZohoFacade _crm;
        private static readonly ILog Log = LogManager.GetLogger("PaypointController");
        private readonly LoanPaymentFacade _loanRepaymentFacade;
        private readonly IPacnetPaypointServiceLogRepository _logRepository;
        private readonly IPaypointTransactionRepository _paypointTransactionRepository;

        public PaypointController(IEzbobWorkplaceContext context, PayPointFacade payPointFacade, IAppCreator appCreator, LoanPaymentFacade loanPaymentFacade, IPacnetPaypointServiceLogRepository pacnetPaypointServiceLogRepository, IPaypointTransactionRepository paypointTransactionRepository, IZohoFacade crm)
        {
            _context = context;
            _payPointFacade = payPointFacade;
            _appCreator = appCreator;
            _logRepository = pacnetPaypointServiceLogRepository;
            _paypointTransactionRepository = paypointTransactionRepository;
            _crm = crm;
            _loanRepaymentFacade = loanPaymentFacade;
        }

        [NoCache]
        public ActionResult Pay(decimal amount, string type, int loanId, int rolloverId)
        {
            try
            {
                Log.InfoFormat("Payment request for customer id {0}, amount {1}", _context.Customer.Id, amount);

                amount = CalculateRealAmount(type, loanId, amount);

                if (amount < 0)
                {
                    return View("Error");
                }

                var callback = Url.Action("Callback", "Paypoint", new {Area = "Customer", loanId, type, username= (_context.User != null ? _context.User.Name : "")}, "https");
                var url = _payPointFacade.GeneratePaymentUrl(amount, callback);

                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to " + url, "Successful", "");
                _crm.UpdateLoans(_context.Customer);

                return Redirect(url);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return View("Error");
            }
        }

        [HttpGet]
        [NoCache]
        [Transactional]
        public ActionResult Callback(bool valid, string trans_id, string code, string auth_code, decimal amount, string ip, string test_status, string hash, string message, string type, int loanId, string card_no, string expiry)
        {
            if (test_status == "true")
            {
                card_no = "1111";
                expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
            }

            var customer = _context.Customer;

            if (!_payPointFacade.CheckHash(hash, Request.Url))
            {
                Log.ErrorFormat("Paypoint callback is not authenticated for user {0}", customer.Id);
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint callback is not authenticated for user {0}", customer.Id));
                return View("Error");
            }

            var statusDescription = PayPointStatusTranslator.TranslateStatusCode(code);

            if (!valid || code != "A")
            {
                Log.ErrorFormat("Paypoint result code is : {0} ({1}). Message: {2}", code, string.Join(", ", statusDescription.ToArray()), message);
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint result code is : {0} ({1}). Message: {2}", code, string.Join(", ", statusDescription.ToArray()), message));
                return View("Error");
            }

            //if there is transaction with such id in database,
            //it means that customer refreshes page
            //show in this case cashed result
            if (_paypointTransactionRepository.ByGuid(trans_id).Any())
            {
                var data = TempData.Get<PaymentConfirmationModel>();
                if (data == null) return RedirectToAction("Index", "Profile", new {Area = "Customer"});
                return View(TempData.Get<PaymentConfirmationModel>());
            }

            var res = _loanRepaymentFacade.MakePayment(trans_id, amount, ip, type, loanId, customer);

            _appCreator.PayEarly(_context.User, DateTime.Now, amount, customer.PersonalInfo.FirstName);
            _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Callback", "Successful", "");

            var refNumber = "";

            if(loanId > 0)
            {
                var loan = customer.GetLoan(loanId);
                if(loan != null)
                {
                    refNumber = loan.RefNumber;
                }
            }

            customer.TryAddPayPointCard(trans_id, card_no, expiry, customer.PersonalInfo.Fullname);

            var confirmation = new PaymentConfirmationModel
                {
                    amount = amount.ToString(CultureInfo.InvariantCulture),
                    saved = res.Saved,
                    savedPounds = res.SavedPounds,
                    card_no = customer.CreditCardNo,
                    email = customer.Name,
                    surname = customer.PersonalInfo.Surname,
                    name = customer.PersonalInfo.FirstName,
                    refnum = refNumber,
                    transRefnums = res.TransactionRefNumbersFormatted,
                    hasLateLoans = customer.HasLateLoans,
                    isRolloverPaid = res.RolloverWasPaid
                };

            _crm.UpdateLoans(_context.Customer);
            TempData.Put(confirmation);
            return View(confirmation);
        }

        [Transactional]
        [HttpPost]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult PayFast(string amount, string type, string paymentType, int loanId, int cardId)
        {
            try
            {
                decimal realAmount = decimal.Parse(amount, CultureInfo.InvariantCulture);

                var customer = _context.Customer;

                Log.InfoFormat("Payment request for customer id {0}, amount {1}", customer.Id, realAmount);

                realAmount = CalculateRealAmount(type, loanId, realAmount);

                if(realAmount < 0)
                {
                    return this.JsonNet(new {error = "amount is too small"});
                }

                var paypoint = new PayPointApi();

                var card = customer.PayPointCards.FirstOrDefault(c => c.Id == cardId);

                var payPointTransactionId = card == null ? customer.PayPointTransactionId : card.TransactionId;

                paypoint.RepeatTransactionEx(payPointTransactionId, realAmount);

                var payFastModel = _loanRepaymentFacade.MakePayment(payPointTransactionId, realAmount, null, type, loanId, customer, DateTime.UtcNow, "payment from customer", paymentType);
                
                _appCreator.PayEarly(_context.User, DateTime.Now, realAmount, customer.PersonalInfo.FirstName);
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Early Fast Callback", "Successful", "");
                _crm.UpdateLoans(_context.Customer);

                return this.JsonNet(payFastModel);
            }
            catch (PayPointException e)
            {
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Early Fast Callback", "Failed", e.ToString());
                return this.JsonNet(new { error = "Error occurred while making payment" });
            }
            catch(Exception e)
            {
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Early Fast Callback", "Failed", e.ToString());
                return this.JsonNet(new { error = e.Message });
            }
        }

        private decimal CalculateRealAmount(string type, int loanId, decimal realAmount)
        {
            if (type == "total")
            {
                realAmount = _context.Customer.TotalEarlyPayment();
            }

            if (type == "loan")
            {
                Loan loan = _context.Customer.Loans.Single(l => l.Id == loanId);
                realAmount = Math.Min(realAmount, loan.TotalEarlyPayment());
                realAmount = Math.Max(realAmount, 0);
            }
            return realAmount;
        }

        [NoCache]
        public ActionResult Error()
        {
            var code = (string)TempData["code"];
            var message = (string)TempData["message"];

            if(string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Profile", new {Area = "Customer"});
            }

            var statusDescription = PayPointStatusTranslator.TranslateStatusCode(code);

            var msg = string.Format("Paypoint result code is : {0} ({1}). Message: {2}", code,
                                    string.Join(", ", statusDescription.ToArray()), message);

            Log.Error(msg);

            ViewData["Message"] = msg;

            return View("Error");
        }

        [NoCache]
        public ActionResult ErrorOfferDate()
        {
            ViewData["Message"] = "Unfortunately, time of the offer expired! Please apply for a new offer.";
            return View("ErrorOfferDate");
        }
    }
}
