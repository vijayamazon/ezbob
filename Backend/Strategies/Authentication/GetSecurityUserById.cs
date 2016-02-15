namespace Ezbob.Backend.Strategies.Authentication {
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.Authentication;
    using Ezbob.Database;

    public class GetSecurityUser : AStrategy {
        public override string Name { get { return "Get Users"; } }

        public GetSecurityUser(string userName, int? originID) {
            UserName = userName;
            OriginID = originID;
        }

        public User User { get; set; }
        public string UserName { get; set; }
        public int? OriginID { get; set; }

        public override void Execute() {

            //get user data
            var originIdText = (OriginID == null) ? "is null" : string.Format("='{0}'", OriginID);
            User = DB.FillFirst<User>(string.Format("select top 1 su.UserId,su.UserName,su.OriginID from Security_UserRoleRelation ur join Security_User su on ur.UserId = su.UserId where su.UserName = '{0}' and su.OriginID {1}", UserName, originIdText), CommandSpecies.Text);            

            //get roles id's of that user
            var userRulesRel = DB.Fill<UserRoleRelation>(string.Format("select UserId, RoleId from Security_UserRoleRelation where UserId = {0}", User.UserId), CommandSpecies.Text);

            //get the actual roles of that user
            var userRolesIds = string.Join(",", userRulesRel.Select(x => x.RoleId));
            User.UserRoles = DB.Fill<Role>(string.Format("Select RoleId, Name, Description  from Security_Role where RoleId in ({0})", userRolesIds), CommandSpecies.Text);

            //get permissions id's of that user rolles
            var rolePermissionRels = DB.Fill<RolePermissionRelation>(string.Format("Select RoleId, PermissionId from Security_RolePermissionRel where RoleId in ({0})", userRolesIds), CommandSpecies.Text);

            //get the actual permissions of that user rolles.
            var userPermissionsIds = string.Join(",", rolePermissionRels.Select(x => x.PermissionId));
            var userPermissions = DB.Fill<Permission>(string.Format("Select Id, Name, Description from Security_Permission where id in ({0})", userPermissionsIds), CommandSpecies.Text);

            foreach (var role in User.UserRoles) {
                var rolePermissionsIds = rolePermissionRels.Where(x => x.RoleId == role.RoleId).Select(x => x.PermissionId).ToList();
                if (rolePermissionsIds.Any()) {
                    role.Premissions = new List<Permission>();
                    foreach (var permissionsId in rolePermissionsIds) {
                        var permission = userPermissions.FirstOrDefault(x => x.Id == permissionsId);
                        role.Premissions.Add(permission);
                    }
                }

            }
        }
    }
}
