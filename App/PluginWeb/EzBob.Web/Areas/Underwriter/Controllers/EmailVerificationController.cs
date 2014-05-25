namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Code.Email;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.Membership;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class EmailVerificationController : Controller
	{
		public EmailVerificationController(IEmailConfirmation confirmation, ICustomerRepository customers, IUsersRepository users)
		{
			_confirmation = confirmation;
			_customers = customers;
			m_oServiceClient = new ServiceClient();
			_users = users;
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

		[HttpPost]
		[Permission(Name = "EmailConfirmationButton")]
		public JsonResult ChangeEmail(int id, string email) {
			string sErrorMessage;

			try {
				StringActionResult sar = m_oServiceClient.Instance.UserChangeEmail(id, email);
				sErrorMessage = sar.Value;
			}
			catch (Exception e) {
				sErrorMessage = "Failed to change a user email.";
				ms_oLog.Alert(e, sErrorMessage);
			} // try

			return Json(new { success = string.IsNullOrWhiteSpace(sErrorMessage), error = sErrorMessage, });
		} // ChangeEmail

		private readonly IEmailConfirmation _confirmation;
		private readonly ICustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
		private readonly IUsersRepository _users;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (EmailVerificationController));
	} // class EmailVerificationController
} // namespace
