﻿namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Data;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Utils;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.Membership;
	using Models;
	using Infrastructure.csrf;
	using ServiceClientProxy;

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

		[Transactional]
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

			return Json(new { });
		}


		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ChangePassword(string oldPassword, string newPassword)
		{
			bool success = ChangePasswordTrn(context.User.Name, oldPassword, newPassword);
			if (success)
			{
				context.User.IsPasswordRestored = false;
				m_oServiceClient.Instance.PasswordChanged(context.User.Id, newPassword);
			}

			return Json(new { status = success ? "ChangeOk" : "SomeError" });
		}

		[NonAction]
		[Transactional]
		private bool ChangePasswordTrn(string name, string oldPassword, string newPassword)
		{
			return provider.ChangePassword(name, oldPassword, newPassword);
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