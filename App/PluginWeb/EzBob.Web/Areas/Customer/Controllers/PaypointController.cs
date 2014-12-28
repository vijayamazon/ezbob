﻿namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using EzBob.Web.Areas.Customer.Models;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using log4net;
	using PaymentServices.Calculators;
	using PaymentServices.PayPoint;
	using ServiceClientProxy;

	public class PaypointController : Controller {
		public PaypointController(
			IEzbobWorkplaceContext context,
			PayPointFacade payPointFacade,
			LoanPaymentFacade loanPaymentFacade,
			IPacnetPaypointServiceLogRepository pacnetPaypointServiceLogRepository,
			IPaypointTransactionRepository paypointTransactionRepository,
			PayPointApi paypoint,
			ICustomerRepository customerRepository,
			PayPointAccountRepository payPointAccountRepository) {
			_context = context;
			_payPointFacade = payPointFacade;
			m_oServiceClient = new ServiceClient();
			_logRepository = pacnetPaypointServiceLogRepository;
			_paypointTransactionRepository = paypointTransactionRepository;
			_loanRepaymentFacade = loanPaymentFacade;
			_paypoint = paypoint;
			_customerRepository = customerRepository;
			this.payPointAccountRepository = payPointAccountRepository;
		}

		[HttpGet]
		[NoCache]
		[Transactional]
		public ActionResult Callback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string type, int loanId, string card_no, string customer, string expiry) {
			if (test_status == "true") {
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
				if (random4Digits.Length > 4)
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);
				card_no = random4Digits;
				expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2)
					.Year.ToString()
					.Substring(2, 2));
			}

			var customerContext = _context.Customer;

			if (!_payPointFacade.CheckHash(hash, Request.Url)) {
				Log.ErrorFormat("Paypoint callback is not authenticated for user {0}", customerContext.Id);
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint callback is not authenticated for user {0}", customerContext.Id));
				return View("Error");
			}

			var statusDescription = PayPointStatusTranslator.TranslateStatusCode(code);

			if (!valid || code != "A") {
				if (code == "N") {
					Log.WarnFormat("Paypoint result code is : {0} ({1}). Message: {2}", code,
						string.Join(", ", statusDescription.ToArray()), message);
				} else {
					Log.ErrorFormat("Paypoint result code is : {0} ({1}). Message: {2}", code,
						string.Join(", ", statusDescription.ToArray()), message);
				}
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint result code is : {0} ({1}). Message: {2}", code, string.Join(", ", statusDescription.ToArray()), message));
				return View("Error");
			}

			if (!amount.HasValue) {
				Log.ErrorFormat("Paypoint amount is null. Message: {0}", message);
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to ", "Failed", String.Format("Paypoint amount is null. Message: {0}", message));
				return View("Error");
			}

			//if there is transaction with such id in database,
			//it means that customer refreshes page
			//show in this case cashed result
			if (_paypointTransactionRepository.ByGuid(trans_id)
				.Any()) {
				var data = TempData.Get<PaymentConfirmationModel>();
				if (data == null) {
					return RedirectToAction("Index", "Profile", new {
						Area = "Customer"
					});
				}
				return View(TempData.Get<PaymentConfirmationModel>());
			}

			var res = _loanRepaymentFacade.MakePayment(trans_id, amount.Value, ip, type, loanId, customerContext);

			SendEmails(loanId, amount.Value, customerContext);

			_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Callback", "Successful", "");

			var refNumber = "";
			bool isEarly = false;
			if (loanId > 0) {
				var loan = customerContext.GetLoan(loanId);
				if (loan != null) {
					refNumber = loan.RefNumber;
					if (loan.Schedule != null) {
						var scheduledPayments = loan.Schedule.Where(x => x.Status == LoanScheduleStatus.StillToPay ||
																		x.Status == LoanScheduleStatus.Late ||
																		x.Status == LoanScheduleStatus.AlmostPaid);

						if (scheduledPayments.Any()) {
							DateTime earliestSchedule = scheduledPayments.Min(x => x.Date);
							if (earliestSchedule.Date >= DateTime.UtcNow && (earliestSchedule.Date.Year != DateTime.UtcNow.Year || earliestSchedule.Date.Month != DateTime.UtcNow.Month || earliestSchedule.Date.Day != DateTime.UtcNow.Day))
								isEarly = true;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(customer))
				customer = customerContext.PersonalInfo.Fullname;
			var account = payPointAccountRepository.GetDefaultAccount();
			customerContext.TryAddPayPointCard(trans_id, card_no, expiry, customer, account);

			var confirmation = new PaymentConfirmationModel {
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
				isRolloverPaid = res.RolloverWasPaid,
				IsEarly = isEarly
			};

			TempData.Put(confirmation);
			return View(confirmation);
		}

		[NoCache]
		public ActionResult Error() {
			var code = (string)TempData["code"];
			var message = (string)TempData["message"];

			if (string.IsNullOrEmpty(code)) {
				return RedirectToAction("Index", "Profile", new {
					Area = "Customer"
				});
			}

			var statusDescription = PayPointStatusTranslator.TranslateStatusCode(code);

			var msg = string.Format("Paypoint result code is : {0} ({1}). Message: {2}", code,
				string.Join(", ", statusDescription.ToArray()), message);

			if (code == "N")
				Log.Warn(msg);
			else
				Log.Error(msg);

			ViewData["Message"] = msg;

			return View("Error");
		}

		[NoCache]
		public ActionResult ErrorOfferDate() {
			ViewData["Message"] = "Unfortunately, time of the offer expired! Please apply for a new offer.";
			return View("ErrorOfferDate");
		}

		[NoCache]
		public ActionResult Pay(decimal amount, string type, int loanId, int rolloverId) {
			try {
				Log.InfoFormat("Payment request for customer id {0}, amount {1}", _context.Customer.Id, amount);

				amount = CalculateRealAmount(type, loanId, amount);
				var paypointAccount = payPointAccountRepository.GetDefaultAccount();
				if (amount < 0)
					return View("Error");
				var oCustomer = _context.Customer;
				int payPointCardExpiryMonths = paypointAccount.CardExpiryMonths;
				DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointCardExpiryMonths);

				var callback = Url.Action("Callback", "Paypoint", new {
					Area = "Customer",
					loanId,
					type,
					username = (_context.User != null ? _context.User.Name : ""),
					cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate),
					hideSteps = true,
					payEarly = true
				}, "https");

				var url = _payPointFacade.GeneratePaymentUrl(oCustomer, amount, callback);
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Redirect to " + url, "Successful", "");

				return Redirect(url);
			} catch (Exception e) {
				Log.Error(e);
				return View("Error");
			}
		}

		[Transactional]
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PayFast(string amount, string type, string paymentType, int loanId, int cardId) {
			try {
				decimal realAmount = decimal.Parse(amount, CultureInfo.InvariantCulture);

				var customer = _context.Customer;

				Log.InfoFormat("Payment request for customer id {0}, amount {1}", customer.Id, realAmount);

				realAmount = CalculateRealAmount(type, loanId, realAmount);

				if (realAmount < 0) {
					return Json(new {
						error = "amount is too small"
					});
				}

				PayPointCard card = null;
				if (cardId == -1)
					card = customer.PayPointCards.FirstOrDefault(c => c.IsDefaultCard);
				else
					card = customer.PayPointCards.FirstOrDefault(c => c.Id == cardId);

				if (card == null)
					throw new Exception("Card not found");

				_paypoint.RepeatTransactionEx(card.PayPointAccount, card.TransactionId, realAmount);

				var payFastModel = _loanRepaymentFacade.MakePayment(card.TransactionId, realAmount, null, type, loanId, customer, DateTime.UtcNow, "manual payment from customer", paymentType, "CustomerAuto");
				payFastModel.CardNo = card.CardNo;

				SendEmails(loanId, realAmount, customer);
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Early Fast Callback", "Successful", "");

				return Json(payFastModel);
			} catch (PayPointException e) {
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Early Fast Callback", "Failed", e.ToString());
				return Json(new {
					error = "Error occurred while making payment"
				});
			} catch (Exception e) {
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint Pay Early Fast Callback", "Failed", e.ToString());
				return Json(new {
					error = e.Message
				});
			}
		}

		private decimal CalculateRealAmount(string type, int loanId, decimal realAmount) {
			if (type == "total")
				realAmount = _context.Customer.TotalEarlyPayment();

			if (type == "loan") {
				Loan loan = _context.Customer.Loans.Single(l => l.Id == loanId);
				realAmount = Math.Min(realAmount, loan.TotalEarlyPayment());
				realAmount = Math.Max(realAmount, 0);
			}
			return realAmount;
		}

		private void SendEmails(int loanId, decimal realAmount, Customer customer) {
			var loan = customer.GetLoan(loanId);

			m_oServiceClient.Instance.PayEarly(_context.User.Id, realAmount, loan.RefNumber);

			if (loan.Status == LoanStatus.PaidOff)
				m_oServiceClient.Instance.LoanFullyPaid(customer.Id, loan.RefNumber);
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof (PaypointController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly ICustomerRepository _customerRepository;
		private readonly LoanPaymentFacade _loanRepaymentFacade;
		private readonly IPacnetPaypointServiceLogRepository _logRepository;
		private readonly PayPointApi _paypoint;
		private readonly PayPointFacade _payPointFacade;
		private readonly IPaypointTransactionRepository _paypointTransactionRepository;
		private readonly ServiceClient m_oServiceClient;
		private readonly PayPointAccountRepository payPointAccountRepository;
	}
}
