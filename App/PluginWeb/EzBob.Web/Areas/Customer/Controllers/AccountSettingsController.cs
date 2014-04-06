namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using Ezbob.Utils;
	using Infrastructure.Attributes;
	using Infrastructure.Membership;
	using Models;
	using Infrastructure.csrf;
	using NHibernateWrapper.NHibernate.Model;
	using NHibernateWrapper.Web;

	public class AccountSettingsController : Controller
    {
        private readonly IWorkplaceContext context;
        private readonly ServiceClient m_oServiceClient;
		private readonly EzbobMembershipProvider provider;

        public AccountSettingsController(IWorkplaceContext context)
        {
            this.context = context;
	        m_oServiceClient = new ServiceClient();
			provider = new EzbobMembershipProvider();
        }

        [Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonResult UpdateSecurityQuestion(SecurityQuestionModel model, string password)
        {
            var user = context.User;

			var passwordHash = PasswordEncryptor.EncodePassword(password, user.Name, user.CreationDate);
            if (user.Password != passwordHash)
            {
                return Json(new { error = "Incorrect password" });
            }

            user.SecurityAnswer = model.Answer;
            user.SecurityQuestion = new SecurityQuestion
            {
                Id = model.Question
            };

            return Json(new {});
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonResult ChangePassword(string oldPassword, string newPassword)
        {
			bool success = provider.ChangePassword(context.User.Name, oldPassword, newPassword);
			if (success)
            {
                context.User.IsPasswordRestored = false;
				m_oServiceClient.Instance.PasswordChanged(context.User.Id, newPassword);
            }

			return Json(new { status = success ? "ChangeOk" : "SomeError" }); 
        }

        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Ajax]
        public string IsEqualsOldPassword(string new_Password)
        {
            var result = provider.IsEqualsOldPassword(context.User.Name, new_Password);
            return result ? "false" : "true";
        }
    }
}