namespace EzBob.Web.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Helpers;
	using System.Web.Mvc;
	using System.Web.Security;
	using Areas.Customer.Controllers.Exceptions;
	using ConfigManager;
	using DbConstants;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib.Ebusiness;
	using Code;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Web.Infrastructure.Email;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.Filters;
	using Infrastructure.Membership;
	using Infrastructure.csrf;
	using JetBrains.Annotations;
	using Models;
	using Models.Strings;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Alibaba;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class AccountController : Controller {
		public AccountController() {
			m_oDatabaseHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			m_oUsers = ObjectFactory.GetInstance<IUsersRepository>();
			m_oCustomers = ObjectFactory.GetInstance<CustomerRepository>();
			m_oServiceClient = new ServiceClient();
			m_oContext = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();
			m_oSessionIpLog = ObjectFactory.GetInstance<ICustomerSessionsRepository>();
			m_oTestCustomers = ObjectFactory.GetInstance<ITestCustomerRepository>();
			m_oCustomerStatusesRepository = ObjectFactory.GetInstance<ICustomerStatusesRepository>();
			m_oBrokerHelper = new BrokerHelper();
			m_oLogOffMode = (LogOffMode)(int)CurrentValues.Instance.LogOffMode;
			m_oVipRequestRepository = ObjectFactory.GetInstance<IVipRequestRepository>();
			m_oDB = DbConnectionGenerator.Get(ms_oLog);
			_whiteLabelProviderRepository = ObjectFactory.GetInstance<WhiteLabelProviderRepository>();
		} // constructor

		protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
			bool hasHost =
				(requestContext != null) &&
				(requestContext.HttpContext != null) &&
				(requestContext.HttpContext.Request != null) &&
				(requestContext.HttpContext.Request.Url != null);

			hostname = hasHost ? requestContext.HttpContext.Request.Url.Host : string.Empty;
			ms_oLog.Info("WizardController Initialize {0}", hostname);
			base.Initialize(requestContext);
		}

		public ActionResult AdminLogOn(string returnUrl) {
			Response.AddHeader("X-FRAME-OPTIONS", "");
			if (!bool.Parse(ConfigurationManager.AppSettings["UnderwriterEnabled"])) {
				return RedirectToAction("LogOn", "Account");
			}

			ViewData["returnUrl"] = returnUrl;
			return View(new LogOnModel { ReturnUrl = returnUrl });
		} // AdminLogOn

		[HttpPost]
		public ActionResult AdminLogOn(LogOnModel model) {
			if (!bool.Parse(ConfigurationManager.AppSettings["UnderwriterEnabled"])) {
				return RedirectToAction("LogOn", "Account");
			}

			if (ModelState.IsValid) {
				try {
					if (m_oBrokerHelper.IsBroker(model.UserName)) {
						ms_oLog.Alert("Broker '{0}' tried to log in as an underwriter!", model.UserName);
						ModelState.AddModelError("LoginError", "Wrong user name/password.");
						return View(model);
					} // if is broker
				} catch (Exception e) {
					ms_oLog.Warn(
						e,
						"Failed to check whether '{0}' is a broker login, continuing as an underwriter.",
						model.UserName
					);
				} // try

				if (m_oCustomers.TryGetByEmail(model.UserName) != null) {
					ms_oLog.Alert("Customer '{0}' tried to log in as an underwriter!", model.UserName);
					ModelState.AddModelError("LoginError", "Wrong user name/password.");
					return View(model);
				} // if

				try {
					string loginError;
					var membershipCreateStatus = ValidateUser(model.UserName, model.Password, null, null, out loginError);
					if (MembershipCreateStatus.Success == membershipCreateStatus) {
						model.SetCookie(LogOnModel.Roles.Underwriter);

						bool bRedirectToUrl =
							Url.IsLocalUrl(model.ReturnUrl) &&
							(model.ReturnUrl.Length > 1) &&
							model.ReturnUrl.StartsWith("/") &&
							!model.ReturnUrl.StartsWith("//") &&
							!model.ReturnUrl.StartsWith("/\\");

						if (bRedirectToUrl)
							return Redirect(model.ReturnUrl);

						return RedirectToAction("Index", "Customers", new { Area = "Underwriter" });
					} // if

					loginError = string.IsNullOrEmpty(loginError) ? "Wrong user name/password." : loginError;
					ModelState.AddModelError("LoginError", loginError);
				} catch (UserNotFoundException ex) {
					ms_oLog.Warn(ex, "Failed to log in as underwriter '{0}'.", model.UserName);
					ModelState.AddModelError("LoginError", "Wrong user name/password.");
				} // try
			} // if

			return View(model);
		} // AdminLogOn

		[IsSuccessfullyRegisteredFilter]
		public ActionResult LogOn(string returnUrl, string scratch_promotion = null) {
			var model = new LogOnModel { ReturnUrl = returnUrl, };

			FillPromotionData(model, scratch_promotion);

			return View(model);
		} // LogOn

		[HttpPost]
		[NoCache]
		public JsonResult CustomerLogOn(LogOnModel model) {
			var customerIp = RemoteIp();

			if (!ModelState.IsValid) {
				ms_oLog.Debug(
					"Customer log on attempt from remote IP {0}: model state is invalid, list of errors:",
					customerIp
				);

				foreach (var val in ModelState.Values) {
					if (val.Errors.Count < 1)
						continue;

					foreach (var err in val.Errors)
						ms_oLog.Debug("Model value '{0}' with error '{1}'.", val.Value, err.ErrorMessage);
				} // for each value

				ms_oLog.Debug("End of list of errors.");

				return Json(new {
					success = false,
					errorMessage = "User not found or incorrect password."
				}, JsonRequestBehavior.AllowGet);
			} // if

			ms_oLog.Debug(
				"Customer log on attempt from remote IP {0} received " +
				"with user name '{1}' and hash '{2}' (promotion: {3})...",
				customerIp,
				model.UserName,
				Ezbob.Utils.Security.SecurityUtils.HashPassword(model.UserName, model.Password),
				model.PromotionDisplayData
			);

			try {
				if (m_oBrokerHelper.IsBroker(model.UserName)) {
					BrokerProperties bp = m_oBrokerHelper.TryLogin(
						model.UserName,
						model.Password,
						model.PromotionName,
						model.PromotionPageVisitTime
					);

					if ((bp != null) && (bp.CurrentTermsID != bp.SignedTermsID)) {
						Session[Constant.Broker.Terms] = bp.CurrentTerms;
						Session[Constant.Broker.TermsID] = bp.CurrentTermsID;
					} // if

					return Json(new {
						success = (bp != null),
						errorMessage = (bp == null) ? "User not found or incorrect password." : string.Empty,
						broker = true,
					});
				} // if is broker
			} catch (Exception e) {
				ms_oLog.Warn(
					e,
					"Failed to check whether '{0}' is a broker login, continuing as a customer.",
					model.UserName
				);
			} // try

			User user;

			try {
				user = m_oUsers.GetUserByLogin(model.UserName);
			} catch (Exception e) {
				ms_oLog.Warn(e, "Failed to retrieve a user by name '{0}'.", model.UserName);
				user = null;
			} // try

			CustomerOrigin uiOrigin = UiCustomerOrigin.Get();

			if (user == null) {
				if (uiOrigin.IsEverline()) {
					var loginLoanChecker = new EverlineLoginLoanChecker();
					var status = loginLoanChecker.GetLoginStatus(model.UserName);

					switch (status.status) {
					case EverlineLoanStatus.Error:
						ms_oLog.Error("Failed to retrieve Everline customer loan status \n{0}", status.Message);
						return Json(new {
							success = false,
							errorMessage = "User not found or incorrect password."
						}, JsonRequestBehavior.AllowGet);

					case EverlineLoanStatus.ExistsWithCurrentLiveLoan:
						return Json(new { success = true, everlineAccount = true }, JsonRequestBehavior.AllowGet);

					case EverlineLoanStatus.ExistsWithNoLiveLoan:
						TempData["IsEverline"] = true;
						TempData["CustomerEmail"] = model.UserName;
						return Json(new { success = true, everlineWizard = true }, JsonRequestBehavior.AllowGet);

					case EverlineLoanStatus.DoesNotExist:
						return Json(new {
							success = false,
							errorMessage = "User not found or incorrect password."
						}, JsonRequestBehavior.AllowGet);

					default:
						ms_oLog.Alert("Unsupported EverlineLoanStatus: {0}.", status.status);
						return Json(new {
							success = false,
							errorMessage = "User not found or incorrect password."
						}, JsonRequestBehavior.AllowGet);
					} // switch
				} // if

				ms_oLog.Warn(
					"Customer log on attempt from remote IP {0} with user name '{1}': could not find a user entry.",
					customerIp,
					model.UserName
				);

				return Json(new {
					success = false,
					errorMessage = "User not found or incorrect password."
				}, JsonRequestBehavior.AllowGet);
			} // if user not found

			var isUnderwriter = user.BranchId == 1;

			ms_oLog.Debug(
				"{1} log on attempt with login '{0}'.",
				model.UserName,
				isUnderwriter ? "Underwriter" : "Customer"
			);

			ms_oLog.Debug(
				"{2} log on attempt from remote IP {0} with user name '{1}': log on attempt #{3}.",
				customerIp,
				model.UserName,
				isUnderwriter ? "Underwriter" : "Customer",
				(user.LoginFailedCount ?? 0) + 1
			);

			if (isUnderwriter) {
				ms_oLog.Debug(
					"Underwriter log on attempt from remote IP {0} with user name '{1}': failed, " +
					"underwriters should use a dedicated log on page.",
					customerIp,
					model.UserName
				);

				return Json(new {
					success = false,
					errorMessage = "Please use dedicated underwriter log on page."
				}, JsonRequestBehavior.AllowGet);
			} // if

			Customer customer;

			try {
				customer = m_oCustomers.Get(user.Id);
			} catch (Exception e) {
				ms_oLog.Warn(e, "Failed to retrieve a customer by id {0}.", user.Id);
				return Json(new {
					success = false,
					errorMessage = "User not found or invalid password."
				}, JsonRequestBehavior.AllowGet);
			} // try

			if (customer.CollectionStatus.CurrentStatus.Name == "Disabled") {
				string sDisabledError =
					"This account is closed, please contact <span class='bold'>ezbob</span> customer care<br/> " +
					uiOrigin.CustomerCareEmail;

				var session = new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false,
					ErrorMessage = sDisabledError,
				};

				m_oSessionIpLog.AddSessionIpLog(session);
				Session["UserSessionId"] = session.Id;

				ms_oLog.Warn(
					"Customer log on attempt from remote IP {0} with user name '{1}': the customer is disabled.",
					customerIp,
					model.UserName
				);

				return Json(new { success = false, errorMessage = sDisabledError, }, JsonRequestBehavior.AllowGet);
			} // if user is disabled

			if (uiOrigin.GetOrigin() != customer.CustomerOrigin.GetOrigin()) {
				ms_oLog.Warn(
					"Customer {0} with origin {1} tried to login with UI origin {2} (from host {3}).",
					user.Id,
					customer.CustomerOrigin.Name,
					uiOrigin.Name,
					this.hostname
				);

				return Json(new {
					success = false,
					errorMessage = "User not found or invalid password.",
				}, JsonRequestBehavior.AllowGet);
			} // if

			string loginError;
			var nStatus = ValidateUser(
				model.UserName,
				model.Password,
				model.PromotionName,
				model.PromotionPageVisitTime,
				out loginError
			);

			if (MembershipCreateStatus.Success == nStatus) {
				model.SetCookie(LogOnModel.Roles.Customer);

				ms_oLog.Debug(
					"Customer log on attempt from remote IP {0} with user name '{1}': success.",
					customerIp,
					model.UserName
				);

				return Json(new { success = true, model, }, JsonRequestBehavior.AllowGet);
			} // if logged in successfully

			string errorMessage = MembershipCreateStatus.InvalidProviderUserKey == nStatus
				? "Three unsuccessful login attempts have been made. <span class='bold'>" +
					customer.CustomerOrigin.Name +
					"</span> has issued you with a temporary password. Please check your e-mail."
				: "User not found or incorrect password.";

			ms_oLog.Warn(
				"Customer log on attempt from remote IP {0} with user name '{1}': failed {2}.",
				customerIp,
				model.UserName,
				errorMessage
			);

			// If we got this far, something failed, redisplay form
			return Json(new { success = false, errorMessage }, JsonRequestBehavior.AllowGet);
		} // CustomerLogOn

		public ActionResult LogOff() {
			EndSession("LogOff customer");

			switch (m_oLogOffMode) {
			case LogOffMode.SignUpOfEnv:
				return RedirectToAction("Index", "Wizard", new { Area = "Customer" });

			case LogOffMode.LogOnOfEnv:
				return RedirectToAction("LogOn", "Account", new { Area = "" });

			default:
				CustomerOrigin uiOrigin = UiCustomerOrigin.Get();
				return Redirect(uiOrigin.FrontendSite);
			} // switch
		} // LogOff

		public ActionResult LogOffUnderwriter() {
			EndSession("LogOff UW");

			return RedirectToAction("Index", "Customers", new { Area = "Underwriter" });
		} // LogOffUnderwriter

		[Ajax]
		[HttpGet]
		public JsonResult GetPropertyStatuses() {
			PropertyStatusesActionResult propertyStatusesActionResult = m_oServiceClient.Instance.GetPropertyStatuses();
			return Json(propertyStatusesActionResult.Groups, JsonRequestBehavior.AllowGet);
		} // GetPropertyStatuses

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[CaptchaValidationFilter(Order = 999999)]
		public JsonResult SignUp(
			User model,
			string FirstName,
			string Surname,
			string signupPass1,
			string signupPass2,
			string securityQuestion,
			string promoCode,
			double? amount,
			string mobilePhone,
			string mobileCode,
			string isInCaptchaMode,
			int whiteLabelId
		) {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			if (model.SecurityAnswer.Length > 199)
				throw new Exception("Maximum answer length is 199 characters");

			CustomerOrigin uiOrigin = UiCustomerOrigin.Get();

			if (uiOrigin.IsAlibaba() && string.IsNullOrWhiteSpace(GetCookie("alibaba_id"))) {
				return Json(new {
					success = false,
					errorMessage = "No Alibaba customer id provided.",
				}, JsonRequestBehavior.AllowGet);
			} // if

			try {
				if (string.IsNullOrEmpty(model.EMail))
					throw new Exception(DbStrings.NotValidEmailAddress);

				if (!string.Equals(signupPass1, signupPass2))
					throw new Exception(DbStrings.PasswordDoesNotMatch);

				var maxPassLength = CurrentValues.Instance.PasswordPolicyType.Value == "hard" ? 7 : 6;

				if (signupPass1.Length < maxPassLength)
					throw new Exception(DbStrings.NotValidEmailAddress);

				bool mobilePhoneVerified = false;

				if (isInCaptchaMode != "True") {
					mobilePhoneVerified = m_oServiceClient.Instance.ValidateMobileCode(mobilePhone, mobileCode).Value;

					if (!mobilePhoneVerified)
						throw new Exception("Invalid code.");
				} // if

				MembershipCreateStatus status = CreateUser(model.EMail, signupPass1, securityQuestion, model.SecurityAnswer);

				if (status == MembershipCreateStatus.DuplicateEmail)
					throw new Exception("This email is already registered.");

				if (status != MembershipCreateStatus.Success)
					throw new Exception("Failed to create user.");

				Customer customer = null;

				var blm = new WizardBrokerLeadModel(Session);

				new Transactional(
					() => customer = CreateCustomer(
						model.EMail,
						FirstName,
						Surname,
						promoCode,
						amount,
						mobilePhone,
						mobilePhoneVerified,
						blm.BrokerFillsForCustomer,
						whiteLabelId
					)
				).Execute();

				string token = m_oServiceClient.Instance.EmailConfirmationGenerate(customer.Id).Token.ToString();

				if (blm.IsSet) {
					m_oServiceClient.Instance.BrokerLeadAcquireCustomer(
						customer.Id,
						blm.LeadID,
						blm.FirstName,
						blm.BrokerFillsForCustomer,
						token
					);
				} else {
					m_oServiceClient.Instance.BrokerCheckCustomerRelevance(
						customer.Id,
						customer.Name,
						customer.IsAlibaba,
						customer.ReferenceSource,
						token
					); // Add is Alibaba, or do after saving it to DB
				} // if

				m_oServiceClient.Instance.SalesForceAddUpdateLeadAccount(
					customer.Id,
					customer.Name,
					customer.Id,
					false,
					false
				);

				FormsAuthentication.SetAuthCookie(model.EMail, false);
				HttpContext.User = new GenericPrincipal(new GenericIdentity(model.EMail), new[] { "Customer" });

				return Json(new {
					success = true,
					antiforgery_token = AntiForgery.GetHtml().ToString(),
					refNumber = customer.RefNumber
				}, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				if (e.Message == MembershipCreateStatus.DuplicateEmail.ToString()) {
					return Json(new {
						success = false,
						errorMessage = DbStrings.EmailAddressAlreadyExists
					}, JsonRequestBehavior.AllowGet);
				} // if

				return Json(new { success = false, errorMessage = e.Message }, JsonRequestBehavior.AllowGet);
			} // try
		} // SignUp

		public ActionResult ForgotPassword() {
			ViewData["CaptchaMode"] = CurrentValues.Instance.CaptchaMode.Value;
			return View("ForgotPassword");
		} // ForgotPassword

		[CaptchaValidationFilter(Order = 999999)]
		[Ajax]
		public JsonResult QuestionForEmail(string email) {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			try {
				if (m_oBrokerHelper.IsBroker(email))
					return Json(new { broker = true }, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				ms_oLog.Warn(
					e,
					"Failed to check whether the email '{0}' is a broker email, continuing as a customer.",
					email
				);
			} // try

			var user = m_oUsers.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email);

			CustomerOrigin uiOrigin = UiCustomerOrigin.Get();

			if (user != null) {
				var customer = m_oCustomers.ReallyTryGet(user.Id);

				if (customer != null) {
					if (customer.CustomerOrigin.GetOrigin() == uiOrigin.GetOrigin()) {
						return Json(new {
							question = user.SecurityQuestion != null ? user.SecurityQuestion.Name : ""
						}, JsonRequestBehavior.AllowGet);
					} // if

					ms_oLog.Warn(
						"Customer {0} {1} tried to restore password from another origin {2} (at {3})",
						customer.Id,
						customer.CustomerOrigin.Name,
						uiOrigin.Name,
						hostname
					);

					return Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet);
				}
			} // if

			if (uiOrigin.IsEverline()) {
				var loginLoanChecker = new EverlineLoginLoanChecker();
				var status = loginLoanChecker.GetLoginStatus(email);

				switch (status.status) {
				case EverlineLoanStatus.Error:
					ms_oLog.Error("Failed to retrieve Everline customer loan status \n{0}", status.Message);
					return Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet);

				case EverlineLoanStatus.ExistsWithCurrentLiveLoan:
					ms_oLog.Warn("Customer {0} ExistsWithCurrentLiveLoan in Everiline tried to restore password", email);
					return Json(new { everlineAccount = true }, JsonRequestBehavior.AllowGet);

				case EverlineLoanStatus.ExistsWithNoLiveLoan:
					ms_oLog.Warn("Customer {0} ExistsWithNoLiveLoan in Everiline tried to restore password", email);
					TempData["IsEverline"] = true;
					TempData["CustomerEmail"] = email;
					return Json(new { everlineWizard = true }, JsonRequestBehavior.AllowGet);

				case EverlineLoanStatus.DoesNotExist:
					ms_oLog.Warn("Customer {0} DoesNotExist in Everiline tried to restore password", email);
					return Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet);

				default:
					ms_oLog.Alert("Unsupported EverlineLoanStatus: {0}.", status.status);
					return Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet);
				} // switch
			} // if

			return Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet);

		} // QuestionForEmail

		[Transactional]
		public JsonResult RestorePassword(string email = "", string answer = "") {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			bool userNotFound =
				string.IsNullOrEmpty(email) ||
					m_oUsers.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email) == null;

			if (userNotFound)
				throw new UserNotFoundException(string.Format("User {0} not found", email));

			if (string.IsNullOrEmpty(answer))
				throw new EmptyAnswerExeption("Answer is empty");

			var user = m_oUsers.GetAll().FirstOrDefault(x =>
				(x.EMail == email || x.Name == email) && (x.SecurityAnswer == answer)
			);

			if (user == null)
				return Json(new { error = "Wrong answer to secret questions" }, JsonRequestBehavior.AllowGet);

			user = m_oUsers.GetUserByLogin(email);
			m_oServiceClient.Instance.PasswordRestored(user.Id);
			user.IsPasswordRestored = true;

			return Json(new { result = true }, JsonRequestBehavior.AllowGet);
		} // RestorePassword

		public ActionResult SimpleCaptcha() {
			return View("SimpleCaptcha");
		} // SimpleCaptcha

		public ActionResult Recaptcha() {
			return View("Recaptcha");
		} // Recaptcha

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult CheckingCompany(
			string postcode,
			string companyName,
			string filter,
			string refNum,
			int? customerId = null
		) {
			ms_oLog.Debug(
				"CheckingCompany cId:{0}, postcode:{1}, companuName:{2}, filter:{3}, refnum:{4}",
				customerId,
				postcode,
				companyName,
				filter,
				refNum
			);

			var nFilter = TargetResults.LegalStatus.DontCare;

			switch (filter.ToUpper()) {
			case "L":
				nFilter = TargetResults.LegalStatus.Limited;
				break;

			case "N":
				nFilter = TargetResults.LegalStatus.NonLimited;
				break;
			} // switch

			try {
				var service = new EBusinessService(m_oDB);
				if (customerId == null && m_oContext.Customer == null) {
					throw new Exception("Customer id is not provided");
				}
				if (customerId == null) {
					customerId = m_oContext.Customer.Id;
				}
				TargetResults result = service.TargetBusiness(companyName, postcode, customerId.Value, nFilter, refNum);
				return Json(result.Targets, JsonRequestBehavior.AllowGet);
			} catch (WebException we) {
				ms_oLog.Debug(we, "WebException caught while executing company targeting.");
				var res = new List<CompanyInfo> { new CompanyInfo { BusName = "", BusRefNum = "exception" } };
				return Json(res, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				if (companyName.ToLower() == "asd" && postcode.ToLower() == "ab10 1ba")
					return Json(GenerateFakeTargetingData(companyName, postcode), JsonRequestBehavior.AllowGet);

				ms_oLog.Alert(e, "Target Business failed.");
				throw;
			} // try
		} // CheckingCompany

		[Ajax]
		[HttpPost]
		public bool GenerateMobileCode(string mobilePhone) {
			return m_oServiceClient.Instance.GenerateMobileCode(mobilePhone).Value;
		} // GenerateMobileCode

		[HttpPost]
		public JsonResult GetTwilioConfig() {
			WizardConfigsActionResult wizardConfigsActionResult = m_oServiceClient.Instance.GetWizardConfigs();

			ms_oLog.Msg(
				"Mobile code visibility related values are: IsSmsValidationActive:{0} NumberOfMobileCodeAttempts:{1}",
				wizardConfigsActionResult.IsSmsValidationActive,
				wizardConfigsActionResult.NumberOfMobileCodeAttempts
			);

			return Json(new {
				isSmsValidationActive = wizardConfigsActionResult.IsSmsValidationActive,
				numberOfMobileCodeAttempts = wizardConfigsActionResult.NumberOfMobileCodeAttempts
			}, JsonRequestBehavior.AllowGet);
		} // GetTwilioConfig

		[IsSuccessfullyRegisteredFilter]
		[NoCache]
		public ActionResult LeadCreatePassword(string sToken, string sFirstName, string sLastName, string sEmail) {
			var oModel = new CreatePasswordModel {
				RawToken = sToken,
				FirstName = sFirstName,
				LastName = sLastName,
				UserName = sEmail,
				BrokerLeadStr = "yes",
			};

			return View("CreatePassword", oModel);
		} // LeadCreatePassword

		[IsSuccessfullyRegisteredFilter]
		[NoCache]
		public ActionResult CreatePassword(string token) {
			var oModel = new CreatePasswordModel {
				RawToken = token,
			};

			if (oModel.IsTokenValid) {
				ms_oLog.Debug("AccountController.CreatePassword: token received {0}.", token);

				try {
					CustomerDetailsActionResult ar =
						m_oServiceClient.Instance.LoadCustomerByCreatePasswordToken(oModel.Token);

					if (ar.Value.CustomerID > 0) {
						oModel.FirstName = ar.Value.FirstName;
						oModel.LastName = ar.Value.LastName;
						oModel.UserName = ar.Value.Email;

						ms_oLog.Debug(
							"AccountController.CreatePassword: token received {0} -> user {1} ({2}, {3}).",
							token,
							oModel.FirstName,
							ar.Value.CustomerID,
							oModel.UserName
						);

						return View(oModel);
					} // if

					ms_oLog.Debug("AccountController.CreatePassword: token received {0} -> no user found.", token);
				} catch (Exception e) {
					ms_oLog.Alert(e, "Failed to check create password token '{0}'.", token);
				} // try
			} else
				ms_oLog.Warn("AccountController.CreatePassword: invalid token received {0}.", token);

			return RedirectToAction("LogOn", "Account", new { Area = "" });
		} // CreatePassword

		[HttpPost]
		[NoCache]
		public JsonResult CustomerCreatePassword(CreatePasswordModel model) {
			var customerIp = RemoteIp();

			if (!ModelState.IsValid) {
				ms_oLog.Debug(
					"Customer create password attempt from remote IP {0}: model state is invalid, list of errors:",
					customerIp
				);

				foreach (var val in ModelState.Values) {
					if (val.Errors.Count < 1)
						continue;

					foreach (var err in val.Errors)
						ms_oLog.Debug("Model value '{0}' with error '{1}'.", val.Value, err.ErrorMessage);
				} // for each value

				ms_oLog.Debug("End of list of errors.");

				return Json(new {
					success = false,
					errorMessage = "Failed to set a password.",
				}, JsonRequestBehavior.AllowGet);
			} // if

			ms_oLog.Debug(
				"Customer create password attempt from remote IP {0} received with user name '{1}' and hash '{2}'...",
				customerIp,
				model.UserName,
				Ezbob.Utils.Security.SecurityUtils.HashPassword(model.UserName, model.Password)
			);

			int nUserID;

			try {
				if (string.IsNullOrEmpty(model.UserName))
					throw new Exception(DbStrings.NotValidEmailAddress);

				if (!string.Equals(model.Password, model.signupPass2))
					throw new Exception(DbStrings.PasswordDoesNotMatch);

				var maxPassLength = CurrentValues.Instance.PasswordPolicyType.Value == "hard" ? 7 : 6;
				if (model.Password.Length < maxPassLength)
					throw new Exception(DbStrings.NotValidEmailAddress);

				nUserID = m_oServiceClient.Instance.SetCustomerPasswordByToken(
					model.UserName,
					new Password(model.Password),
					model.Token,
					model.IsBrokerLead
				).Value;
			} catch (Exception e) {
				ms_oLog.Warn(e, "Failed to retrieve a user by name '{0}'.", model.UserName);
				return Json(new {
					success = false,
					errorMessage = "Failed to set a password.",
				}, JsonRequestBehavior.AllowGet);
			} // try

			if (nUserID <= 0) {
				ms_oLog.Warn("Failed to set a password (returned user id is 0) for user name {0}.", model.UserName);
				return Json(new {
					success = false,
					errorMessage = "Failed to set a password.",
				}, JsonRequestBehavior.AllowGet);
			} // if

			try {
				if (m_oBrokerHelper.IsBroker(model.UserName)) {
					BrokerHelper.SetAuth(model.UserName);

					return Json(new {
						success = true,
						errorMessage = string.Empty,
						broker = true,
					});
				} // if is broker
			} catch (Exception e) {
				ms_oLog.Warn(
					e,
					"Failed to check whether '{0}' is a broker login, continuing as a customer.",
					model.UserName
				);
			} // try

			Customer customer;

			try {
				customer = m_oCustomers.Get(nUserID);
			} catch (Exception e) {
				ms_oLog.Warn(e, "Failed to retrieve a customer by id {0}.", nUserID);
				return Json(new {
					success = false,
					errorMessage = "Failed to set a password."
				}, JsonRequestBehavior.AllowGet);
			} // try

			if (customer.CollectionStatus.CurrentStatus.Name == "Disabled") {
				CustomerOrigin uiOrigin = UiCustomerOrigin.Get();

				string sDisabledError =
					"This account is closed, please contact <span class='bold'>ezbob</span> customer care<br/> " +
					uiOrigin.CustomerCareEmail;

				var session = new CustomerSession {
					CustomerId = nUserID,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false,
					ErrorMessage = sDisabledError,
				};
				m_oSessionIpLog.AddSessionIpLog(session);
				Session["UserSessionId"] = session.Id;

				ms_oLog.Warn(
					"Customer log on attempt from remote IP {0} with user name '{1}': the customer is disabled.",
					customerIp,
					model.UserName
				);

				return Json(new { success = false, errorMessage = sDisabledError, }, JsonRequestBehavior.AllowGet);
			} // if user is disabled

			model.SetCookie(LogOnModel.Roles.Customer);
			return Json(new { success = true, broker = false, errorMessage = string.Empty }, JsonRequestBehavior.AllowGet);
		} // CustomerCreatePassword

        [ValidateJsonAntiForgeryToken]
		[Ajax]
        [HttpPost]
        public JsonResult Iovation(string blackbox, string origin) {
            var ip = RemoteIp();
            var customer = this.m_oContext.Customer;
            if (customer == null) {
                ms_oLog.Info("Iovation black box {0} ip {1} customer is null", blackbox, ip);
                return Json(new { });
            }

            var request = new IovationCheckModel {
                CustomerID = customer.Id,
                AccountCode = customer.RefNumber,
                BeginBlackBox = blackbox,
                Email = customer.Name,
                EndUserIp = ip,
                MobilePhoneNumber = customer.PersonalInfo != null ? customer.PersonalInfo.MobilePhone : string.Empty,
                Origin = origin,
                Type = "application",
                mobilePhoneVerified = customer.PersonalInfo != null && customer.PersonalInfo.MobilePhoneVerified,
                mobilePhoneSmsEnabled = customer.PersonalInfo != null && customer.PersonalInfo.MobilePhoneVerified
            };

            if (customer.WizardStep.TheLastOne) {
                var address = customer.AddressInfo.PersonalAddress.FirstOrDefault();
                request.MoreData = new IovationCheckMoreDataModel {
                    BillingCity = address == null ? "" : address.Town,
                    BillingPostalCode = address == null ? "" : address.Postcode,
                    BillingCountry = address == null ? "" : address.Country,
                    BillingStreet = address == null ? "" : address.Line1,
                    FirstName = customer.PersonalInfo == null ? "" : customer.PersonalInfo.FirstName,
                    LastName = customer.PersonalInfo == null ? "" : customer.PersonalInfo.Surname,
                    HomePhoneNumber = customer.PersonalInfo == null ? "" : customer.PersonalInfo.DaytimePhone,
                    EmailVerified = EmailConfirmationState.IsVerified(customer),
                };
            }

            this.m_oServiceClient.Instance.IovationCheck(request);

            ms_oLog.Info("Iovation black box {0} ip {1} customer {2}, origin {3}", blackbox, ip, customer.Id, origin);
            return Json(new {});
        }

		private MembershipCreateStatus CreateUser(
			string email,
			string password,
			string passwordQuestion,
			string passwordAnswer
		) {
			ms_oLog.Debug("Creating a user '{0}'...", email);

			MembershipCreateStatus status;

			try {
				UserLoginActionResult ular = m_oServiceClient.Instance.CustomerSignup(
					email,
					new Password(password),
					Convert.ToInt32(passwordQuestion),
					passwordAnswer,
					RemoteIp()
				);

				ObjectFactory.GetInstance<IEzbobWorkplaceContext>().SessionId =
					ular.SessionID.ToString(CultureInfo.InvariantCulture);

				status = (MembershipCreateStatus)Enum.Parse(typeof(MembershipCreateStatus), ular.Status);
			} catch (Exception e) {
				ms_oLog.Error(e, "Failed to create user '{0}'.", email);
				throw;
			} // try

			ms_oLog.Debug("User '{0}' has been created.", email);
			return status;
		} // CreateUser

		private Customer CreateCustomer(
			string email,
			string sFirstName,
			string sLastName,
			string promoCode,
			double? amount,
			string mobilePhone,
			bool mobilePhoneVerified,
			bool brokerFillsForCustomer,
			int whiteLabelId
		) {
			var user = m_oUsers.GetUserByLogin(email);
			var g = new RefNumberGenerator(m_oCustomers);
			var isAutomaticTest = IsAutomaticTest(email);
			var vip = m_oVipRequestRepository.RequestedVip(email);
			var whiteLabel = whiteLabelId != 0
				? _whiteLabelProviderRepository.GetAll().FirstOrDefault(x => x.Id == whiteLabelId)
				: null;

			Broker broker = null;

			if (whiteLabel != null) {
				var brokerRepo = ObjectFactory.GetInstance<BrokerRepository>();
				broker = brokerRepo.GetAll().FirstOrDefault(x => x.WhiteLabel == whiteLabel);
			} // if

			sFirstName = (sFirstName ?? string.Empty).Trim();
			sLastName = (sLastName ?? string.Empty).Trim();

			if (sFirstName == string.Empty)
				sFirstName = null;

			if (sLastName == string.Empty)
				sLastName = null;

			var customer = new Customer {
				Name = email,
				Id = user.Id,
				Status = Status.Registered,
				RefNumber = g.GenerateForCustomer(),
				WizardStep = m_oDatabaseHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.SignUp),
				CollectionStatus = new CollectionStatus {
					CurrentStatus = m_oCustomerStatusesRepository.Get((int)CollectionStatusNames.Enabled)
				},
				IsTest = isAutomaticTest,
				IsOffline = null,
				PromoCode = promoCode,
				CustomerInviteFriend = new List<CustomerInviteFriend>(),
				PersonalInfo = new PersonalInfo {
					MobilePhone = mobilePhone,
					MobilePhoneVerified = mobilePhoneVerified,
					FirstName = sFirstName,
					Surname = sLastName,
				},
				TrustPilotStatus = m_oDatabaseHelper.TrustPilotStatusRepository.Find(TrustPilotStauses.Neither),
				GreetingMailSentDate = DateTime.UtcNow,
				Vip = vip,
				WhiteLabel = whiteLabel,
				Broker = broker,
			};

			customer.CustomerOrigin = UiCustomerOrigin.Get();

			ms_oLog.Debug("Customer ({0}): wizard step has been updated to: {1}", customer.Id, (int)WizardStepType.SignUp);
			CampaignSourceRef campaignSourceRef = null;

			if (brokerFillsForCustomer) {
				customer.ReferenceSource = "Broker";
				customer.GoogleCookie = string.Empty;
			} else {
				customer.GoogleCookie = GetAndRemoveCookie("__utmz");
				customer.ReferenceSource = GetAndRemoveCookie("sourceref");
				customer.AlibabaId = GetAndRemoveCookie("alibaba_id");
				customer.IsAlibaba = !string.IsNullOrWhiteSpace(customer.AlibabaId);

				campaignSourceRef = new CampaignSourceRef();
				campaignSourceRef.FContent = GetAndRemoveCookie("fcontent");
				campaignSourceRef.FMedium = GetAndRemoveCookie("fmedium");
				campaignSourceRef.FName = GetAndRemoveCookie("fname");
				campaignSourceRef.FSource = GetAndRemoveCookie("fsource");
				campaignSourceRef.FTerm = GetAndRemoveCookie("fterm");
				campaignSourceRef.FUrl = GetAndRemoveCookie("furl");
				campaignSourceRef.FDate = ToDate(GetAndRemoveCookie("fdate"));
				campaignSourceRef.RContent = GetAndRemoveCookie("rcontent");
				campaignSourceRef.RMedium = GetAndRemoveCookie("rmedium");
				campaignSourceRef.RName = GetAndRemoveCookie("rname");
				campaignSourceRef.RSource = GetAndRemoveCookie("rsource");
				campaignSourceRef.RTerm = GetAndRemoveCookie("rterm");
				campaignSourceRef.RUrl = GetAndRemoveCookie("rurl");
				campaignSourceRef.RDate = ToDate(GetAndRemoveCookie("rdate"));
			} // if

			var customerInviteFriend = new CustomerInviteFriend(customer) {
				InvitedByFriendSource = GetAndRemoveCookie("invite"),
			};
			customer.CustomerInviteFriend.Add(customerInviteFriend);

			customer.ABTesting = GetAndRemoveCookie("ezbobab");
			string visitTimes = GetAndRemoveCookie("sourceref_time");
			customer.FirstVisitTime = HttpUtility.UrlDecode(visitTimes);

			if (Request.Cookies["istest"] != null)
				customer.IsTest = true;

			m_oCustomers.Save(customer);

			customer.CustomerRequestedLoan = new List<CustomerRequestedLoan> { new CustomerRequestedLoan {
				Customer = customer,
				Amount = amount,
				Created = DateTime.UtcNow,
			}};

			var session = new CustomerSession {
				CustomerId = user.Id,
				StartSession = DateTime.UtcNow,
				Ip = RemoteIp(),
				IsPasswdOk = true,
				ErrorMessage = "Registration"
			};
			m_oSessionIpLog.AddSessionIpLog(session);
			Session["UserSessionId"] = session.Id;
			try {
				m_oServiceClient.Instance.SaveSourceRefHistory(
					user.Id,
					customer.ReferenceSource,
					visitTimes,
					campaignSourceRef
				);
			} catch (Exception e) {
				ms_oLog.Warn(e, "Failed to save sourceref history.");
			} // try

			// save AlibabaBuyer
			if (customer.AlibabaId != null && customer.IsAlibaba) {
				try {
					AlibabaBuyer alibabaMember = new AlibabaBuyer();
					alibabaMember.AliId = Convert.ToInt64(customer.AlibabaId);
					alibabaMember.Customer = customer;
					EZBob.DatabaseLib.Model.Alibaba.AlibabaBuyerRepository aliMemberRep = ObjectFactory.GetInstance<AlibabaBuyerRepository>();
					aliMemberRep.SaveOrUpdate(alibabaMember);
				} catch (Exception alieException) {
					ms_oLog.Error(alieException, "Failed to save alibabaMember ID");
				}
			}

			return customer;
		} // CreateCustomer

		private DateTime? ToDate(string dateStr) {
			if (string.IsNullOrEmpty(dateStr))
				return null;

			DateTime date;

			bool bSuccess = DateTime.TryParseExact(
				dateStr,
				"dd/MM/yyyy HH:mm:ss",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
				out date
			);

			return bSuccess ? date : (DateTime?)null;
		} // ToDate

		private string GetCookie(string cookieName) {
			var reqCookie = Request.Cookies[cookieName];

			return reqCookie != null ? HttpUtility.UrlDecode(reqCookie.Value) : null;
		} // GetCookie

		private string GetAndRemoveCookie(string cookieName) {
			var reqCookie = Request.Cookies[cookieName];

			if (reqCookie != null) {
				var cookie = new HttpCookie(cookieName, "") {
					Expires = DateTime.Now.AddMonths(-1),
					HttpOnly = true,
					Secure = true,
				};
				Response.Cookies.Add(cookie);
				return HttpUtility.UrlDecode(reqCookie.Value);
			} // if

			return null;
		} // GetAndRemoveCookie

		private MembershipCreateStatus ValidateUser(
			string username,
			string password,
			string promotionName,
			DateTime? promotionPageVisitTime,
			out string error
		) {
			ms_oLog.Debug("Validating user '{0}' password...", username);

			int nSessionID;
			MembershipCreateStatus nStatus;
			error = null;
			try {
				UserLoginActionResult ular = m_oServiceClient.Instance.UserLogin(
					username,
					new Password(password),
					RemoteIp(),
					promotionName,
					promotionPageVisitTime
				);

				nSessionID = ular.SessionID;
				nStatus = (MembershipCreateStatus)Enum.Parse(typeof(MembershipCreateStatus), ular.Status);
				error = ular.ErrorMessage;
			} catch (Exception e) {
				ms_oLog.Error(e, "Failed to validate user '{0}' credentials.", username);
				return MembershipCreateStatus.ProviderError;
			} // try

			if (nStatus == MembershipCreateStatus.Success) {
				ObjectFactory.GetInstance<IEzbobWorkplaceContext>().SessionId =
					nSessionID.ToString(CultureInfo.InvariantCulture);

				ms_oLog.Debug("User '{0}' password has been validated.", username);
			} // if
			else
				ms_oLog.Debug("User '{0}' password has NOT been validated.", username);

			return nStatus;
		} // ValidateUser

		private bool IsAutomaticTest(string email) {
			bool isAutomaticTest = false;

			if (CurrentValues.Instance.AutomaticTestCustomerMark == "1") {
				var patterns = m_oTestCustomers.GetAllPatterns();
				if (patterns.Any(email.Contains))
					isAutomaticTest = true;
			} // if

			return isAutomaticTest;
		} // IsAutomaticTest

		private static List<CompanyInfo> GenerateFakeTargetingData(string companyName, string postcode) {
			var data = new List<CompanyInfo>();

			for (var i = 0; i < 10; i++) {
				data.Add(new CompanyInfo {
					AddrLine1 = "AddrLine1" + " for company " + i,
					AddrLine2 = "AddrLine2" + " for company " + i,
					AddrLine3 = "AddrLine3" + " for company " + i,
					AddrLine4 = "AddrLine4" + " for company " + i,
					BusName = "BusName" + " for company " + i,
					BusRefNum = "BusRefNum" + " for company " + i,
					BusinessStatus = "BusinessStatus" + " for company " + i,
					LegalStatus = "LegalStatus" + " for company " + i,
					MatchScore = "MatchScore" + " for company " + i,
					MatchedBusName = companyName + " for company " + i,
					MatchedBusNameType = "MatchedBusNameType" + " for company " + i,
					PostCode = postcode + " for company " + i,
					SicCode = "SicCode" + " for company " + i,
					SicCodeDesc = "SicCodeDesc" + " for company " + i,
					SicCodeType = "SicCodeType" + " for company " + i
				});
			}

			return data;
		} // GenerateFakeTargetingData

		private JsonResult GetModelStateErrors(ModelStateDictionary modelStateDictionary) {
			return Json(
				new {
					result = false,
					errorMessage = string.Join("; ",
						modelStateDictionary.Values
							.SelectMany(x => x.Errors)
							.Select(x => x.ErrorMessage)
					)
				}, JsonRequestBehavior.AllowGet);
		} // GetModelStateErrors

		private enum LogOffMode {
			[UsedImplicitly]
			WebProd = 0,

			LogOnOfEnv = 1,

			SignUpOfEnv = 2
		} // enum LogOffMode

		private string RemoteIp() {
			string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (string.IsNullOrEmpty(ip))
				ip = Request.ServerVariables["REMOTE_ADDR"];

			return ip;
		} // RemoteIp

		private void EndSession(string comment) {
			if (!string.IsNullOrWhiteSpace(m_oContext.SessionId)) {
				int nSessionID;

				if (int.TryParse(m_oContext.SessionId, out nSessionID)) {
					try {
						if (nSessionID > 0) {
							m_oServiceClient.Instance.MarkSessionEnded(
								nSessionID,
								comment,
								m_oContext.Customer != null ? m_oContext.Customer.Id : (int?)null
							);
						} // if
					} catch (Exception e) {
						ms_oLog.Debug(e, "Failed to mark customer session as ended.");
					} // try
				} // if
			} // if

			m_oContext.SessionId = null;

			FormsAuthentication.SignOut();
			HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
		} // EndSession

		private void FillPromotionData(LogOnModel model, string scratchPromotion) {
			model.PromotionName = null;
			model.PromotionPageVisitTime = null;

			scratchPromotion = (scratchPromotion ?? string.Empty).Trim();

			ms_oLog.Debug("Scratch promotion data: '{0}'.", scratchPromotion);

			if (string.IsNullOrWhiteSpace(scratchPromotion))
				return;

			string[] tokens = scratchPromotion.Trim().Split(';');

			DateTime ppvt = DateTime.UtcNow;

			bool hasGoodTokens = true;

			if (tokens.Length != 2) {
				hasGoodTokens = false;
				ms_oLog.Debug("Scratch promotion has {0} tokens, while 2 expected.", tokens.Length);
			} // if

			if (string.IsNullOrWhiteSpace(tokens[0])) {
				hasGoodTokens = false;
				ms_oLog.Debug("Scratch promotion name is empty.");
			} // if

			if (!DateTime.TryParseExact(
				tokens[1],
				"yyyy-M-d_H-m-s",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal,
				out ppvt
			)) {
				hasGoodTokens = false;
				ms_oLog.Debug("Failed to parse page visit time using pattern 'yyyy-M-d_H-m-s' from {0}.", tokens[1]);
			} // if

			if (hasGoodTokens) {
				model.PromotionName = tokens[0];
				model.PromotionPageVisitTime = ppvt;
			} // if
		} // FillPromotionData

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(AccountController));

		private readonly IUsersRepository m_oUsers;
		private readonly CustomerRepository m_oCustomers;
		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly ICustomerSessionsRepository m_oSessionIpLog;
		private readonly ITestCustomerRepository m_oTestCustomers;
		private readonly ICustomerStatusesRepository m_oCustomerStatusesRepository;
		private readonly DatabaseDataHelper m_oDatabaseHelper;
		private readonly BrokerHelper m_oBrokerHelper;
		private readonly LogOffMode m_oLogOffMode;
		private readonly IVipRequestRepository m_oVipRequestRepository;
		private readonly AConnection m_oDB;
		private readonly WhiteLabelProviderRepository _whiteLabelProviderRepository;
		private string hostname;
	} // class AccountController
} // namespace
