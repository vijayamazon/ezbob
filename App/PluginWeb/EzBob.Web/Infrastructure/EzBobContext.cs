namespace EzBob.Web.Infrastructure {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using Ezbob.Logger;
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
				var log = new List<string>();

				log.Add(string.Format(
					"Request:\n\thost: {0}\n\tpath and query: {1}\n\tabsolute path: {2}",
					HttpContext.Current.Request.Url.Host,
					HttpContext.Current.Request.Url.PathAndQuery,
					HttpContext.Current.Request.Url.AbsolutePath
				));

				User user = GetCachedUser();

				log.Add(string.Format("Cached user is {0}", user == null ? "-- null --" : user.Id.ToString()));

				if (user != null) {
					// FlushLog(log);
					return user;
				} // if

				var originHolder = InternalGetSessionOrigin();

				log.Add(string.Format(
					"Cached origin holder is {0}", originHolder == null ? "-- null --" : originHolder.ToString()
				));

				if (originHolder == null) {
					const string uwArea = "/underwriter/";

					if (UrlStartsWith(HttpContext.Current, uwArea)) {
						SetSessionOrigin(null);
						originHolder = InternalGetSessionOrigin();

						log.Add(string.Format(
							"Underwriter origin holder is '{0}'",
							originHolder == null ? "-- null --" : originHolder.ToString()
						));
					} // if
				} // if

				if (originHolder == null) {
					const string hearbeat = "/heartbeat";

					if (!UrlStartsWith(HttpContext.Current, hearbeat)) {
						SetSessionOrigin(UiCustomerOrigin.Get(HttpContext.Current.Request.Url).GetOrigin());

						originHolder = InternalGetSessionOrigin();

						log.Add(string.Format(
							"Detected origin holder is '{0}'",
							originHolder == null ? "-- null --" : originHolder.ToString()
						));
					} // if
				} // if

				if (originHolder == null) {
					FlushLog(log);
					return null;
				} // if

				user = this.userRepo.GetUserByLogin(HttpContext.Current.User.Identity.Name, originHolder.Origin);

				log.Add(string.Format("User by login is {0}", user == null ? "-- null --" : user.Id.ToString()));

				SetCachedUser(user);

				if (user == null)
					FlushLog(log);

				return user;
			} // get
		} // User

		public void SetSessionOrigin(CustomerOriginEnum? originID) {
			HttpContext.Current.Session[SessionOriginIDName] = new OriginHolder(originID);
		} // SetSessionOrigin

		public CustomerOriginEnum? GetSessionOrigin() {
			var originHolder = InternalGetSessionOrigin();

			return originHolder == null ? null : originHolder.RawOrigin;
		} // GetSessionOrigin

		public void RemoveSessionOrigin() {
			HttpContext.Current.Session.Remove(SessionOriginIDName);
		} // RemoveSessionOrigin

		public int UserId {
			get { return User == null ? 0 : User.Id; }
		} // UserId

		public List<Permission> UserPermissions {
			get {
				User user = User;

				if (user == null)
					return new List<Permission>();

				return new List<Permission>(user.Permissions);
			} // get
		} // UserPermissions

		public List<string> UserRoles {
			get {
				User user = User;

				if (user == null)
					return new List<string>();

				return new List<string>(user.Roles.Select(r => r.Name));
			} // get
		} // UserRoles

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

		private OriginHolder InternalGetSessionOrigin() {
			return HttpContext.Current.Session[SessionOriginIDName] as OriginHolder;
		} // InternalGetSessionOrigin

		private void FlushLog(List<string> lst) {
			new SafeILog(this).Debug(
				"\n\nEzbobContext.get_User - result is null - begin:\n\n{0}" +
				"\n\nEzbobContext.get_User - result is null - end.\n",
				string.Join("\n", lst)
			);
		} // FlushLog

		private static User GetCachedUser() {
			return HttpContext.Current.Items[RequestUserItemName] as User;
		} // GetCachedUser

		private static void SetCachedUser(User u) {
			if (u == null)
				HttpContext.Current.Items.Remove(RequestUserItemName);
			else
				HttpContext.Current.Items[RequestUserItemName] = u;
		} // SetCachedUser

		private static bool UrlStartsWith(HttpContext httpContext, string withWhat) {
			string path = httpContext.Request.Url.AbsolutePath;

			if (path.Length >= withWhat.Length) {
				string prefix = path.Length == withWhat.Length ? path : path.Substring(0, withWhat.Length);

				if (prefix.ToLowerInvariant().StartsWith(withWhat))
					return true;
			} // if

			return false;
		} // UrlStartsWith

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

			public CustomerOriginEnum? RawOrigin {
				get { return this.hasValue ? this.origin : (CustomerOriginEnum?)null; }
			} // RawOrigin

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