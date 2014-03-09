﻿namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using CommonLib;
	using Models;
	using Code;
	using Infrastructure;
	using PaymentServices.Calculators;
	using PaymentServices.PacNet;
	using Scorto.Web;
	using StructureMap;
	using log4net;

	public class GetCashController : Controller {
		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext _context;
		private readonly IPayPointFacade _payPointFacade;
		private readonly ICustomerNameValidator _validator;
		private static readonly ILog _log = LogManager.GetLogger("EzBob.Web.Areas.Customer.Controllers.GetCashController");
		private readonly IPacnetPaypointServiceLogRepository _logRepository;
		private readonly ICustomerRepository _customerRepository;
		private readonly ILoanCreator _loanCreator;

		public GetCashController(
			IEzbobWorkplaceContext context,
			IPayPointFacade payPointFacade,
			ICustomerNameValidator validator,
			IPacnetPaypointServiceLogRepository logRepository,
			ICustomerRepository customerRepository,
			ILoanCreator loanCreator
		) {
			_context = context;
			_payPointFacade = payPointFacade;
			m_oServiceClient = new ServiceClient();
			_validator = validator;
			_logRepository = logRepository;
			_customerRepository = customerRepository;
			_loanCreator = loanCreator;
		}

		[NoCache]
		public RedirectResult GetTransactionId(decimal loan_amount, int loanType, int repaymentPeriod)
		{
			Customer customer = _context.Customer;

			CheckCustomerStatus(customer);

			if (loan_amount < 0)
			{
				loan_amount = (int)Math.Floor(customer.CreditSum.Value);
			}
			var cr = customer.LastCashRequest;

			if (customer.IsLoanTypeSelectionAllowed == 1)
			{
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				cr.RepaymentPeriod = repaymentPeriod;
				cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

			DateTime lastDateOfPayment = DateTime.UtcNow.AddMonths(cr.RepaymentPeriod);

			var fee = (new SetupFeeCalculator(cr.UseSetupFee, cr.UseBrokerSetupFee, cr.ManualSetupFeeAmount, cr.ManualSetupFeePercent)).Calculate(loan_amount);

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
			string url = _payPointFacade.GeneratePaymentUrl(customer.IsOffline.Value, 5.00m, callback);
			_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Redirect to " + url, "Successful", "");
			return Redirect(url);
		}

		private void CheckCustomerStatus(Customer customer)
		{
			if (
				!customer.CreditSum.HasValue ||
				!customer.Status.HasValue ||
				customer.Status.Value != Status.Approved ||
				!customer.CollectionStatus.CurrentStatus.IsEnabled || 
				!customer.LastCashRequest.LoanLegals.Any())
			{
				throw new Exception("Invalid customer state");
			}
		}
		/// <summary>
		/// Callback from paypoint after trying to charge customer with 5 pounds on adding dabit card
		/// </summary>
		/// <param name="valid">is card valid</param>
		/// <param name="trans_id">transaction id to charge customer via paypoint instead of using his card details</param>
		/// <param name="code">A - success other is some error</param>
		/// <param name="auth_code"></param>
		/// <param name="amount">amount charged</param>
		/// <param name="ip">customer's ip</param>
		/// <param name="test_status">if fake test card was entered than true</param>
		/// <param name="hash">hash of the request</param>
		/// <param name="message">error description</param>
		/// <param name="loan_amount">loan amount</param>
		/// <param name="card_no">4 last digits of credit card</param>
		/// <param name="customer">customer name</param>
		/// <param name="expiry">card exipiry month/year</param>
		/// <returns>redirects customer to confirmation page/error page</returns>
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

					try {
						m_oServiceClient.Instance.GetCashFailed(_context.User.Id);
					}
					catch (Exception e) {
						_log.Error("Failed to send 'get cash failed' email.", e);
					} // try

					TempData["code"] = code;
					TempData["message"] = message;

					return RedirectToAction("Error", "Paypoint", new { Area = "Customer" });
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

				Customer cus = _context.Customer;

				ValidateCustomerName(customer, cus);

				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Successful", "");


				var card = cus.TryAddPayPointCard(trans_id, card_no, expiry, customer);

				var loan = _loanCreator.CreateLoan(cus, loan_amount, card, now);

				RebatePayment(amount, loan, trans_id, now);

				cus.PayPointErrorsCount = 0;
				cus.PayPointTransactionId = trans_id;
				cus.CreditCardNo = card_no;

				TempData["amount"] = loan_amount;
				TempData["bankNumber"] = cus.BankAccount.AccountNumber;
				TempData["card_no"] = card_no;

				_customerRepository.Update(cus);

				return RedirectToAction("Index", "PacnetStatus", new { Area = "Customer" });
			}
			catch (OfferExpiredException)
			{
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
								   "Invalid apply for a loan period");
				return RedirectToAction("ErrorOfferDate", "Paypoint", new { Area = "Customer" });
			}
			catch (PacnetException)
			{
				try {
					m_oServiceClient.Instance.TransferCashFailed(_context.User.Id);
				}
				catch (Exception e) {
					_log.Error("Failed to send 'transfer cash failed' email.", e);
				} // try
				return RedirectToAction("Error", "Pacnet", new { Area = "Customer" });
			}
			catch (TargetInvocationException)
			{
				return RedirectToAction("ErrorOfferDate", "Paypoint", new { Area = "Customer" });
			}
		}

		private void RebatePayment(decimal? amount, Loan loan, string transId, DateTime now)
		{
			if (amount == null || amount <= 0) return;
			var f = new LoanPaymentFacade();
			f.PayLoan(loan, transId, amount.Value, Request.UserHostAddress, now, "system-repay");
		}

		[Transactional]
		[HttpPost]
		public JsonNetResult Now(int cardId, decimal amount)
		{
			var cus = _context.Customer;
			var card = cus.PayPointCards.First(c => c.Id == cardId);
			DateTime now = DateTime.UtcNow;
			var loan = _loanCreator.CreateLoan(cus, amount, card, now);

			var url = Url.Action("Index", "PacnetStatus", new { Area = "Customer" }, "https");

			return this.JsonNet(new { url = url });
		}

		private void ValidateCustomerName(string customer, Customer cus)
		{
			if (!_validator.CheckCustomerName(customer, cus.PersonalInfo.FirstName, cus.PersonalInfo.Surname))
			{
				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Warning",
								   String.Format("Name {0} did not passed validation check for {1} {2}", customer,
												 cus.PersonalInfo.Surname, cus.PersonalInfo.Surname));
				_log.WarnFormat("Name {0} did not passed validation check for {1} {2}", customer,
								cus.PersonalInfo.Surname,
								cus.PersonalInfo.Surname);
				try {
					m_oServiceClient.Instance.PayPointNameValidationFailed(_context.User.Id, cus.Id, customer);
				}
				catch (Exception e) {
					_log.Error("Failed to send 'paypoint name validation failed' email.", e);
				} // try
			}
		}

		[Transactional]
		[HttpPost]
		public JsonNetResult LoanLegalSigned(bool preAgreementTermsRead = false, bool agreementTermsRead = false, bool euAgreementTermsRead = false)
		{
			_log.DebugFormat("LoanLegalModel agreementTermsRead: {0} preAgreementTermsRead: {1} euAgreementTermsRead: {2}", agreementTermsRead, preAgreementTermsRead, euAgreementTermsRead);

			var cashRequest = _context.Customer.LastCashRequest;
			var typeOfBusiness = _context.Customer.PersonalInfo.TypeOfBusiness.Reduce();

			if (!preAgreementTermsRead || !agreementTermsRead ||
				(cashRequest.LoanSource.Name == "EU" && !euAgreementTermsRead))
			{
				return this.JsonNet(new { error = "You must agree to all agreements." });
			}

			_context.Customer.LastCashRequest.LoanLegals.Add(new LoanLegal
				{
					CashRequest = cashRequest,
					Created = DateTime.UtcNow,
					EUAgreementAgreed = cashRequest.LoanSource.Name == "EU",
					CreditActAgreementAgreed = typeOfBusiness == TypeOfBusinessReduced.Personal || typeOfBusiness == TypeOfBusinessReduced.NonLimited,
					PreContractAgreementAgreed = typeOfBusiness == TypeOfBusinessReduced.Personal || typeOfBusiness == TypeOfBusinessReduced.NonLimited,
					PrivateCompanyLoanAgreementAgreed = typeOfBusiness == TypeOfBusinessReduced.Limited,
					GuarantyAgreementAgreed = typeOfBusiness == TypeOfBusinessReduced.Limited,
				});

			return this.JsonNet(new { });
		}
	}
}