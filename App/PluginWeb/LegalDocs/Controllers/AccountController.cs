namespace LegalDocs.Controllers {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB.Authentication;
    using Ezbob.Logger;
    using EzBob.Web.Infrastructure;
    using EZBob.DatabaseLib.Model.Database;
    using LegalDocs.Models;
    using ServiceClientProxy;
    using ServiceClientProxy.EzServiceReference;

    public class AccountController : Controller {


        public ServiceClient serviceClient = new ServiceClient();
        public IEzbobWorkplaceContext context = new EzBobContext(null, null);
        private static readonly ASafeLog log = new SafeILog(typeof(AccountController));

        public static ASafeLog Log
        {
            get { return log; }
        }

        public System.Web.Mvc.ActionResult Login() 
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public System.Web.Mvc.ActionResult Login(LoginModel model) {
            if (ModelState.IsValid) {
                string loginError;
                var membershipCreateStatus = ValidateUser(CustomerOriginEnum.ezbob, model.UserName, model.Password,null, null, out loginError);
                if (membershipCreateStatus == MembershipCreateStatus.Success) {
                    ModelState.Clear();
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);



                    return RedirectToAction("LegalDocs", "LegalDocs", new { area = "Work" });
                    //if (user != null && user.UserRoles.Any(x=>x.Name == "manager"))
                    //return RedirectToAction("ReviewLegalDocs", "LegalDocs", new { area = "Work" });
                }

            }
            ModelState.AddModelError("LoginError", "Wrong user name/password.");
            return View("Login",model);
        }

        [HttpPost]
        public System.Web.Mvc.ActionResult Logout() {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        private MembershipCreateStatus ValidateUser(CustomerOriginEnum? originID,string username,string password,string promotionName,DateTime? promotionPageVisitTime,out string error) {
            
            int nSessionID;
            MembershipCreateStatus nStatus;
            error = null;

            CustomerOriginEnum? remoteOriginID = null;

            if (originID != null)
                remoteOriginID = (CustomerOriginEnum)(int)originID.Value;

            try {
                UserLoginActionResult ular = this.serviceClient.Instance.UserLogin(remoteOriginID,username,new DasKennwort(password),RemoteIp(),promotionName,promotionPageVisitTime);
                nSessionID = ular.SessionID;
                nStatus = (MembershipCreateStatus)Enum.Parse(typeof(MembershipCreateStatus), ular.Status);
                error = ular.ErrorMessage;
            }
            catch (Exception e) {
                return MembershipCreateStatus.ProviderError;
            } // try

            if (nStatus == MembershipCreateStatus.Success) {
                this.context.SessionId = nSessionID.ToString(CultureInfo.InvariantCulture);
                Session["UserSessionId"] = nSessionID;
                Session["UserId"] = username;
                User user = LegalDocs.Code.Session.Instance.GetUser(username, (int?)originID);
                user.SessionId = nSessionID.ToString();
            }
            return nStatus;
        } // ValidateUser

        private string RemoteIp() {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
                ip = Request.ServerVariables["REMOTE_ADDR"];

            return ip;
        } // RemoteIp
    }
} // namespace
