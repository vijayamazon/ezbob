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
        private readonly PayPointApi _paypoint;

        public PaypointController(IEzbobWorkplaceContext context, PayPointFacade payPointFacade, IAppCreator appCreator, LoanPaymentFacade loanPaymentFacade, IPacnetPaypointServiceLogRepository pacnetPaypointServiceLogRepository, IPaypointTransactionRepository paypointTransactionRepository, IZohoFacade crm, PayPointApi paypoint)
        {
            _context = context;
            _payPointFacade = payPointFacade;
            _appCreator = appCreator;
            _logRepository = pacnetPaypointServiceLogRepository;
            _paypointTransactionRepository = paypointTransactionRepository;
            _crm = crm;
            _loanRepaymentFacade = loanPaymentFacade;
            _paypoint = paypoint;
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
        public ActionResult Callback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string type, int loanId, string card_no, string customer, string expiry)
        {
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

            var customerContext = _context.Customer;

            if (!_payPointFacade.CheckHash(hash, Request.Url))
            {
                Log.ErrorFormat("Paypoint callback is not authenticated for user {0}", customerContext.Id);
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint callback is not authenticated for user {0}", customerContext.Id));
                return View("Error");
            }

            var statusDescription = PayPointStatusTranslator.TranslateStatusCode(code);

            if (!valid || code != "A")
            {
                Log.ErrorFormat("Paypoint result code is : {0} ({1}). Message: {2}", code, string.Join(", ", statusDescription.ToArray()), message);
                _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint result code is : {0} ({1}). Message: {2}", code, string.Join(", ", statusDescription.ToArray()), message));
                return View("Error");
            }

			if (!amount.HasValue)
			{
				Log.ErrorFormat("Paypoint amount is null. Message: {0}", message);
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint amount is null. Message: {0}", message));
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

            var res = _loanRepaymentFacade.MakePayment(trans_id, amount.Value, ip, type, loanId, customerContext);

            SendEmails(loanId, amount.Value, customerContext);

            _logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Callback", "Successful", "");

            var refNumber = "";

            if(loanId > 0)
            {
                var loan = customerContext.GetLoan(loanId);
                if(loan != null)
                {
                    refNumber = loan.RefNumber;
                }
            }
            
            if (string.IsNullOrEmpty(customer)) customer = customerContext.PersonalInfo.Fullname;

            customerContext.TryAddPayPointCard(trans_id, card_no, expiry, customer);

            var confirmation = new PaymentConfirmationModel
                {
                    amount = amount.Value.ToString(CultureInfo.InvariantCulture),
                    saved = res.Saved,
                    savedPounds = res.SavedPounds,
                    card_no = card_no,
                    email = customerContext.Name,
                    surname = customerContext.PersonalInfo.Surname,
                    name = customerContext.PersonalInfo.FirstName,
                    refnum = refNumber,
                    transRefnums = res.TransactionRefNumbersFormatted,
                    hasLateLoans = customerContext.HasLateLoans,
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

                var card = customer.PayPointCards.FirstOrDefault(c => c.Id == cardId);

                var payPointTransactionId = card == null ? customer.PayPointTransactionId : card.TransactionId;

                _paypoint.RepeatTransactionEx(payPointTransactionId, realAmount);

                var payFastModel = _loanRepaymentFacade.MakePayment(payPointTransactionId, realAmount, null, type, loanId, customer, DateTime.UtcNow, "payment from customer", paymentType);
                payFastModel.CardNo = card == null ? customer.CreditCardNo : card.CardNo;

                SendEmails(loanId, realAmount, customer);
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

        private void SendEmails(int loanId, decimal realAmount, EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var loan = customer.GetLoan(loanId);
            
            _appCreator.PayEarly(_context.User, DateTime.Now, realAmount, customer.PersonalInfo.FirstName, loan.RefNumber);
            
            if (loan.Status == LoanStatus.PaidOff)
            {
                _appCreator.LoanFullyPaid(loan);
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
