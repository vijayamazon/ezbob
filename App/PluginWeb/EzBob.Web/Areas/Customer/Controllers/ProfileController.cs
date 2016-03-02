namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using Ezbob.Logger;
	using Iesi.Collections.Generic;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.Filters;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using StructureMap;
	using NHibernate;
	using NHibernate.Linq;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using PaymentServices.Calculators;

	public class ProfileController : Controller {

		public ProfileController(
			CustomerModelBuilder oCustomerModelBuilder,
			IEzbobWorkplaceContext oContext,
			ISession oSession,
			ISecurityQuestionRepository questions) {
			m_oCustomerModelBuilder = oCustomerModelBuilder;
			m_oContext = oContext;
			m_oServiceClient = new ServiceClient();
			m_oSession = oSession;
			this.questions = questions;
		} // constructor

		protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
			hostname = requestContext.HttpContext.Request.Url.Host;
			ms_oLog.Info("WizardController Initialize {0}", hostname);
			base.Initialize(requestContext);
		}


		[IsSuccessfullyRegisteredFilter]
		public ViewResult Index() {
			var wizardModel = m_oCustomerModelBuilder.BuildWizardModel(
				m_oContext.Customer,
				Session,
				null,
				hostname,
				true
			);
			ViewData["Questions"] = this.questions.GetAll().ToList();
			ViewData["ShowChangePasswordPage"] = m_oContext.User.IsPasswordRestored;

			ViewData["MarketPlaces"] = m_oSession
				.Query<MP_MarketplaceType>()
				.ToArray();

			ViewData["MarketPlaceGroups"] = m_oSession
				.Query<MP_MarketplaceGroup>()
				.ToArray();

			bool wizardComplete = (TempData["WizardComplete"] != null && (bool)TempData["WizardComplete"]) || (Session["WizardComplete"] != null && (bool)Session["WizardComplete"]);
			ViewData["WizardComplete"] = wizardComplete;
			Session["WizardComplete"] = false;
			TempData["WizardComplete"] = false;

			return View("Index", wizardModel);
		} // Index

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Details() {
			var details = m_oCustomerModelBuilder.BuildWizardModel(m_oContext.Customer, Session, null, this.Request.Url.Host, true);
			return Json(details.Customer, JsonRequestBehavior.AllowGet);
		} // Details

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ClaimsTrustPilotReview() {
			var customer = m_oContext.Customer;

			if (customer == null)
				return Json(new { status = "error", error = "Customer not found." });

			if (ReferenceEquals(customer.TrustPilotStatus, null) || customer.TrustPilotStatus.IsMe(TrustPilotStauses.Neither)) {
				var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

				customer.TrustPilotStatus = oHelper.TrustPilotStatusRepository.Find(TrustPilotStauses.Claims);

				m_oSession.Flush();
			} // if

			return Json(new { status = "ok", error = "" });
		} // ClaimsTrustPilotReview

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult EntrepreneurTargeting() {
			var customer = m_oContext.Customer;

			if (customer == null)
				return Json(new { status = "error", error = "Customer not found." });

			if (customer.PersonalInfo.TypeOfBusiness == TypeOfBusiness.Entrepreneur && customer.Company == null) {
				var address = customer.AddressInfo.PersonalAddress.FirstOrDefault();

				return Json(new {
					companyTargeting = true,
					companyName = string.Format("{0} {1}", customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname),
					companyPostcode = address != null ? address.Postcode : "",
					companyNumber = "",
					companyType = "N"
				});
			} // if

			if (
				customer.Company != null &&
				string.IsNullOrEmpty(customer.Company.ExperianRefNum) &&
				!string.IsNullOrEmpty(customer.Company.CompanyName) &&
				customer.Company.TypeOfBusiness.Reduce() != TypeOfBusinessReduced.Personal
			) {
				var companyAddress = customer.Company.CompanyAddress.FirstOrDefault();

				return Json(new {
					companyTargeting = true,
					companyName = customer.Company.CompanyName,
					companyNumber = customer.Company.CompanyNumber ?? String.Empty,
					companyPostcode = companyAddress != null ? companyAddress.Postcode : "",
					companyType = customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited ? "L" : "N"
				});
			} // if

			return Json(new { companyTargeting = false });
		} // EntrepreneurTargeting

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveTargeting(CompanyInfo company) {
			var customer = m_oContext.Customer;

			if (customer.Company == null)
				customer.Company = new Company { TypeOfBusiness = customer.PersonalInfo.TypeOfBusiness };

			customer.Company.ExperianRefNum = company.BusRefNum;
			customer.Company.ExperianCompanyName = company.BusName;

			customer.Company.ExperianCompanyAddress = new HashedSet<CustomerAddress> { new CustomerAddress {
				Line1 = company.AddrLine1,
				Line2 = company.AddrLine2,
				Line3 = company.AddrLine3,
				County = company.AddrLine4,
				Postcode = company.PostCode,
				AddressType = CustomerAddressType.ExperianCompanyAddress,
				Customer = customer,
				Company = customer.Company
			}};

			return Json(new { });
		} // SaveTargeting

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ApplyForALoan() {
			var oModel = new ApplyForLoanResultModel(m_oContext.Customer, false);

			if (oModel.IsReadyForApply())
				DoApplyForLoan();

			return Json(oModel);
		} // ApplyForALoan

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult DirectApplyForLoan() {
			var oModel = new ApplyForLoanResultModel(m_oContext.Customer, true);

			if (oModel.IsReadyForApply())
				DoApplyForLoan();

			return Json(oModel);
		} // DirectApplyForLoan

		public ViewResult RenewEbayToken() {
			return View();
		} // RenewEbayToken

		[Ajax]
		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SetDefaultCard(int cardId) {
			var customer = m_oContext.Customer;

			if (!customer.DefaultCardSelectionAllowed)
				return Json(new { error = "Default card selection is not allowed" });

			var card = customer.PayPointCards.SingleOrDefault(c => c.Id == cardId);

			if (card == null)
				return Json(new { error = "Card not found" });

			foreach (var paypointCard in customer.PayPointCards) {
				paypointCard.IsDefaultCard = false;
			}
			card.IsDefaultCard = true;

			return Json(new { });
		} // SetDefaultCard

		public RedirectResult AddPayPoint() {
			var oCustomer = m_oContext.Customer;
			PayPointFacade payPointFacade = new PayPointFacade(oCustomer.MinOpenLoanDate(), oCustomer.CustomerOrigin.Name);
			int payPointCardExpiryMonths = payPointFacade.PayPointAccount.CardExpiryMonths;
			DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointCardExpiryMonths);
			var callback = Url.Action("PayPointCallback", "Profile",
				new {
					Area = "Customer",
					customerId = oCustomer.Id,
					cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate),
					hideSteps = true,
					origin = oCustomer.CustomerOrigin.Name
				}, "https");



			var url = payPointFacade.GeneratePaymentUrl(oCustomer, 5.00m, callback);

			return Redirect(url);
		} // AddPayPoint

		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[Transactional]
		[HttpGet]
		public ActionResult PayPointCallback(
			bool valid,
			string trans_id,
			string code,
			string auth_code,
			decimal? amount,
			string ip,
			string test_status,
			string hash,
			string message,
			string card_no,
			string customer,
			string expiry,
			int customerId
		) {
			ms_oLog.Debug(
				"PayPointCallback with args:" +
				"\n\tvalid: '{0}'" +
				"\n\ttrans_id: '{1}'" +
				"\n\tcode: '{2}'" +
				"\n\tauth_code: '{3}'" +
				"\n\tamount: '{4}'" +
				"\n\tip: '{5}'" +
				"\n\ttest_status: '{6}'" +
				"\n\thash: '{7}'" +
				"\n\tmessage: '{8}'" +
				"\n\tcard_no: '{9}'" +
				"\n\tcustomer: '{10}'" +
				"\n\texpiry: '{11}'" +
				"\n\tcustomer id: '{12}'",
				valid,
				trans_id,
				code,
				auth_code,
				amount,
				ip,
				test_status,
				hash,
				message,
				card_no,
				customer,
				expiry,
				customerId
			);

			if (test_status == "true") {
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);

				if (random4Digits.Length > 4)
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);

				card_no = random4Digits;
				expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
			} // if

			if (!valid || code != "A") {
				ms_oLog.Debug("Failed to add debit card: invalid or code ain't no 'A'.");

				TempData["code"] = code;
				TempData["message"] = message;
				return View(new { error = "Failed to add debit card" });
			} // if

			var cust = m_oContext.Customer;

			PayPointFacade payPointFacade = new PayPointFacade(cust.MinOpenLoanDate(), cust.CustomerOrigin.Name);

			if (!payPointFacade.CheckHash(hash, Request.Url)) {
				ms_oLog.Debug("Failed to add debit card: failed to CheckHash(\n\t hash: '{0}',\n\t Url: '{1}'\n).", hash, Request.Url);

				return View(new { error = "Failed to add debit card" });
			} // if

			cust.TryAddPayPointCard(
				trans_id,
				card_no,
				expiry,
				cust.PersonalInfo.Fullname,
				payPointFacade.PayPointAccount
			);

			bool hasOpenLoans = cust.Loans.Any(x => x.Status != LoanStatus.PaidOff);
			if (amount > 0 && hasOpenLoans) {

				Loan loan = cust.Loans.First(x => x.Status != LoanStatus.PaidOff);

				NL_Payments	nlPayment = new NL_Payments() {
					Amount = amount.Value,
					CreatedByUserID = this.m_oContext.UserId,
					//	LoanID = nlLoanId,
					//PaymentStatusID = (int)NLPaymentStatuses.Active,
					PaymentSystemType = NLPaymentSystemTypes.Paypoint,
					PaymentMethodID = (int)NLLoanTransactionMethods.SystemRepay,
					PaymentTime = DateTime.UtcNow,
					Notes = "add payPoint card",
					CreationTime = DateTime.UtcNow
				};

				var f = new LoanPaymentFacade();
				f.PayLoan(loan, trans_id, amount.Value, Request.UserHostAddress, DateTime.UtcNow, "system-repay", false, null, nlPayment);
			}

			if (amount > 0 && !hasOpenLoans) {
				this.m_oServiceClient.Instance.PayPointAddedWithoutOpenLoan(cust.Id, cust.Id, amount.Value, trans_id);
			}
			return View(new { success = true });
		} // PayPointCallback

		private void DoApplyForLoan() {
			Customer customer = m_oContext.Customer;

			ms_oLog.Debug("Customer {0} applied for a loan, processing...", customer.Stringify());

			Transactional.Execute(() => {
				customer.CreditResult = null;

				customer.OfferStart = DateTime.UtcNow;
				customer.OfferValidUntil = DateTime.UtcNow.AddHours(CurrentValues.Instance.OfferValidForHours);

				customer.ApplyCount++;

				if (customer.LastCashRequest != null && customer.LastCashRequest.HasLoans)
					m_oServiceClient.Instance.RequestCashWithoutTakenLoan(customer.Id);

				m_oSession.Flush();
			});

			new MainStrategyClient(
				customer.Id,
				customer.Id,
				customer.IsAvoid,
				NewCreditLineOption.UpdateEverythingAndApplyAutoRules,
				null,
				CashRequestOriginator.RequestCashBtn
			).ExecuteAsync();

			ms_oLog.Debug("Customer {0} applied for a loan, processing complete.", customer.Stringify());
		} // DoApplyForLoan

		[Ajax]
		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonResult UpdateTurnover(decimal turnover) {
			var customer = m_oContext.Customer;
			customer.PersonalInfo.OverallTurnOver = turnover;
			customer.Turnovers.Add(new CustomerTurnover {
				CustomerID = customer.Id,
				Timestamp = DateTime.UtcNow,
				Turnover = turnover
			});
			
			return Json(new { });
		} // UpdateTurnover

		private readonly CustomerModelBuilder m_oCustomerModelBuilder;
		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly ServiceClient m_oServiceClient;
		private readonly ISession m_oSession;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(ProfileController));
		private string hostname;
		private readonly ISecurityQuestionRepository questions;
	} // class ProfileController
} // namespace
