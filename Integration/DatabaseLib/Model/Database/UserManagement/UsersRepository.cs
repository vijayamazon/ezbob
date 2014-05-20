namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using ApplicationMng.Repository;
	using NHibernate;
	using NHibernate.Criterion;
	using NHibernate.Linq;
	using System.Linq;

	public interface IUsersRepository : IRepository<User>
	{
		User GetUserByLogin(string login);
		bool IsUserInRole(int userId, string role);
		bool CheckUserLogin(int userId, string userName);
		bool CheckUserDomainName(int userId, string domainUserName);
		IQueryable<User> GetActiveUsers();
		void EvictAll();
		void Refresh(User user);
	}

	public class UsersRepository : NHibernateRepositoryBase<User>, IUsersRepository
	{
		public UsersRepository(ISession session) : base(session)
		{
		}
		public User GetUserByLogin(string login)
		{
			User user = null;
			EnsureTransaction(() =>
			{
				user = (
					from u in GetAll()
					where u.Name.ToLower() == login.ToLower() && u.IsDeleted == 0
					select u).Cacheable<User>().CacheRegion("Longest").SingleOrDefault();
			});
			return user;
		}
		public bool IsUserInRole(int userId, string role)
		{
			return _session.Load<User>(userId).Roles.Any(r => r.Name.ToUpper() == role);
		}
		public bool CheckUserLogin(int userId, string userName)
		{
			return !GetAll().Any(u => u.Name.ToLower() == userName.ToLower() && u.Id != userId && u.IsDeleted == 0);
		}
		public bool CheckUserDomainName(int userId, string domainUserName)
		{
			return string.IsNullOrEmpty(domainUserName) || !GetAll().Any(u => u.DomainUserName == domainUserName && u.Id != userId && u.IsDeleted == 0);
		}
		public IQueryable<User> GetActiveUsers()
		{
			return 
				from u in GetAll()
				where u.IsDeleted == 0
				select u;
		}
		public void Refresh(User user)
		{
			_session.Refresh(user);
		}
	}
}
