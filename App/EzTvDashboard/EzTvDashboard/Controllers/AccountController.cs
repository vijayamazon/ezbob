using System.Web.Mvc;

namespace EzTvDashboard.Controllers
{
	using System.Web.Security;
	using Models;

	public class AccountController : Controller
	{
		public ActionResult Login(string returnUrl)
		{
			return View(new LoginModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		public ActionResult Login(LoginModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.UserName == "TvDashboard" && model.Password == "123456")
				{
					ModelState.Clear();
					return SetCookieAndRedirectAdmin(model);
				}
			}
			ModelState.AddModelError("LoginError", "Wrong user name/password.");
			return View(model);
		}

		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction("Login", "Account");
		}

		private ActionResult SetCookieAndRedirectAdmin(LoginModel model)
		{
			FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
			if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 && model.ReturnUrl.StartsWith("/")
				&& !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
			{
				return Redirect(model.ReturnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Dashboard");
			}
		}
	}
}
