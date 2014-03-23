namespace EzBob.Web.Infrastructure.Membership
{
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using System.Text;
	using ApplicationMng.Model;
	using Areas.Customer.Controllers.Exceptions;
	using Code;
	using NHibernate;
	using Newtonsoft.Json;
	using NHibernateWrapper.NHibernate.Model;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Security;
	using ApplicationMng.Repository;
	using StructureMap;
	using log4net;
	using NHibernateWrapper.Web;

	public class UserAlreadyExistsException : Exception
	{
		public UserAlreadyExistsException(string message)
			: base(message)
		{
		}
	}

	public class EzbobMembershipProvider : MembershipProvider
    {
		private class EzbobMembershipProviderConfigs
		{
			public string LoginValidationStringForWeb { get; set; }
			public int NumOfInvalidPasswordAttempts { get; set; }
			public int InvalidPasswordAttemptsPeriodSeconds { get; set; }
			public int InvalidPasswordBlockSeconds { get; set; }
			public string PasswordValidity { get; set; }
			public string LoginValidity { get; set; }
		}

		private static readonly ILog log = LogManager.GetLogger("EzbobMembershipProvider");
		private readonly IUsersRepository usersRepository;
		private readonly IRolesRepository roles;
		private readonly SessionRepository sessionRepository;
		private readonly IWorkplaceContext context;
		private readonly ServiceClient serviceClient;
		private EzbobMembershipProviderConfigs configs;

        public EzbobMembershipProvider()
        {
            roles = ObjectFactory.GetInstance<IRolesRepository>();
	        usersRepository = ObjectFactory.GetInstance<IUsersRepository>();
			sessionRepository = ObjectFactory.GetInstance<SessionRepository>();
			serviceClient = new ServiceClient();
			ReadConfiguration();
        }

		public EzbobMembershipProvider(IWorkplaceContext context, IRolesRepository roles)
        {
			this.context = context;
			this.roles = roles;
			usersRepository = ObjectFactory.GetInstance<IUsersRepository>();
			sessionRepository = ObjectFactory.GetInstance<SessionRepository>();
			serviceClient = new ServiceClient();
			ReadConfiguration();
        }

		private void ReadConfiguration()
		{
			var basicInterestRatesList = serviceClient.Instance.GetSpResultTable("GetUserManagementConfigs", null);
			var deserializedArray = JsonConvert.DeserializeObject<EzbobMembershipProviderConfigs[]>(basicInterestRatesList.SerializedDataTable);
			configs = deserializedArray[0];
		}

		public override MembershipUser CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			try
			{
				var webRoles = roles.GetAll().Where(r => r.Name == "Web").ToList();
				if (!webRoles.Any())
				{
					throw new RoleNotFoundException("Web");
				}

				if (!Regex.IsMatch(userName.ToLower(), configs.LoginValidationStringForWeb))
				{
					throw new Exception("Login does not conform to the passwordsecurity policy");
				}

				UpdateUser(userName, password, email, userName, webRoles, 0, null, null, null, false, false, null, passwordQuestion, passwordAnswer);

				status = MembershipCreateStatus.Success;
				return null;
			}
			catch (UserAlreadyExistsException)
			{
				log.WarnFormat("User with email {0} already exists", userName);
				status = MembershipCreateStatus.DuplicateEmail;
				return null;
			}
			catch (Exception e)
			{
				log.Error("Failed to Create/Update user", e);
				status = MembershipCreateStatus.ProviderError;
				throw;
			}
		}

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
			return InnerChangePassword(username, oldPassword, newPassword);
        }

		private bool InnerChangePassword(string userName, string password, string newPassword)
		{
			if (!PolicyValidatePassword(password))
			{
				return false;
			}
			
			User userByLogin = usersRepository.GetUserByLogin(userName);
			if (userByLogin.DisablePassChange == true)
			{
				return false;
			}

			if (userByLogin.Password == EncodePassword(password, userByLogin.Name, userByLogin.CreationDate) && userByLogin.IsDeleted != 1)
			{
				string encodedNewPassword = EncodePassword(newPassword, userByLogin.Name, userByLogin.CreationDate);

				userByLogin.Password = encodedNewPassword;
				userByLogin.PassSetTime = DateTime.UtcNow;
				userByLogin.LoginFailedCount = null;
				userByLogin.LastBadLogin = null;
				userByLogin.ForcePassChange = null;


				usersRepository.SaveOrUpdate(userByLogin);
				return true;
			}
			
			return false;
		}

        public string GenerateSimplePassword(int size)
        {
            const string passwordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string password = "";
            var random = new Random();

            for (int i = 0; i < size; i++)
            {
                password += passwordChars[random.Next(0, passwordChars.Length)];
            }

            return password;
        }

        public override string ResetPassword(string username, string question)
        {
            var password = GenerateSimplePassword(8);
			InnerResetPassword(username, password);
            return password;
        }

		public void InnerResetPassword(string userName, string password)
		{
			if (PolicyValidatePassword(password))
			{
				User userByLogin = usersRepository.GetUserByLogin(userName);
				if (userByLogin.DisablePassChange != true)
				{
					string newPassword = EncodePassword(password, userByLogin.Name, userByLogin.CreationDate);
					userByLogin.Password = newPassword;
					userByLogin.PassSetTime = DateTime.UtcNow;
					userByLogin.LoginFailedCount = null;
					userByLogin.LastBadLogin = null;
					userByLogin.ForcePassChange = null;
					usersRepository.SaveOrUpdate(userByLogin);
				}
			}
		}

        public override bool ValidateUser(string username, string password)
        {
	        string guid;
            if (LoginUser(username, password, context.SecAppId, HttpContext.Current.Request.UserHostAddress, out guid))
            {
                context.SessionId = guid;
	            return true;
            }

	        return false;
		}

        public override string GetUserNameByEmail(string email)
        {
	        var user = usersRepository.GetUserByLogin(email);
            return user == null ? null : Convert.ToString(user.Id);
        }

		public User UpdateUser(string userName, string password, string fullName, string eMail, List<Role> webRoles, int userId, string certificateThumbprint, string ownerUserName, int? passwordTerm, bool? forcePasswordChange, bool? disablePasswordChange, string domainUserName, string securityQuestion, string securityAnswer)
		{
			bool flag = userId == 0;
			User result;
			try
			{
				User user = flag ? new User {PassSetTime = DateTime.UtcNow} : usersRepository.Get(userId);
				
				certificateThumbprint = (string.IsNullOrEmpty(domainUserName) ? certificateThumbprint : null);
				if (!PolicyValidateLogin(userName.ToLower()))
				{
					throw new Exception("Can't validate login");
				}
				if ((password != null && userId > 0) || flag)
				{
					if (!PolicyValidatePassword(password))
					{
						throw new Exception("Can't validate password");
					}
					password = EncodePassword(password, userName, user.CreationDate);
				}

				if (!usersRepository.CheckUserLogin(userId, userName) || !usersRepository.CheckUserDomainName(userId, userName))
				{
					throw new UserAlreadyExistsException(string.Format("The email {0} already exists", userName));
				}

				user.Roles.Clear();
				user.Roles.AddAll(webRoles);
				user.Name = userName;
				user.FullName = fullName;
				user.CertificateThumbprint = certificateThumbprint;
				user.DomainUserName = domainUserName;
				if (password != null)
				{
					user.Password = password;
				}
				user.EMail = eMail;
				int fetchedUserId = user.Id;
				user.Creator = usersRepository.Get(fetchedUserId);
				user.PassExpPeriod = passwordTerm;
				if (!string.IsNullOrEmpty(securityQuestion))
				{
					user.SecurityQuestion = ObjectFactory.GetInstance<ISession>().Load<SecurityQuestion>(Convert.ToInt32(securityQuestion));
					user.SecurityAnswer = securityAnswer;
				}
				user.DisablePassChange = ((disablePasswordChange == true) ? new bool?(true) : null);
				user.ForcePassChange = ((forcePasswordChange == true) ? new bool?(true) : null);
				usersRepository.Save(user);
				result = user;
			}
			catch (Exception exception)
			{
				log.ErrorFormat("Error while creating a user:{0}", exception);
				throw;
			}
			return result;
		}

		public bool PolicyValidateLogin(string login)
		{
			try
			{
				return Regex.IsMatch(login, configs.LoginValidity);
			}
			catch
			{
				log.WarnFormat("Login:{0} doesn't match:{1}", login, configs.LoginValidity);
				return false;
			}
		}

		public bool PolicyValidatePassword(string password)
		{
			try
			{
				return Regex.IsMatch(password, configs.PasswordValidity);
			}
			catch
			{
				log.WarnFormat("Password:{0} doesn't match:{1}", password, configs.PasswordValidity);
				return false;
			}
		}

		public string EncodePassword(string password, string userName, DateTime creationDate)
		{
			var hMacsha = new HMACSHA1 {Key = GetKey()};
			string s = userName.ToUpperInvariant() + password + creationDate.ToString("dd-MM-yyyy hh:mm:ss");
			return Convert.ToBase64String(hMacsha.ComputeHash(Encoding.Unicode.GetBytes(s)));
		}

		public string EncodeOldPassword(string password)
		{
			var hMacsha = new HMACSHA1 {Key = GetKey()};
			return Convert.ToBase64String(hMacsha.ComputeHash(Encoding.Unicode.GetBytes(password)));
		}

		private byte[] GetKey()
		{
			byte[] array = new byte[64];
			array[0] = 217;
			array[1] = 197;
			array[2] = 36;
			array[3] = 73;
			array[4] = 245;
			array[5] = 170;
			array[6] = 52;
			array[7] = 86;
			array[8] = 16;
			array[9] = 196;

			array[10] = 190;
			array[11] = 197;
			array[12] = 158;
			array[13] = 222;
			array[14] = 60;
			array[15] = 108;
			array[16] = 212;
			array[17] = 45;
			array[18] = 234;
			array[19] = 232;

			array[20] = 27;
			array[21] = 169;
			array[22] = 165;
			array[23] = 13;
			array[24] = 12;
			array[25] = 242;
			array[26] = 30;
			array[27] = 203;
			array[28] = 10;
			array[29] = 229;

			array[30] = 81;
			array[31] = 42;
			array[32] = 201;
			array[33] = 35;
			array[34] = 31;
			array[35] = 194;
			array[36] = 112;
			array[37] = 159;
			array[38] = 161;
			array[39] = 77;

			array[40] = 44;
			array[41] = 125;
			array[42] = 4;
			array[43] = 25;
			array[44] = 109;
			array[45] = 92;
			array[46] = 211;
			array[47] = 39;
			array[48] = 80;
			array[49] = 117;

			array[50] = 230;
			array[51] = 173;
			array[52] = 106;
			array[53] = 87;
			array[54] = 105;
			array[55] = 195;
			array[56] = 62;
			array[57] = 171;
			array[58] = 89;
			array[59] = 189;

			array[60] = 230;
			array[61] = 39;
			array[62] = 60;
			array[63] = 148;

			return array;
		}


		public bool LoginUser(string login, string password, int securityAppId, string hostAddress, out string guid)
        {
		    User userByLogin = usersRepository.GetUserByLogin(login);
		    if (userByLogin == null)
		    {
			    throw new UserNotFoundException(login);
		    }

		    string passwordHash = EncodePassword(password, userByLogin.Name, userByLogin.CreationDate);
		    string oldModelPasswordHash = EncodeOldPassword(password);

			if (InnerLoginUser(userByLogin, passwordHash, oldModelPasswordHash))
            {
				guid = DoLogin(securityAppId, userByLogin.Id, hostAddress);
	            return true;
            }

			guid = string.Empty;
			return false;
        }

		private string DoLogin(int securityAppId, int userId, string hostAddress)
		{
			if (!usersRepository.HasAccesTo(userId, securityAppId))
			{
				return string.Empty;
			}

			return StartSession(userId, securityAppId, hostAddress);
		}

		public string StartSession(int userId, int secAppId, string hostAddress)
		{
			SecuritySession session = sessionRepository.CreateNewSession(userId, secAppId);
            session.HostAddress = hostAddress;
            sessionRepository.Save(session);
            return session.Id;
        }

		public bool InnerLoginUser(User user, string passwordHash, string oldModelPasswordHash)
		{
			if (user.IsDeleted != 0)
			{
				return false;
			}
			if (user.DisableDate.HasValue && user.DisableDate.Value < DateTime.UtcNow)
			{
				user.IsDeleted = 2;
				usersRepository.Update(user);
				return false;
			}
			if (user.ForcePassChange.HasValue && user.ForcePassChange.Value) 
			{
				return false;
			}
			if (user.PassSetTime.HasValue && user.PassExpPeriod.HasValue && user.PassSetTime.Value.Add(TimeSpan.FromSeconds(user.PassExpPeriod.Value)) < DateTime.UtcNow)
			{
				return false;
			}
			if (user.LastBadLogin.HasValue)
			{
				TimeSpan span = (user.LoginFailedCount.GetValueOrDefault() >= configs.NumOfInvalidPasswordAttempts) ? 
					TimeSpan.FromSeconds(configs.InvalidPasswordBlockSeconds) : 
					TimeSpan.FromSeconds(configs.InvalidPasswordAttemptsPeriodSeconds);
				if (DateTime.UtcNow.Subtract(user.LastBadLogin.Value) > span)
				{
					user.LastBadLogin = null;
					user.LoginFailedCount = 0;
					usersRepository.Update(user);
				}
			}
			if (user.LoginFailedCount.HasValue && user.LoginFailedCount.Value > configs.NumOfInvalidPasswordAttempts)
			{
				return false;
			}
			if ((user.Password != passwordHash) && (user.Password != oldModelPasswordHash))
			{
				user.LoginFailedCount = user.LoginFailedCount.GetValueOrDefault() + 1;
				user.LastBadLogin = DateTime.UtcNow;
				usersRepository.Update(user);

				if (user.LoginFailedCount.HasValue && user.LoginFailedCount.Value >= configs.NumOfInvalidPasswordAttempts)
				{
					return false;
				}

				return false;
			}
			if (user.Password == oldModelPasswordHash)
			{
				user.Password = passwordHash;
				usersRepository.Update(user);
			}
			return true;
		}

		public bool IsEqualsOldPassword(string login, string newPassword)
		{
			User userByLogin = usersRepository.GetUserByLogin(login);
			string encodedNewPassword = EncodePassword(newPassword, userByLogin.Name, userByLogin.CreationDate);
			return userByLogin.Password == encodedNewPassword;
		}

		public string ChangeEmailAndPassword(User user, string newEmail)
		{
			string newPassword = GenerateSimplePassword(16);
			user.EMail = newEmail;
			user.Name = newEmail;
			user.FullName = newEmail;
			user.Password = EncodePassword(newPassword, user.Name, user.CreationDate);
			user.PassSetTime = DateTime.UtcNow;
			usersRepository.SaveOrUpdate(user);

			return newPassword;
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override bool EnablePasswordRetrieval
		{
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordReset
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { throw new NotImplementedException(); }
		}

		public override string ApplicationName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { throw new NotImplementedException(); }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { throw new NotImplementedException(); }
		}
    }
}