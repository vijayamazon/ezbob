namespace EzBob.Web.Infrastructure {
	using System;
	using System.Web;
	// using Ezbob.Logger;
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
				// var log = new SafeILog(this);

				User user = GetCachedUser();

				// log.Debug("Cached user is {0}", user == null ? "-- null --" : user.Id.ToString());

				if (user != null)
					return user;

				var originHolder = GetSessionOrigin();

				// log.Debug("Cached origin holder is {0}", originHolder == null ? "-- null --" : originHolder.ToString());

				if (originHolder == null) {
					const string uwArea = "/underwriter/";

					var path = HttpContext.Current.Request.Url.PathAndQuery;
					if (path.Length >= uwArea.Length) {
						if (path.Substring(0, uwArea.Length).ToLowerInvariant().StartsWith(uwArea)) {
							SetSessionOrigin(null);
							originHolder = GetSessionOrigin();

							//log.Debug(
							//	"Underwriter origin holder is '{0}'",
							//	originHolder == null ? "-- null --" : originHolder.ToString()
							//);
						} // if
					} // if
				} // if

				if (originHolder == null) {
					SetSessionOrigin(UiCustomerOrigin.Get(HttpContext.Current.Request.Url).GetOrigin());

					originHolder = GetSessionOrigin();

					//log.Debug(
					//	"Detected origin holder is '{0}'",
					//	originHolder == null ? "-- null --" : originHolder.ToString()
					//);
				} // if

				if (originHolder == null)
					return null;

				user = this.userRepo.GetUserByLogin(HttpContext.Current.User.Identity.Name, originHolder.Origin);

				// log.Debug("User by login is {0}", user == null ? "-- null --" : user.Id.ToString());

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

		private OriginHolder GetSessionOrigin() {
			return HttpContext.Current.Session[SessionOriginIDName] as OriginHolder;
		} // GetSessionOrigin

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

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format("origin value is {0}", Origin == null ? "-- null --" : Origin.Value.ToString());
			} // ToString

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