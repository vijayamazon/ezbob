using System;
using System.Web.Mvc;
using EzBob.Web.Code.Email;
using Scorto.Web;

namespace EzBob.Web.Controllers
{
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
            catch (EmailConfirmationRequestInvalidStateException e)
            {
                return View("ConfirmationError", new {Message = "Email is already confirmed"});
            }
            catch (EmailConfirmationRequestNotFoundException e)
            {
                return View("ConfirmationError", new {Message = "Confirmation request was not found"});
            }
            catch (Exception)
            {
                return View("ConfirmationError");
            }
        }
    }
}