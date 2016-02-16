namespace EzBob.Web.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Globalization;
	using System.Linq;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Helpers;
	using System.Web.Mvc;
	using System.Web.Security;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Code;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Security;
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

	using ActionResult = System.Web.Mvc.ActionResult;

	public class AccountController : Controller {
		public AccountController() {
			this.cookiesToRemoveOnSignup = new SortedSet<string>();
			this.userRepo = ObjectFactory.GetInstance<IUsersRepository>();
			this.serviceClient = new ServiceClient();
			this.context = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();
			this.brokerHelper = new BrokerHelper();
			this.logOffMode = (LogOffMode)(int)CurrentValues.Instance.LogOffMode;
		} // constructor

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
			if (!bool.Parse(ConfigurationManager.AppSettings["UnderwriterEnabled"]))
				return RedirectToAction("LogOn", "Account");

			if (ModelState.IsValid) {
				if (this.userRepo.ExternalUserCount(model.UserName) > 0) {
					log.Alert("External user '{0}' tried to log in as an underwriter!", model.UserName);
					ModelState.AddModelError("LoginError", "Wrong user name/password.");
					return View(model);
				} // if

				string loginError;
				var membershipCreateStatus = ValidateUser(
					null,
					model.UserName,
					model.Password,
					null,
					null,
					out loginError
				);

				if (MembershipCreateStatus.Success == membershipCreateStatus) {
					model.SetCookie(LogOnModel.Roles.Underwriter);

					this.context.SetSessionOrigin(null);

					bool bRedirectToUrl =
						Url.IsLocalUrl(model.ReturnUrl) &&
						(model.ReturnUrl.Length > 1) &&
						model.ReturnUrl.StartsWith("/") &&
						!model.ReturnUrl.StartsWith("//") &&
						!model.ReturnUrl.StartsWith("/\\");

					if (bRedirectToUrl)
						return Redirect(model.ReturnUrl);

					if (this.context.UserPermissions.Any(x => x.Name == "UnderwriterDashboard")) {
						return RedirectToAction("Index", "Customers", new { Area = "Underwriter" });
					} else {
						return RedirectToAction("Main", "SalesForce", new { Area = "Underwriter" });
					}
				} // if

				loginError = string.IsNullOrEmpty(loginError) ? "Wrong user name/password." : loginError;
				ModelState.AddModelError("LoginError", loginError);
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
			string customerIp = RemoteIp();
			CustomerOriginEnum origin = UiCustomerOrigin.Get().GetOrigin();

			if (!ModelState.IsValid) {
				log.Debug(
					"Customer log on attempt from remote IP {0} to origin '{1}': model state is invalid, list of errors:",
					customerIp,
					origin
				);

				foreach (var val in ModelState.Values) {
					if (val.Errors.Count < 1)
						continue;

					foreach (var err in val.Errors)
						log.Debug("Model value '{0}' with error '{1}'.", val.Value, err.ErrorMessage);
				} // for each value

				log.Debug("End of list of errors.");

				return Json(new {
					success = false,
					errorMessage = "User not found or incorrect password."
				}, JsonRequestBehavior.AllowGet);
			} // if

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			log.Debug(
				"Customer log on attempt from remote IP {0} received " +
				"with user name '{1}' and hash '{2}' (promotion: {3})...",
				customerIp,
				model.UserName,
				pu.Generate(model.UserName, model.Password),
				model.PromotionDisplayData
			);

			try {
				if (this.brokerHelper.IsBroker(model.UserName)) {
					BrokerProperties bp = this.brokerHelper.TryLogin(
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
				log.Warn(
					e,
					"Failed to check whether '{0}' is a broker login at origin '{1}', continuing as a customer.",
					model.UserName,
					origin
				);
			} // try

			var loginModel = new LoginCustomerMultiOriginModel {
				UserName = model.UserName,
				Origin = origin,
				Password = new DasKennwort(model.Password),
				PromotionName = model.PromotionName,
				PromotionPageVisitTime = model.PromotionPageVisitTime,
				RemoteIp = customerIp,
			};

			UserLoginActionResult ular = this.serviceClient.Instance.LoginCustomerMutliOrigin(loginModel);

			if (MembershipCreateStatus.Success.ToString() == ular.Status) {
				model.SetCookie(LogOnModel.Roles.Customer);
				this.context.SetSessionOrigin(origin);
				return Json(new { success = true, model, }, JsonRequestBehavior.AllowGet);
			} // if

			// If we got this far, something failed, redisplay form
			return Json(new { success = false, errorMessage = ular.ErrorMessage }, JsonRequestBehavior.AllowGet);
		} // CustomerLogOn

		public ActionResult LogOff() {
			EndSession("LogOff customer", true);
			this.context.RemoveSessionOrigin();

			switch (this.logOffMode) {
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
			EndSession("LogOff UW", false);
			this.context.RemoveSessionOrigin();

			return RedirectToAction("AdminLogOn", "Account");
		} // LogOffUnderwriter

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
			string mobilePhone,
			string mobileCode,
			string isInCaptchaMode,
			int whiteLabelId
		) {
			string id = Guid.NewGuid().ToString("N");
			const int idChunkSize = 4;

			string uniqueID = string.Join("-",
				Enumerable.Range(0, id.Length / idChunkSize).Select(i => id.Substring(i * idChunkSize, idChunkSize))
			);

			log.Debug("Sign up client attempt id: '{0}'...", uniqueID);

			this.cookiesToRemoveOnSignup.Clear();

			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			if (model.SecurityAnswer.Length > 199)
				throw new Exception(DbStrings.MaximumAnswerLengthExceeded);

			CustomerOrigin uiOrigin = UiCustomerOrigin.Get();

			string alibabaID = GetAndRemoveCookie("alibaba_id");

			if (uiOrigin.IsAlibaba() && string.IsNullOrWhiteSpace(alibabaID)) {
				return Json(new {
					success = false,
					errorMessage = "No Alibaba customer id provided.",
				}, JsonRequestBehavior.AllowGet);
			} // if

			var blm = new WizardBrokerLeadModel(Session);

			CampaignSourceRef campaignSourceRef = null;

			if (!blm.BrokerFillsForCustomer) {
				campaignSourceRef = new CampaignSourceRef {
					FContent = GetAndRemoveCookie("fcontent"),
					FMedium = GetAndRemoveCookie("fmedium"),
					FName = GetAndRemoveCookie("fname"),
					FSource = GetAndRemoveCookie("fsource"),
					FTerm = GetAndRemoveCookie("fterm"),
					FUrl = GetAndRemoveCookie("furl"),
					FDate = ToDate(GetAndRemoveCookie("fdate")),
					RContent = GetAndRemoveCookie("rcontent"),
					RMedium = GetAndRemoveCookie("rmedium"),
					RName = GetAndRemoveCookie("rname"),
					RSource = GetAndRemoveCookie("rsource"),
					RTerm = GetAndRemoveCookie("rterm"),
					RUrl = GetAndRemoveCookie("rurl"),
					RDate = ToDate(GetAndRemoveCookie("rdate")),
				};
			} // if

			string visitTimes = GetAndRemoveCookie("sourceref_time");

			var signupModel = new SignupCustomerMultiOriginModel {
				UserName = model.EMail,
				Origin = uiOrigin.GetOrigin(),
				RawPassword = new DasKennwort(signupPass1),
				RawPasswordAgain = new DasKennwort(signupPass2),
				PasswordQuestion = Convert.ToInt32(securityQuestion),
				PasswordAnswer = model.SecurityAnswer,
				RemoteIp = RemoteIp(),
				FirstName = FirstName,
				LastName = Surname,
				CaptchaMode = isInCaptchaMode == "True",
				MobilePhone = mobilePhone,
				MobileVerificationCode = mobileCode,
				BrokerFillsForCustomer = blm.BrokerFillsForCustomer,
				WhiteLabelID = whiteLabelId,
				IsTest = (Request.Cookies["istest"] != null) ? true : (bool?)null,
				CampaignSourceRef = campaignSourceRef,
				GoogleCookie = blm.BrokerFillsForCustomer ? string.Empty : GetAndRemoveCookie("__utmz"),
				ReferenceSource = blm.BrokerFillsForCustomer ? "Broker" : GetAndRemoveCookie("sourceref"),
				AlibabaID = blm.BrokerFillsForCustomer ? null : GetAndRemoveCookie("alibaba_id"),
				ABTesting = GetAndRemoveCookie("ezbobab"),
				VisitTimes = visitTimes,
				FirstVisitTime = HttpUtility.UrlDecode(visitTimes),
				RequestedLoanAmount = GetAndRemoveCookie("loan_amount"),
				RequestedLoanTerm = GetAndRemoveCookie("loan_period"),
				BrokerLeadID = blm.LeadID,
				BrokerLeadEmail = blm.LeadEmail,
				BrokerLeadFirstName = blm.FirstName,
			};

			log.Debug(
				"Sign up client attempt id: '{0}', model is {1}.",
				uniqueID,
				signupModel.ToLogStr()
			);

			try {
				log.Debug("Sign up client attempt id: '{0}', requesting backend sign up.", uniqueID);

				UserLoginActionResult signupResult = this.serviceClient.Instance.SignupCustomerMultiOrigin(signupModel);

				log.Debug("Sign up client attempt id: '{0}', backend sign up complete.", uniqueID);

				MembershipCreateStatus status = (MembershipCreateStatus)Enum.Parse(
					typeof(MembershipCreateStatus),
					signupResult.Status
				);

				log.Debug("Sign up client attempt id: '{0}', status is {1}.", uniqueID, status);

				if (status == MembershipCreateStatus.DuplicateEmail) {
					return Json(
						new {
							success = false,
							errorMessage = signupResult.ErrorMessage,
						},
						JsonRequestBehavior.AllowGet
					);
				} // if

				if ((status != MembershipCreateStatus.Success) || !string.IsNullOrWhiteSpace(signupResult.ErrorMessage)) {
					throw new Exception(string.IsNullOrWhiteSpace(signupResult.ErrorMessage)
						? string.Format("Failed to sign up (error code is '{0}').", uniqueID)
						: signupResult.ErrorMessage
					);
				} // if

				ObjectFactory.GetInstance<IEzbobWorkplaceContext>().SessionId =
					signupResult.SessionID.ToString(CultureInfo.InvariantCulture);

				Session["UserSessionId"] = signupResult.SessionID;

				this.context.SetSessionOrigin(uiOrigin.GetOrigin());
				FormsAuthentication.SetAuthCookie(model.EMail, false);
				HttpContext.User = new GenericPrincipal(new GenericIdentity(model.EMail), new[] { "Customer" });

				RemoveCookiesOnSignup();

				log.Debug("Sign up client attempt id: '{0}', sign up complete.", uniqueID);

				return Json(
					new {
						success = true,
						antiforgery_token = AntiForgery.GetHtml().ToString(),
						refNumber = signupResult.RefNumber,
					},
					JsonRequestBehavior.AllowGet
				);
			} catch (Exception e) {
				log.Alert(e, "Failed to sign up, client attempt id: {0}.", uniqueID);

				return Json(
					new {
						success = false,
						errorMessage = string.Format(
							"Failed to sign up, please call support (error code is '{0}').",
							uniqueID
						),
					},
					JsonRequestBehavior.AllowGet
				);
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
				if (this.brokerHelper.IsBroker(email))
					return Json(new { broker = true }, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				log.Warn(
					e,
					"Failed to check whether the email '{0}' is a broker email, continuing as a customer.",
					email
				);
			} // try

			CustomerOrigin uiOrigin = UiCustomerOrigin.Get();

			try {
				StringActionResult sar = this.serviceClient.Instance.GetCustomerSecurityQuestion(
					email,
					uiOrigin.GetOrigin()
				);

				if (string.IsNullOrWhiteSpace(sar.Value)) {
					return Json(new {
						error = "Security question not found for user " + email,
					}, JsonRequestBehavior.AllowGet);
				} // if

				return Json(new { question = sar.Value, }, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				log.Alert(
					e,
					"Failed to detect security question for customer '{0}' with origin '{1}'.",
					email,
					uiOrigin.GetOrigin()
				);
				return Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet);
			} // try
		} // QuestionForEmail

		[Transactional]
		public JsonResult RestorePassword(string email = "", string answer = "") {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			if (string.IsNullOrWhiteSpace(answer))
				throw new EmptyAnswerExeption("Answer is empty.");

			CustomerOrigin uiOrigin = UiCustomerOrigin.Get();

			try {
				StringActionResult sar = this.serviceClient.Instance.ValidateSecurityAnswer(
					email,
					uiOrigin.GetOrigin(),
					answer
				);

				if (string.IsNullOrWhiteSpace(sar.Value))
					return Json(new { result = true }, JsonRequestBehavior.AllowGet);

				return Json(new { error = "Wrong answer to secret question." }, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				log.Alert(
					e,
					"Failed to validate an answer to security question for customer '{0}' with origin '{1}'.",
					email,
					uiOrigin.GetOrigin()
				);
				
				return Json(new { error = "Wrong answer to secret questions" }, JsonRequestBehavior.AllowGet);
			} // try
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
			log.Debug(
				"CheckingCompany cId:{0}, postcode:{1}, companuName:{2}, filter:{3}, refnum:{4}",
				customerId,
				postcode,
				companyName,
				filter,
				refNum
			);

			if (customerId == null && this.context.Customer == null)
				throw new Exception(DbStrings.CustomeIdNotProvided);

			if (customerId == null)
				customerId = this.context.Customer.Id;

			ExperianTargetingActionResult response = this.serviceClient.Instance.ExperianTarget(
				customerId.Value,
				this.context.UserId,
				new ExperianTargetingRequest {
					CompanyName = companyName,
					CustomerID = customerId.Value,
					Filter = filter,
					Postcode = postcode,
					RefNum = refNum
				});

			return Json(response.CompanyInfos, JsonRequestBehavior.AllowGet);
		} // CheckingCompany

		[Ajax]
		[HttpPost]
		public bool GenerateMobileCode(string mobilePhone) {
			return this.serviceClient.Instance.GenerateMobileCode(mobilePhone).Value;
		} // GenerateMobileCode

		[HttpPost]
		public JsonResult GetTwilioConfig() {
			WizardConfigsActionResult wizardConfigsActionResult = this.serviceClient.Instance.GetWizardConfigs();

			log.Msg(
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
				log.Debug("AccountController.CreatePassword: token received {0}.", token);

				try {
					CustomerDetailsActionResult ar =
						this.serviceClient.Instance.LoadCustomerByCreatePasswordToken(oModel.Token);

					if (ar.Value.CustomerID > 0) {
						oModel.FirstName = ar.Value.FirstName;
						oModel.LastName = ar.Value.LastName;
						oModel.UserName = ar.Value.Email;

						log.Debug(
							"AccountController.CreatePassword: token received {0} -> user {1} ({2}, {3}).",
							token,
							oModel.FirstName,
							ar.Value.CustomerID,
							oModel.UserName
						);

						return View(oModel);
					} // if

					log.Debug("AccountController.CreatePassword: token received {0} -> no user found.", token);
				} catch (Exception e) {
					log.Alert(e, "Failed to check create password token '{0}'.", token);
				} // try
			} else
				log.Warn("AccountController.CreatePassword: invalid token received {0}.", token);

			return RedirectToAction("LogOn", "Account", new { Area = "" });
		} // CreatePassword

		[HttpPost]
		[NoCache]
		public JsonResult CustomerCreatePassword(CreatePasswordModel model) {
			var customerIp = RemoteIp();

			if (!ModelState.IsValid) {
				log.Debug(
					"Customer create password attempt from remote IP {0}: model state is invalid, list of errors:",
					customerIp
				);

				foreach (var val in ModelState.Values) {
					if (val.Errors.Count < 1)
						continue;

					foreach (var err in val.Errors)
						log.Debug("Model value '{0}' with error '{1}'.", val.Value, err.ErrorMessage);
				} // for each value

				log.Debug("End of list of errors.");

				return Json(new {
					success = false,
					errorMessage = "Failed to set a password.",
				}, JsonRequestBehavior.AllowGet);
			} // if

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			log.Debug(
				"Customer create password attempt from remote IP {0} received: {1}...",
				customerIp,
				pu.Generate(model.UserName, model.Password)
			);

			CustomerOriginEnum origin = UiCustomerOrigin.Get().GetOrigin();
			SetPasswordActionResult spar;

			try {
				spar = this.serviceClient.Instance.SetCustomerPasswordByToken(
					model.Token,
					origin,
					new DasKennwort(model.Password),
					new DasKennwort(model.signupPass2),
					model.IsBrokerLead,
					customerIp
				);
			} catch (Exception e) {
				log.Warn(e, "Failed to set new password for user '{0}'.", model.UserName);

				return Json(new {
					success = false,
					errorMessage = "Failed to set a password.",
				}, JsonRequestBehavior.AllowGet);
			} // try

			if (!string.IsNullOrWhiteSpace(spar.ErrorMsg)) {
				log.Warn(
					"Failed to set a password for user name {0}, error message returned: {1}.",
					model.UserName,
					spar.ErrorMsg
				);

				return Json(new {
					success = false,
					errorMessage = spar.ErrorMsg,
				}, JsonRequestBehavior.AllowGet);
			} // if

			if (spar.UserID <= 0) {
				log.Warn("Failed to set a password (returned user id is 0) for user name {0}.", model.UserName);

				return Json(new {
					success = false,
					errorMessage = "Failed to set a password.",
				}, JsonRequestBehavior.AllowGet);
			} // if

			if (spar.IsBroker) {
				BrokerHelper.SetAuth(spar.Email);

				return Json(new {
					success = true,
					errorMessage = string.Empty,
					broker = true,
				});
			} // if is broker

			if (spar.SessionID != 0)
				Session["UserSessionId"] = spar.SessionID;

			if (string.IsNullOrWhiteSpace(spar.ErrorMsg)) {
				model.SetCookie(LogOnModel.Roles.Customer);
				this.context.SetSessionOrigin(origin);
			} // if

			return Json(new {
				success = string.IsNullOrWhiteSpace(spar.ErrorMsg),
				errorMessage = spar.ErrorMsg,
				broker = false,
			}, JsonRequestBehavior.AllowGet);
		} // CustomerCreatePassword

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		public JsonResult Iovation(string blackbox, string origin) {
			var ip = RemoteIp();
			var customer = this.context.Customer;

			if (customer == null) {
				log.Info("Iovation black box {0} ip {1} customer is null", blackbox, ip);
				return Json(new { });
			} // if

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
			} // if

			this.serviceClient.Instance.IovationCheck(request);

			log.Info("Iovation black box {0} ip {1} customer {2}, origin {3}", blackbox, ip, customer.Id, origin);
			return Json(new { });
		} // Iovation

		protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
			bool hasHost =
				(requestContext != null) &&
				(requestContext.HttpContext != null) &&
				(requestContext.HttpContext.Request != null) &&
				(requestContext.HttpContext.Request.Url != null);

			this.hostname = hasHost ? requestContext.HttpContext.Request.Url.Host : string.Empty;
			log.Info("AccountController Initialize {0}", this.hostname);
			base.Initialize(requestContext);
		} // Initialize

		private static DateTime? ToDate(string dateStr) {
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

		private string GetAndRemoveCookie(string cookieName) {
			var reqCookie = Request.Cookies[cookieName];

			if (reqCookie != null) {
				this.cookiesToRemoveOnSignup.Add(cookieName);
				return HttpUtility.UrlDecode(reqCookie.Value);
			} // if

			return null;
		} // GetAndRemoveCookie

		private void RemoveCookiesOnSignup() {
			foreach (string cookieName in this.cookiesToRemoveOnSignup) {
				Response.Cookies.Add(new HttpCookie(cookieName, string.Empty) {
					Expires = DateTime.UtcNow.AddMonths(-1),
					HttpOnly = true,
					Secure = true,
				});
			} // if

			this.cookiesToRemoveOnSignup.Clear();
		} // RemoveCookiesOnSignup

		private readonly SortedSet<string> cookiesToRemoveOnSignup;

		private MembershipCreateStatus ValidateUser(
			CustomerOriginEnum? originID,
			string username,
			string password,
			string promotionName,
			DateTime? promotionPageVisitTime,
			out string error
		) {
			log.Debug(
				"Validating user '{0}' from origin '{1}' password...",
				username,
				originID.HasValue ? originID.Value.ToString() : "-- null --"
			);

			int nSessionID;
			MembershipCreateStatus nStatus;
			error = null;

			try {
				UserLoginActionResult ular = this.serviceClient.Instance.UserLogin(
					originID,
					username,
					new DasKennwort(password),
					RemoteIp(),
					promotionName,
					promotionPageVisitTime
				);

				nSessionID = ular.SessionID;
				nStatus = (MembershipCreateStatus)Enum.Parse(typeof(MembershipCreateStatus), ular.Status);
				error = ular.ErrorMessage;
			} catch (Exception e) {
				log.Alert(
					e,
					"Failed to validate user '{0}' from origin '{1}' credentials.",
					username,
					originID.HasValue ? originID.Value.ToString() : "-- null --"
				);
				return MembershipCreateStatus.ProviderError;
			} // try

			if (nStatus == MembershipCreateStatus.Success) {
				this.context.SessionId = nSessionID.ToString(CultureInfo.InvariantCulture);
				Session["UserSessionId"] = nSessionID;

				log.Debug(
					"User '{0}' from origin '{1}' password has been validated.",
					username,
					originID.HasValue ? originID.Value.ToString() : "-- null --"
				);
			} else {
				log.Debug(
					"User '{0}' from origin '{1}' password has NOT been validated.",
					username,
					originID.HasValue ? originID.Value.ToString() : "-- null --"
				);
			} // if

			return nStatus;
		} // ValidateUser

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

		private void EndSession(string comment, bool lookForCustomer) {
			if (!string.IsNullOrWhiteSpace(this.context.SessionId)) {
				int nSessionID;

				if (int.TryParse(this.context.SessionId, out nSessionID) && (nSessionID > 0)) {
					try {
						int? userID = (this.context.User == null) ? (int?)null : this.context.User.Id;

						int? customerID = null;

						if ((userID != null) && lookForCustomer)
							customerID = (this.context.Customer != null) ? this.context.Customer.Id : (int?)null;

						this.serviceClient.Instance.MarkSessionEnded(nSessionID, comment, userID, customerID);
					} catch (Exception e) {
						log.Debug(e, "Failed to mark customer session as ended.");
					} // try
				} // if
			} // if

			this.context.SessionId = null;

			FormsAuthentication.SignOut();
			HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
		} // EndSession

		private void FillPromotionData(LogOnModel model, string scratchPromotion) {
			model.PromotionName = null;
			model.PromotionPageVisitTime = null;

			scratchPromotion = (scratchPromotion ?? string.Empty).Trim();

			log.Debug("Scratch promotion data: '{0}'.", scratchPromotion);

			if (string.IsNullOrWhiteSpace(scratchPromotion))
				return;

			string[] tokens = scratchPromotion.Trim().Split(';');

			DateTime ppvt = DateTime.UtcNow;

			bool hasGoodTokens = true;

			if (tokens.Length != 2) {
				hasGoodTokens = false;
				log.Debug("Scratch promotion has {0} tokens, while 2 expected.", tokens.Length);
			} // if

			if (string.IsNullOrWhiteSpace(tokens[0])) {
				hasGoodTokens = false;
				log.Debug("Scratch promotion name is empty.");
			} // if

			if (!DateTime.TryParseExact(
				tokens[1],
				"yyyy-M-d_H-m-s",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal,
				out ppvt
			)) {
				hasGoodTokens = false;
				log.Debug("Failed to parse page visit time using pattern 'yyyy-M-d_H-m-s' from {0}.", tokens[1]);
			} // if

			if (hasGoodTokens) {
				model.PromotionName = tokens[0];
				model.PromotionPageVisitTime = ppvt;
			} // if
		} // FillPromotionData

		private static readonly ASafeLog log = new SafeILog(typeof(AccountController));

		private readonly IUsersRepository userRepo;
		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;
		private readonly BrokerHelper brokerHelper;
		private readonly LogOffMode logOffMode;
		private string hostname;
	} // class AccountController
} // namespace
