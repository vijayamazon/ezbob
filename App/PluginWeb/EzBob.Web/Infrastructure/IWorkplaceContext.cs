namespace EzBob.Web.Infrastructure {
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	public interface IWorkplaceContext {
		User User { get; }
		int UserId { get; }
		string SessionId { get; set; }
		void SetSessionOrigin(CustomerOriginEnum? originID);
		void RemoveSessionOrigin();
	} // interface IWorkplaceContext
} // namespace
