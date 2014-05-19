namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Data;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Code.Email;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.Membership;
	using ServiceClientProxy;

	public class EmailVerificationController : Controller
	{
		private readonly IEmailConfirmation _confirmation;
		private readonly ICustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
		private readonly IUsersRepository _users;
		private readonly EzbobMembershipProvider provider;

		public EmailVerificationController(IEmailConfirmation confirmation, ICustomerRepository customers, IUsersRepository users)
		{
			_confirmation = confirmation;
			_customers = customers;
			m_oServiceClient = new ServiceClient();
			_users = users;
			provider = new EzbobMembershipProvider();
		}

		[Transactional]
		[HttpPost]
		[Permission(Name = "EmailConfirmationButton")]
		public JsonResult ManuallyConfirm(int id)
		{
			var customer = _customers.Get(id);
			_confirmation.ConfirmEmail(customer);
			return Json(new { });
		}

		[Transactional]
		[Permission(Name = "EmailConfirmationButton")]
		[HttpPost]
		public JsonResult Resend(int id)
		{
			var customer = _customers.Get(id);
			var user = _users.Get(id);

			var address = _confirmation.GenerateLink(customer);
			m_oServiceClient.Instance.SendEmailVerification(user.Id, customer.Name, address);
			return Json(new { });
		}

		[Transactional]
		[HttpPost]
		[Permission(Name = "EmailConfirmationButton")]
		public JsonResult ChangeEmail(int id, string email)
		{
			var customer = _customers.Get(id);
			var user = _users.Get(id);

			string newPassword = provider.ChangeEmailAndPassword(user, email);

			customer.Name = email;
			m_oServiceClient.Instance.PasswordChanged(user.Id, newPassword);
			var address = _confirmation.GenerateLink(customer);
			m_oServiceClient.Instance.SendEmailVerification(user.Id, customer.Name, address);

			return Json(new { });
		}
	}
}