using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using ApplicationMng.Signal;
using DB.Security;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using ExperianLib.Ebusiness;
using EzBob.Signals.ZohoCRM;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Code;
using EzBob.Web.Code.Email;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.Filters;
using EzBob.Web.Infrastructure.Membership;
using EzBob.Web.Infrastructure.csrf;
using EzBob.Web.Models;
using EzBob.Web.Models.Strings;
using Scorto.Security.UserManagement.Sessions;
using Scorto.Web;
using ZohoCRM;
using log4net;

namespace EzBob.Web.Controllers
{
    public class AccountController : Controller
    {

        private static readonly ILog _log = log4net.LogManager.GetLogger(typeof(AccountController));
        private readonly MembershipProvider _membershipProvider;
        private readonly IUsersRepository _users;
        private readonly CustomerRepository _customers;
        private readonly IAppCreator _appCreator;
        private readonly IEzBobConfiguration _config;
        private readonly ISessionManager _sessionManager;
        private readonly IEzbobWorkplaceContext _context;
        private readonly IEmailConfirmation _confirmation;
        private readonly IZohoFacade _zoho;

        //------------------------------------------------------------------------
        public AccountController(
                                    MembershipProvider membershipProvider, 
                                    IUsersRepository users,
                                    CustomerRepository customers, 
                                    IAppCreator appCreator, 
                                    IEzBobConfiguration config,
                                    ISessionManager sessionManager,
                                    IEzbobWorkplaceContext context,
                                    IEmailConfirmation confirmation,
                                    IZohoFacade zoho
            )
        {
            _membershipProvider = membershipProvider;
            _users = users;
            _customers = customers;
            _appCreator = appCreator;
            _config = config;
            _sessionManager = sessionManager;
            _context = context;
            _confirmation = confirmation;
            _zoho = zoho;
        }
        //------------------------------------------------------------------------
        [IsSuccessfullyRegisteredFilter]
        public ActionResult LogOn(string returnUrl)
        {
            return View( new LogOnModel{ ReturnUrl = returnUrl } );
        }
        //------------------------------------------------------------------------
        public ActionResult AdminLogOn(string returnUrl)
        {
            ViewData["returnUrl"] = returnUrl;
            return View();
        }
        //------------------------------------------------------------------------
        [HttpPost]
        public ActionResult LogOn(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _users.GetUserByLogin(model.UserName);
                
                if(user == null)
                {
                    ModelState.AddModelError("", "User not found or incorrect password.");
                    
                }else
                {

                    var isUnderwriter = user.Roles.Any(r => r.Id == 31 || r.Id ==32 || r.Id ==33  );
                    if (!isUnderwriter)
                    {
                        var customer = _customers.Get(user.Id);
                        if (customer.CollectionStatus.CurrentStatus == CollectionStatusType.Disabled)
                        {
                            ModelState.AddModelError("",
                                                     "This account is closed, you cannot use it, please contact EZBOB customer care customercare@ezbob.com");
                            return View(model);
                        }
                    }

                    if (_membershipProvider.ValidateUser(model.UserName, model.Password))
                    {
                        user.LoginFailedCount = 0;
                        return SetCookieAndRedirect(model);
                    }
                    if (user.LoginFailedCount >=3)
                    {
                        _log.InfoFormat("More than 3 unsuccessful login attempts have been made. Resetting user password");
                        //RestorePassword(user.EMail, user.SecurityAnswer);
                        
                        var password = _membershipProvider.ResetPassword(user.EMail, "");
                        _appCreator.ThreeInvalidAttempts(user, user.Name, password);
                        user.IsPasswordRestored = true;
                        user.LoginFailedCount = 0;

                        ModelState.AddModelError("", "Three unsuccessful login attempts have been made. EZBOB has issued you with a temporary password. Please check your e-mail.");
                    }
                    ModelState.AddModelError("", "User not found or incorrect password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
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
        public JsonNetResult SignUpAjax(User model, string signupPass1, string signupPass2, string securityQuestion)
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
                SignUpInternal(model, signupPass1, signupPass2, securityQuestion);
                FormsAuthentication.SetAuthCookie(model.EMail, false);

                return this.JsonNet(new { success = true });
            }
            catch (Exception e)
            {
                if (e.Message == MembershipCreateStatus.DuplicateEmail.ToString())
                {
                    return this.JsonNet(new { success = false, errorMessage = DbStrings.EmailAddressAlreadyExists });
                }
                return this.JsonNet(new {success = false, errorMessage = e.Message});
            }
        }

        [HttpPost]
        [Transactional]
        [Ajax(false)]
        [CaptchaValidationFilter]
        public ActionResult SignUp(User model, string signupPass1, string signupPass2, string securityQuestion)
        {
            if (!ModelState.IsValid)
            {
                return GetModelStateErrors(ModelState);
            }

            SignUpInternal(model, signupPass1, signupPass2, securityQuestion);

            return SetCookieAndRedirect(new LogOnModel { Password = signupPass1, UserName = model.EMail, ReturnUrl = Url.Action("Index", "Profile", new { Area = "Customer" }) });
        }

        private void SignUpInternal(User model, string signupPass1, string signupPass2, string securityQuestion)
        {
            MembershipCreateStatus status;

            if (string.IsNullOrEmpty(model.EMail)) throw new Exception(DbStrings.NotValidEmailAddress);
            if (!string.Equals(signupPass1, signupPass2)) throw new Exception(DbStrings.PasswordDoesNotMatch);

            var maxPassLength = _config.PasswordPolicyType == "hard" ? 7 : 6;
            if (signupPass1.Length < maxPassLength)
            {
                throw new Exception(DbStrings.NotValidEmailAddress);
            }

            _membershipProvider.CreateUser(model.EMail, signupPass1, model.EMail, securityQuestion, model.SecurityAnswer, false, null, out status);
            if (status == MembershipCreateStatus.Success)
            {
                var user = _users.GetUserByLogin(model.EMail);
                var g = new RefNumberGenerator(_customers);
                var customer = new Customer()
                {
                    Name = model.EMail,
                    Id = user.Id,
                    Status = Status.Registered,
                    RefNumber = g.GenerateForCustomer(),
                    WizardStep = WizardStepType.SignUp
                };

                var sourceref = Request.Cookies["sourceref"];
                if(sourceref != null)
                {
                    var cookie = new HttpCookie("sourceref", "") { Expires = DateTime.Now.AddMonths(-1), HttpOnly = true, Secure = true };
                    Response.Cookies.Add(cookie);
                    customer.ReferenceSource = sourceref.Value;
                }

                var link = _confirmation.GenerateLink(customer);

                _zoho.RegisterLead(customer);
                
                if (Request.Cookies["istest"] != null)
                {
                    customer.IsTest = true;
                }

                _customers.Save(customer);               

                _appCreator.AfterSignup(user, link);
            }
            if(status == MembershipCreateStatus.DuplicateEmail)
            {
                throw new Exception("This email is already registered");
            }
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
                this.JsonNet(new { question = user.SecurityQuestion !=null ? user.SecurityQuestion.Name : "" });
        }

        //------------------------------------------------------------------------        
        [Transactional]
        [CaptchaValidationFilter]
        public JsonNetResult RestorePassword(string email="", string answer="")
        {
            if (!ModelState.IsValid)
            {
                return GetModelStateErrors(ModelState);
            }

            if (_users.GetAll().FirstOrDefault(x => x.EMail == email || x.Name == email) == null || string.IsNullOrEmpty(email))
            {
                throw new UserNotFoundException(string.Format("User {0} not found",email));
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
        public JsonNetResult CheckingCompany(string postcode, string companyName)
        {
            try
            {
                var service = new EBusinessService();
                var result = service.TargetBusiness(companyName, postcode, _context.UserId);
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
    }

}