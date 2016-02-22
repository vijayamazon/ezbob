namespace EzBob.Web.Infrastructure {
	using System;
	using System.Linq;
	using StructureMap;

	public class PermissionsAndRoles {
		public struct Result {
			public string BodyClassName { get; set; }
			public string ErrorMessage { get; set; }
		} // Result

		public static PermissionsAndRoles.Result Fetch() {
			var result = new Result();

			try {
				var context = ObjectFactory.GetInstance<IWorkplaceContext>();

				if ((context != null) && (context.User != null)) {
					string sPermissions = context.UserPermissions.Count < 1
						? ""
						: string.Join(" ", context.UserPermissions.Select(p => "permission-" + p.Name));

					string sRoles = context.UserRoles.Count < 1 ? "" : string.Join(" ", context.UserRoles.Select(r => "role-" + r));

					result.BodyClassName = sRoles + " " + sPermissions;
				}
			} catch (Exception e) {
				result.ErrorMessage = "Error fetching user permissions and roles: " + e.Message;
			} // try

			return result;
		} // Fetch
	} // class PermissionsAndRoles
} // namespace
