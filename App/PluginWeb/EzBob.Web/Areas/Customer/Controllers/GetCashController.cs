namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.LegalDocs;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils.Extensions;
	using EzBob.CommonLib;
	using EzBob.Web.Areas.Customer.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using log4net;
	using PaymentServices.Calculators;
	using PaymentServices.PacNet;
	using ServiceClientProxy;
	using StructureMap;

	public class GetCashController : Controller {
		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;
		private readonly ICustomerNameValidator validator;
		private static readonly ILog log = LogManager.GetLogger(typeof(GetCashController));
		private readonly IPacnetPaypointServiceLogRepository logRepository;
		private readonly ICustomerRepository customerRepository;
		private readonly ILoanCreator loanCreator;

		public GetCashController(
			IEzbobWorkplaceContext context,
			ICustomerNameValidator validator,
			IPacnetPaypointServiceLogRepository logRepository,
			ICustomerRepository customerRepository,
			ILoanCreator loanCreator) {
			this.context = context;
		    this.serviceClient = new ServiceClient();
			this.validator = validator;
		    this.logRepository = logRepository;
		    this.customerRepository = customerRepository;
		    this.loanCreator = loanCreator;
		}

		[NoCache]
		public RedirectResult GetTransactionId(decimal loan_amount, int loanType, int repaymentPeriod) {
			Customer customer = this.context.Customer;

			CheckCustomerStatus(customer);

			if (loan_amount < 0)
				loan_amount = (int)Math.Floor(customer.CreditSum ?? 0);

			var cr = customer.LastCashRequest;

			PayPointFacade payPointFacade = new PayPointFacade(customer.MinOpenLoanDate(), customer.CustomerOrigin.Name);
			if (customer.IsLoanTypeSelectionAllowed == 1) {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				cr.RepaymentPeriod = repaymentPeriod;
				cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

			DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointFacade.PayPointAccount.CardExpiryMonths);

			var fee = new SetupFeeCalculator(cr.ManualSetupFeePercent, cr.BrokerSetupFeePercent).Calculate(loan_amount).Total;

			string callback = Url.Action("PayPointCallback", "GetCash",
										 new {
											 Area = "Customer",
											 loan_amount,
											 fee,
											 username = this.context.User.Name,
											 cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate),
											 origin = customer.CustomerOrigin.Name
										 },
										 "https");

			string url = payPointFacade.GeneratePaymentUrl(customer, 5.00m, callback);
		    this.logRepository.Log(this.context.UserId, DateTime.Now, "Paypoint GetCash Redirect to " + url, "Successful", "");
			return Redirect(url);
		}

		private void CheckCustomerStatus(Customer customer) {
			bool invalidState =
				!customer.CreditSum.HasValue ||
				!customer.Status.HasValue ||
				(customer.Status.Value != Status.Approved) ||
				!customer.CollectionStatus.IsEnabled ||
				!customer.LastCashRequest.LoanLegals.Any();

			if (invalidState)
				throw new Exception("Invalid customer state");
		} // CheckCustomerStatus

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
		public RedirectToRouteResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, decimal loan_amount, string card_no, string customer, string expiry) {
			//Session.Lock(_context.Customer, LockMode.Upgrade);

			if (test_status == "true") {
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
				if (random4Digits.Length > 4) {
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);
				}
				card_no = random4Digits;
				expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
			}

			DateTime now = DateTime.UtcNow;
			Customer cus = this.context.Customer;
			try {
				if (!valid || code != "A") {
					if (code == "N") {
						log.WarnFormat("Invalid transaction. Id = {0}, Code: {1}, Message: {2}", trans_id, code, message);
					} else {
						log.ErrorFormat("Invalid transaction. Id = {0}, Code: {1}, Message: {2}", trans_id, code, message);
					}

					// continue to log paypoint and pacnet transactions also for NL, i.e. do nothig new
				    this.logRepository.Log(this.context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
									   String.Format("Invalid transaction. Id = {0}, Code: {1}, Message: {2}", trans_id,
													 code, message));

					this.context.Customer.PayPointErrorsCount++;

					try {
						// sending mail "Mandrill - Debit card authorization problem"
					    this.serviceClient.Instance.GetCashFailed(this.context.User.Id);
					} catch (Exception e) {
						log.Error("Failed to send 'get cash failed' email.", e);
					} // try

					TempData["code"] = code;
					TempData["message"] = message;

					return RedirectToAction("Error", "Paypoint", new { Area = "Customer" });
				}

				PayPointFacade payPointFacade = new PayPointFacade(cus.MinOpenLoanDate(), cus.CustomerOrigin.Name);
				if (!payPointFacade.CheckHash(hash, Request.Url)) {
					log.ErrorFormat("Paypoint callback is not authenticated for user {0}", this.context.Customer.Id);
					// continue to log paypoint transaction also for NL
				    this.logRepository.Log(this.context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
									   String.Format("Paypoint callback is not authenticated for user {0}",
													 this.context.Customer.Id));
					//return View("Error");
					throw new Exception("check hash failed");
				}

				ValidateCustomerName(customer, cus);

				// "x rebate" pounds charged successfully, continue to save PayPointCard, create new loan, and make "rebate" payment 

				// continue to log paypoint transaction also for NL
			    this.logRepository.Log(this.context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Successful", "");

				// save new PayPointCard 
				var card = cus.TryAddPayPointCard(trans_id, card_no, expiry, customer, payPointFacade.PayPointAccount);

				Loan loan = this.loanCreator.CreateLoan(cus, loan_amount, card, now);

				RebatePayment(amount, loan, trans_id, now);

				cus.PayPointErrorsCount = 0;

				TempData["amount"] = loan_amount;
				TempData["bankNumber"] = cus.BankAccount.AccountNumber;
				TempData["card_no"] = card_no;

			    this.customerRepository.Update(cus);

				// el: TODO save NL_Payments -> NL_PaypointTransactions for this PayPointCard; "AssignPaymentToLoan" strategy; 
				/* 
				* 1. save NL_Payments with PaymentStatusID (NL_PaymentStatuses ("rebate"? / "system-repay" ?)), PaymentMethodID ([LoanTransactionMethod] 'Auto' ID 2)
				* 
				* 2. save NL_PaypointTransactions with:
				* PaypointCardID - from just created PayPointCard.Id, 
				* PaypointTransactionStatusID =1 (Done) NL_PaypointTransactionStatuses  
				* IP - from LoanTransaction IP
				* Amount - from the method argument amount if not null, otherwise 5 pounds
				* PaymentID - from 1.				 
				* 
				* 3. new strategy AssignPaymentToLoan: argument: NL_Model with Loan.LoanID; decimal amount; output: NL_Model containing list of fees/schedules that covered by the amount/payment
				* closest unpaid loan fees and schedules (1.fee if exists; 2.interest; 3.principal)
				* 
				* 4. save into NL_LoanSchedulePayments : PaymentID just created + NL_LoanSchedules from AssignPaymentToLoan strategy
				*/

				return RedirectToAction("Index", "PacnetStatus", new { Area = "Customer" });

			} catch (OfferExpiredException) {
			    this.logRepository.Log(this.context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Falied",
								   "Invalid apply for a loan period");
				return RedirectToAction("ErrorOfferDate", "Paypoint", new { Area = "Customer" });
			} catch (PacnetException) {
				try {
				    this.serviceClient.Instance.TransferCashFailed(this.context.User.Id);
				} catch (Exception e) {
					log.Error("Failed to send 'transfer cash failed' email.", e);
				} // try
				return RedirectToAction("Error", "Pacnet", new { Area = "Customer" });
			} catch (TargetInvocationException) {
				return RedirectToAction("ErrorOfferDate", "Paypoint", new { Area = "Customer" });
			}
		}

		private void RebatePayment(decimal? amount, Loan loan, string transId, DateTime now) {
			if (amount == null || amount <= 0)
				return;
			var f = new LoanPaymentFacade();
			f.PayLoan(loan, transId, amount.Value, Request.UserHostAddress, now, "system-repay");
		}

		[Transactional]
		[HttpPost]
		public JsonResult Now(int cardId, decimal amount) {
			var cus = this.context.Customer;
			var card = cus.PayPointCards.First(c => c.Id == cardId);
			DateTime now = DateTime.UtcNow;

			NL_Model nlModel = new NL_Model(cus.Id);
			var loan = this.loanCreator.CreateLoan(cus, amount, card, now, nlModel);

			var url = Url.Action("Index", "PacnetStatus", new { Area = "Customer" }, "https");

			return Json(new { url = url });
		}

		private void ValidateCustomerName(string customer, Customer cus) {
			if (!this.validator.CheckCustomerName(customer, cus.PersonalInfo.FirstName, cus.PersonalInfo.Surname)) {
			    this.logRepository.Log(this.context.UserId, DateTime.Now, "Paypoint GetCash Callback", "Warning",
								   String.Format("Name {0} did not passed validation check for {1} {2}", customer,
												 cus.PersonalInfo.Surname, cus.PersonalInfo.Surname));
				log.WarnFormat("Name {0} did not passed validation check for {1} {2}", customer,
								cus.PersonalInfo.Surname,
								cus.PersonalInfo.Surname);
				try {
				    this.serviceClient.Instance.PayPointNameValidationFailed(this.context.User.Id, cus.Id, customer);
				} catch (Exception e) {
					log.Error("Failed to send 'paypoint name validation failed' email.", e);
				} // try
			}
		}

		[Transactional]
		[HttpPost]
		public JsonResult LoanLegalSigned(FormCollection collection) {
			decimal loanAmount = Convert.ToDecimal(collection["loanAmount"]);
			int repaymentPeriod = Convert.ToInt32(collection["repaymentPeriod"]);
			string signedName = collection["signedName"];
			bool notInBankruptcy = Convert.ToBoolean(collection["notInBankruptcy"]);
			bool euAgreementTermsRead = Convert.ToBoolean(collection["euAgreementTermsRead"]);
			bool cosmeAgreementTermsRead = Convert.ToBoolean(collection["cosmeAgreementTermsRead"]);
			decimal manualSetupFeePercent = Convert.ToDecimal(collection["manualSetupFeePercent"]);
			decimal brokerFeePercent = Convert.ToDecimal(collection["brokerFeePercent"]);

			var dynamicLoanAgreements = new Dictionary<LegalDocsEnums.LoanAgreementTemplateType, bool>();

			foreach (var loanAgreementTemplateName in Enum.GetNames(typeof(LegalDocsEnums.LoanAgreementTemplateType))) {
				var key = loanAgreementTemplateName + "TermsRead";

				if (collection.AllKeys.Contains(key)) {
					var value = Convert.ToBoolean(collection[key]);

					dynamicLoanAgreements.Add(
						(LegalDocsEnums.LoanAgreementTemplateType)Enum.Parse(
							typeof(LegalDocsEnums.LoanAgreementTemplateType),
							loanAgreementTemplateName
						),
						value
					);
				} // if
			} // for each

			var dynamicLoanAgreementsStringified = new JavaScriptSerializer().Serialize(
				dynamicLoanAgreements.ToDictionary(x => x.Key.DescriptionAttr(), x => x.Value.ToString())
			);

			log.DebugFormat(
				"LoanLegalModel - " +
				"dynamicLoanAgreementsStringified : {0}" +
				"euAgreementTermsRead: {1}" +
				"cosmeAgreementTermsRead: {2}",
				dynamicLoanAgreementsStringified,
				euAgreementTermsRead,
				cosmeAgreementTermsRead
			);

			var cashRequest = this.context.Customer.LastCashRequest;
			var typeOfBusiness = this.context.Customer.PersonalInfo.TypeOfBusiness.AgreementReduce();

			// Dynamic agreements validation
			foreach (var dynamicLoanAgreement in dynamicLoanAgreements) {
				if (dynamicLoanAgreement.Value != true) {
					return Json(new {
						error = string.Format("You must agree on {0} agreement", dynamicLoanAgreement.Key.DescriptionAttr())
					});
				} // if
			} // for each

			var personalInfo = cashRequest.Customer.PersonalInfo;

			// Customer name validation
			const string separator = " ";

			List<String> nameCombinations = new List<string> {
				string.Format("{0}{3}{1}{3}{2}", personalInfo.FirstName, personalInfo.MiddleInitial, personalInfo.Surname, separator).Replace(separator + separator, separator),
				string.Format("{0}{3}{2}{3}{1}", personalInfo.FirstName, personalInfo.MiddleInitial, personalInfo.Surname, separator).Replace(separator + separator, separator),
				string.Format("{1}{3}{0}{3}{2}", personalInfo.FirstName, personalInfo.MiddleInitial, personalInfo.Surname, separator).Replace(separator + separator, separator),
				string.Format("{1}{3}{2}{3}{0}", personalInfo.FirstName, personalInfo.MiddleInitial, personalInfo.Surname, separator).Replace(separator + separator, separator),
				string.Format("{2}{3}{0}{3}{1}", personalInfo.FirstName, personalInfo.MiddleInitial, personalInfo.Surname, separator).Replace(separator + separator, separator),
				string.Format("{2}{3}{1}{3}{0}", personalInfo.FirstName, personalInfo.MiddleInitial, personalInfo.Surname, separator).Replace(separator + separator, separator),
				string.Format("{0}{2}{1}", personalInfo.FirstName, personalInfo.MiddleInitial, separator),
				string.Format("{1}{2}{0}", personalInfo.FirstName, personalInfo.MiddleInitial, separator),
			};

			if (!nameCombinations.Contains(signedName))
				return Json(new { error = "sign name supplied is incorrect" });

			bool somethingIsMissing =
				((cashRequest.LoanSource.Name == LoanSourceName.EU.ToString()) && !euAgreementTermsRead) ||
				((cashRequest.LoanSource.Name == LoanSourceName.COSME.ToString()) && !cosmeAgreementTermsRead) ||
				!notInBankruptcy;

			if (somethingIsMissing)
				return Json(new { error = "You must agree to all agreements." });

			var productSubTypeID = cashRequest.ProductSubTypeID;
			var originId = cashRequest.Customer.CustomerOrigin.CustomerOriginID;
			var isRegulated = cashRequest.Customer.PersonalInfo.TypeOfBusiness.IsRegulated();

			var requiredlegalDocsTemplates = this.serviceClient.Instance.GetLegalDocs(
				cashRequest.Customer.Id,
				this.context.UserId,
				originId,
				isRegulated,
				productSubTypeID ?? 0
			).LoanAgreementTemplates.Select(x => x.TemplateTypeID);

			// Validate sign on the right agreements
			foreach (var requiredlegalDocTemplate in requiredlegalDocsTemplates) {
				if (!dynamicLoanAgreements.ContainsKey((LegalDocsEnums.LoanAgreementTemplateType)requiredlegalDocTemplate))
					return Json(new { error = "You must agree to all agreements." });
			} // foreach

			DateTime now = DateTime.UtcNow;

			this.context.Customer.LastCashRequest.LoanLegals.Add(new LoanLegal {
				CashRequest = cashRequest,
				Created = now,
				EUAgreementAgreed = euAgreementTermsRead,
				COSMEAgreementAgreed = cosmeAgreementTermsRead,
				CreditActAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Personal,
				PreContractAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Personal,
				PrivateCompanyLoanAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Business,
				GuarantyAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Business,
				SignedName = signedName,
				NotInBankruptcy = notInBankruptcy,
				SignedLegalDocs = dynamicLoanAgreementsStringified,
				ManualSetupFeePercent = manualSetupFeePercent,
				BrokerSetupFeePercent = brokerFeePercent,
			});

			NL_LoanLegals nlLoanLegals = new NL_LoanLegals {
				Amount = loanAmount,
				RepaymentPeriod = repaymentPeriod,
				SignatureTime = now,
				EUAgreementAgreed = euAgreementTermsRead,
				COSMEAgreementAgreed = cosmeAgreementTermsRead,
				CreditActAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Personal,
				PreContractAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Personal,
				PrivateCompanyLoanAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Business,
				GuarantyAgreementAgreed = typeOfBusiness == TypeOfBusinessAgreementReduced.Business,
				SignedName = signedName,
				NotInBankruptcy = notInBankruptcy,
				SignedLegalDocs = dynamicLoanAgreementsStringified,
				// TODO ManualSetupFeePercent = manualSetupFeePercent,
				// TODO BrokerSetupFeePercent = brokerFeePercent,
			};

			var nlStrategyLegals = this.serviceClient.Instance.AddLoanLegals(this.context.UserId, this.context.Customer.Id, nlLoanLegals);

			//_log.Debug("NL_LoanLegals: ID {0}, Error: {1}", nlStrategyLegals.Value, nlStrategyLegals.Error);

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
			log.DebugFormat(
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

			var customer = this.context.Customer;

			if ((customer == null) || (customer.Id != customerID)) {
				log.ErrorFormat(
					"CreditLineSigned: invalid or unmatched customer ({0}) for requested id {1}.",
					customer.Stringify(),
					customerID
				);

				return Json(new { success = false, error = "Invalid customer name.", });
			} // if

			CashRequest cashRequest = customer.CashRequests.FirstOrDefault(r => r.Id == cashRequestID);

			if (cashRequest == null) {
				log.WarnFormat(
					"CreditLineSigned: cash request not found by id {0} at customer {1}.",
					cashRequestID,
					customer.Stringify()
				);

				return Json(new { success = false, error = "Invalid credit line.", });
			} // if

			bool hasError = !creditFacilityAccepted;

			if (hasError) {
				log.WarnFormat(
					"CreditLineSigned: credit facility not accepted for cash request {0} at customer {1}.",
					cashRequestID,
					customer.Stringify()
				);

				return Json(new { success = false, error = "You must agree to all agreements.", });
			} // if

            var productSubTypeID = cashRequest.ProductSubTypeID;
		    var originId = customer.CustomerOrigin.CustomerOriginID;
            var isRegulated = customer.PersonalInfo.TypeOfBusiness.IsRegulated();

		    var loanAgreementTemplates =
		        this.serviceClient.Instance.GetLegalDocs(customer.Id, this.context.UserId, originId, isRegulated, productSubTypeID ?? 0)
		            .LoanAgreementTemplates.ToDictionary(x => x.TemplateTypeName, x => true);

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
                SignedLegalDocs = new JavaScriptSerializer().Serialize(loanAgreementTemplates.ToDictionary(x => x.Key.DescriptionAttr(), x => x.Value.ToString()))
			});

			//el: TODO add LoanLegal for offer

			return Json(new { success = true, error = string.Empty, });
		}// CreditLineSigned
	}
}//ns
