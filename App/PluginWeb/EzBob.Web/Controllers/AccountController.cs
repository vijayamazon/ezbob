using EZBob.DatabaseLib;

namespace EzBob.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using ApplicationMng.Model;
	using ApplicationMng.Repository;
	using Code.ApplicationCreator;
	using DB.Security;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib.Ebusiness;
	using Code;
	using Code.Email;
	using Infrastructure;
	using Infrastructure.Filters;
	using Infrastructure.Membership;
	using Infrastructure.csrf;
	using Models;
	using Models.Strings;
	using Scorto.Security.UserManagement.Sessions;
	using Scorto.Web;
	using log4net;

	public class AccountController : Controller
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(AccountController));
		private readonly MembershipProvider _membershipProvider;
		private readonly IUsersRepository _users;
		private readonly CustomerRepository _customers;
		private readonly IAppCreator _appCreator;
		private readonly IEzBobConfiguration _config;
		private readonly ISessionManager _sessionManager;
		private readonly IEzbobWorkplaceContext _context;
		private readonly IEmailConfirmation _confirmation;
		private readonly ICustomerSessionsRepository _sessionIpLog;
		private readonly ITestCustomerRepository _testCustomers;
		private readonly IConfigurationVariablesRepository _configurationVariables;
		private readonly ICustomerStatusesRepository _customerStatusesRepository;
		private readonly DatabaseDataHelper _helper;

		private readonly ICustomerReasonRepository _reasons;
		private readonly ICustomerSourceOfRepaymentRepository _sources;

		//------------------------------------------------------------------------
		public AccountController(
			DatabaseDataHelper helper,
			MembershipProvider membershipProvider,
			IUsersRepository users,
			CustomerRepository customers,
			IAppCreator appCreator,
			IEzBobConfiguration config,
			ISessionManager sessionManager,
			IEzbobWorkplaceContext context,
			IEmailConfirmation confirmation,
			ICustomerSessionsRepository sessionIpLog,
			ITestCustomerRepository testCustomers,
			IConfigurationVariablesRepository configurationVariables,
			ICustomerStatusesRepository customerStatusesRepository, ICustomerReasonRepository reasons, ICustomerSourceOfRepaymentRepository sources
		)
		{
			_helper = helper;
			_membershipProvider = membershipProvider;
			_users = users;
			_customers = customers;
			_appCreator = appCreator;
			_config = config;
			_sessionManager = sessionManager;
			_context = context;
			_confirmation = confirmation;
			_sessionIpLog = sessionIpLog;
			_testCustomers = testCustomers;
			_configurationVariables = configurationVariables;
			_customerStatusesRepository = customerStatusesRepository;
			_reasons = reasons;
			_sources = sources;
		}

		//------------------------------------------------------------------------
		[IsSuccessfullyRegisteredFilter]
		public ActionResult LogOn(string returnUrl)
		{
			return View(new LogOnModel { ReturnUrl = returnUrl });
		}
		//------------------------------------------------------------------------
		public ActionResult AdminLogOn(string returnUrl)
		{
			ViewData["returnUrl"] = returnUrl;
			return View(new LogOnModel { ReturnUrl = returnUrl });
		}
		//------------------------------------------------------------------------

		[HttpPost]
		public ActionResult AdminLogOn(LogOnModel model)
		{
			if (ModelState.IsValid)
			{
				var user = _users.GetUserByLogin(model.UserName);

				if (_membershipProvider.ValidateUser(model.UserName, model.Password))
				{
					user.LoginFailedCount = 0;
					return SetCookieAndRedirectAdmin(model);
				}
			}
			ModelState.AddModelError("", "Wrong user name/password.");
			return View(model);
		}


		[HttpPost]
		public ActionResult LogOn(LogOnModel model)
		{
			if (ModelState.IsValid)
			{
				var user = _users.GetUserByLogin(model.UserName);

				if (user == null)
				{
					ModelState.AddModelError("", "User not found or incorrect password.");

				}
				else
				{

					var isUnderwriter = user.Roles.Any(r => r.Id == 31 || r.Id == 32 || r.Id == 33);
					if (!isUnderwriter)
					{
						var customer = _customers.Get(user.Id);
						;
						if (customer.CollectionStatus.CurrentStatus.Name == "Disabled")
						{
							ModelState.AddModelError("",
													 "This account is closed, please contact <span class='bold'>ezbob</span> customer care<br/>customercare@ezbob.com");
							return View(model);
						}
					}

					if (_membershipProvider.ValidateUser(model.UserName, model.Password))
					{
						user.LoginFailedCount = 0;
						return SetCookieAndRedirect(model);
					}
					if (user.LoginFailedCount >= 3)
					{
						_log.InfoFormat("More than 3 unsuccessful login attempts have been made. Resetting user password");
						//RestorePassword(user.EMail, user.SecurityAnswer);

						var password = _membershipProvider.ResetPassword(user.EMail, "");
						_appCreator.ThreeInvalidAttempts(user, user.Name, password);
						user.IsPasswordRestored = true;
						user.LoginFailedCount = 0;

						ModelState.AddModelError("", "Three unsuccessful login attempts have been made. <span class='bold'>ezbob</span> has issued you with a temporary password. Please check your e-mail.");
					}
					ModelState.AddModelError("", "User not found or incorrect password.");
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}
		//------------------------------------------------------------------------
		[HttpPost]
		public JsonNetResult CustomerLogOn(LogOnModel model)
		{

			var customerIp = Request.ServerVariables["REMOTE_ADDR"];


			string errorMessage = null;
			if (ModelState.IsValid)
			{
				var user = _users.GetUserByLogin(model.UserName);

				if (user == null)
				{
					errorMessage = @"User not found or incorrect password.";
				}
				else
				{
					var isUnderwriter = user.Roles.Any(r => r.Id == 31 || r.Id == 32 || r.Id == 33);
					if (!isUnderwriter)
					{
						var customer = _customers.Get(user.Id);
						if (customer.CollectionStatus.CurrentStatus.Name == "Disabled")
						{
							errorMessage = @"This account is closed, please contact <span class='bold'>ezbob</span> customer care<br/> customercare@ezbob.com";
							_sessionIpLog.AddSessionIpLog(new CustomerSession()
							{
								CustomerId = user.Id,
								StartSession = DateTime.Now,
								Ip = customerIp,
								IsPasswdOk = false,
								ErrorMessage = errorMessage
							});
							return this.JsonNet(new { success = false, errorMessage });
						}
					}

					if (_membershipProvider.ValidateUser(model.UserName, model.Password))
					{
						_sessionIpLog.AddSessionIpLog(new CustomerSession()
							{
								CustomerId = user.Id,
								StartSession = DateTime.Now,
								Ip = customerIp,
								IsPasswdOk = true
							});

						user.LoginFailedCount = 0;
						SetCookie(model);
						return this.JsonNet(new { success = true, model });
					}
					else
					{
						if (user.LoginFailedCount < 3)
						{
							_sessionIpLog.AddSessionIpLog(new CustomerSession()
							{
								CustomerId = user.Id,
								StartSession = DateTime.Now,
								Ip = customerIp,
								IsPasswdOk = false
							});
						}
					}

					if (user.LoginFailedCount >= 3)
					{
						errorMessage =
							"More than 3 unsuccessful login attempts have been made. Resetting user password";
						_sessionIpLog.AddSessionIpLog(new CustomerSession()
						{
							CustomerId = user.Id,
							StartSession = DateTime.Now,
							Ip = customerIp,
							IsPasswdOk = false,
							ErrorMessage = errorMessage
						});

						_log.InfoFormat(errorMessage);
						//RestorePassword(user.EMail, user.SecurityAnswer);

						var password = _membershipProvider.ResetPassword(user.EMail, "");
						_appCreator.ThreeInvalidAttempts(user, user.Name, password);
						user.IsPasswordRestored = true;
						user.LoginFailedCount = 0;

						errorMessage =
							@"Three unsuccessful login attempts have been made. <span class='bold'>ezbob</span> has issued you with a temporary password. Please check your e-mail.";
					}
					else
					{
						errorMessage = @"User not found or incorrect password.";
					}
				}
			}

			// If we got this far, something failed, redisplay form
			return this.JsonNet(new { success = false, errorMessage });
		}
		//------------------------------------------------------------------------
		private void SetCookie(LogOnModel model)
		{
			FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
		}
		//------------------------------------------------------------------------
		private ActionResult SetCookieAndRedirect(LogOnModel model)
		{
			FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
			if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 && model.ReturnUrl.StartsWith("/")
				&& !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
			{
				return Redirect(model.ReturnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Profile", new { Area = "Customer" });
			}
		}
		//------------------------------------------------------------------------
		private ActionResult SetCookieAndRedirectAdmin(LogOnModel model)
		{
			FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
			if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 && model.ReturnUrl.StartsWith("/")
				&& !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
			{
				return Redirect(model.ReturnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Customers", new { Area = "Underwriter" });
			}
		}
		//------------------------------------------------------------------------
		public ActionResult LogOff(bool isUnderwriterPage = false)
		{
			_sessionManager.EndSession(_context.SessionId);
			_context.SessionId = null;
			FormsAuthentication.SignOut();

			return !isUnderwriterPage ? (ActionResult)Redirect(@"http://www.ezbob.com") :
				RedirectToAction("Index", "Customers", new { Area = "Underwriter" });
		}
		//------------------------------------------------------------------------
		[HttpPost]
		[Transactional]
		[Ajax]
		[ActionName("SignUp")]
		[ValidateJsonAntiForgeryToken]
		[CaptchaValidationFilter]
		public JsonNetResult SignUpAjax(User model, string signupPass1, string signupPass2, string securityQuestion, string promoCode, double? amount, string mobilePhone, string mobileCode, string switchedToCaptcha)
		{
			if (!ModelState.IsValid)
			{
				return GetModelStateErrors(ModelState);
			}
			if (model.SecurityAnswer.Length > 199)
			{
				throw new Exception("Maximum answer length is 199 characters");
			}
			try
			{
				var customerIp = Request.ServerVariables["REMOTE_ADDR"];
				SignUpInternal(model.EMail, signupPass1, signupPass2, securityQuestion, model.SecurityAnswer, promoCode, amount, mobilePhone, mobileCode, switchedToCaptcha == "True");
				FormsAuthentication.SetAuthCookie(model.EMail, false);

				var user = _users.GetUserByLogin(model.EMail);
				_sessionIpLog.AddSessionIpLog(new CustomerSession()
							{
								CustomerId = user.Id,
								StartSession = DateTime.Now,
								Ip = customerIp,
								IsPasswdOk = true,
								ErrorMessage = "Registration"
							});
				return this.JsonNet(new { success = true });
			}
			catch (Exception e)
			{
				if (e.Message == MembershipCreateStatus.DuplicateEmail.ToString())
				{
					return this.JsonNet(new { success = false, errorMessage = DbStrings.EmailAddressAlreadyExists });
				}
				return this.JsonNet(new { success = false, errorMessage = e.Message });
			}
		}

		private void SignUpInternal(string email, string signupPass1, string signupPass2, string securityQuestion, string securityAnswer, string promoCode, double? amount, string mobilePhone, string mobileCode, bool switchedToCaptcha)
		{
			MembershipCreateStatus status;

			if (string.IsNullOrEmpty(email)) throw new Exception(DbStrings.NotValidEmailAddress);
			if (!string.Equals(signupPass1, signupPass2)) throw new Exception(DbStrings.PasswordDoesNotMatch);

			var maxPassLength = _config.PasswordPolicyType == "hard" ? 7 : 6;
			if (signupPass1.Length < maxPassLength)
			{
				throw new Exception(DbStrings.NotValidEmailAddress);
			}

			bool isTwilioEnabled = Convert.ToBoolean(@Session["IsSmsValidationActive"]);
			if (isTwilioEnabled && !switchedToCaptcha)
			{
				bool isCorrect = _appCreator.ValidateMobileCode(mobilePhone, mobileCode);
				if (!isCorrect)
				{
					throw new Exception("Invalid code");
				}
			}

			_membershipProvider.CreateUser(email, signupPass1, email, securityQuestion, securityAnswer, false, null, out status);
			if (status == MembershipCreateStatus.Success)
			{
				var user = _users.GetUserByLogin(email);
				var g = new RefNumberGenerator(_customers);
				var isAutomaticTest = IsAutomaticTest(email);
				var customer = new Customer
					{
						Name = email,
						Id = user.Id,
						Status = Status.Registered,
						RefNumber = g.GenerateForCustomer(),
						WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.SignUp),
						CollectionStatus =
							new CollectionStatus { CurrentStatus = _customerStatusesRepository.GetByName("Enabled") },
						IsTest = isAutomaticTest,
						IsOffline = (bool?)null,
						PromoCode = promoCode,
						CustomerInviteFriend = new List<CustomerInviteFriend>(),
						PersonalInfo = new PersonalInfo { MobilePhone = mobilePhone },
						TrustPilotStatus = _helper.TrustPilotStatusRepository.Find(TrustPilotStauses.Nether),
					};
				_log.DebugFormat("Customer {1} ({0}): wizard step has been updated to :{2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.SignUp);

				var sourceref = Request.Cookies["sourceref"];
				if (sourceref != null)
				{
					var cookie = new HttpCookie("sourceref", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customer.ReferenceSource = sourceref.Value;
				}

				var googleCookie = Request.Cookies["__utmz"];
				if (googleCookie != null)
				{
					var cookie = new HttpCookie("__utmz", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customer.GoogleCookie = googleCookie.Value;
				}

				var customerInviteFriend = new CustomerInviteFriend(customer);
				var inviteFriend = Request.Cookies["invite"];
				if (inviteFriend != null)
				{
					var cookie = new HttpCookie("inviteFriend", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customerInviteFriend.InvitedByFriendSource = inviteFriend.Value;
				}

				customer.CustomerInviteFriend.Add(customerInviteFriend);
				var ezbobab = Request.Cookies["ezbobab"];
				if (ezbobab != null)
				{
					var cookie = new HttpCookie("ezbobab", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
					Response.Cookies.Add(cookie);
					customer.ABTesting = ezbobab.Value;
				}

				var link = _confirmation.GenerateLink(customer);

				if (Request.Cookies["istest"] != null)
				{
					customer.IsTest = true;
				}

				_customers.Save(customer);

				customer.CustomerRequestedLoan = new List<CustomerRequestedLoan>
					{
						new CustomerRequestedLoan
							{
								Customer = customer,
								Amount = amount,
								Created = DateTime.UtcNow,
							}
					};

				_appCreator.AfterSignup(user, link);
			}
			if (status == MembershipCreateStatus.DuplicateEmail)
			{
				throw new Exception("This email is already registered");
			}
		}

		private bool IsAutomaticTest(string email)
		{
			bool isAutomaticTest = false;
			var isAutomaticTestCustomerMark = _configurationVariables.GetByName("AutomaticTestCustomerMark");
			if (isAutomaticTestCustomerMark.Value == "1")
			{
				var patterns = _testCustomers.GetAllPatterns();
				if (patterns.Any(email.Contains))
				{
					isAutomaticTest = true;
				}
			}

			return isAutomaticTest;
		}

		//------------------------------------------------------------------------        
		public ActionResult ForgotPassword()
		{
			ViewData["CaptchaMode"] = _config.CaptchaMode;
			return View("ForgotPassword");
		}

		//------------------------------------------------------------------------        
		[CaptchaValidationFilter]
		[Ajax]
		public JsonNetResult QuestionForEmail(string email)
		{
			if (!ModelState.IsValid)
			{
				return GetModelStateErrors(ModelState);
			}
			var user = _users.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email);
			return user == null ?
				this.JsonNet(new { error = "User : '" + email + "' was not found" }) :
				this.JsonNet(new { question = user.SecurityQuestion != null ? user.SecurityQuestion.Name : "" });
		}

		//------------------------------------------------------------------------        
		[Transactional]
		public JsonNetResult RestorePassword(string email = "", string answer = "")
		{
			if (!ModelState.IsValid)
			{
				return GetModelStateErrors(ModelState);
			}

			if (_users.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email) == null || string.IsNullOrEmpty(email))
			{
				throw new UserNotFoundException(string.Format("User {0} not found", email));
			}

			if (string.IsNullOrEmpty(answer))
			{
				throw new EmptyAnswerExeption("Answer is empty");
			}

			var user = _users.GetAll().FirstOrDefault(x => (x.EMail == email || x.Name == email) && (x.SecurityAnswer == answer));

			if (user == null) return this.JsonNet(new { error = "Wrong answer to secret questions" });

			var password = _membershipProvider.ResetPassword(email, "");

			user = _users.GetUserByLogin(email);
			var customer = _customers.Get(user.Id);
			_appCreator.PasswordRestored(user, email, customer.PersonalInfo != null ? customer.PersonalInfo.FirstName : "", password);
			user.IsPasswordRestored = true;

			return this.JsonNet(new { result = true });
		}

		public ActionResult SimpleCaptcha()
		{
			return View("SimpleCaptcha");
		}
		public ActionResult Recaptcha()
		{
			return View("Recaptcha");
		}

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Authorize(Roles = "Underwriter, Web")]
		public JsonNetResult CheckingCompany(string postcode, string companyName, string filter, string refNum)
		{
			var nFilter = TargetResults.LegalStatus.DontCare;

			switch (filter.ToUpper())
			{
				case "L":
					nFilter = TargetResults.LegalStatus.Limited;
					break;

				case "N":
					nFilter = TargetResults.LegalStatus.NonLimited;
					break;
			} // switch

			try
			{
				var service = new EBusinessService();
				var result = service.TargetBusiness(companyName, postcode, _context.UserId, nFilter, refNum);
				if (result.Targets.Any())
				{
					foreach (var t in result.Targets)
					{
						t.BusName = string.IsNullOrEmpty(t.BusName) ? string.Empty : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.BusName.ToLower());
						t.AddrLine1 = string.IsNullOrEmpty(t.AddrLine1) ? string.Empty : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine1.ToLower());
						t.AddrLine2 = string.IsNullOrEmpty(t.AddrLine2) ? string.Empty : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine2.ToLower());
						t.AddrLine3 = string.IsNullOrEmpty(t.AddrLine3) ? string.Empty : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine3.ToLower());
						t.AddrLine4 = string.IsNullOrEmpty(t.AddrLine4) ? string.Empty : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine4.ToLower());
					}
					if (result.Targets.Count > 1)
					{
						result.Targets.Add(new CompanyInfo { BusName = "Company not found", BusRefNum = "skip" });
					}
				}



				return this.JsonNet(result.Targets);
			}
			catch (Exception e)
			{
				if (companyName.ToLower() == "asd" && postcode.ToLower() == "ab10 1ba")
				{
					return this.JsonNet(GenerateFakeTargetingData(companyName, postcode));
				}
				_log.Error("Target Bussiness failed", e);
				throw;
			}
		}

		private static List<CompanyInfo> GenerateFakeTargetingData(string companyName, string postcode)
		{
			var data = new List<CompanyInfo>();

			for (var i = 0; i < 10; i++)
			{
				data.Add(new CompanyInfo
				{
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
		}
		//-----------------------------------------------------------------------------
		private JsonNetResult GetModelStateErrors(ModelStateDictionary modelStateDictionary)
		{
			return this.JsonNet(
				new
					{
						result = false,
						errorMessage = string.Join("; ", modelStateDictionary.Values
													  .SelectMany(x => x.Errors)
													  .Select(x => x.ErrorMessage))
					});
		}

		[Ajax]
		[HttpPost]
		public bool GenerateMobileCode(string mobilePhone)
		{
			return _appCreator.GenerateMobileCode(mobilePhone);
		}
	}
}