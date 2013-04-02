using System;
using System.Linq;
using ApplicationMng.Repository;
using GenericCache;
using StructureMap;

namespace EzBob.Web.Infrastructure
{
    public class ScortoRoleProvider : System.Web.Security.RoleProvider
    {
        private readonly GenericAspCache<string[]> _cache;

        public ScortoRoleProvider()
        {
            _cache = new GenericAspCache<string[]>(TimeSpan.FromMinutes(25));
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            return _cache.GetElement(username, key =>
                {
                    var users = ObjectFactory.GetInstance<IUsersRepository>();
                    var user = users.GetUserByLogin(username);
                    return user == null ? new string[0] : user.Roles.Select(r => r.Name).ToArray();
                });
        }

        public override void CreateRole(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new System.NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new System.NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new System.NotImplementedException();
        }

        public override string ApplicationName
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }
    }
}