namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using EzBob.Web.Areas.Customer.Models;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Repository;
	using PaymentServices.Calculators;
	using PaymentServices.PayPoint;
	using ServiceClientProxy;

	public class PaypointController : Controller {
		public PaypointController(
			IEzbobWorkplaceContext context,
			IPacnetPaypointServiceLogRepository pacnetPaypointServiceLogRepository,
			IPaypointTransactionRepository paypointTransactionRepository,
			PayPointApi paypoint,
			ILoanOptionsRepository loanOptionsRepository
		) {
			this.context = context;
			this.logRepository = pacnetPaypointServiceLogRepository;
			this.paypointTransactionRepository = paypointTransactionRepository;
			this.paypointApi = paypoint;
			this.loanOptionsRepository = loanOptionsRepository;

			this.serviceClient = new ServiceClient();
		} // constructor

		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[HttpGet]
		[NoCache]
		[Transactional]
		public ActionResult Callback(
			bool valid,
			string trans_id,
			string code,
			string auth_code,
			decimal? amount,
			string ip,
			string test_status,
			string hash,
			string message,
			string type,
			int loanId,
			string card_no,
			string customer,
			string expiry
		) {
			if (test_status == "true") {
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);

				if (random4Digits.Length > 4)
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);

				card_no = random4Digits;

				expiry = string.Format(
					"{0}{1}",
					"01",
					DateTime.UtcNow.AddYears(2).Year.ToString().Substring(2, 2)
				);
			} // if

			var customerContext = this.context.Customer;

			PayPointFacade payPointFacade = new PayPointFacade(
				customerContext.MinOpenLoanDate(),
				customerContext.CustomerOrigin.Name
			);

			if (!payPointFacade.CheckHash(hash, Request.Url)) {
				log.Alert("Paypoint callback is not authenticated for user {0}", customerContext.Id);

				this.logRepository.Log(
					this.context.UserId,
					DateTime.UtcNow,
					"Paypoint Pay Redirect to ",
					"Failed",
					String.Format("Paypoint callback is not authenticated for user {0}", customerContext.Id)
				);

				return View("Error");
			} // if

			var statusDescription = PayPointStatusTranslator.TranslateStatusCode(code);

			if (!valid || code != "A") {
				if (code == "N") {
					log.Warn(
						"Paypoint result code is : {0} ({1}). Message: {2}",
						code,
						string.Join(", ", statusDescription.ToArray()),
						message
					);
				} else {
					log.Alert(
						"Paypoint result code is : {0} ({1}). Message: {2}",
						code,
						string.Join(", ", statusDescription.ToArray()),
						message
					);
				} // if

				this.logRepository.Log(
					this.context.UserId,
					DateTime.UtcNow,
					"Paypoint Pay Redirect to ",
					"Failed",
					string.Format(
						"Paypoint result code is : {0} ({1}). Message: {2}",
						code,
						string.Join(", ", statusDescription.ToArray()),
						message
					)
				);

				return View("Error");
			} // if

			if (!amount.HasValue) {
				log.Alert("Paypoint amount is null. Message: {0}", message);

				this.logRepository.Log(
					this.context.UserId,
					DateTime.UtcNow,
					"Paypoint Pay Redirect to ",
					"Failed",
					String.Format("Paypoint amount is null. Message: {0}", message)
				);

				return View("Error");
			} // if

			// If there is transaction with such id in database,
			// it means that customer refreshes page
			// show in this case cashed result
			if (this.paypointTransactionRepository.ByGuid(trans_id).Any()) {
				var data = TempData.Get<PaymentConfirmationModel>();

				if (data == null)
					return RedirectToAction("Index", "Profile", new { Area = "Customer" });

				return View(TempData.Get<PaymentConfirmationModel>());
			} // if
			
			NL_Payments nlPayment = new NL_Payments() {
				CreatedByUserID = this.context.UserId,
				Amount = amount.Value,
				PaymentMethodID = (int)NLLoanTransactionMethods.CustomerAuto,
				PaymentSystemType = NLPaymentSystemTypes.Paypoint
			};

			log.Debug("Callback: Sending nlPayment: {0} for customer {1}, oldloanId {2}", nlPayment, this.context.UserId, loanId);

			LoanPaymentFacade loanRepaymentFacade = new LoanPaymentFacade();
			PaymentResult res = loanRepaymentFacade.MakePayment(trans_id, amount.Value, ip, type, loanId, customerContext, null, "payment from customer", null, null, nlPayment);

			SendEmails(loanId, amount.Value, customerContext);

			this.logRepository.Log(this.context.UserId, DateTime.UtcNow, "Paypoint Pay Callback", "Successful", "");

			var refNumber = "";

			bool isEarly = false;

			if (loanId > 0) {
				var loan = customerContext.GetLoan(loanId);

				if (loan != null) {
					refNumber = loan.RefNumber;

					if (loan.Schedule != null) {
						List<LoanScheduleItem> scheduledPayments = loan.Schedule
							.Where(
								x => x.Status == LoanScheduleStatus.StillToPay ||
								x.Status == LoanScheduleStatus.Late ||
								x.Status == LoanScheduleStatus.AlmostPaid
							).ToList();

						if (scheduledPayments.Any()) {
							DateTime earliestSchedule = scheduledPayments.Min(x => x.Date);

							bool scheduleIsEarly = earliestSchedule.Date >= DateTime.UtcNow && (
								earliestSchedule.Date.Year != DateTime.UtcNow.Year ||
								earliestSchedule.Date.Month != DateTime.UtcNow.Month ||
								earliestSchedule.Date.Day != DateTime.UtcNow.Day
							);

							if (scheduleIsEarly)
								isEarly = true;
						} // if
					} // if has schedule
				} // if loan
			} // if loan id

			if (string.IsNullOrEmpty(customer))
				customer = customerContext.PersonalInfo.Fullname;

			customerContext.TryAddPayPointCard(trans_id, card_no, expiry, customer, payPointFacade.PayPointAccount);

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
		} // Callback

		[NoCache]
		public ActionResult Error() {
			var code = (string)TempData["code"];
			var message = (string)TempData["message"];

			if (string.IsNullOrEmpty(code))
				return RedirectToAction("Index", "Profile", new { Area = "Customer" });

			var statusDescription = PayPointStatusTranslator.TranslateStatusCode(code);

			var msg = string.Format("Paypoint result code is : {0} ({1}). Message: {2}", code,
				string.Join(", ", statusDescription.ToArray()), message);

			log.Say(code == "N" ? Severity.Warn : Severity.Alert, msg);

			ViewData["Message"] = msg;

			return View("Error");
		} // Error

		[NoCache]
		public ActionResult ErrorOfferDate() {
			ViewData["Message"] = "Unfortunately, time of the offer expired! Please apply for a new offer.";
			return View("ErrorOfferDate");
		} // ErrorOfferDate

		[NoCache]
		public ActionResult Pay(decimal amount, string type, int loanId, int rolloverId) {
			try {
				log.Msg("Payment request for customer id {0}, amount {1}", this.context.Customer.Id, amount);

				amount = CalculateRealAmount(type, loanId, amount);
				if (amount < 0)
					return View("Error");

				var oCustomer = this.context.Customer;

				PayPointFacade payPointFacade = new PayPointFacade(
					oCustomer.MinOpenLoanDate(),
					oCustomer.CustomerOrigin.Name
				);

				int payPointCardExpiryMonths = payPointFacade.PayPointAccount.CardExpiryMonths;
				DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointCardExpiryMonths);

				var callback = Url.Action("Callback", "Paypoint", new {
					Area = "Customer",
					loanId,
					type,
					username = (this.context.User != null ? this.context.User.Name : ""),
					cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate),
					hideSteps = true,
					payEarly = true,
					origin = oCustomer.CustomerOrigin.Name
				}, "https");

				var url = payPointFacade.GeneratePaymentUrl(oCustomer, amount, callback);

				this.logRepository.Log(
					this.context.UserId,
					DateTime.UtcNow,
					"Paypoint Pay Redirect to " + url,
					"Successful",
					""
				);

				return Redirect(url);
			} catch (Exception e) {
				log.Alert(
					e,
					"Error while executing Pay(amount = {0}, type = '{1}', loan id = {2}, rollover id = {3}).",
					amount,
					type,
					loanId,
					rolloverId
				);
				return View("Error");
			} // try
		} // Pay


		[Transactional]
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PayFast(string amount, string type, string paymentType, int loanId, int cardId) {
			try {
				decimal realAmount = decimal.Parse(amount, CultureInfo.InvariantCulture);

				var customer = this.context.Customer;

				log.Msg("Payment request for customer id {0}, amount {1}", customer.Id, realAmount);

				// TotalEarlyPayment
				realAmount = CalculateRealAmount(type, loanId, realAmount);

				if (realAmount < 0)
					return Json(new { error = "amount is too small" });

				PayPointCard card = cardId == -1
					? customer.PayPointCards.FirstOrDefault(c => c.IsDefaultCard)
					: customer.PayPointCards.FirstOrDefault(c => c.Id == cardId);

				if (card == null)
					throw new Exception("Card not found");

				this.paypointApi.RepeatTransactionEx(card.PayPointAccount, card.TransactionId, realAmount);

				NL_Payments nlPayment = new NL_Payments() {
					CreatedByUserID = this.context.UserId,
					Amount = realAmount,
					PaymentMethodID = (int)NLLoanTransactionMethods.CustomerAuto,
					PaymentSystemType = NLPaymentSystemTypes.Paypoint
				};

				log.Debug("PayFast: Sending nlPayment: {0} for customer {1}", nlPayment, customer.Id);

				LoanPaymentFacade loanRepaymentFacade = new LoanPaymentFacade();

				PaymentResult payFastModel = loanRepaymentFacade.MakePayment(
					card.TransactionId,
					realAmount,
					null,
					type,
					loanId,
					customer,
					DateTime.UtcNow,
					"manual payment from customer",
					paymentType,
					"CustomerAuto",
				   nlPayment
				);

				payFastModel.CardNo = card.CardNo;

				SendEmails(loanId, realAmount, customer);

				this.logRepository.Log(
					this.context.UserId,
					DateTime.UtcNow,
					"Paypoint Pay Early Fast Callback",
					"Successful",
					""
				);

				return Json(payFastModel);
			} catch (PayPointException e) {
				this.logRepository.Log(
					this.context.UserId,
					DateTime.UtcNow,
					"Paypoint Pay Early Fast Callback",
					"Failed",
					e.ToString()
				);

				return Json(new { error = "Error occurred while making payment" });
			} catch (Exception e) {
				this.logRepository.Log(
					this.context.UserId,
					DateTime.UtcNow,
					"Paypoint Pay Early Fast Callback",
					"Failed",
					e.ToString()
				);

				return Json(new { error = e.Message });
			} // try
		} // PayFast

		private decimal CalculateRealAmount(string type, int loanId, decimal realAmount) {
			if (type == "total")
				realAmount = this.context.Customer.TotalEarlyPayment();

			if (type == "loan") {
				Loan loan = this.context.Customer.Loans.Single(l => l.Id == loanId);
				realAmount = Math.Min(realAmount, loan.TotalEarlyPayment());
				realAmount = Math.Max(realAmount, 0);
			} // if

			return realAmount;
		} // CalculateRealAmount

		private void SendEmails(int loanId, decimal realAmount, Customer customer) {
			Loan loan = customer.GetLoan(loanId);

			LoanOptions loanOptions = this.loanOptionsRepository.GetByLoanId(loanId);

			this.serviceClient.Instance.PayEarly(this.context.User.Id, realAmount, loan.RefNumber);

			this.serviceClient.Instance.LoanStatusAfterPayment(
				this.context.UserId,
				customer.Id,
				customer.Name,
				loanId,
				realAmount,
				loanOptions == null || loanOptions.EmailSendingAllowed,
				loan.Balance,
				loan.Status == LoanStatus.PaidOff
			);
		} // SendEmails

		private readonly IEzbobWorkplaceContext context;
		private readonly IPacnetPaypointServiceLogRepository logRepository;
		private readonly PayPointApi paypointApi;
		private readonly IPaypointTransactionRepository paypointTransactionRepository;
		private readonly ServiceClient serviceClient;
		private readonly ILoanOptionsRepository loanOptionsRepository;

		private static readonly ASafeLog log = new SafeILog(typeof(PaypointController));
	} // class PaypointController
} // namespace
