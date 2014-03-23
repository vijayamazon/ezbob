namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using Infrastructure.Membership;
	using Models;
	using Infrastructure.csrf;
	using NHibernateWrapper.NHibernate.Model;
	using Scorto.Web;
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
        public JsonNetResult UpdateSecurityQuestion(SecurityQuestionModel model, string password)
        {
            var user = context.User;

			var passwordHash = provider.EncodePassword(password, user.Name, user.CreationDate);
            if (user.Password != passwordHash)
            {
                return this.JsonNet(new { error = "Incorrect password" });
            }

            user.SecurityAnswer = model.Answer;
            user.SecurityQuestion = new SecurityQuestion
            {
                Id = model.Question
            };

            return this.JsonNet(new {});
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult ChangePassword(string oldPassword, string newPassword)
        {
			bool success = provider.ChangePassword(context.User.Name, oldPassword, newPassword);
			if (success)
            {
                context.User.IsPasswordRestored = false;
				m_oServiceClient.Instance.PasswordChanged(context.User.Id, newPassword);
            }

			return this.JsonNet(new { status = success ? "ChangeOk" : "SomeError" }); 
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