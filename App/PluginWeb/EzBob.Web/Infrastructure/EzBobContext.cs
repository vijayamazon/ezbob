namespace EzBob.Web.Infrastructure {
	using System;
	using System.Web;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	public class EzBobContext : IEzbobWorkplaceContext {
		public EzBobContext(IUsersRepository users, ICustomerRepository customers) {
			this.userRepo = users;
			this.customerRepo = customers;
		} // constructor

		public User User {
			get {
				User user = GetCachedUser();

				if (user != null)
					return user;

				var originHolder = HttpContext.Current.Session[SessionOriginIDName] as OriginHolder;

				if (originHolder == null)
					return null;

				user = this.userRepo.GetUserByLogin(HttpContext.Current.User.Identity.Name, originHolder.Origin);

				SetCachedUser(user);

				return user;
			} // get
		} // User

		public void SetSessionOrigin(CustomerOriginEnum? originID) {
			HttpContext.Current.Session[SessionOriginIDName] = new OriginHolder(originID);
		} // SetSessionOrigin

		public void RemoveSessionOrigin() {
			HttpContext.Current.Session.Remove(SessionOriginIDName);
		} // RemoveSessionOrigin

		public int UserId {
			get { return User.Id; }
		} // UserId

		public string SessionId {
			get {
				var cookie = HttpContext.Current.Request.Cookies[SessionCookieName];
				return cookie == null ? string.Empty : cookie.Value;
			} // get

			set {
				var httpCookie = new HttpCookie(SessionCookieName, value) { HttpOnly = true, Secure = true };

				if (value == null)
					httpCookie.Expires = DateTime.UtcNow.AddDays(-1);

				HttpContext.Current.Response.SetCookie(httpCookie);
			} // set
		} // SessionId

		public Customer Customer {
			get { return User == null ? null : this.customerRepo.ReallyTryGet(User.Id); }
		} // Customer

		private static User GetCachedUser() {
			return HttpContext.Current.Items[RequestUserItemName] as User;
		} // GetCachedUser

		private static void SetCachedUser(User u) {
			HttpContext.Current.Items[RequestUserItemName] = u;
		} // SetCachedUser

		private class OriginHolder {
			public OriginHolder(CustomerOriginEnum? originID) {
				if (originID == null)
					this.hasValue = false;
				else {
					this.hasValue = true;
					this.origin = originID.Value;
				} // if
			} // constructor

			public int? Origin {
				get { return this.hasValue ? (int)this.origin : (int?)null; }
			} // Origin

			private readonly bool hasValue;
			private readonly CustomerOriginEnum origin;
		} // class OriginHolder

		private const string SessionCookieName = "_EzbobSession_";
		private const string SessionOriginIDName = "__EzbobSessionOriginID__";
		private const string RequestUserItemName = "__EzbobCurrentUser__";

		private readonly IUsersRepository userRepo;
		private readonly ICustomerRepository customerRepo;
	} // class EzBobContext
} // namespace