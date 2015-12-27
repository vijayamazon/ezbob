namespace EzBob.Web.Infrastructure.Membership {
	using System;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using ServiceClientProxy;

	public class EzbobRoleProvider : System.Web.Security.RoleProvider {
		public EzbobRoleProvider() {
			this.cache = new Cache<string, string[]>(
				TimeSpan.FromMinutes(25),
				userName => {
					var log = new SafeILog(this);

					log.Debug("Loading all the roles for login '{0}'...", userName);

					var result = new ServiceClient().Instance.LoadAllLoginRoles(userName).Records;

					log.Debug(
						"Login '{0}' has {1}{2}.",
						userName,
						Grammar.Number(result.Length, "role"),
						result.Length < 1 ? string.Empty : ": " + string.Join(", ", result)
					);
					return result;
				}
			);
		} // constructor

		public override bool IsUserInRole(string username, string roleName) {
			throw new System.NotImplementedException();
		}

		public override string[] GetRolesForUser(string username) {
			return this.cache[username];
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

		private readonly Cache<string, string[]> cache;
	} // class EzbobRoleProvider
} // namespace
