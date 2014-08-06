namespace EzBob.Web.Areas.Customer.Controllers {
	#region using

	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using ExperianLib.Ebusiness;
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
	using Web.Models;
	using NHibernate;
	using NHibernate.Linq;
	using System.Linq;

	#endregion using

	public class ProfileController : Controller {
		#region public

		#region constructor

		public ProfileController(
			CustomerModelBuilder oCustomerModelBuilder,
			IEzbobWorkplaceContext oContext,
			CashRequestBuilder oCashRequestBuilder,
			ISession oSession,
			IPayPointFacade oPayPointFacade
		) {
			m_oCustomerModelBuilder = oCustomerModelBuilder;
			m_oContext = oContext;
			m_oServiceClient = new ServiceClient();
			m_oCashRequestBuilder = oCashRequestBuilder;
			m_oSession = oSession;
			m_oPayPointFacade = oPayPointFacade;
		} // constructor

		#endregion constructor

		#region action Index

		[IsSuccessfullyRegisteredFilter]
		public ViewResult Index() {
			var wizardModel = new WizardModel { Customer = m_oCustomerModelBuilder.BuildWizardModel(m_oContext.Customer, Session, true) };
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

		#endregion action Index

		#region action Details

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Details() {
			var details = m_oCustomerModelBuilder.BuildWizardModel(m_oContext.Customer, Session, true);
			return Json(details, JsonRequestBehavior.AllowGet);
		} // Details

		#endregion action Details

		#region action ClaimsTrustPilotReview

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

		#endregion action ClaimsTrustPilotReview

		#region action EntrepreneurTargeting

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

		#endregion action EntrepreneurTargeting

		#region action SaveTargeting

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

		#endregion action SaveTargeting

		#region action GetRefreshInterval

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetRefreshInterval() {
			int refreshInterval = new ServiceClient().Instance.GetCustomerStatusRefreshInterval().Value;
			return Json(new { Interval = refreshInterval });
		} // GetRefreshInterval

		#endregion action GetRefreshInterval

		#region action GetCustomerStatus

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetCustomerStatus(int customerId) {
			string state = new ServiceClient().Instance.GetCustomerState(customerId).Value;
			return Json(new { State = state });
		} // GetCustomerStatus

		#endregion action GetCustomerStatus

		#region action ApplyForALoan

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ApplyForALoan() {
			var oModel = new ApplyForLoanResultModel(m_oContext.Customer, false);

			if (oModel.IsReadyForApply())
				DoApplyForLoan();

			return Json(oModel);
		} // ApplyForALoan

		#endregion action ApplyForALoan

		#region action DirectApplyForLoan

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult DirectApplyForLoan() {
			var oModel = new ApplyForLoanResultModel(m_oContext.Customer, true);

			if (oModel.IsReadyForApply())
				DoApplyForLoan();

			return Json(oModel);
		} // DirectApplyForLoan

		#endregion action DirectApplyForLoan

		#region action RenewEbayToken

		public ViewResult RenewEbayToken() {
			return View();
		} // RenewEbayToken

		#endregion action RenewEbayToken

		#region action SetDefaultCard

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

			customer.PayPointTransactionId = card.TransactionId;
			customer.CreditCardNo = card.CardNo;

			return Json(new { });
		} // SetDefaultCard

		#endregion action SetDefaultCard

		#region action AddPayPoint

		public RedirectResult AddPayPoint() {
			var oCustomer = m_oContext.Customer;
			int payPointCardExpiryMonths = CurrentValues.Instance.PayPointCardExpiryMonths;
			DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointCardExpiryMonths);
			var callback = Url.Action("PayPointCallback", "Profile",
				new
				{
					Area = "Customer",
					customerId = oCustomer.Id,
					cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate),
					hideSteps = true
				}, "https");
			var url = m_oPayPointFacade.GeneratePaymentUrl(oCustomer, 5m, callback);

			return Redirect(url);
		} // AddPayPoint

		#endregion action AddPayPoint

		#region action PayPointCallback

		[Transactional]
		[HttpGet]
		public ActionResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string card_no, string customer, string expiry, int customerId) {
			if (test_status == "true") {
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);

				if (random4Digits.Length > 4)
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);

				card_no = random4Digits;
				expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
			} // if

			if (!valid || code != "A") {
				TempData["code"] = code;
				TempData["message"] = message;
				return View(new { error = "Failed to add debit card" });
			} // if

			if (!m_oPayPointFacade.CheckHash(hash, Request.Url))
				return View(new { error = "Failed to add debit card" });

			var cust = m_oContext.Customer;
			var card = cust.TryAddPayPointCard(trans_id, card_no, expiry, cust.PersonalInfo.Fullname);

			if (string.IsNullOrEmpty(cust.PayPointTransactionId))
				SetDefaultCard(card.Id);

			return View(new { success = true });
		} // PayPointCallback

		#endregion action PayPointCallback

		#endregion public

		#region private

		#region method DoApplyForLoan

		private void DoApplyForLoan() {
			Customer customer = m_oContext.Customer;

			ms_oLog.Debug("Customer {0} applied for loan, creating a cash request...", customer.Stringify());

			Transactional.Execute(() => {
				customer.CreditResult = null;

				customer.OfferStart = DateTime.UtcNow;
				customer.OfferValidUntil = DateTime.UtcNow.AddHours(CurrentValues.Instance.OfferValidForHours);

				customer.ApplyCount++;

				if (customer.LastCashRequest != null && customer.LastCashRequest.HasLoans)
					m_oServiceClient.Instance.RequestCashWithoutTakenLoan(customer.Id);

				m_oCashRequestBuilder.CreateCashRequest(customer, CashRequestOriginator.RequestCashBtn);
				m_oCashRequestBuilder.ForceEvaluate(customer.Id, customer, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, false, false);

				m_oSession.Flush();
			});

			ms_oLog.Debug("Customer {0} applied for loan, cash request is ready.", customer.Stringify());
		} // DoApplyForLoan

		#endregion method DoApplyForLoan

		#region fields

		private readonly CustomerModelBuilder m_oCustomerModelBuilder;
		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly ServiceClient m_oServiceClient;
		private readonly CashRequestBuilder m_oCashRequestBuilder;
		private readonly ISession m_oSession;
		private readonly IPayPointFacade m_oPayPointFacade;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (ProfileController));

		#endregion fields

		#endregion private
	} // class ProfileController
} // namespace
