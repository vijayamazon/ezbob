using System;
using System.Web;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using Scorto.NHibernate.Repository;

namespace EzBob.Web.Infrastructure
{
    public class EzBobContext : IEzbobWorkplaceContext
    {
        private readonly ISecurityApplicationsRepository _apps;
        private readonly IUsersRepository _users;
        private readonly ICustomerRepository _customers;
        private SecurityApplication _wp;

        public EzBobContext(ISecurityApplicationsRepository apps, IUsersRepository users, ICustomerRepository customers)
        {
            _apps = apps;
            _users = users;
            _customers = customers;
        }

        public SecurityApplication SecApp
        {
            get { return _wp ?? (_wp = _apps.Get(1)); }
        }

        public int SecAppId
        {
            get { return 1; }
        }

        public User User
        {
            get
            {
                var cached = HttpContext.Current.Items["EzBobUser"] as User;
                if (cached != null) return cached;
                
                var login = HttpContext.Current.User.Identity.Name;                
                
                var user = _users.GetUserByLogin(login);
                HttpContext.Current.Items["EzBobUser"] = user;
                
                return user;
            }
        }

        public int UserId
        {
            get { return User.Id; }
        }

        public string SessionId
        {
            get
            {
                var cookie = HttpContext.Current.Request.Cookies["_ScortoSession_"];
                if (cookie != null)
                {
                    string sid = cookie.Value;
                    return sid;
                }
                return "";
            }
            set
            {
                var httpCookie = new HttpCookie("_ScortoSession_", value) {HttpOnly = true, Secure = true};
                if(value == null)
                {
                    httpCookie.Expires = DateTime.Now.AddDays(-1d);
                }
                HttpContext.Current.Response.SetCookie(httpCookie);
            }
        }

        public Customer Customer
        {
            get
            {
                if (User == null) return null; 
                return _customers.TryGet(User.Id);
            }
        }
    }
}