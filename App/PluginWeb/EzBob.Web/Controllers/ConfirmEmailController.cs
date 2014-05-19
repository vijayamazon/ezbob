namespace EzBob.Web.Controllers
{
	using System;
	using System.Web.Mvc;
	using Code.Email;
	using Infrastructure.Attributes;

	public class ConfirmEmailController : Controller
	{
		private readonly IEmailConfirmation _confirmation;

		public ConfirmEmailController(IEmailConfirmation confirmation)
		{
			_confirmation = confirmation;
		}

		[Transactional]
		public ViewResult Index(string code)
		{
			try
			{
				var guid = Guid.Parse(code);
				_confirmation.ConfirmEmail(guid);
				return View();
			}
			catch (EmailConfirmationRequestInvalidStateException)
			{
				return View("ConfirmationError", new { Message = "Email is already confirmed" });
			}
			catch (EmailConfirmationRequestNotFoundException)
			{
				return View("ConfirmationError", new { Message = "Confirmation request was not found" });
			}
			catch (Exception)
			{
				return View("ConfirmationError");
			}
		}
	}
}