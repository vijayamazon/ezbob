namespace EzBob.Web.Controllers
{
	using System;
	using System.Web;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib;
	using EzServiceReference;
	using Scorto.Web;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class HomeController : Controller
    {
		private readonly AskvilleRepository askvilleRepository;
		private readonly IAppCreator appCreator;
		private readonly WizardConfigsActionResult wizardConfigsActionResult;

		public HomeController(AskvilleRepository askvilleRepository, IAppCreator appCreator)
        {
            this.askvilleRepository = askvilleRepository;
			this.appCreator = appCreator;

			wizardConfigsActionResult = appCreator.GetWizardConfigs();
        }

		private bool sessionInitialized;

        public ActionResult Index(string sourceref = "", string shop = "", string ezbobab = "", string offline = "", string invite = "")
        {
            Session["Shop"] = shop;

			if (!sessionInitialized)
			{
				Session["IsSmsValidationActive"] = wizardConfigsActionResult.IsSmsValidationActive;
				Session["NumberOfMobileCodeAttempts"] = wizardConfigsActionResult.NumberOfMobileCodeAttempts;
				sessionInitialized = true;
			}

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

			Session["isoffline"] = "";

            if ((offline ?? "").Trim().ToLower().Equals("yes")) {
                var cookie = new HttpCookie("isoffline", "yes") { Expires = DateTime.Now.AddMonths(3), HttpOnly = false, Secure = true };
                Response.Cookies.Add(cookie);
				Session["isoffline"] = "yes";
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
                var askville = askvilleRepository.GetAskvilleByGuid(id);
                var confirmStatus = (bool) approve ? AskvilleStatus.Confirmed : AskvilleStatus.NotConfirmed;
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

		[HttpPost]
		public JsonNetResult GetTwilioConfig()
		{
			if (!sessionInitialized)
			{
				Session["IsSmsValidationActive"] = wizardConfigsActionResult.IsSmsValidationActive;
				Session["NumberOfMobileCodeAttempts"] = wizardConfigsActionResult.NumberOfMobileCodeAttempts;
				sessionInitialized = true;
			}
			return this.JsonNet(new { isSmsValidationActive = Session["IsSmsValidationActive"], numberOfMobileCodeAttempts = Session["NumberOfMobileCodeAttempts"] });
		}
    }
}