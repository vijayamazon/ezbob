namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using Ezbob.Utils.Security;
	using ConfigManager;
	using EKM;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using ExperianLib.Ebusiness;
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
	using EZBob.DatabaseLib.Model;

	public class ProfileController : Controller
	{
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ServiceClient m_oServiceClient;
		private readonly CashRequestBuilder _crBuilder;
		private readonly ISession _session;
		private readonly IConfigurationVariablesRepository _configurationVariablesRepository;
		private readonly IPayPointFacade _payPointFacade;
		public ProfileController(
			CustomerModelBuilder customerModelBuilder,
			IEzbobWorkplaceContext context,
			CashRequestBuilder crBuilder,
			ISession session,
			IConfigurationVariablesRepository configurationVariablesRepository,
			IPayPointFacade payPointFacade)
		{
			_customerModelBuilder = customerModelBuilder;
			_context = context;
			m_oServiceClient = new ServiceClient();
			_crBuilder = crBuilder;
			_session = session;
			_configurationVariablesRepository = configurationVariablesRepository;
			_payPointFacade = payPointFacade;
		} // constructor

		//----------------------------------------------------------------------
		[IsSuccessfullyRegisteredFilter]
		public ViewResult Index()
		{
			var wizardModel = new WizardModel { Customer = _customerModelBuilder.BuildWizardModel(_context.Customer, Session) };
			ViewData["ShowChangePasswordPage"] = _context.User.IsPasswordRestored;

			ViewData["MarketPlaces"] = _session
				.Query<MP_MarketplaceType>()
				.ToArray();

			ViewData["MarketPlaceGroups"] = _session
				.Query<MP_MarketplaceGroup>()
				.ToArray();

			bool wizardComplete = (TempData["WizardComplete"] != null && (bool)TempData["WizardComplete"]) || (Session["WizardComplete"] != null && (bool)Session["WizardComplete"]);
			ViewData["WizardComplete"] = wizardComplete;
			Session["WizardComplete"] = false;
			TempData["WizardComplete"] = false;

			return View("Index", wizardModel);
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Details()
		{
			var details = _customerModelBuilder.BuildWizardModel(_context.Customer, Session);
			return Json(details, JsonRequestBehavior.AllowGet);
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ClaimsTrustPilotReview()
		{
			var customer = _context.Customer;

			if (customer == null)
				return Json(new { status = "error", error = "Customer not found." });

			if (ReferenceEquals(customer.TrustPilotStatus, null) || customer.TrustPilotStatus.IsMe(TrustPilotStauses.Nether))
			{
				var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

				customer.TrustPilotStatus = oHelper.TrustPilotStatusRepository.Find(TrustPilotStauses.Claims);

				_session.Flush();
			} // if

			return Json(new { status = "ok", error = "" });
		} // ClaimsTrustPilotReview

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult EntrepreneurTargeting()
		{
			var customer = _context.Customer;

			if (customer == null)
				return Json(new { status = "error", error = "Customer not found." });

			if (customer.PersonalInfo.TypeOfBusiness == TypeOfBusiness.Entrepreneur && customer.Company == null)
			{
				var address = customer.AddressInfo.PersonalAddress.FirstOrDefault();
				return Json(new
					{
						companyTargeting = true,
						companyName = string.Format("{0} {1}", customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname),
						companyPostcode = address != null ? address.Postcode : "",
						companyNumber = "",
						companyType = "N"
					});
			}

			if (customer.Company != null &&
				string.IsNullOrEmpty(customer.Company.ExperianRefNum) &&
				!string.IsNullOrEmpty(customer.Company.CompanyName) &&
				customer.Company.TypeOfBusiness.Reduce() != TypeOfBusinessReduced.Personal)
			{
				var companyAddress = customer.Company.CompanyAddress.FirstOrDefault();
				return Json(new
							{
								companyTargeting = true,
								companyName = customer.Company.CompanyName,
								companyNumber = customer.Company.CompanyNumber ?? String.Empty,
								companyPostcode = companyAddress != null ? companyAddress.Postcode : "",
								companyType = customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited ? "L" : "N"
							});
			}
			return Json(new { companyTargeting = false });
		} // EntrepreneurTargeting

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveTargeting(CompanyInfo company)
		{
			var customer = _context.Customer;

			if (customer.Company == null)
			{
				customer.Company = new Company { TypeOfBusiness = customer.PersonalInfo.TypeOfBusiness };
			}
			customer.Company.ExperianRefNum = company.BusRefNum;
			customer.Company.ExperianCompanyName = company.BusName;

			customer.Company.ExperianCompanyAddress =
				new HashedSet<CustomerAddress>
					{
						new CustomerAddress
							{
								Line1 = company.AddrLine1,
								Line2 = company.AddrLine2,
								Line3 = company.AddrLine3,
								County = company.AddrLine4,
								Postcode = company.PostCode,
								AddressType = CustomerAddressType.ExperianCompanyAddress,
								Customer = customer,
								Company = customer.Company
							}
					};
			return Json(new { });
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetRefreshInterval()
		{
			int refreshInterval = new ServiceClient().Instance.GetCustomerStatusRefreshInterval().Value;
			return Json(new { Interval = refreshInterval });
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetCustomerStatus(int customerId)
		{
			string state = new ServiceClient().Instance.GetCustomerState(customerId).Value;
			return Json(new { State = state });
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ApplyForALoan()
		{
			var customer = _context.Customer;
			if (customer == null)
			{
				return Json(new { });
			}
			var ekmType = new EkmDatabaseMarketPlace();
			var ekms = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == ekmType.InternalId).ToList();
			if (ekms.Any())
			{
				var validator = new EkmConnector();
				foreach (var ekm in ekms)
				{
					var name = ekm.DisplayName;
					var password = Encrypted.Decrypt(ekm.SecurityData);
					string error;
					var isValid = validator.Validate(name, password, out error);
					if (!isValid)
					{
						return Json(new { hasBadEkm = true, error = error, ekm = ekm.DisplayName });
					}
				}

			}
			customer.CreditResult = null;

			customer.OfferStart = DateTime.UtcNow;
			int offerValidForHours = (int)_configurationVariablesRepository.GetByNameAsDecimal("OfferValidForHours");
			customer.OfferValidUntil = DateTime.UtcNow.AddHours(offerValidForHours);

			customer.ApplyCount = customer.ApplyCount + 1;

			var oldOffer = customer.LastCashRequest;
			if (oldOffer != null && oldOffer.HasLoans)
				m_oServiceClient.Instance.RequestCashWithoutTakenLoan(customer.Id);

			var cashRequest = _crBuilder.CreateCashRequest(customer, CashRequestOriginator.RequestCashBtn);

			_crBuilder.ForceEvaluate(customer.Id, customer, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, false, false);

			var yodlees = customer.GetYodleeAccounts().ToList();
			bool refreshYodleeEnabled = CurrentValues.Instance.RefreshYodleeEnabled;
			if (yodlees.Any() && refreshYodleeEnabled)
				return Json(new { hasYodlee = true });

			return Json(new { });
		}


		public ViewResult RenewEbayToken()
		{
			return View();
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SetDefaultCard(int cardId)
		{
			var customer = _context.Customer;
			var card = customer.PayPointCards.SingleOrDefault(c => c.Id == cardId);
			if (card == null)
			{
				return Json(new { error = "card not found" });
			}
			customer.PayPointTransactionId = card.TransactionId;
			customer.CreditCardNo = card.CardNo;

			return Json(new {});
		}

		public RedirectResult AddPayPoint()
		{
			var oCustomer = _context.Customer;
			int payPointCardExpiryMonths = CurrentValues.Instance.PayPointCardExpiryMonths;
			DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointCardExpiryMonths);
			var callback = Url.Action("PayPointCallback", "Profile", new { Area = "Customer", customerId = oCustomer.Id, cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate) }, "https");
			var url = _payPointFacade.GeneratePaymentUrl(oCustomer, 5m, callback);

			return Redirect(url);
		}

		[Transactional]
		[HttpGet]
		public ActionResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string card_no, string customer, string expiry, int customerId)
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
			if (!valid || code != "A")
			{
				TempData["code"] = code;
				TempData["message"] = message;
				return View(new { error = "Failed to add debit card"});
			}

			if (!_payPointFacade.CheckHash(hash, Request.Url))
			{
				return View(new { error = "Failed to add debit card" });
			}

			var cust = _context.Customer;
			var card = cust.TryAddPayPointCard(trans_id, card_no, expiry, cust.PersonalInfo.Fullname);

			if (string.IsNullOrEmpty(cust.PayPointTransactionId))
			{
				SetDefaultCard(card.Id);
			}

			return View(new {success = true});
		}
	}
}
