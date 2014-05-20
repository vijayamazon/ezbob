namespace EzBob.Web.Controllers {
	#region using

	using System;
	using System.Collections.Generic;
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
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib.Ebusiness;
	using Code;
	using Code.Email;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
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
	using ActionResult = System.Web.Mvc.ActionResult;

	#endregion using

	public class AccountController : Controller {
		#region public

		#region constructor

		public AccountController() {
			m_oDatabaseHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			m_oMembershipProvider = ObjectFactory.GetInstance<MembershipProvider>();
			m_oUsers = ObjectFactory.GetInstance<IUsersRepository>();
			m_oCustomers = ObjectFactory.GetInstance<CustomerRepository>();
			m_oServiceClient = new ServiceClient();
			m_oContext = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();
			m_oConfirmation = ObjectFactory.GetInstance<IEmailConfirmation>();
			m_oSessionIpLog = ObjectFactory.GetInstance<ICustomerSessionsRepository>();
			m_oTestCustomers = ObjectFactory.GetInstance<ITestCustomerRepository>();
			m_oCustomerStatusesRepository = ObjectFactory.GetInstance<ICustomerStatusesRepository>();
			m_oBrokerHelper = new BrokerHelper();
			m_oSessionRepository = ObjectFactory.GetInstance<SessionRepository>();
			m_oLogOffMode = (LogOffMode)(int)ConfigManager.CurrentValues.Instance.LogOffMode;
			m_oVipRequestRepository = ObjectFactory.GetInstance<IVipRequestRepository>();
		} // constructor

		#endregion constructor

		#region action AdminLogOn

		public ActionResult AdminLogOn(string returnUrl) {
			ViewData["returnUrl"] = returnUrl;
			return View(new LogOnModel { ReturnUrl = returnUrl });
		} // AdminLogOn

		[HttpPost]
		public ActionResult AdminLogOn(LogOnModel model) {
			if (ModelState.IsValid) {
				var user = m_oUsers.GetUserByLogin(model.UserName);

				try {
					if (m_oMembershipProvider.ValidateUser(model.UserName, model.Password)) {
						user.LoginFailedCount = 0;

						model.SetCookie("Underwriter");

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
				}
				catch (UserNotFoundException ex) {
					ms_oLog.Warn(ex, "Failed to log in as underwriter '{0}'.", model.UserName);
				} // try
			} // if

			ModelState.AddModelError("", "Wrong user name/password.");
			return View(model);
		} // AdminLogOn

		#endregion action AdminLogOn

		#region action LogOn

		[IsSuccessfullyRegisteredFilter]
		public ActionResult LogOn(string returnUrl) {
			return View(new LogOnModel { ReturnUrl = returnUrl });
		} // LogOn

		[HttpPost]
		[NoCache]
		public JsonResult CustomerLogOn(LogOnModel model) {
			var customerIp = RemoteIp();

			if (!ModelState.IsValid) {
				ms_oLog.Debug("Customer log on attempt from remote IP {0}: model state is invalid.", customerIp);
				return Json(new { success = false, errorMessage = @"User not found or incorrect password." }, JsonRequestBehavior.AllowGet);
			} // if

			ms_oLog.Debug(
				"Customer log on attempt from remote IP {0} received with user name '{1}' and hash '{2}'...",
				customerIp,
				model.UserName,
				Ezbob.Utils.Security.SecurityUtils.HashPassword(model.UserName, model.Password)
			);

			try {
				if (m_oBrokerHelper.IsBroker(model.UserName)) {
					BrokerProperties bp = m_oBrokerHelper.TryLogin(model.UserName, model.Password);

					return Json(new {
						success = (bp != null),
						errorMessage = (bp == null) ? "User not found or incorrect password." : string.Empty,
						broker = true,
					});
				} // if is broker
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to check whether '{0}' is a broker login, continuing as a customer.", model.UserName);
			} // try

			var user = m_oUsers.GetUserByLogin(model.UserName);

			if (user == null) {
				ms_oLog.Warn(
					"Customer log on attempt from remote IP {0} with user name '{1}': could not find a user entry.",
					customerIp,
					model.UserName
				);

				return Json(new { success = false, errorMessage = @"User not found or incorrect password." }, JsonRequestBehavior.AllowGet);
			} // if user not found

			var isUnderwriter = user.Roles.Any(r => r.Id == 31 || r.Id == 32 || r.Id == 33);

			ms_oLog.Debug("{1} log on attempt with login '{0}'.", model.UserName, isUnderwriter ? "Underwriter" : "Customer");

			ms_oLog.Debug(
				"{2} log on attempt from remote IP {0} with user name '{1}': log on attempt #{3}.",
				customerIp,
				model.UserName,
				isUnderwriter ? "Underwriter" : "Customer",
				(user.LoginFailedCount ?? 0) + 1
			);

			if (isUnderwriter) {
				ms_oLog.Debug(
					"Underwriter log on attempt from remote IP {0} with user name '{1}': failed, underwriters should use a dedicated log on page.",
					customerIp,
					model.UserName
				);

				return Json(new { success = false, errorMessage = "Please use dedicated underwriter log on page." }, JsonRequestBehavior.AllowGet);
			} // if

// ReSharper disable RedundantAssignment
			string errorMessage = null;
// ReSharper restore RedundantAssignment

			var customer = m_oCustomers.Get(user.Id);

			if (customer.CollectionStatus.CurrentStatus.Name == "Disabled") {
				errorMessage = @"This account is closed, please contact <span class='bold'>ezbob</span> customer care<br/> customercare@ezbob.com";

				m_oSessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false,
					ErrorMessage = errorMessage
				});

				ms_oLog.Warn(
					"Customer log on attempt from remote IP {0} with user name '{1}': the customer is disabled.",
					customerIp,
					model.UserName
				);

				return Json(new { success = false, errorMessage }, JsonRequestBehavior.AllowGet);
			} // if user is disabled

			if (m_oMembershipProvider.ValidateUser(model.UserName, model.Password)) {
				m_oSessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = true
				});

				user.LoginFailedCount = 0;
				model.SetCookie("Customer");

				ms_oLog.Debug(
					"Customer log on attempt from remote IP {0} with user name '{1}': success.",
					customerIp,
					model.UserName
				);

				return Json(new { success = true, model, }, JsonRequestBehavior.AllowGet);
			} // if logged in successfully

			if (user.LoginFailedCount < 3) {
				m_oSessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false
				});

				errorMessage = @"User not found or incorrect password.";
			} // if login failed count is still ok
			else {
				m_oSessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false,
					ErrorMessage = "More than 3 unsuccessful login attempts have been made. Resetting user password.",
				});

				var password = m_oMembershipProvider.ResetPassword(user.EMail, "");
				m_oServiceClient.Instance.ThreeInvalidAttempts(user.Id, password);
				user.IsPasswordRestored = true;
				user.LoginFailedCount = 0;

				errorMessage = @"Three unsuccessful login attempts have been made. <span class='bold'>ezbob</span> has issued you with a temporary password. Please check your e-mail.";
			} // if

			ms_oLog.Warn(
				"Customer log on attempt from remote IP {0} with user name '{1}': failed {2}.",
				customerIp,
				model.UserName,
				errorMessage
			);

			// If we got this far, something failed, redisplay form
			return Json(new { success = false, errorMessage }, JsonRequestBehavior.AllowGet);
		} // CustomerLogOn

		#endregion action LogOn

		#region action LogOff

		public ActionResult LogOff(bool isUnderwriterPage = false) {
			SecuritySession securitySession = m_oSessionRepository.Get(m_oContext.SessionId);

			if (securitySession != null) {
				securitySession.State = 0;
				m_oSessionRepository.SaveOrUpdate(securitySession);
			} // if

			m_oContext.SessionId = null;
			FormsAuthentication.SignOut();
			HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

			if (isUnderwriterPage)
				return RedirectToAction("Index", "Customers", new { Area = "Underwriter" });

			switch (m_oLogOffMode) {
			case LogOffMode.SignUpOfEnv:
				return RedirectToAction("Index", "Wizard", new { Area = "Customer" });
			case LogOffMode.LogOnOfEnv:
				return RedirectToAction("LogOn", "Account", new { Area = "" });
			default:
				return Redirect(@"http://www.ezbob.com");
			} // switch
		} // LogOff

		#endregion action LogOff

		#region action SignUp

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[CaptchaValidationFilter(Order = 999999)]
		public JsonResult SignUp(
			User model,
			string signupPass1,
			string signupPass2,
			string securityQuestion,
			string promoCode,
			double? amount,
			string mobilePhone,
			string mobileCode,
			string isInCaptchaMode
		) {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			if (model.SecurityAnswer.Length > 199)
				throw new Exception("Maximum answer length is 199 characters");

			try {
				SignUpInternal(model.EMail, signupPass1, signupPass2, securityQuestion, model.SecurityAnswer, promoCode, amount, mobilePhone, mobileCode, isInCaptchaMode == "True");

				FormsAuthentication.SetAuthCookie(model.EMail, false);
				HttpContext.User = new GenericPrincipal(new GenericIdentity(model.EMail), new[] { "Customer" });

				var user = m_oUsers.GetUserByLogin(model.EMail);

				m_oSessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = RemoteIp(),
					IsPasswdOk = true,
					ErrorMessage = "Registration"
				});

				return Json(new { success = true, antiforgery_token = AntiForgery.GetHtml().ToString() }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				if (e.Message == MembershipCreateStatus.DuplicateEmail.ToString())
					return Json(new { success = false, errorMessage = DbStrings.EmailAddressAlreadyExists }, JsonRequestBehavior.AllowGet);

				return Json(new { success = false, errorMessage = e.Message }, JsonRequestBehavior.AllowGet);
			} // try
		} // SignUp

		#endregion action SignUp

		#region action ForgotPassword

		public ActionResult ForgotPassword() {
			ViewData["CaptchaMode"] = CurrentValues.Instance.CaptchaMode.Value;
			return View("ForgotPassword");
		} // ForgotPassword

		#endregion action ForgotPassword

		#region action QuestionForEmail

		[CaptchaValidationFilter(Order = 999999)]
		[Ajax]
		public JsonResult QuestionForEmail(string email) {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			try {
				if (m_oBrokerHelper.IsBroker(email))
					return Json(new { broker = true }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to check whether the email '{0}' is a broker email, continuing as a customer.", email);
			} // try

			var user = m_oUsers.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email);

			return user == null ?
				Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet) :
				Json(new { question = user.SecurityQuestion != null ? user.SecurityQuestion.Name : "" }, JsonRequestBehavior.AllowGet);
		} // QuestionForEmail

		#endregion action QuestionForEmail

		#region action RestorePassword

		[Transactional]
		public JsonResult RestorePassword(string email = "", string answer = "") {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			if (m_oUsers.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email) == null || string.IsNullOrEmpty(email))
				throw new UserNotFoundException(string.Format("User {0} not found", email));

			if (string.IsNullOrEmpty(answer))
				throw new EmptyAnswerExeption("Answer is empty");

			var user = m_oUsers.GetAll().FirstOrDefault(x => (x.EMail == email || x.Name == email) && (x.SecurityAnswer == answer));

			if (user == null)
				return Json(new { error = "Wrong answer to secret questions" }, JsonRequestBehavior.AllowGet);

			var password = m_oMembershipProvider.ResetPassword(email, "");

			user = m_oUsers.GetUserByLogin(email);
			m_oServiceClient.Instance.PasswordRestored(user.Id, password);
			user.IsPasswordRestored = true;

			return Json(new { result = true }, JsonRequestBehavior.AllowGet);
		} // RestorePassword

		#endregion action RestorePassword

		#region action Captcha

		public ActionResult SimpleCaptcha() {
			return View("SimpleCaptcha");
		} // SimpleCaptcha

		public ActionResult Recaptcha() {
			return View("Recaptcha");
		} // Recaptcha

		#endregion action Captcha

		#region action CheckingCompany

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Authorize(Roles = "Underwriter, Web")]
		public JsonResult CheckingCompany(string postcode, string companyName, string filter, string refNum) {
			var nFilter = TargetResults.LegalStatus.DontCare;

			switch (filter.ToUpper()) {
			case "L":
				nFilter = TargetResults.LegalStatus.Limited;
				break;

			case "N":
				nFilter = TargetResults.LegalStatus.NonLimited;
				break;
			} // switch

			var result = new TargetResults(null);
			try {
				var service = new EBusinessService();

				result = service.TargetBusiness(companyName, postcode, m_oContext.UserId, nFilter, refNum);

				if (result.Targets.Any()) {
					foreach (var t in result.Targets) {
						t.BusName = string.IsNullOrEmpty(t.BusName)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.BusName.ToLower());
						t.AddrLine1 = string.IsNullOrEmpty(t.AddrLine1)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine1.ToLower());
						t.AddrLine2 = string.IsNullOrEmpty(t.AddrLine2)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine2.ToLower());
						t.AddrLine3 = string.IsNullOrEmpty(t.AddrLine3)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine3.ToLower());
						t.AddrLine4 = string.IsNullOrEmpty(t.AddrLine4)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine4.ToLower());
					} // for each

					if (result.Targets.Count > 1)
						result.Targets.Add(new CompanyInfo { BusName = "Company not found", BusRefNum = "skip" });
				} // if

				return Json(result.Targets, JsonRequestBehavior.AllowGet);
			}
			catch (WebException we) {
				ms_oLog.Debug(we, "WebException caught while executing company targeting.");
				result.Targets.Add(new CompanyInfo { BusName = "", BusRefNum = "exception" });
				return Json(result.Targets, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				if (companyName.ToLower() == "asd" && postcode.ToLower() == "ab10 1ba")
					return Json(GenerateFakeTargetingData(companyName, postcode), JsonRequestBehavior.AllowGet);

				ms_oLog.Alert(e, "Target Business failed.");
				throw;
			} // try
		} // CheckingCompany

		#endregion action CheckingCompany

		#region action GenerateMobileCode

		[Ajax]
		[HttpPost]
		public bool GenerateMobileCode(string mobilePhone) {
			return m_oServiceClient.Instance.GenerateMobileCode(mobilePhone).Value;
		} // GenerateMobileCode

		#endregion action GenerateMobileCode

		#region action GetTwilioConfig

		[HttpPost]
		public JsonResult GetTwilioConfig() {
			WizardConfigsActionResult wizardConfigsActionResult = m_oServiceClient.Instance.GetWizardConfigs();

			ms_oLog.Msg("Mobile code visibility related values are: IsSmsValidationActive:{0} NumberOfMobileCodeAttempts:{1}",
				wizardConfigsActionResult.IsSmsValidationActive, wizardConfigsActionResult.NumberOfMobileCodeAttempts);

			return Json(new {
				isSmsValidationActive = wizardConfigsActionResult.IsSmsValidationActive,
				numberOfMobileCodeAttempts = wizardConfigsActionResult.NumberOfMobileCodeAttempts
			}, JsonRequestBehavior.AllowGet);
		} // GetTwilioConfig

		#endregion action GetTwilioConfig

		#endregion public

		#region private

		#region method SignUpInternal

		// ReSharper disable UnusedParameter.Local
		private void SignUpInternal(
			string email,
			string signupPass1,
			string signupPass2,
			string securityQuestion,
			string securityAnswer,
			string promoCode,
			double? amount,
			string mobilePhone,
			string mobileCode,
			bool isInCaptchaMode
		) {
			MembershipCreateStatus status;

			if (string.IsNullOrEmpty(email))
				throw new Exception(DbStrings.NotValidEmailAddress);

			if (!string.Equals(signupPass1, signupPass2))
				throw new Exception(DbStrings.PasswordDoesNotMatch);

			var maxPassLength = CurrentValues.Instance.PasswordPolicyType.Value == "hard" ? 7 : 6;
			if (signupPass1.Length < maxPassLength)
				throw new Exception(DbStrings.NotValidEmailAddress);

			if (!isInCaptchaMode) {
				bool isCorrect = m_oServiceClient.Instance.ValidateMobileCode(mobilePhone, mobileCode).Value;
				if (!isCorrect)
					throw new Exception("Invalid code");
			} // if

			m_oMembershipProvider.CreateUser(email, signupPass1, email, securityQuestion, securityAnswer, false, null, out status);

			if (status == MembershipCreateStatus.Success) {
				var user = m_oUsers.GetUserByLogin(email);
				var g = new RefNumberGenerator(m_oCustomers);
				var isAutomaticTest = IsAutomaticTest(email);
				var vip = m_oVipRequestRepository.RequestedVip(email);

				var customer = new Customer {
					Name = email,
					Id = user.Id,
					Status = Status.Registered,
					RefNumber = g.GenerateForCustomer(),
					WizardStep = m_oDatabaseHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.SignUp),
					CollectionStatus = new CollectionStatus { CurrentStatus = m_oCustomerStatusesRepository.GetByName("Enabled") },
					IsTest = isAutomaticTest,
					IsOffline = null,
					PromoCode = promoCode,
					CustomerInviteFriend = new List<CustomerInviteFriend>(),
					PersonalInfo = new PersonalInfo { MobilePhone = mobilePhone, },
					TrustPilotStatus = m_oDatabaseHelper.TrustPilotStatusRepository.Find(TrustPilotStauses.Nether),
					GreetingMailSentDate = DateTime.UtcNow,
					Vip = vip
				};

				ms_oLog.Debug("Customer ({0}): wizard step has been updated to: {1}", customer.Id, (int)WizardStepType.SignUp);

				var sourceref = Request.Cookies["sourceref"];
				if (sourceref != null) {
					var cookie = new HttpCookie("sourceref", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customer.ReferenceSource = sourceref.Value;
				} // if

				var googleCookie = Request.Cookies["__utmz"];
				if (googleCookie != null) {
					var cookie = new HttpCookie("__utmz", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customer.GoogleCookie = googleCookie.Value;
				} // if

				var customerInviteFriend = new CustomerInviteFriend(customer);
				var inviteFriend = Request.Cookies["invite"];
				if (inviteFriend != null) {
					var cookie = new HttpCookie("inviteFriend", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customerInviteFriend.InvitedByFriendSource = inviteFriend.Value;
				} // if

				customer.CustomerInviteFriend.Add(customerInviteFriend);
				var ezbobab = Request.Cookies["ezbobab"];
				if (ezbobab != null) {
					var cookie = new HttpCookie("ezbobab", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customer.ABTesting = ezbobab.Value;
				} // if

				if (Request.Cookies["istest"] != null)
					customer.IsTest = true;

				m_oCustomers.Save(customer);
				string link = m_oConfirmation.GenerateLink(customer);

				customer.CustomerRequestedLoan = new List<CustomerRequestedLoan> { new CustomerRequestedLoan {
					Customer = customer,
					Amount = amount,
					Created = DateTime.UtcNow,
				}};

				var blm = new WizardBrokerLeadModel(Session);

				if (blm.IsSet)
					m_oServiceClient.Instance.BrokerLeadAcquireCustomer(customer.Id, blm.LeadID, blm.BrokerFillsForCustomer ? string.Empty : link);
				else
					m_oServiceClient.Instance.BrokerCheckCustomerRelevance(customer.Id, customer.Name, customer.ReferenceSource, link);
			} // if user created successfully 

			if (status == MembershipCreateStatus.DuplicateEmail)
				throw new Exception("This email is already registered");
		} // SignupInternal
		// ReSharper restore UnusedParameter.Local

		#endregion method SignUpInternal

		#region method IsAutomaticTest

		private bool IsAutomaticTest(string email) {
			bool isAutomaticTest = false;

			if (CurrentValues.Instance.AutomaticTestCustomerMark == "1") {
				var patterns = m_oTestCustomers.GetAllPatterns();
				if (patterns.Any(email.Contains))
					isAutomaticTest = true;
			} // if

			return isAutomaticTest;
		} // IsAutomaticTest

		#endregion method IsAutomaticTest

		#region method GenerateFakeTargetingData

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

		#endregion method GenerateFakeTargetingData

		#region method GetModelStateErrors

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

		#endregion method GetModelStateErrors

		#region enum LogOffMode {

		private enum LogOffMode {
			[UsedImplicitly]
			WebProd = 0,

			LogOnOfEnv = 1,

			SignUpOfEnv = 2
		} // enum LogOffMode

		#endregion enum LogOffMode {

		#region method RemoteIp

		private string RemoteIp() {
			string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (string.IsNullOrEmpty(ip))
				ip = Request.ServerVariables["REMOTE_ADDR"];

			return ip;
		} // RemoteIp

		#endregion method RemoteIp

		#region fields

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(AccountController));

		private readonly MembershipProvider m_oMembershipProvider;
		private readonly IUsersRepository m_oUsers;
		private readonly CustomerRepository m_oCustomers;
		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly IEmailConfirmation m_oConfirmation;
		private readonly ICustomerSessionsRepository m_oSessionIpLog;
		private readonly ITestCustomerRepository m_oTestCustomers;
		private readonly ICustomerStatusesRepository m_oCustomerStatusesRepository;
		private readonly DatabaseDataHelper m_oDatabaseHelper;
		private readonly BrokerHelper m_oBrokerHelper;
		private readonly SessionRepository m_oSessionRepository;
		private readonly LogOffMode m_oLogOffMode;
		private readonly IVipRequestRepository m_oVipRequestRepository;

		#endregion fields

		#endregion private
	} // class AccountController
} // namespace
