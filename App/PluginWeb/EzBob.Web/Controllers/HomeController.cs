using System;
using System.Web;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Repository;
using ExperianLib;
using Scorto.Web;

namespace EzBob.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AskvilleRepository _askvilleRepository;

        public HomeController(AskvilleRepository askvilleRepository)
        {
            _askvilleRepository = askvilleRepository;
        }

        public ActionResult Index(string sourceref = "", string shop = "", string ezbobab = "", string offline = "", string invite = "")
        {

            Session["Shop"] = shop;

            if(!string.IsNullOrEmpty(sourceref))
            {
                var cookie = new HttpCookie("sourceref", sourceref) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
                Response.Cookies.Add(cookie);
            }

			if (!string.IsNullOrEmpty(invite))
			{
				var cookie = new HttpCookie("invite", invite) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			}

            if (!string.IsNullOrEmpty(ezbobab)) {
                var cookie = new HttpCookie("ezbobab", ezbobab) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
                Response.Cookies.Add(cookie);
            }

            if ((offline ?? "").Trim().ToLower().Equals("yes")) {
                var cookie = new HttpCookie("isoffline", "yes") { Expires = DateTime.Now.AddMonths(3), HttpOnly = false, Secure = true };
                Response.Cookies.Add(cookie);
            }
			else {
                var cookie = new HttpCookie("isoffline", "no") { Expires = DateTime.Now.AddMonths(3), HttpOnly = false, Secure = true };
                Response.Cookies.Add(cookie);
            }

            return RedirectToActionPermanent("Index", User.Identity.IsAuthenticated ? "Profile" : "Wizard", new {Area = "Customer"});
        }

        [Transactional]
        public ActionResult ActivateStore(string id, bool? approve )
        {
            if (approve != null)
            {
                var askville = _askvilleRepository.GetAskvilleByGuid(id);
                var confirmStatus = (bool) approve ? AskvilleStatus.Confirmed : AskvilleStatus.NotConfirmed;
                if (askville != null)
                {
                    askville.Status = confirmStatus;
                    askville.IsPassed = true;
                    _askvilleRepository.SaveOrUpdate(askville);
                    Utils.WriteLog("Askville confirmation", "Confirmation status " + confirmStatus.ToString(), "Askville", askville.MarketPlace.Customer.Id);
                }
                ViewData["Approve"] = approve;
            }
            return View();
        }
    }
}