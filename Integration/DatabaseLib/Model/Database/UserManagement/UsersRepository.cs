namespace EZBob.DatabaseLib.Model.Database.UserManagement {
	using System;
	using ApplicationMng.Repository;
	using NHibernate;
	using NHibernate.Linq;
	using System.Linq;
	using log4net;

	public interface IUsersRepository : IRepository<User> {
		User GetUserByLogin(string login);
	}

	public class UsersRepository : NHibernateRepositoryBase<User>, IUsersRepository {
		protected static ILog Log = LogManager.GetLogger(typeof(UsersRepository));

		public UsersRepository(ISession session)
			: base(session) {
		}
		public User GetUserByLogin(string login) {
			User user = null;
			try {
				EnsureTransaction(() => {
					user = GetAll()
						.Where(u => u.Name == login && u.IsDeleted == 0)
						.Cacheable<User>()
						.CacheRegion("Longest")
						.SingleOrDefault();
				});
			} catch (Exception ex) {
				Log.Error("Failed to retrieve user by login " + login, ex);
			}
			
			return user;
		}
	}
}
