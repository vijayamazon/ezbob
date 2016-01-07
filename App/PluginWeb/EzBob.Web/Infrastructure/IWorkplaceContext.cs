namespace EzBob.Web.Infrastructure {
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	public interface IWorkplaceContext {
		User User { get; }
		int UserId { get; }
		List<string> UserRoles { get; }
		List<Permission> UserPermissions { get; }
		string SessionId { get; set; }
		void SetSessionOrigin(CustomerOriginEnum? originID);
		CustomerOriginEnum? GetSessionOrigin();
		void RemoveSessionOrigin();
	} // interface IWorkplaceContext
} // namespace
