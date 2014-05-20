namespace EzBob.Web.Infrastructure.Membership {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Security;
	using Areas.Customer.Controllers.Exceptions;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using JetBrains.Annotations;
	using NHibernate;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using StructureMap;

	#endregion using

	#region class UserAlreadyExistsException

	public class UserAlreadyExistsException : Exception {
		public UserAlreadyExistsException(string message) : base(message) { } // constructor
	} // class UserAlreadyExistsException

	#endregion class UserAlreadyExistsException

	#region class EzbobMembershipProvider

	public class EzbobMembershipProvider : MembershipProvider {
		#region public

		#region constructor

		public EzbobMembershipProvider() {
			roles = ObjectFactory.GetInstance<IRolesRepository>();
			usersRepository = ObjectFactory.GetInstance<IUsersRepository>();
			sessionRepository = ObjectFactory.GetInstance<SessionRepository>();
			serviceClient = new ServiceClient();
			ReadConfiguration();
		} // constructor

		public EzbobMembershipProvider(IWorkplaceContext context, IRolesRepository roles) {
			this.context = context;
			this.roles = roles;
			usersRepository = ObjectFactory.GetInstance<IUsersRepository>();
			sessionRepository = ObjectFactory.GetInstance<SessionRepository>();
			serviceClient = new ServiceClient();
			ReadConfiguration();
		} // constructor

		#endregion constructor

		#region method CreateUser

		public override MembershipUser CreateUser(string ignoredUserName, string password, string email, string passwordQuestion, string passwordAnswer, bool ignoredIsApproved, object ignoredProviderUserKey, out MembershipCreateStatus status) {
			try {
				var webRoles = roles.GetAll().Where(r => r.Name == "Web").ToList();

				if (!webRoles.Any())
					throw new RoleNotFoundException("Web");

				if (!Regex.IsMatch(email.ToLower(), configs.LoginValidationStringForWeb))
					throw new Exception("Login does not conform to the password security policy.");

				var user = new User {
					PassSetTime = DateTime.UtcNow
				};

				if (!PolicyValidateLogin(email.ToLower()))
					throw new Exception("Can't validate login");

				if (!PolicyValidatePassword(password))
					throw new Exception("Can't validate password");

				user.EzPassword = Ezbob.Utils.Security.SecurityUtils.HashPassword(email, password);

				password = PasswordEncryptor.EncodePassword(password, email, user.CreationDate);

				if (!usersRepository.CheckUserLogin(0, email) || !usersRepository.CheckUserDomainName(0, email))
					throw new UserAlreadyExistsException(string.Format("The email {0} already exists.", email));

				user.Roles.Clear();
				user.Roles.AddAll(webRoles);

				user.Name = email;
				user.FullName = email;
				user.CertificateThumbprint = null;
				user.DomainUserName = null;

				if (password != null)
					user.Password = password;

				user.EMail = email;
				user.Creator = usersRepository.Get(user.Id);
				user.PassExpPeriod = null;

				if (!string.IsNullOrEmpty(passwordQuestion)) {
					user.SecurityQuestion = ObjectFactory.GetInstance<ISession>().Load<SecurityQuestion>(Convert.ToInt32(passwordQuestion));
					user.SecurityAnswer = passwordAnswer;
				} // if

				user.DisablePassChange = null;
				user.ForcePassChange = null;
				usersRepository.Save(user);

				status = MembershipCreateStatus.Success;
			}
			catch (UserAlreadyExistsException) {
				log.Warn("User with email {0} already exists.", email);
				status = MembershipCreateStatus.DuplicateEmail;
			}
			catch (Exception e) {
				log.Error(e, "Failed to create user.");
				status = MembershipCreateStatus.ProviderError;
				throw;
			} // try

			return null;
		} // CreateUser

		#endregion method CreateUser

		#region method ChangePassword

		public override bool ChangePassword(string username, string oldPassword, string newPassword) {
			if (!PolicyValidatePassword(oldPassword))
				return false;

			User userByLogin = usersRepository.GetUserByLogin(username);
			if (userByLogin.DisablePassChange == true)
				return false;

			if (userByLogin.Password == PasswordEncryptor.EncodePassword(oldPassword, userByLogin.Name, userByLogin.CreationDate) && userByLogin.IsDeleted != 1) {
				string encodedNewPassword = PasswordEncryptor.EncodePassword(newPassword, userByLogin.Name, userByLogin.CreationDate);

				userByLogin.Password = encodedNewPassword;
				userByLogin.PassSetTime = DateTime.UtcNow;
				userByLogin.LoginFailedCount = null;
				userByLogin.LastBadLogin = null;
				userByLogin.ForcePassChange = null;

				usersRepository.SaveOrUpdate(userByLogin);
				return true;
			} // if

			return false;
		} // ChangePassword

		#endregion method ChangePassword

		#region method ResetPassword

		public override string ResetPassword(string username, string question) {
			var password = GenerateSimplePassword(8);

			if (PolicyValidatePassword(password)) {
				User userByLogin = usersRepository.GetUserByLogin(username);

				if (userByLogin.DisablePassChange != true) {
					string newPassword = PasswordEncryptor.EncodePassword(password, userByLogin.Name, userByLogin.CreationDate);
					userByLogin.Password = newPassword;
					userByLogin.PassSetTime = DateTime.UtcNow;
					userByLogin.LoginFailedCount = null;
					userByLogin.LastBadLogin = null;
					userByLogin.ForcePassChange = null;
					usersRepository.SaveOrUpdate(userByLogin);
				} // if
			} // if

			return password;
		} // ResetPassword

		#endregion method ResetPassword

		#region method ValidateUser

		public override bool ValidateUser(string username, string password) {
			string guid = null;

			string hostAddress = HttpContext.Current.Request.UserHostAddress;

			User user = usersRepository.GetUserByLogin(username);

			if (user == null)
				throw new UserNotFoundException(username);

			string passwordHash = PasswordEncryptor.EncodePassword(password, user.Name, user.CreationDate);
			string oldModelPasswordHash = PasswordEncryptor.EncodeOldPassword(password);

			if (InnerLoginUser(user, passwordHash, oldModelPasswordHash)) {
				SecuritySession session = sessionRepository.CreateNewSession(user.Id);
				session.HostAddress = hostAddress;
				sessionRepository.Save(session);
				guid = session.Id;
			} // if

			if (guid != null) {
				context.SessionId = guid;
				return true;
			} // if

			return false;
		} // ValidateUser

		#endregion method ValidateUser

		#region method IsEqualsOldPassword

		public bool IsEqualsOldPassword(string login, string newPassword) {
			User userByLogin = usersRepository.GetUserByLogin(login);
			string encodedNewPassword = PasswordEncryptor.EncodePassword(newPassword, userByLogin.Name, userByLogin.CreationDate);
			return userByLogin.Password == encodedNewPassword;
		} // IsEqualsOldPassword

		#endregion method IsEqualsOldPassword

		#region method ChangeEmailAndPassword

		public string ChangeEmailAndPassword(User user, string newEmail) {
			string newPassword = GenerateSimplePassword(16);
			user.EMail = newEmail;
			user.Name = newEmail;
			user.FullName = newEmail;
			user.Password = PasswordEncryptor.EncodePassword(newPassword, user.Name, user.CreationDate);
			user.PassSetTime = DateTime.UtcNow;
			usersRepository.SaveOrUpdate(user);

			return newPassword;
		} // ChangeEmailAndPassword

		#endregion method ChangeEmailAndPassword

		#region not implemented

		public override string GetUserNameByEmail(string email) {
			throw new NotImplementedException();
			// var user = usersRepository.GetUserByLogin(email);
			// return user == null ? null : Convert.ToString(user.Id);
		}

		public override void UpdateUser(MembershipUser user) {
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName) {
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline) {
			throw new NotImplementedException();
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer) {
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData) {
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline() {
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
			throw new NotImplementedException();
		}

		public override bool EnablePasswordRetrieval {
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordReset {
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer {
			get { throw new NotImplementedException(); }
		}

		public override string ApplicationName {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override int MaxInvalidPasswordAttempts {
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow {
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail {
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat {
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength {
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters {
			get { throw new NotImplementedException(); }
		}

		public override string PasswordStrengthRegularExpression {
			get { throw new NotImplementedException(); }
		}

		#endregion not implemented

		#endregion public

		#region private

		#region method GenerateSimplePassword

		private string GenerateSimplePassword(int size) {
			const string passwordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			string password = "";
			var random = new Random();

			for (int i = 0; i < size; i++)
				password += passwordChars[random.Next(0, passwordChars.Length)];

			return password;
		} // GenerateSimplePassword

		#endregion method GenerateSimplePassword

		#region method PolicyValidateLogin

		private bool PolicyValidateLogin(string login) {
			try {
				return Regex.IsMatch(login, configs.LoginValidity);
			}
			catch {
				log.Warn("Login: '{0}' doesn't match: '{1}'.", login, configs.LoginValidity);
				return false;
			} // try
		} // PolicyValidateLogin

		#endregion method PolicyValidateLogin

		#region method PolicyValidatePassword

		private bool PolicyValidatePassword(string password) {
			try {
				return Regex.IsMatch(password, configs.PasswordValidity);
			}
			catch {
				log.Warn("Password: '{0}' doesn't match validity mask: '{1}'.", password, configs.PasswordValidity);
				return false;
			} // try
		} // PolicyValidatePassword

		#endregion method PolicyValidatePassword

		#region method InnerLoginUser

		private bool InnerLoginUser(User user, string passwordHash, string oldModelPasswordHash) {
			if (user.IsDeleted != 0)
				return false;

			if (user.DisableDate.HasValue && user.DisableDate.Value < DateTime.UtcNow) {
				user.IsDeleted = 2;
				usersRepository.Update(user);
				return false;
			} // if

			if (user.ForcePassChange.HasValue && user.ForcePassChange.Value)
				return false;

			if (user.PassSetTime.HasValue && user.PassExpPeriod.HasValue && user.PassSetTime.Value.Add(TimeSpan.FromSeconds(user.PassExpPeriod.Value)) < DateTime.UtcNow)
				return false;

			if (user.LastBadLogin.HasValue) {
				TimeSpan span = (user.LoginFailedCount.GetValueOrDefault() >= configs.NumOfInvalidPasswordAttempts) ?
					TimeSpan.FromSeconds(configs.InvalidPasswordBlockSeconds) :
					TimeSpan.FromSeconds(configs.InvalidPasswordAttemptsPeriodSeconds);

				if (DateTime.UtcNow.Subtract(user.LastBadLogin.Value) > span) {
					user.LastBadLogin = null;
					user.LoginFailedCount = 0;
					usersRepository.Update(user);
				} // if
			} // if

			if (user.LoginFailedCount.HasValue && user.LoginFailedCount.Value > configs.NumOfInvalidPasswordAttempts)
				return false;

			if ((user.Password != passwordHash) && (user.Password != oldModelPasswordHash)) {
				user.LoginFailedCount = user.LoginFailedCount.GetValueOrDefault() + 1;
				user.LastBadLogin = DateTime.UtcNow;
				usersRepository.Update(user);
				return false;
			} // if

			if (user.Password == oldModelPasswordHash) {
				user.Password = passwordHash;
				usersRepository.Update(user);
			} // if

			return true;
		} // InnerLoginUser

		#endregion method InnerLoginUser

		#region method ReadConfiguration

		private void ReadConfiguration() {
			try {
				var basicInterestRatesList = serviceClient.Instance.GetSpResultTable("GetUserManagementConfigs", null);
				var deserializedArray = JsonConvert.DeserializeObject<EzbobMembershipProviderConfigs[]>(basicInterestRatesList.SerializedDataTable);
				configs = deserializedArray[0];
			}
			catch (Exception e) {
				log.Error(e, "Error reading EzbobMembershipProvider configs.");
			}
		} // ReadConfiguration

		#endregion method ReadConfiguration

		#region class EzbobMembershipProviderConfigs

		private class EzbobMembershipProviderConfigs {
			[UsedImplicitly]
			public string LoginValidationStringForWeb { get; set; }

			[UsedImplicitly]
			public int NumOfInvalidPasswordAttempts { get; set; }

			[UsedImplicitly]
			public int InvalidPasswordAttemptsPeriodSeconds { get; set; }

			[UsedImplicitly]
			public int InvalidPasswordBlockSeconds { get; set; }

			[UsedImplicitly]
			public string PasswordValidity { get; set; }

			[UsedImplicitly]
			public string LoginValidity { get; set; }
		} // class EzbobMembershipProviderConfigs

		#endregion class EzbobMembershipProviderConfigs

		#region fields

		private readonly IUsersRepository usersRepository;
		private readonly IRolesRepository roles;
		private readonly SessionRepository sessionRepository;
		private readonly IWorkplaceContext context;
		private readonly ServiceClient serviceClient;
		private EzbobMembershipProviderConfigs configs;

		private static readonly ASafeLog log = new SafeILog(typeof(EzbobMembershipProvider));

		#endregion fields

		#endregion private
	} // class EzbobMembershipProvider

	#endregion class EzbobMembershipProvider
} // namespace