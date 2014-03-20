namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using Infrastructure;
	using Infrastructure.Membership;
	using Models;
	using Infrastructure.csrf;
	using Scorto.NHibernate.Model;
	using Scorto.Security.UserManagement;
	using Scorto.Web;

	public class AccountSettingsController : Controller
    {
        private readonly IWorkplaceContext _context;
        private readonly IPasswordEncoder _passwordEncoder;
        private readonly ServiceClient m_oServiceClient;
		private readonly EzbobMembershipProvider provider;

        //------------------------------------------------------------------------------------
        public AccountSettingsController(IWorkplaceContext context, IPasswordEncoder passwordEncoder)
        {
            _context = context;
            _passwordEncoder = passwordEncoder;
	        m_oServiceClient = new ServiceClient();
			provider = new EzbobMembershipProvider();
        }
        //------------------------------------------------------------------------------------
        [Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult UpdateSecurityQuestion(SecurityQuestionModel model, string password)
        {
            var user = _context.User;

            var passwordHash = _passwordEncoder.EncodePassword(password, user.Name, user.CreationDate);
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
			bool success = provider.ChangePassword(_context.User.Name, oldPassword, newPassword);
			if (success)
            {
                _context.User.IsPasswordRestored = false;
				m_oServiceClient.Instance.PasswordChanged(_context.User.Id, newPassword);
            }

			return this.JsonNet(new { status = success ? "ChangeOk" : "SomeError" }); 
        }

        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Ajax]
        public string IsEqualsOldPassword(string new_Password)
        {
            var result = provider.IsEqualsOldPassword(_context.User.Name, new_Password);
            return result ? "false" : "true";
        }
    }
}