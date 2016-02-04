namespace LegalDocs.Code.Infra {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class PermissionsAndRoles {
        public struct Result {
            public string BodyClassName { get; set; }
            public string ErrorMessage { get; set; }
        } // Result

        public static Result Fetch() {
            var result = new Result();

            var userPermissions = new List<string>();
            string sPermissions = string.Empty;
            string sRoles = string.Empty;
            try {
                var name = HttpContext.Current.User.Identity.Name;
                var user = Session.Instance.Users.FirstOrDefault(x => x.UserName == name);
                if (user != null) {
                    foreach (var role in user.UserRoles) {
                        if (role.Premissions != null) {
                            userPermissions.AddRange(role.Premissions.Select(x => x.Name));
                        }
                    }
                    sPermissions = userPermissions.Count < 1 ? string.Empty : string.Join(" ", userPermissions.Distinct().Select(p => "permission-" + p));
                    sRoles = user.UserRoles.Count < 1 ? "" : string.Join(" ", user.UserRoles.Select(x => x.Name).Select(r => "role-" + r));
                }
                result.BodyClassName = sRoles + " " + sPermissions;
            }
            catch (Exception e) {
                result.ErrorMessage = "Error fetching user permissions and roles: " + e.Message;
            } // try

            return result;
        } // Fetch
    } // class PermissionsAndRoles
} // namespace
