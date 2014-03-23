using ApplicationMng.Model;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class UsersRepository : NHibernateRepositoryBase<User>, IUsersRepository, IRepository<User>, System.IDisposable
	{
		public UsersRepository(ISession session) : base(session)
		{
		}
		public User GetUserByLogin(string login)
		{
			User user = null;
			this.EnsureTransaction(() =>
			{
				user = (
					from u in this.GetAll()
					where u.Name.ToLower() == login.ToLower() && u.IsDeleted == 0
					select u).Cacheable<User>().CacheRegion("Longest").SingleOrDefault<User>();
			});
			return user;
		}
		public bool IsUserInRole(int userId, string role)
		{
			return this._session.Load<User>(userId).Roles.Any((Role r) => r.Name.ToUpper() == role);
		}
		public bool CheckUserLogin(int userId, string userName)
		{
			return !this.GetAll().Any((User u) => u.Name.ToLower() == userName.ToLower() && u.Id != userId && u.IsDeleted == 0);
		}
		public bool CheckUserDomainName(int userId, string domainUserName)
		{
			return string.IsNullOrEmpty(domainUserName) || !this.GetAll().Any((User u) => u.DomainUserName == domainUserName && u.Id != userId && u.IsDeleted == 0);
		}
		public void BlockUser(User user)
		{
			this.EnsureTransaction(() =>
			{
				user.IsDeleted = 2;
				this._session.CreateQuery("update ApplicationMng.Model.SecuritySession s set s.State = 0 where s.User = :user").SetEntity("user", user).ExecuteUpdate();
				this._session.CreateQuery("update ApplicationMng.Model.Application a set a.Locker = null where a.Locker = :user").SetEntity("user", user).ExecuteUpdate();
				this.Update(user);
			});
		}
		public void DisableSessionsAndApplications(User user)
		{
			this.EnsureTransaction(() =>
			{
				this._session.CreateQuery("update ApplicationMng.Model.SecuritySession s set s.State = 0 where s.User = :user").SetEntity("user", user).ExecuteUpdate();
				this._session.CreateQuery("update ApplicationMng.Model.Application a set a.Locker = null where a.Locker = :user").SetEntity("user", user).ExecuteUpdate();
			});
		}
		public void UnBlockUser(User user)
		{
			user.IsDeleted = 0;
			user.LoginFailedCount = null;
			user.LastBadLogin = null;
			this.Update(user);
		}
		public System.Linq.IQueryable<User> GetActiveUsers()
		{
			return 
				from u in this.GetAll()
				where u.IsDeleted == 0
				select u;
		}
		public System.Collections.Generic.IList<User> GetUsers(int page, int itemsPerPage, string query)
		{
			return this._session.CreateQuery("from ApplicationMng.Model.User u" + query).SetFirstResult(page * itemsPerPage).SetMaxResults(itemsPerPage).List<User>();
		}
		public long GetUsersCount(string query)
		{
			return this._session.CreateQuery("select count(u.id) from ApplicationMng.Model.User u" + query).UniqueResult<long>();
		}
		public bool HasAccesTo(int userId, int appId)
		{
			return this._session.CreateCriteria<User>("u").CreateAlias("u.Roles", "r").CreateAlias("r.Applications", "a").Add(Restrictions.Eq("a.Id", appId)).Add(Restrictions.Eq("u.Id", userId)).SetProjection(new IProjection[]
			{
				Projections.Count("u.Id")
			}).UniqueResult<int>() > 0;
		}
		public void Refresh(User user)
		{
			this._session.Refresh(user);
		}
	}
}
