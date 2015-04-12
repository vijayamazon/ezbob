namespace EzBob.Web.Areas.Customer.Controllers
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
	using Ezbob.Backend.Models;
	using Infrastructure.Attributes;
	using Code;
	using EzBob.Models.Agreements;
	using EzBob.Web.Areas.Customer.Models;
	using Infrastructure;
	using PaymentServices.Calculators;
	using PaymentServices.PacNet;
	using ServiceClientProxy;
	using StructureMap;
	using log4net;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Loans;

	public class GetCashController : Controller {
		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ICustomerNameValidator _validator;
		private static readonly ILog _log = LogManager.GetLogger(typeof(GetCashController));
		private readonly IPacnetPaypointServiceLogRepository _logRepository;
		private readonly ICustomerRepository _customerRepository;
		private readonly ILoanCreator _loanCreator;
		private readonly PayPointAccountRepository payPointAccountRepository;

		public GetCashController(
			IEzbobWorkplaceContext context,
			ICustomerNameValidator validator,
			IPacnetPaypointServiceLogRepository logRepository,
			ICustomerRepository customerRepository,
			ILoanCreator loanCreator,
			PayPointAccountRepository payPointAccountRepository
		) {
			_context = context;
			m_oServiceClient = new ServiceClient();
			_validator = validator;
			_logRepository = logRepository;
			_customerRepository = customerRepository;
			_loanCreator = loanCreator;
			this.payPointAccountRepository = payPointAccountRepository;
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



            PayPointFacade payPointFacade = new PayPointFacade(customer.MinOpenLoanDate());
			if (customer.IsLoanTypeSelectionAllowed == 1)
			{
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				cr.RepaymentPeriod = repaymentPeriod;
				cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if
			
			DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointFacade.PayPointAccount.CardExpiryMonths);

			var fee = new SetupFeeCalculator(cr.ManualSetupFeePercent,cr.BrokerSetupFeePercent).Calculate(loan_amount);

			string callback = Url.Action("PayPointCallback", "GetCash",
										 new
											 {
												 Area = "Customer",
												 loan_amount,
												 fee,
												 username = _context.User.Name,
												 cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate),
												 origin = customer.CustomerOrigin.Name
											 },
										 "https");

			string url = payPointFacade.GeneratePaymentUrl(customer, 5.00m, callback);
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
		[NoCache]
		public RedirectToRouteResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, decimal loan_amount, string card_no, string customer, string expiry)
		{
			//Session.Lock(_context.Customer, LockMode.Upgrade);

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
			Customer cus = _context.Customer;
			try
			{
				if (!valid || code != "A")
				{
					if (code == "N")
					{
						_log.WarnFormat("Invalid transaction. Id = {0}, Code: {1}, Message: {2}", trans_id, code, message);
					}
					else
					{
						_log.ErrorFormat("Invalid transaction. Id = {0}, Code: {1}, Message: {2}", trans_id, code, message);
					}

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
				
                PayPointFacade payPointFacade = new PayPointFacade(cus.MinOpenLoanDate());
				if (!payPointFacade.CheckHash(hash, Request.Url))
				{
					_log.ErrorFormat("Paypoint callback is not authenticated for user {0}", _context.Customer.Id);
					_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
									   String.Format("Paypoint callback is not authenticated for user {0}",
													 _context.Customer.Id));
					//return View("Error");
					throw new Exception("check hash failed");
				}

				

				ValidateCustomerName(customer, cus);

				_logRepository.Log(_context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Successful", "");

				var card = cus.TryAddPayPointCard(trans_id, card_no, expiry, customer, payPointFacade.PayPointAccount);

				var loan = _loanCreator.CreateLoan(cus, loan_amount, card, now);

				RebatePayment(amount, loan, trans_id, now);

				cus.PayPointErrorsCount = 0;
				
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
		public JsonResult Now(int cardId, decimal amount)
		{
			var cus = _context.Customer;
			var card = cus.PayPointCards.First(c => c.Id == cardId);
			DateTime now = DateTime.UtcNow;
			var loan = _loanCreator.CreateLoan(cus, amount, card, now);

			var url = Url.Action("Index", "PacnetStatus", new { Area = "Customer" }, "https");

			return Json(new { url = url });
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
		public JsonResult LoanLegalSigned(
			bool preAgreementTermsRead = false,
			bool agreementTermsRead = false,
			bool euAgreementTermsRead = false,
			bool cosmeAgreementTermsRead = false,
			string signedName = "",
			bool notInBankruptcy = false
		) {
			_log.DebugFormat(
				"LoanLegalModel " +
				"agreementTermsRead: {0}" +
				"preAgreementTermsRead: {1}" +
				"euAgreementTermsRead: {2}" +
				"cosmeAgreementTermsRead: {3}", 
				agreementTermsRead,
				preAgreementTermsRead,
				euAgreementTermsRead,
				cosmeAgreementTermsRead
			);

			var cashRequest = _context.Customer.LastCashRequest;
			var typeOfBusiness = _context.Customer.PersonalInfo.TypeOfBusiness.AgreementReduce();

			bool hasError =
				!preAgreementTermsRead ||
				!agreementTermsRead ||
				(cashRequest.LoanSource.Name == LoanSourceName.EU.ToString() && !euAgreementTermsRead) ||
				(cashRequest.LoanSource.Name == LoanSourceName.COSME.ToString() && !cosmeAgreementTermsRead) ||
				!notInBankruptcy;

			if (hasError)
				return Json(new { error = "You must agree to all agreements." });

			_context.Customer.LastCashRequest.LoanLegals.Add(new LoanLegal {
				CashRequest = cashRequest,
				Created = DateTime.UtcNow,
				EUAgreementAgreed = euAgreementTermsRead,
				COSMEAgreementAgreed = cosmeAgreementTermsRead,
				CreditActAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Personal,
				PreContractAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Personal,
				PrivateCompanyLoanAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Business,
				GuarantyAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Business,
				SignedName = signedName,
				NotInBankruptcy = notInBankruptcy,
				AlibabaCreditFacilityTemplate = null,
			});

			return Json(new { });
		} // LoanLegalSigned

		[Transactional]
		[HttpPost]
		public JsonResult CreditLineSigned(
			int customerID = 0,
			int cashRequestID = 0,
			string signedName = "",
			bool creditFacilityAccepted = false
		) {
			_log.DebugFormat(
				"CreditLineSignatureModel " +
				"customer ID: {0}" +
				"cash request ID: {1}" +
				"signed name: {2}" +
				"credit facility accepted: {3}", 
				customerID,
				cashRequestID,
				signedName,
				creditFacilityAccepted
			);

			var customer = _context.Customer;

			if ((customer == null) || (customer.Id != customerID)) {
				_log.ErrorFormat(
					"CreditLineSigned: invalid or unmatched customer ({0}) for requested id {1}.",
					customer.Stringify(),
					customerID
				);

				return Json(new { success = false, error = "Invalid customer name.", });
			} // if

			CashRequest cashRequest = customer.CashRequests.FirstOrDefault(r => r.Id == cashRequestID);

			if (cashRequest == null) {
				_log.WarnFormat(
					"CreditLineSigned: cash request not found by id {0} at customer {1}.",
					cashRequestID,
					customer.Stringify()
				);

				return Json(new { success = false, error = "Invalid credit line.", });
			} // if

			bool hasError = !creditFacilityAccepted;

			if (hasError) {
				_log.WarnFormat(
					"CreditLineSigned: credit facility not accepted for cash request {0} at customer {1}.",
					cashRequestID,
					customer.Stringify()
				);

				return Json(new { success = false, error = "You must agree to all agreements.", });
			} // if

			IAgreementsTemplatesProvider templateProvider = ObjectFactory.GetInstance<IAgreementsTemplatesProvider>();

			string templateText = templateProvider.GetTemplate(LoanAgreementTemplateType.AlibabaCreditFacility);

			var template = ObjectFactory.GetInstance<DatabaseDataHelper>()
				.LoadOrCreateLoanAgreementTemplate(templateText, LoanAgreementTemplateType.AlibabaCreditFacility);

			customer.LastCashRequest.LoanLegals.Add(new LoanLegal {
				CashRequest = cashRequest,
				Created = DateTime.UtcNow,
				EUAgreementAgreed = false,
				COSMEAgreementAgreed = false,
				CreditActAgreementAgreed = false,
				PreContractAgreementAgreed = false,
				PrivateCompanyLoanAgreementAgreed = false,
				GuarantyAgreementAgreed = false,
				SignedName = signedName,
				NotInBankruptcy = false,
				AlibabaCreditFacilityTemplate = template,
			});

			return Json(new { success = true, error = string.Empty, });
		} // CreditLineSigned
	}
}
