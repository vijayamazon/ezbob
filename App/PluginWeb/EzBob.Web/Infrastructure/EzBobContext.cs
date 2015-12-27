namespace EzBob.Web.Infrastructure {
	using System;
	using System.Linq;
	using System.Web;
	using System.Web.Routing;
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
				var cached = HttpContext.Current.Items[UserItemName] as User;
				if (cached != null)
					return cached;

				var login = HttpContext.Current.User.Identity.Name;
				int? originID = null;

				RouteData routeData = HttpContext.Current.Request.RequestContext.RouteData;

				if (routeData.Values.Count > 0) {
					string area = routeData.DataTokens["area"] as string;

					if (this.originAreas.Contains((area ?? string.Empty).ToLowerInvariant())) {
						var uiOrigin = UiCustomerOrigin.Get(HttpContext.Current.Request.Url);
						originID = (int)uiOrigin.GetOrigin();
					} // if
				} // if

				var user = this.userRepo.GetUserByLogin(login, originID);
				HttpContext.Current.Items[UserItemName] = user;

				return user;
			} // get
		} // User

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

		private const string SessionCookieName = "_EzbobSession_";
		private const string UserItemName = "EzBobUser";

		private readonly IUsersRepository userRepo;
		private readonly ICustomerRepository customerRepo;
		private readonly string[] originAreas = { "broker", "customer", };
	} // class EzBobContext
} // namespace