namespace EzBob.Web.Infrastructure
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Security.Cryptography;
	using System.Text;
	using ApplicationMng.Model;
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

        private readonly IWorkplaceContext _context;
        private readonly UserManager _userManager;
        private readonly IRolesRepository _roles;

        public EzbobMembershipProvider()
        {
            _roles = ObjectFactory.GetInstance<IRolesRepository>();
        }

		public EzbobMembershipProvider(IWorkplaceContext context, UserManager userManager, IRolesRepository roles)
        {
            _context = context;
            _userManager = userManager;
            _roles = roles;
        }

        public override MembershipUser CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            try
            {
                var roles = _roles.GetAll().Where(r => r.Name == "Web").ToList();
                if(!roles.Any())
                {
                    throw new RoleNotFoundException("Web");
                }
                SecurityPolicyConfiguration config = ConfigurationRootWeb.GetConfiguration().SecurityPolicy;
                if (!Regex.IsMatch(userName.ToLower(), config.LoginValidationStringForWeb))
                {
                    throw new Exception("Login does not conform to the passwordsecurity policy");
                }
				var user = _userManager.UpdateUser(userName, password, email, userName, roles, 0, null, null, null, false, false, false, null, passwordQuestion, passwordAnswer);

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
    }
}