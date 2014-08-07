namespace EzBob.Web.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using ServiceClientProxy;

	public class ConfirmEmailController : Controller {
		public ViewResult Index(string code) {
			try {
				var guid = Guid.Parse(code);

				var ar = new ServiceClient().Instance.EmailConfirmationCheckOne(guid);

				if (ar.Value == (int)EmailConfirmationResponse.NotFound)
					return View("ConfirmationError", new { Message = "Confirmation request was not found" });

				if (ar.Value == (int)EmailConfirmationResponse.InvalidState)
					return View("ConfirmationError", new { Message = "Email is already confirmed" });

				return View();
			}
			catch (Exception) {
				return View("ConfirmationError");
			} // try
		} // Index

	} // ConfirmEmailController
} // namespace
