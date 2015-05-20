namespace EzBob.Web.Infrastructure {
	using System;
	using System.Web;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	public class EzBobContext : IEzbobWorkplaceContext {
		private readonly IUsersRepository _users;
		private readonly ICustomerRepository _customers;

		public EzBobContext(IUsersRepository users, ICustomerRepository customers) {
			_users = users;
			_customers = customers;
		}

		public User User {
			get {
				var cached = HttpContext.Current.Items["EzBobUser"] as User;
				if (cached != null)
					return cached;

				var login = HttpContext.Current.User.Identity.Name;

				var user = _users.GetUserByLogin(login);
				HttpContext.Current.Items["EzBobUser"] = user;

				return user;
			}
		}

		public int UserId {
			get { return User.Id; }
		}

		public string SessionId {
			get {
				var cookie = HttpContext.Current.Request.Cookies["_EzbobSession_"];

				if (cookie != null) {
					string sid = cookie.Value;
					return sid;
				}

				return "";
			}
			set {
				var httpCookie = new HttpCookie("_EzbobSession_", value) { HttpOnly = true, Secure = true };

				if (value == null)
					httpCookie.Expires = DateTime.UtcNow.AddDays(-1);

				HttpContext.Current.Response.SetCookie(httpCookie);
			}
		}

		public Customer Customer {
			get {
				if (User == null)
					return null;

				return _customers.ReallyTryGet(User.Id);
			}
		}
	}
}