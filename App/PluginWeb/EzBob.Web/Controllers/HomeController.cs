namespace EzBob.Web.Controllers
{
	using System;
	using System.Web;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib;
	using Scorto.Web;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class HomeController : Controller
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(HomeController));
		private readonly AskvilleRepository askvilleRepository;

		public HomeController(AskvilleRepository askvilleRepository, IAppCreator appCreator)
		{
			this.askvilleRepository = askvilleRepository;

			log.Info("Got configs");
		}

		public ActionResult Index(string sourceref = "", string shop = "", string ezbobab = "", string invite = "")
		{
			Session["Shop"] = shop;

			if (!string.IsNullOrEmpty(sourceref))
			{
				var cookie = new HttpCookie("sourceref", sourceref) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			}

			if (!string.IsNullOrEmpty(invite))
			{
				var cookie = new HttpCookie("invite", invite) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			}

			if (!string.IsNullOrEmpty(ezbobab))
			{
				var cookie = new HttpCookie("ezbobab", ezbobab) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			}

			return RedirectToActionPermanent("Index", User.Identity.IsAuthenticated ? "Profile" : "Wizard", new { Area = "Customer" });
		}

		[Transactional]
		public ActionResult ActivateStore(string id, bool? approve)
		{
			if (approve != null)
			{
				var askville = askvilleRepository.GetAskvilleByGuid(id);
				var confirmStatus = (bool)approve ? AskvilleStatus.Confirmed : AskvilleStatus.NotConfirmed;
				if (askville != null)
				{
					askville.Status = confirmStatus;
					askville.IsPassed = true;
					askvilleRepository.SaveOrUpdate(askville);
					Utils.WriteLog("Askville confirmation", "Confirmation status " + confirmStatus.ToString(), "Askville", askville.MarketPlace.Customer.Id);
				}
				ViewData["Approve"] = approve;
			}
			return View();
		}
	}
}