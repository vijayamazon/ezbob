namespace EzBob.Web.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using ServiceClientProxy;

	public class ConfirmEmailController : Controller {
		public ActionResult Index(string code) {
			bool bIgnoredHere;

			return ProcessConfirmation(code, out bIgnoredHere);
		} // Index

		public ActionResult EmailChanged(string code) {
			bool bSuccess;

			var oView = ProcessConfirmation(code, out bSuccess);

			if (!bSuccess)
				return oView;

			return RedirectToAction("CreatePassword", "Account", new { token = code, });
		} // EmailChanged

		private ActionResult ProcessConfirmation(string code, out bool bSuccess) {
			bSuccess = false;

			try {
				var guid = Guid.Parse(code);

				var ar = new ServiceClient().Instance.EmailConfirmationCheckOne(guid);

				if (ar.Value == (int)EmailConfirmationResponse.NotFound)
					return View("ConfirmationError", new { Message = "Confirmation request was not found." });

				if (ar.Value == (int)EmailConfirmationResponse.InvalidState)
					return View("ConfirmationError", new { Message = "Email is already confirmed." });

				bSuccess = true;

				return View("Index");
			}
			catch (Exception) {
				return View("ConfirmationError");
			} // try
		} // ProcessConfirmation
	} // ConfirmEmailController
} // namespace
