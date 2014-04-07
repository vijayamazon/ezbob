using System;
using System.Linq;
using ApplicationMng.Repository;
using StructureMap;

namespace EzBob.Web.Infrastructure {
	using Ezbob.Utils;

	public class EzbobRoleProvider : System.Web.Security.RoleProvider {
		private readonly Cache<string, string[]> m_oCache;  

		public EzbobRoleProvider() {
			m_oCache = new Cache<string, string[]>(TimeSpan.FromMinutes(25), sUserName => {
				var users = ObjectFactory.GetInstance<IUsersRepository>();
				var user = users.GetUserByLogin(sUserName);
				return user == null ? new string[0] : user.Roles.Select(r => r.Name).ToArray();
			});
		} // constructor

		public override bool IsUserInRole(string username, string roleName) {
			throw new System.NotImplementedException();
		}

		public override string[] GetRolesForUser(string username) {
			return m_oCache[username];
		} // GetRolesForUser

		public override void CreateRole(string roleName) {
			throw new System.NotImplementedException();
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) {
			throw new System.NotImplementedException();
		}

		public override bool RoleExists(string roleName) {
			throw new System.NotImplementedException();
		}

		public override void AddUsersToRoles(string[] usernames, string[] roleNames) {
			throw new System.NotImplementedException();
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) {
			throw new System.NotImplementedException();
		}

		public override string[] GetUsersInRole(string roleName) {
			throw new System.NotImplementedException();
		}

		public override string[] GetAllRoles() {
			throw new System.NotImplementedException();
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch) {
			throw new System.NotImplementedException();
		}

		public override string ApplicationName {
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}
	} // class EzbobRoleProvider
} // namespace
