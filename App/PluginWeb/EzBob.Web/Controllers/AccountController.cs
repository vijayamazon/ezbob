namespace EzBob.Web.Controllers {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Net;
	using System.Web;
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
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.Filters;
	using Infrastructure.Membership;
	using Infrastructure.csrf;
	using Models;
	using Models.Strings;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using log4net;
	using EZBob.DatabaseLib;
	using ActionResult = System.Web.Mvc.ActionResult;

	#endregion using

	public class AccountController : Controller {
		public AccountController() {
			_helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			_membershipProvider = ObjectFactory.GetInstance<MembershipProvider>();
			_users = ObjectFactory.GetInstance<IUsersRepository>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			m_oServiceClient = new ServiceClient();
			_context = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();
			_confirmation = ObjectFactory.GetInstance<IEmailConfirmation>();
			_sessionIpLog = ObjectFactory.GetInstance<ICustomerSessionsRepository>();
			_testCustomers = ObjectFactory.GetInstance<ITestCustomerRepository>();
			_customerStatusesRepository = ObjectFactory.GetInstance<ICustomerStatusesRepository>();
			m_oBrokerHelper = new BrokerHelper();
			sessionRepository = ObjectFactory.GetInstance<SessionRepository>();
			logOffMode = (LogOffMode)(int)ConfigManager.CurrentValues.Instance.LogOffMode;
			_vipRequestRepository = ObjectFactory.GetInstance<IVipRequestRepository>();
		} // constructor

		public ActionResult AdminLogOn(string returnUrl) {
			ViewData["returnUrl"] = returnUrl;
			return View(new LogOnModel { ReturnUrl = returnUrl });
		} // AdminLogOn

		[HttpPost]
		public ActionResult AdminLogOn(LogOnModel model) {
			if (ModelState.IsValid) {
				var user = _users.GetUserByLogin(model.UserName);
				try
				{
					if (_membershipProvider.ValidateUser(model.UserName, model.Password))
					{
						user.LoginFailedCount = 0;
						return SetCookieAndRedirect(model, "Underwriter", "Customers");
					} // if
				}
				catch (UserNotFoundException ex)
				{
					
				}
			} // if

			ModelState.AddModelError("", "Wrong user name/password.");
			return View(model);
		} // AdminLogOn

		[IsSuccessfullyRegisteredFilter]
		public ActionResult LogOn(string returnUrl) {
			return View(new LogOnModel { ReturnUrl = returnUrl });
		} // LogOn

		[HttpPost]
		[NoCache]
		public JsonResult CustomerLogOn(LogOnModel model) {
			var customerIp = RemoteIp();

			if (!ModelState.IsValid) {
				_log.DebugFormat("Customer log on attempt from remote IP {0}: model state is invalid.", customerIp);
				return Json(new { success = false, errorMessage = (string)null }, JsonRequestBehavior.AllowGet);
			} // if

			_log.DebugFormat(
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
						errorMessage = (bp == null) ? "User not found or incorrect password." : "",
						broker = true,
					});
				} // if is broker
			}
			catch (Exception e) {
				_log.Warn("Failed to check whether '" + model.UserName + "' is a broker login, continuing as a customer.", e);
			} // try

			var user = _users.GetUserByLogin(model.UserName);

			if (user == null) {
				_log.WarnFormat(
					"Customer log on attempt from remote IP {0} with user name '{1}': could not find a user entry.",
					customerIp,
					model.UserName
				);

				return Json(new { success = false, errorMessage = @"User not found or incorrect password." }, JsonRequestBehavior.AllowGet);
			} // if user not found

			var isUnderwriter = user.Roles.Any(r => r.Id == 31 || r.Id == 32 || r.Id == 33);

			_log.DebugFormat("{1} log on attempt with login '{0}'.", model.UserName, isUnderwriter ? "Underwriter" : "Customer");

			_log.DebugFormat(
				"{2} log on attempt from remote IP {0} with user name '{1}': log on attempt #{3}.",
				customerIp,
				model.UserName,
				isUnderwriter ? "Underwriter" : "Customer",
				(user.LoginFailedCount ?? 0) + 1
			);

			if (isUnderwriter) {
				_log.DebugFormat(
					"Underwriter log on attempt from remote IP {0} with user name '{1}': failed, underwriters should use a dedicated log on page.",
					customerIp,
					model.UserName
				);

				return Json(new {success = false, errorMessage = "Please use dedicated underwriter log on page."}, JsonRequestBehavior.AllowGet);
			} // if

			string errorMessage = null;

			var customer = _customers.Get(user.Id);

			if (customer.CollectionStatus.CurrentStatus.Name == "Disabled") {
				errorMessage = @"This account is closed, please contact <span class='bold'>ezbob</span> customer care<br/> customercare@ezbob.com";

				_sessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false,
					ErrorMessage = errorMessage
				});

				_log.WarnFormat(
					"Customer log on attempt from remote IP {0} with user name '{1}': the customer is disabled.",
					customerIp,
					model.UserName
				);

				return Json(new { success = false, errorMessage }, JsonRequestBehavior.AllowGet);
			} // if user is disabled

			if (_membershipProvider.ValidateUser(model.UserName, model.Password)) {
				_sessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = true
				});

				user.LoginFailedCount = 0;
				model.SetCookie();

				_log.DebugFormat(
					"Customer log on attempt from remote IP {0} with user name '{1}': success.",
					customerIp,
					model.UserName
				);

				return Json(new { success = true, model }, JsonRequestBehavior.AllowGet);
			} // if logged in successfully

			if (user.LoginFailedCount < 3) {
				_sessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false
				});

				errorMessage = @"User not found or incorrect password.";
			} // if login failed count is still ok
			else {
				_sessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = customerIp,
					IsPasswdOk = false,
					ErrorMessage = "More than 3 unsuccessful login attempts have been made. Resetting user password.",
				});

				var password = _membershipProvider.ResetPassword(user.EMail, "");
				m_oServiceClient.Instance.ThreeInvalidAttempts(user.Id, password);
				user.IsPasswordRestored = true;
				user.LoginFailedCount = 0;

				errorMessage = @"Three unsuccessful login attempts have been made. <span class='bold'>ezbob</span> has issued you with a temporary password. Please check your e-mail.";
			} // if

			_log.WarnFormat(
				"Customer log on attempt from remote IP {0} with user name '{1}': failed {2}.",
				customerIp,
				model.UserName,
				errorMessage
			);

			// If we got this far, something failed, redisplay form
			return Json(new { success = false, errorMessage }, JsonRequestBehavior.AllowGet);
		} // CustomerLogOn

		public ActionResult LogOff(bool isUnderwriterPage = false) {
			SecuritySession securitySession = sessionRepository.Get(_context.SessionId);

			if (securitySession != null) {
				securitySession.State = 0;
				sessionRepository.SaveOrUpdate(securitySession);
			} // if

			_context.SessionId = null;
			FormsAuthentication.SignOut();

			if (isUnderwriterPage)
				return RedirectToAction("Index", "Customers", new { Area = "Underwriter" });

			switch (logOffMode) {
			case LogOffMode.SignUpOfEnv:
				return RedirectToAction("Index", "Wizard", new { Area = "Customer" });
			case LogOffMode.LogOnOfEnv:
				return RedirectToAction("LogOn", "Account", new { Area = "" });
			default:
				return Redirect(@"http://www.ezbob.com");
			} // switch
		} // LogOff

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
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

				var user = _users.GetUserByLogin(model.EMail);

				_sessionIpLog.AddSessionIpLog(new CustomerSession {
					CustomerId = user.Id,
					StartSession = DateTime.Now,
					Ip = RemoteIp(),
					IsPasswdOk = true,
					ErrorMessage = "Registration"
				});

				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				if (e.Message == MembershipCreateStatus.DuplicateEmail.ToString())
					return Json(new { success = false, errorMessage = DbStrings.EmailAddressAlreadyExists }, JsonRequestBehavior.AllowGet);

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
			}
			catch (Exception e) {
				_log.Warn("Failed to check whether the email '" + email + "' is a broker email, continuing as a customer.", e);
			} // try

			var user = _users.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email);

			return user == null ?
				Json(new { error = "User : '" + email + "' was not found" }, JsonRequestBehavior.AllowGet) :
				Json(new { question = user.SecurityQuestion != null ? user.SecurityQuestion.Name : "" }, JsonRequestBehavior.AllowGet);
		} // QuestionForEmail

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult RestorePassword(string email = "", string answer = "") {
			if (!ModelState.IsValid)
				return GetModelStateErrors(ModelState);

			if (_users.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email) == null || string.IsNullOrEmpty(email))
				throw new UserNotFoundException(string.Format("User {0} not found", email));

			if (string.IsNullOrEmpty(answer))
				throw new EmptyAnswerExeption("Answer is empty");

			var user = _users.GetAll().FirstOrDefault(x => (x.EMail == email || x.Name == email) && (x.SecurityAnswer == answer));

			if (user == null)
				return Json(new { error = "Wrong answer to secret questions" }, JsonRequestBehavior.AllowGet);

			var password = _membershipProvider.ResetPassword(email, "");

			user = _users.GetUserByLogin(email);
			m_oServiceClient.Instance.PasswordRestored(user.Id, password);
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

				result = service.TargetBusiness(companyName, postcode, _context.UserId, nFilter, refNum);

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
				result.Targets.Add(new CompanyInfo { BusName = "", BusRefNum = "exception" });
				return Json(result.Targets, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				if (companyName.ToLower() == "asd" && postcode.ToLower() == "ab10 1ba")
					return Json(GenerateFakeTargetingData(companyName, postcode), JsonRequestBehavior.AllowGet);

				_log.Error("Target Bussiness failed", e);
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

			_log.InfoFormat("Mobile code visibility related values are: IsSmsValidationActive:{0} NumberOfMobileCodeAttempts:{1}",
				wizardConfigsActionResult.IsSmsValidationActive, wizardConfigsActionResult.NumberOfMobileCodeAttempts);

			return Json(new {
				isSmsValidationActive = wizardConfigsActionResult.IsSmsValidationActive,
				numberOfMobileCodeAttempts = wizardConfigsActionResult.NumberOfMobileCodeAttempts
			}, JsonRequestBehavior.AllowGet);
		} // GetTwilioConfig

		#region private

		#region method SetCookieAndRedirect

		public ActionResult SetCookieAndRedirect(LogOnModel model, string sArea, string sControllerName) {
			model.SetCookie();

			bool bRedirectToUrl =
				Url.IsLocalUrl(model.ReturnUrl) &&
				(model.ReturnUrl.Length > 1) &&
				model.ReturnUrl.StartsWith("/") &&
				!model.ReturnUrl.StartsWith("//") &&
				!model.ReturnUrl.StartsWith("/\\");

			if (bRedirectToUrl)
				return Redirect(model.ReturnUrl);

			return RedirectToAction("Index", sControllerName, new { Area = sArea });
		} // SetCookieAndRedirect

		#endregion method SetCookieAndRedirect

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

			_membershipProvider.CreateUser(email, signupPass1, email, securityQuestion, securityAnswer, false, null, out status);

			if (status == MembershipCreateStatus.Success) {
				var user = _users.GetUserByLogin(email);
				var g = new RefNumberGenerator(_customers);
				var isAutomaticTest = IsAutomaticTest(email);
				var vip = _vipRequestRepository.RequestedVip(email);

				var customer = new Customer {
					Name = email,
					Id = user.Id,
					Status = Status.Registered,
					RefNumber = g.GenerateForCustomer(),
					WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.SignUp),
					CollectionStatus = new CollectionStatus { CurrentStatus = _customerStatusesRepository.GetByName("Enabled") },
					IsTest = isAutomaticTest,
					IsOffline = (bool?)null,
					PromoCode = promoCode,
					CustomerInviteFriend = new List<CustomerInviteFriend>(),
					PersonalInfo = new PersonalInfo { MobilePhone = mobilePhone, },
					TrustPilotStatus = _helper.TrustPilotStatusRepository.Find(TrustPilotStauses.Nether),
					GreetingMailSentDate = DateTime.UtcNow,
					Vip = vip
				};

				_log.DebugFormat("Customer ({0}): wizard step has been updated to: {1}", customer.Id, (int)WizardStepType.SignUp);

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

				_customers.Save(customer);
				string link = _confirmation.GenerateLink(customer);

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

		private bool IsAutomaticTest(string email) {
			bool isAutomaticTest = false;

			if (CurrentValues.Instance.AutomaticTestCustomerMark == "1") {
				var patterns = _testCustomers.GetAllPatterns();
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
			WebProd = 0,
			LogOnOfEnv = 1,
			SignUpOfEnv = 2
		} // enum LogOffMode

		#region method RemoteIp

		private string RemoteIp() {
			string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (string.IsNullOrEmpty(ip))
				ip = Request.ServerVariables["REMOTE_ADDR"];

			return ip;
		} // RemoteIp

		#endregion method RemoteIp

		private static readonly ILog _log = LogManager.GetLogger(typeof(AccountController));
		private readonly MembershipProvider _membershipProvider;
		private readonly IUsersRepository _users;
		private readonly CustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext _context;
		private readonly IEmailConfirmation _confirmation;
		private readonly ICustomerSessionsRepository _sessionIpLog;
		private readonly ITestCustomerRepository _testCustomers;
		private readonly ICustomerStatusesRepository _customerStatusesRepository;
		private readonly DatabaseDataHelper _helper;
		private readonly BrokerHelper m_oBrokerHelper;
		private readonly SessionRepository sessionRepository;
		private readonly LogOffMode logOffMode;
		private readonly IVipRequestRepository _vipRequestRepository;

		#endregion private
	} // class AccountController
} // namespace
