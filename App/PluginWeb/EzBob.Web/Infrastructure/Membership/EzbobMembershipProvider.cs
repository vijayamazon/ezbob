namespace EzBob.Web.Infrastructure
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Security.Cryptography;
	using System.Text;
	using ApplicationMng.Model;
	using Areas.Customer.Controllers.Exceptions;
	using Iesi.Collections.Generic;
	using NHibernate;
	using Scorto.NHibernate.Model;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Security;
	using ApplicationMng.Repository;
	using Scorto.Configuration;
	using Scorto.Security.UserManagement;
	using Scorto.Web;
	using StructureMap;
	using log4net;
	using Scorto.Security.UserManagement.Resources;

	public class EzbobMembershipProvider : MembershipProvider
    {
		private static readonly ILog log = LogManager.GetLogger("EzbobMembershipProvider");
		private readonly IUsersRepository usersRepository;
		private readonly IRolesRepository roles;
		private readonly SessionRepository sessionRepository;

        private readonly IWorkplaceContext _context;
        private readonly UserManager _userManager;

        public EzbobMembershipProvider()
        {
            roles = ObjectFactory.GetInstance<IRolesRepository>();
	        usersRepository = ObjectFactory.GetInstance<IUsersRepository>();
			sessionRepository = ObjectFactory.GetInstance<SessionRepository>();
        }

		public EzbobMembershipProvider(IWorkplaceContext context, UserManager userManager, IRolesRepository roles)
        {
            _context = context;
            _userManager = userManager;
			this.roles = roles;
			usersRepository = ObjectFactory.GetInstance<IUsersRepository>();
			sessionRepository = ObjectFactory.GetInstance<SessionRepository>();
        }

        public override MembershipUser CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            try
            {
                var roles = this.roles.GetAll().Where(r => r.Name == "Web").ToList();
                if(!roles.Any())
                {
                    throw new RoleNotFoundException("Web");
                }

				string loginValidationStringForWeb = @"(^((([a-z]|\d|[!#\$%'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$)";
				if (!Regex.IsMatch(userName.ToLower(), loginValidationStringForWeb))
                {
                    throw new Exception("Login does not conform to the passwordsecurity policy");
                }
				var user = _userManager.UpdateUser(userName, password, email, userName, roles, 0, null, null, null, false, false, false, null, passwordQuestion, passwordAnswer);

				// TODO: replace with
				//var user = UpdateUser(userName, password, email, userName, roles, 0, null, null, null, false, false, null, passwordQuestion, passwordAnswer);

                status = MembershipCreateStatus.Success;
                return new MembershipUser("SimpleMembershipProvider",
                                          user.Name,
                                          user.Id,                                         
                                          user.EMail,
                                          "", "", false, false, user.CreationDate, DateTime.MinValue, DateTime.MinValue,
                                          DateTime.MinValue, DateTime.MinValue);
            }
            catch (UserAlreadyExistsException )
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

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new System.NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new System.NotImplementedException();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            _userManager.ChangePassword(username, oldPassword, newPassword);
            return true;
        }
        //------------------------------------------------------------------------            
        public string GenerateSimplePassword(int size) //If you need a strong password that will be used Membership.GeneratePassword
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
        //------------------------------------------------------------------------            
        public override string ResetPassword(string username, string question)
        {
            var password = GenerateSimplePassword(8);
            _userManager.ResetPassword(username, password);
            return password;
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new System.NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
		{
			var res = _userManager.LoginUser(username, password, _context.SecAppId, HttpContext.Current.Request.UserHostAddress);
			
			// TODO: replace with
			//var res = LoginUser(username, password, _context.SecAppId, HttpContext.Current.Request.UserHostAddress);
            if (res.Result ==  LoginStatus.LoginOk)
            {
                _context.SessionId = res.Guid;
            }
            return res.Result == LoginStatus.LoginOk;
        }

        public override bool UnlockUser(string userName)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {            
            throw new System.NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new System.NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
			// can get from db - server side
            var userId = _userManager.GetUserIdByLogin(email);
            return userId == 0 ? null : Convert.ToString(userId);            
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool EnablePasswordReset
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string ApplicationName
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new System.NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new System.NotImplementedException(); }
        }



		public User UpdateUser(string userName, string password, string fullName, string eMail, List<Role> roles, int userId, string certificateThumbprint, string ownerUserName, int? passwordTerm, bool? forcePasswordChange, bool? disablePasswordChange, string domainUserName, string securityQuestion, string securityAnswer)
		{
			bool flag = userId == 0;
			User result;
			try
			{
				ISession instance = ObjectFactory.GetInstance<ISession>();
				User user;
				
				if (flag)
				{
					user = new User() {PassSetTime = new DateTime?(DateTime.UtcNow)};
				}
				else
				{
					user = usersRepository.Get(userId);
				}
				
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
					//password = EncodeOldPassword(password);
				}

				if (!this.usersRepository.CheckUserLogin(userId, userName))
				{
					throw new UserAlreadyExistsException(string.Format(Security.USER_ALREADY_EXISTS_STR, userName));
				}

				if (!this.usersRepository.CheckUserDomainName(userId, userName))
				{
					throw new UserAlreadyExistsException(string.Format(Security.SECURITY_USER_NullsTrigger, new object[0]));
				}
				HashedSet<Role> hashedSet = new HashedSet<Role>(user.Roles);
				user.Roles.Clear();
				user.Roles.AddAll(roles);
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
					user.SecurityQuestion = instance.Load<SecurityQuestion>(Convert.ToInt32(securityQuestion));
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
			string s = @"(^[.\-@_a-zA-Z0-9\\]{1,40}$)|(^((([a-z]|\d|[!#\$%'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$)";
			try
			{
				return Regex.IsMatch(login, s);
			}
			catch
			{
				log.WarnFormat("Login:{0} doesn't match:{1}", login, s);
				return false;
			}
		}

		public bool PolicyValidatePassword(string password)
		{
			string s = @"(^.{6,}$)";
			try
			{
				return Regex.IsMatch(password, s);
			}
			catch
			{
				log.WarnFormat("Password:{0} doesn't match:{1}", password, s);
				return false;
			}
		}

		public string EncodePassword(string password, string userName, DateTime creationDate)
		{
			HMACSHA1 hMACSHA = new HMACSHA1();
			hMACSHA.Key = GetKey();
			string s = userName.ToUpperInvariant() + password + creationDate.ToString("dd-MM-yyyy hh:mm:ss");
			return Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.Unicode.GetBytes(s)));
		}

		public string EncodeOldPassword(string password)
		{
			HMACSHA1 hMACSHA = new HMACSHA1();
			hMACSHA.Key = GetKey();
			return Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.Unicode.GetBytes(password)));
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

		
        public AuthenticationResult LoginUser(string login, string password, int securityAppId, string hostAddress)
        {
	        AuthenticationResult result = new AuthenticationResult {
                SecAppId = securityAppId,
                Result = LoginStatus.LoginFailed
            };

	        try
	        {

		        User userByLogin = this.usersRepository.GetUserByLogin(login);
		        if (userByLogin == null)
		        {
			        throw new UserNotFoundException(login);
		        }

		        string passwordHash = this.EncodePassword(password, userByLogin.Name, userByLogin.CreationDate);
		        string oldModelPasswordHash = this.EncodeOldPassword(password);

				result.Result = this.InnerLoginUser(userByLogin, passwordHash, oldModelPasswordHash);
				if (result.Result == LoginStatus.LoginOk)
                {
					stx(securityAppId, result, userByLogin.Id, hostAddress);
                }
                if (result.Result == LoginStatus.LoginFailed)
                {
                    //Log4NetExtender.ErrorJournal(, 10, 0, .(), new object[] { userByLogin.get_Name(), ThreadContext.get_Properties().get_Item(.(-2066775471)), ThreadContext.get_Properties().get_Item(.(-2066775482)) });
                }
            }
            catch (Exception exception)
            {
				//Log4NetExtender.ErrorJournal(, 10, 0, .(), new object[] { login, ThreadContext.get_Properties().get_Item(.(-2066775471)), ThreadContext.get_Properties().get_Item(.(-2066775482)) });
				//.Error(exception);
				//result.Result = LoginStatus.LoginFailed;
				//result.Message = exception.get_Message();
            }
			return result;
        }
		private void stx(int stxi, AuthenticationResult etx, int enq, string bs)
		{
			if (!usersRepository.HasAccesTo(enq, stxi))
			{
				etx.Result = LoginStatus.AccessDenied;
			}
			else
			{
				etx.Guid = StartSession(enq, stxi, bs);
			}
		}

		public string StartSession(int userId, int secAppId, string hostAddress)
		{
			SecuritySession session = sessionRepository.CreateNewSession(userId, secAppId);
            session.HostAddress = hostAddress;
            sessionRepository.Save(session);
            //(1, new SessionStateChangedArguments(session.get_Id(), userId));
            return session.Id;
        }

		public LoginStatus InnerLoginUser(User user, string passwordHash, string oldModelPasswordHash)
		{
			int invalidPasswordAttempts = 3; //TODO: form config
			int invalidPasswordAttemptsPeriodSeconds = 30; //TODO: form config
			int invalidPasswordBlockSeconds = 30; //TODO: form config
			if (user.IsDeleted != 0)
			{
				return LoginStatus.LoginLocked;
			}
			if (user.DisableDate.HasValue && user.DisableDate.Value < DateTime.UtcNow)
			{
				user.IsDeleted = 2;
				usersRepository.Update(user);
				return LoginStatus.LoginLocked;
			}
			if (user.ForcePassChange.HasValue && user.ForcePassChange.Value) 
			{
				return LoginStatus.NeedPasswordChange;
			}
			if (user.PassSetTime.HasValue && user.PassExpPeriod.HasValue && user.PassSetTime.Value.Add(TimeSpan.FromSeconds(user.PassExpPeriod.Value)) < DateTime.UtcNow)
			{
				return LoginStatus.NeedPasswordChange;
			}
			if (user.LastBadLogin.HasValue)
			{
				TimeSpan span = (user.LoginFailedCount.GetValueOrDefault() >= invalidPasswordAttempts) ? 
					TimeSpan.FromSeconds(invalidPasswordBlockSeconds) : 
					TimeSpan.FromSeconds(invalidPasswordAttemptsPeriodSeconds);
				if (DateTime.UtcNow.Subtract(user.LastBadLogin.Value) > span)
				{
					user.LastBadLogin = null;
					user.LoginFailedCount = 0;
					usersRepository.Update(user);
				}
			}
			if (user.LoginFailedCount.HasValue && user.LoginFailedCount.Value > invalidPasswordAttempts)
			{
				return LoginStatus.ExceededMaxLoginAttempts;
			}
			if ((user.Password != passwordHash) && (user.Password != oldModelPasswordHash))
			{
				user.LoginFailedCount++;
				user.LastBadLogin = DateTime.UtcNow;
				usersRepository.Update(user);

				if (user.LoginFailedCount.HasValue && user.LoginFailedCount.Value >= invalidPasswordAttempts)
				{
					return LoginStatus.ExceededMaxLoginAttempts;
				}
				
				return LoginStatus.LoginFailed;
			}
			if (user.Password == oldModelPasswordHash)
			{
				user.Password = passwordHash;
				usersRepository.Update(user);
			}
			return LoginStatus.LoginOk;
		}
    }
}