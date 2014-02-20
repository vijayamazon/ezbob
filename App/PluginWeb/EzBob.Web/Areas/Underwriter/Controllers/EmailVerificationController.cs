namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Data;
	using Code.ApplicationCreator;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Code.Email;
	using Infrastructure;
	using Scorto.Security.UserManagement;
	using Scorto.Web;

	public class EmailVerificationController : Controller
    {
        private readonly IEmailConfirmation _confirmation;
        private readonly ICustomerRepository _customers;
        private readonly IAppCreator _creator;
        private readonly IUsersRepository _users;
        private readonly UserManager _userManager;

        public EmailVerificationController(IEmailConfirmation confirmation, ICustomerRepository customers, IAppCreator creator, IUsersRepository users, UserManager userManager)
        {
            _confirmation = confirmation;
            _customers = customers;
            _creator = creator;
            _users = users;
            _userManager = userManager;
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [HttpPost]
        [Permission(Name = "EmailConfirmationButton")]
        public JsonNetResult ManuallyConfirm(int id)
        {
            var customer = _customers.Get(id);
            _confirmation.ConfirmEmail(customer);
            return this.JsonNet(new {});
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Permission(Name = "EmailConfirmationButton")]
        [HttpPost]
        public JsonNetResult Resend(int id)
        {
            var customer = _customers.Get(id);
            var user = _users.Get(id);

            var address = _confirmation.GenerateLink(customer);
            _creator.SendEmailVerification(user, customer, address);
            return this.JsonNet(new {});
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [HttpPost]
        [Permission(Name = "EmailConfirmationButton")]
        public JsonNetResult ChangeEmail(int id, string email)
        {
            var customer = _customers.Get(id);
            var user = _users.Get(id);

            var provider = new ScortoMembershipProvider();
            var newPassword = provider.GenerateSimplePassword(16);

            var result = _userManager.ChangeEmailAndPassword(user, email, newPassword);
            
            if (result == ChangePasswordStatus.ChangeOk)
            {
                customer.Name = email;

                _creator.PasswordChanged(user, customer.PersonalInfo.FirstName, newPassword);

                var address = _confirmation.GenerateLink(customer);
                _creator.SendEmailVerification(user, customer, address);
            }

            return this.JsonNet(new {});
        }
    }
}