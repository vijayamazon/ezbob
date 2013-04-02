using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EzBob.Web.Models;
using NHibernate;

namespace EzBob.Web.Controllers
{
    public class LandingController : Controller
    {
        private readonly ISession _session;

        public LandingController(ISession session)
        {
            _session = session;
        }

        //
        // GET: /Landing/
        [HttpGet]
        public ActionResult Index(string ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        public RedirectResult Index(string email, string ReturnUrl)
        {

            if (_session.QueryOver<AllowedEmail>().Where(c => c.Email == email).RowCount() == 0)
            {
                return Redirect("http://storefunding.e-zoteric.net/");
            }

            Response.Cookies.Set(new HttpCookie("Email", email));
            return Redirect(ReturnUrl);
        }
    }
}
